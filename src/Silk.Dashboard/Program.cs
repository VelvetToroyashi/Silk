using AspNet.Security.OAuth.Discord;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MudBlazor.Services;
using Remora.Discord.Rest.Extensions;
using Silk.Dashboard.Services;
using Silk.Data;
using Silk.Shared.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appSettings.json", true, false);
builder.Configuration.AddUserSecrets<Program>(true, false);

// Add services to the container.
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddControllers();
builder.Services.AddServerSideBlazor();

builder.Services.AddMudServices();

builder.Services.AddHttpClient();
builder.Services.AddMediatR(typeof(GuildContext));

/* Todo: Consolidate Adding SilkConfigurationOptions to common location? */
var silkConfigSection = builder.Configuration.GetSection(SilkConfigurationOptions.SectionKey);
builder.Services.Configure<SilkConfigurationOptions>(silkConfigSection);

var silkConfig = silkConfigSection.Get<SilkConfigurationOptions>();
builder.Services.AddDbContextFactory<GuildContext>(dbBuilder =>
{
    dbBuilder.UseNpgsql(silkConfig.Persistence.GetConnectionString());
#if DEBUG
    dbBuilder.EnableSensitiveDataLogging();
    dbBuilder.EnableDetailedErrors();
#endif
});

//services.TryAdd(new ServiceDescriptor(typeof(GuildContext),
//    p => p.GetRequiredService<IDbContextFactory<GuildContext>>().CreateDbContext(),
//    ServiceLifetime.Transient));

/* Todo: This uses the BotToken for 'Scheme', so don't care about Discord OAuth2 Token? */
builder.Services.AddDiscordRest(provider => provider.GetRequiredService<IOptions<SilkConfigurationOptions>>()
                                                    .Value.Discord.BotToken);

builder.Services.AddScoped<DiscordRestClientService>();

builder.Services.AddAuthentication(opt =>
        {
            opt.DefaultScheme          = CookieAuthenticationDefaults.AuthenticationScheme;
            opt.DefaultSignInScheme    = CookieAuthenticationDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
        })
       .AddDiscord(opt =>
        {
            opt.UsePkce      = true;
            opt.SaveTokens   = true;
            
            opt.Scope.Add("guilds");
            
            opt.ClientId     = silkConfig.Discord.ClientId;
            opt.ClientSecret = silkConfig.Discord.ClientSecret;

            opt.CallbackPath = DiscordAuthenticationDefaults.CallbackPath;
        })
       .AddCookie(options =>
        {
            /* Set Cookie expiry (default Discord access token expiry time?) */
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
        });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapControllers();
app.MapFallbackToPage("/_Host");

app.Run();