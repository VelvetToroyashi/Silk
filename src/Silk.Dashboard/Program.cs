using AspNet.Security.OAuth.Discord;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Remora.Discord.Rest.Extensions;
using Silk.Dashboard.Extensions;
using Silk.Dashboard.Services;
using Silk.Dashboard.Services.DiscordTokenStorage;
using Silk.Dashboard.Services.DiscordTokenStorage.Interfaces;
using Silk.Data;
using Silk.Shared.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appSettings.json", true, false);
builder.Configuration.AddUserSecrets<Program>(true, false);

// Add services to the container.
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddControllers();
builder.Services.AddServerSideBlazor();

builder.Services.AddDataProtection().SetDefaultKeyLifetime(TimeSpan.FromDays(14));
builder.Services.AddMemoryCache();

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

builder.Services.AddMudServices();

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

builder.Services.AddSingleton<IDiscordTokenStore, DiscordTokenStore>();

// Todo: Hack/clever way to use OAuth instead of Bot token
builder.Services.AddDiscordRest(_ => "_", clientBuilder =>
{
    clientBuilder.ConfigureHttpClient((provider, client) =>
    {
        // Todo: Check that using the context accessor works (NOT recommended by Blazor team due to HttpContext only hitting _Host.cshtml page on initial request or full page reload)
        var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
        var tokenStore          = provider.GetRequiredService<IDiscordTokenStore>();

        var userId      = httpContextAccessor.HttpContext?.User.GetUserId();
        var accessToken = tokenStore.GetToken(userId!)?.AccessToken;

        client.DefaultRequestHeaders.Authorization = new("Bearer", accessToken);
    });
});

builder.Services.AddScoped<DiscordRestClientService>();

builder.Services.AddAuthentication(opt =>
        {
            opt.DefaultScheme          = CookieAuthenticationDefaults.AuthenticationScheme;
            opt.DefaultSignInScheme    = CookieAuthenticationDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
        })
       .AddDiscord(opt =>
        {
            opt.UsePkce    = true;
            opt.SaveTokens = true;

            opt.Scope.Add("guilds");

            opt.ClientId     = silkConfig.Discord.ClientId;
            opt.ClientSecret = silkConfig.Discord.ClientSecret;

            opt.CallbackPath       = DiscordAuthenticationDefaults.CallbackPath;
            opt.AccessDeniedPath   = new("/");
            opt.ReturnUrlParameter = string.Empty;

            opt.Events.OnCreatingTicket = context =>
            {
                var tokenStore = context.HttpContext.RequestServices.GetRequiredService<IDiscordTokenStore>();

                var userId= context.Principal.GetUserId();

                const string tokenExpiryKey = ".Token.expires_at";
                var tokenExpiry = DiscordTokenStoreExtensions.GetTokenExpiry(context.Properties.Items[tokenExpiryKey]);
                var tokenEntry = new DiscordTokenStoreEntry(context.AccessToken,
                                                            context.RefreshToken,
                                                            tokenExpiry,
                                                            context.TokenType);
                tokenStore.SetToken(userId!, tokenEntry);
                return Task.CompletedTask;
            };
        })
       .AddCookie(options =>
        {
            // Todo: Find way to set expiration based on OAuth token expiry
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