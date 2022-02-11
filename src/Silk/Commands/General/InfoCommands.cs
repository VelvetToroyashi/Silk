using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Extensions.Caching.Memory;
using OneOf;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Caching;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Extensions;
using Silk.Extensions.Remora;
using Silk.Utilities.HelpFormatter;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = System.Drawing.Color;

namespace Silk.Commands.General;

[HelpCategory(Categories.General)]
public class InfoCommands : CommandGroup
{
    private readonly IMemoryCache           _cache;
    private readonly MessageContext         _context;
    private readonly IDiscordRestUserAPI    _userApi;
    private readonly IDiscordRestGuildAPI   _guilds;
    private readonly IDiscordRestChannelAPI _channels;

    public InfoCommands
    (
        IMemoryCache           cache,
        MessageContext         context,
        IDiscordRestUserAPI    userApi,
        IDiscordRestGuildAPI   guilds,
        IDiscordRestChannelAPI channels
    )
    {
        _cache    = cache;
        _context  = context;
        _userApi  = userApi;
        _guilds   = guilds;
        _channels = channels;
    }

    [Command("info")]
    [RequireContext(ChannelContext.Guild)]
    [Description("Get information about a member!")]
    public async Task<IResult> GetMemberInfoAsync(IGuildMember member)
    {
        var roleResult = await _guilds.GetGuildRolesAsync(_context.GuildID.Value);

        if (!roleResult.IsDefined(out var roleList))
            return roleResult;

        var roles = roleList.ToDictionary(r => r.ID, r => r);
        
        UncacheUser(member.User.Value.ID);
        
        var userResult = await _userApi.GetUserAsync(member.User.Value.ID);

        if (!userResult.IsDefined(out var user))
            return userResult;
        
        var avatar = CDN.GetUserAvatarUrl(user, imageSize: 4096);

        if (!avatar.IsSuccess)
            avatar = CDN.GetDefaultUserAvatarUrl(user, imageSize: 4096);
        
        var bannerUrl = CDN.GetUserBannerUrl(user, imageSize: 4096);

        var bannerImage = default(Stream);

        if (!bannerUrl.IsSuccess)
            bannerImage = !user.AccentColour.IsDefined(out var accent)
                ? default
                : await GenerateBannerColorImageAsync(accent.ToString()!);
        
        var embed = new Embed
        {
            Title = $"{user.Username}#{user.Discriminator:0000}",
            Thumbnail = new EmbedThumbnail(avatar.Entity.ToString()),
            Image = new EmbedImage(bannerUrl.IsSuccess ? bannerUrl.Entity.ToString() : "attachment://banner.png"),
            Fields = new EmbedField[]
            {
                new("Joined", member.JoinedAt.ToTimestamp(), true),
                new("Account Created", user.ID.Timestamp.ToTimestamp(), true),
                new("Flags", user.PublicFlags.IsDefined(out var flags) ? flags.Humanize(LetterCasing.Title) : "None", true),
                new("Roles", string.Join(",\n", member.Roles.OrderByDescending(r => roles[r].Position).Select(x => $"<@&{x}>")), true),
            }
        };

        var res = await _channels.CreateMessageAsync
            (
             _context.ChannelID,
             embeds: new[] {embed},
             attachments: bannerUrl.IsSuccess || bannerImage is null
                 ? default(Optional<IReadOnlyList<OneOf<FileData, IPartialAttachment>>>)
                 : new[] { OneOf<FileData, IPartialAttachment>.FromT0(new("banner.png", bannerImage!)) }
            );

        return res;
    }

    [Command("info")]
    [Description("Get information about a user or yourself!")]
    public async Task<IResult> GetUserInfoAsync(IUser? user = null)
    {
        user ??= _context.User;
        
        UncacheUser(user.ID);
        
        var userResult = await _userApi.GetUserAsync(user.ID);
        
        if (!userResult.IsDefined(out user))
            return userResult;
        
        var avatar = CDN.GetUserAvatarUrl(user, imageSize: 4096);
        
        if (!avatar.IsSuccess)
            avatar = CDN.GetDefaultUserAvatarUrl(user, imageSize: 4096);
        
        var bannerUrl = CDN.GetUserBannerUrl(user, imageSize: 4096);
        
        var bannerImage = default(Stream);
        
        if (!bannerUrl.IsSuccess)
            bannerImage = !user.AccentColour.IsDefined(out var accent)
                ? default
                : await GenerateBannerColorImageAsync(accent.ToString()!);
        
        var embed = new Embed
        {
            Title     = $"{user.Username}#{user.Discriminator:0000}",
            Thumbnail = new EmbedThumbnail(avatar.Entity.ToString()),
            Image     = new EmbedImage(bannerUrl.IsSuccess ? bannerUrl.Entity.ToString() : "attachment://banner.png"),
            Fields = new EmbedField[]
            {
                new("Account Created", user.ID.Timestamp.ToTimestamp(), true),
                new("Flags", user.PublicFlags.IsDefined(out var flags) ? flags.Humanize(LetterCasing.Title) : "None", true),
            }
        };
        
        var res = await _channels.CreateMessageAsync
            (
             _context.ChannelID,
             embeds: new[] {embed},
             attachments: bannerUrl.IsSuccess || bannerImage is null
                 ? default(Optional<IReadOnlyList<OneOf<FileData, IPartialAttachment>>>)
                 : new[] { OneOf<FileData, IPartialAttachment>.FromT0(new("banner.png", bannerImage!)) }
            );

        return res;
    }
    
    [Command("info")]
    [RequireContext(ChannelContext.Guild)]
    [Description("Get information about a role!")]
    public async Task<IResult> GetRoleInfoAsync(IRole role)
    {
        var hierarchyResult = await GetRoleHiearchyStringAsync(role);
        
        if (!hierarchyResult.IsDefined(out var hierarchy))
            return hierarchyResult;

        
        var permissions = role
                         .Permissions
                         .GetPermissions()
                         .Select(p => p.Humanize(LetterCasing.Title))
                         .OrderBy(p => p[0])
                         .ThenBy(p => p.Length)
                         .Select(p => $"`{p}`")
                         .Chunk(4)
                         .Select(p => p.Aggregate((c, n) => n.Length > 18 ? $"{c}\n{n}" : $"{c}, {n}"))
                         .Join("\n");
        
        var embed = new Embed
        {
            Title = $"{role.Name}",
            Colour = role.Colour,
            Image = new EmbedImage("attachment://swatch.png"),
            Fields = new EmbedField[]
            {
                new("ID", role.ID.ToString(), true),
                new("Created", $"{role.ID.Timestamp.ToTimestamp(TimestampFormat.LongDate)}\n({role.ID.Timestamp.ToTimestamp()})", true),
                new("Position", role.Position.ToString(), true),
                new("Color", $"#{role.Colour.Name[2..]}", true),
                new("Hierarchy", hierarchy, true),
                new("Mentionable", role.IsMentionable ? "Yes" : "No", true),
                new("Hoisted", role.IsHoisted ? "Yes" : "No", true),
                new("Bot/Managed", role.IsManaged ? "Yes" : "No", true),
                new("Permissions", permissions),
            }
        };

        await using var swatchImage = await GenerateRoleColorSwatchAsync(role.Colour);
        
        var res = await _channels.CreateMessageAsync
            (
             _context.ChannelID,
             embeds: new[] {embed},
             attachments: new[] { OneOf<FileData, IPartialAttachment>.FromT0(new("swatch.png", swatchImage)) }
            );

        return res;
    }


    private async Task<Result<string>> GetRoleHiearchyStringAsync(IRole role)
    {
        var roleResult = await _guilds.GetGuildRolesAsync(_context.GuildID.Value);
        
        if (!roleResult.IsDefined(out var roles))
            return Result<string>.FromError(roleResult.Error!);

        roles = roles.OrderBy(r => r.Position).ToArray();
        
        var roleIDs = roles.OrderBy(r => r.Position).Select(r => r.ID).ToArray();

        var currentIndex = roleIDs.IndexOf(role.ID);

        var sb = new StringBuilder();
        
        if (currentIndex < roles.Count)
        {
            var next = roles[currentIndex + 1];
            
            sb.AppendLine($"{next.Mention()}");
            sb.AppendLine("\u200b\t↑");
        }
        
        sb.AppendLine($"{role.Mention()}");
        
        if (currentIndex > 0)
        {
            var prev = roles[currentIndex - 1];
            
            sb.AppendLine("\u200b\t↑");
            sb.AppendLine($"{prev.Mention()}");
        }
        
        return Result<string>.FromSuccess(sb.ToString());
    }

    private void UncacheUser(Snowflake userID) => _cache.Remove(KeyHelpers.CreateUserCacheKey(userID));

    private async Task<Stream> GenerateRoleColorSwatchAsync(Color roleColor)
    {
        using var image = new Image<Rgb24>(600, 200, new Rgb24(roleColor.R, roleColor.G, roleColor.B));

        var stream = new MemoryStream();
        
        await image.SaveAsPngAsync(stream);

        stream.Seek(0, SeekOrigin.Begin);
        
        return stream;
    }
    
    private async Task<Stream> GenerateBannerColorImageAsync(string bannerColor)
    {
        using var image = new Image<Rgba32>(4096, 2048, Rgba32.ParseHex(bannerColor));

        var stream = new MemoryStream();
        
        await image.SaveAsPngAsync(stream);

        stream.Seek(0, SeekOrigin.Begin);

        return stream;
    }
}