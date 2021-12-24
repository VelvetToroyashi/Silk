/*using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FuzzySharp;
using FuzzySharp.Extractor;
using Microsoft.Extensions.Caching.Memory;
using Silk.Data.Entities;
using Silk.Services.Guild;

namespace Silk.Infrastructure.ChoiceProviders;

public class TagChoiceProvider : IAutocompleteProvider
{

    private readonly IMemoryCache _cache;
    private readonly TagService   _tags;

    public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
    {
        // This is possibly inefficient. //
        TagEntity[] tags = await GetTagsAsync(ctx);

        var query = ctx.OptionValue.ToString()!;

        IEnumerable<ExtractedResult<TagEntity>>? results = Process.ExtractSorted(new() { Name = query }, tags, s => s.Name).Where(r => r.Score > 40);

        return results.Select(r => new DiscordAutoCompleteChoice($"{r.Value.Name} ({r.Score}% match)", r.Value.Name));
    }

    private async Task<TagEntity[]> GetTagsAsync(AutocompleteContext ctx)
    {
        TagEntity[] tags;
        if (!_cache.TryGetValue($"guild_{ctx.Interaction.GuildId}_tags", out object? tagsObj))
        {
            IEnumerable<TagEntity>? dbTags = await _tags.GetGuildTagsAsync(new(ctx.Interaction.Guild.Id));
            tags = dbTags.ToArray();

            _cache.Set($"guild_{ctx.Interaction.GuildId}_tags", tags);
        }
        else
        {
            tags = (tagsObj as TagEntity[])!;
        }
        return tags;
    }
}*/