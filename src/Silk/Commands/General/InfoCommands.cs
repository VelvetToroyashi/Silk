using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using OneOf;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Caching;
using Remora.Discord.Caching.Abstractions;
using Remora.Discord.Caching.Abstractions.Services;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Extensions;
using Silk.Extensions.Remora;
using Silk.Shared.Constants;
using Silk.Utilities;
using Silk.Utilities.HelpFormatter;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = System.Drawing.Color;

namespace Silk.Commands.General;

[Category(Categories.General)]
public class InfoCommands : CommandGroup
{
    private readonly ICacheProvider         _cache;
    private readonly ITextCommandContext         _context;
    private readonly IDiscordRestUserAPI    _users;
    private readonly IDiscordRestEmojiAPI   _emojis;
    private readonly IDiscordRestGuildAPI   _guilds;
    private readonly IDiscordRestChannelAPI _channels;

    public InfoCommands
    (
        ICacheProvider         cache,
        ITextCommandContext         context,
        IDiscordRestUserAPI    users,
        IDiscordRestEmojiAPI   emojis,
        IDiscordRestGuildAPI   guilds,
        IDiscordRestChannelAPI channels
    )
    {
        _cache    = cache;
        _context  = context;
        _users    = users;
        _emojis   = emojis;
        _guilds   = guilds;
        _channels = channels;
    }

    [Command("info")]
    [Description("Get information about a user or member!")]
    public async Task<IResult> GetMemberOrUserInfoAsync(OneOf<IGuildMember, IUser>? memberOrUser = null)
    {
        if (memberOrUser is null && _context.GuildID.IsDefined(out var guild))
        {
            var memberResult = await _guilds.GetGuildMemberAsync(guild, _context.GetUserID());
            
            if (memberResult.IsSuccess)
                memberOrUser = OneOf<IGuildMember, IUser>.FromT0(memberResult.Entity);
        }

        if (memberOrUser is not { } mou)
            return await GetUserInfoAsync();
        
        return mou.TryPickT0(out var member, out var user)
            ? await GetMemberInfoAsync(member)
            : await GetUserInfoAsync(user);
    }

    public async Task<IResult> GetMemberInfoAsync(IGuildMember member)
    {
        var roleResult = await _guilds.GetGuildRolesAsync(_context.GuildID.Value);

        if (!roleResult.IsDefined(out var roleList))
            return roleResult;

        var roles = roleList.ToDictionary(r => r.ID, r => r);
        
        await UncacheUserAsync(member.User.Value.ID);
        
        var userResult = await _users.GetUserAsync(member.User.Value.ID);

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
                : await GenerateBannerColorImageAsync(accent.Value);

        var embed = new Embed
        {
            Title     = user.ToDiscordTag(),
            Thumbnail = new EmbedThumbnail(avatar.Entity.ToString()),
            Colour = Color.DodgerBlue,
            Image     = new EmbedImage(bannerUrl.IsSuccess ? bannerUrl.Entity.ToString() : "attachment://banner.png"),
            Fields = new EmbedField[]
            {
                new("Account Created", user.ID.Timestamp.ToTimestamp(), true),
                new("Joined", member.JoinedAt.ToTimestamp(), true),
                new("Flags", user.PublicFlags.IsDefined(out var flags) ? (int)flags is 0 ? "None" : flags.ToString().Split(' ').Join("\n").Humanize(LetterCasing.Title) : "None", true),
                new("Roles", string.Join(",\n", member.Roles.Append(roles[_context.GuildID.Value].ID).OrderByDescending(r => roles[r].Position).Select(x => $"<@&{x}>"))),
            }
        };

        var res = await _channels.CreateMessageAsync
        (
         _context.GetChannelID(),
         embeds: new[] {embed},
         attachments: bannerUrl.IsSuccess || bannerImage is null
             ? default(Optional<IReadOnlyList<OneOf<FileData, IPartialAttachment>>>)
             : new[] { OneOf<FileData, IPartialAttachment>.FromT0(new("banner.png", bannerImage!)) }
        );

        return res;
    }

    public async Task<IResult> GetUserInfoAsync(IUser? user = null)
    {
        user ??= _context.GetUser();
        
        await UncacheUserAsync(user.ID);
        
        var userResult = await _users.GetUserAsync(user.ID);
        
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
                : await GenerateBannerColorImageAsync(accent.Value);
        
        var embed = new Embed
        {
            Title     = user.ToDiscordTag(),
            Thumbnail = new EmbedThumbnail(avatar.Entity.ToString()),
            Colour = Color.DodgerBlue,
            Image     = new EmbedImage(bannerUrl.IsSuccess ? bannerUrl.Entity.ToString() : "attachment://banner.png"),
            Fields = new EmbedField[]
            {
                new("Account Created", user.ID.Timestamp.ToTimestamp(), true),
                new("Flags", user.PublicFlags.IsDefined(out var flags) ? flags.Humanize(LetterCasing.Title) : "None", true),
            }
        };
        
        var res = await _channels.CreateMessageAsync
        (
             _context.GetChannelID(),
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
        var roleResult = await _guilds.GetGuildRolesAsync(_context.GuildID.Value);
        
        if (!roleResult.IsDefined(out var roles))
            return roleResult;

        var highestRole = roles.Max(r => r.Position);
        
        var hierarchyResult = await GetRoleHierarchyStringAsync(role);
        
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
                new("Position", $"{role.Position}/{highestRole}", true),
                new("Color", $"#{role.Colour.Name[2..].ToUpper()}", true),
                new("Hierarchy", hierarchy, true),
                new("Mentionable", role.IsMentionable ? "Yes" : "No", true),
                new("Hoisted", role.IsHoisted ? "Yes" : "No", true),
                new("Bot/Managed", role.IsManaged ? "Yes" : "No", true),
                new("Permissions", permissions),
            },
        };

        await using var swatchImage = await GenerateRoleColorSwatchAsync(role.Colour);
        
        var res = await _channels.CreateMessageAsync
        (
         _context.GetChannelID(),
         embeds: new[] {embed},
         attachments: new[] { OneOf<FileData, IPartialAttachment>.FromT0(new("swatch.png", swatchImage)) }
        );

        return res;
    }

    [Command("info")]
    [RequireContext(ChannelContext.Guild)]
    [Description("Get information about an emoji!")]
    public async Task<IResult> GetEmojiInfoAsync(IPartialEmoji emoji)
    {
        if (!emoji.ID.IsDefined(out var emojiID))
            return await _channels.CreateMessageAsync(_context.GetChannelID(), $"{Emojis.WarningEmoji}  This appears to be a unicode emoji. I can't tell you anything about it!");
        
        var emojiResult = await _emojis.ListGuildEmojisAsync(_context.GuildID.Value);
        
        if (!emojiResult.IsDefined(out var emojis))
            return emojiResult;
        
        var guildEmoji = emojis.FirstOrDefault(e => e.ID == emojiID);
        
        Embed embed;

        var emojiUrl = CDN.GetEmojiUrl(guildEmoji?.ID ?? emojiID.Value, imageSize: 256);

        if (!emojiUrl.IsDefined(out var url))
            return emojiUrl;
        
        if (guildEmoji is null)
        {
            embed = new()
            {
                Title  = $"Info about {(emoji.Name.IsDefined(out var eName) ? eName : "(This emoji is unnamed. Potential bug?)")}",
                Colour = Color.DodgerBlue,
                Image = new EmbedImage(url.ToString()),
                Fields = new EmbedField[]
                {
                    new("ID", emoji.ID.ToString()),
                    new("Created", emoji.ID.Value.Value.Timestamp.ToTimestamp(TimestampFormat.LongDate))
                }
            };
        }
        else
        {
            var roleLocked = guildEmoji.Roles.IsDefined(out var roles) && roles.Any();

            embed = new()
            {
                Title  = $"Emoji info for {guildEmoji.Name ?? "(This emoji is unnamed. Potential bug?)"}",
                Colour = Color.DodgerBlue,
                Image  = new EmbedImage(url.ToString()),
                Fields = new EmbedField[]
                {
                    new("ID", emoji.ID.Value.Value.ToString()!),
                    new("Created", emoji.ID.Value.Value.Timestamp.ToTimestamp(TimestampFormat.LongDate)),
                    new("Animated", guildEmoji.IsAnimated.IsDefined(out var anim)  && anim ? Emojis.ConfirmEmoji : Emojis.DeclineEmoji),
                    new("Managed", guildEmoji.IsManaged.IsDefined(out var managed) && managed ? Emojis.ConfirmEmoji : Emojis.DeclineEmoji),
                    new("Added By", guildEmoji.User.IsDefined(out var addedBy) ? addedBy.ToDiscordTag() : "Unknown"),
                    new("Role-Locked", roleLocked ? Emojis.ConfirmEmoji : Emojis.DeclineEmoji),
                    new("Role-Locked to", roleLocked ? roles!.Select(r => $"<@&{r}>").Join(",\n") : "None")
                }
            };
        }

        return await _channels.CreateMessageAsync(_context.GetChannelID(), embeds: new[] {embed});
    }
    
    private async Task<Result<string>> GetRoleHierarchyStringAsync(IRole role)
    {
        var roleResult = await _guilds.GetGuildRolesAsync(_context.GuildID.Value);
        
        if (!roleResult.IsDefined(out var roles))
            return Result<string>.FromError(roleResult.Error!);

        roles = roles.OrderBy(r => r.Position).ToArray();
        
        var roleIDs = roles.OrderBy(r => r.Position).Select(r => r.ID).ToArray();
        
        var currentIndex = Array.IndexOf(roleIDs, role.ID); 

        var sb = new StringBuilder();
        
        if (currentIndex < roles.Count - 1)
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

    private async Task UncacheUserAsync(Snowflake userID) => await _cache.EvictAsync(new KeyHelpers.UserCacheKey(userID));

    private async Task<Stream> GenerateRoleColorSwatchAsync(Color roleColor)
    {
        using var image = new Image<Rgb24>(600, 200, new Rgb24(roleColor.R, roleColor.G, roleColor.B));

        var stream = new MemoryStream();
        
        await image.SaveAsPngAsync(stream);

        stream.Seek(0, SeekOrigin.Begin);
        
        return stream;
    }
    
    private async Task<Stream> GenerateBannerColorImageAsync(Color bannerColor)
    {
        var key = CacheKey.StringKey($"Color:{bannerColor.ToArgb()}");
        
        // Can you even serialize a memory stream??
        var cacheResult = await _cache.RetrieveAsync<byte[]>(key);

        if (cacheResult.IsDefined(out var bytes))
        {
            return new MemoryStream(bytes);
        }
        
        using var image = new Image<Rgba32>(4096, 2048, new(bannerColor.R, bannerColor.G, bannerColor.B, 255));

        var stream = new MemoryStream();
        
        await image.SaveAsPngAsync(stream);

        stream.Seek(0, SeekOrigin.Begin);
        
        await _cache.CacheAsync(key, stream.ToArray(), new() { AbsoluteExpiration = DateTimeOffset.UtcNow + TimeSpan.FromHours(1) });
        
        return stream;
    }
}