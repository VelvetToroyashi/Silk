// This file is part of Companion Cube project, initially licensed under Apache 2.0.
//
// Copyright (C) 2018-2021 Emzi0767
// 


using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Commands.Conditions;
using Silk.Utilities;
using Silk.Utilities.HelpFormatter;
using CollectionExtensions = Silk.Extensions.CollectionExtensions;

namespace Silk.Commands.Bot;

// THIS COMMAND WAS RIPPED FROM Emzi0767#1837. I ONLY MADE IT EVAL INLINE CODE  ~Velvet, as always //

[Category(Categories.Bot)]
public class EvalCommand : CommandGroup
{
    private readonly ITextCommandContext _context;
    
    private readonly IDiscordRestUserAPI                _users;
    private readonly IDiscordRestGuildAPI               _guilds;
    private readonly IDiscordRestChannelAPI             _channels;
    private readonly IDiscordRestGuildScheduledEventAPI _events;

    private readonly IServiceProvider _services;

    private const string _usings = @"
System
System.Collections
System.Collections.Generic
System.ComponentModel
System.Drawing
System.Linq
System.Reflection
System.Runtime.CompilerServices
System.Text.RegularExpressions
System.Threading.Tasks
Remora.Commands.Attributes
Remora.Commands.Groups
Remora.Discord.API.Abstractions.Objects
Remora.Discord.API.Abstractions.Rest
Remora.Discord.API.Objects
Remora.Discord.Commands.Contexts
Remora.Rest.Core
Remora.Results
Silk.Commands.Conditions
Silk.Utilities.HelpFormatter
Mediator
Microsoft.EntityFrameworkCore
Remora.Rest.Core
Remora.Results
Silk.Data.Entities
Silk.Data
Silk.Extensions
System.Text
Silk
Microsoft.Extensions.Logging
";

    private static readonly IEmbed _evaluatingEmbed = new Embed
    {
        Title  = "Evaluating. Please wait.",
        Colour = Color.HotPink
    };
    
    public EvalCommand(ITextCommandContext context, IDiscordRestChannelAPI channels, IDiscordRestUserAPI users, IDiscordRestGuildAPI guilds, IDiscordRestGuildScheduledEventAPI events, IServiceProvider services)
    {
        _context       = context;
        _channels      = channels;
        _users         = users;
        _guilds        = guilds;
        _events        = events;
        _services = services;
    }
    
    [Command("eval")]
    [RequireTeamOrOwner]
    [Description("Evaluates code.")]
    public async Task<Result> EvalCS([Greedy] string _)
    {
        var cs = Regex.Replace(_context.Message.Content.Value, @"^(?:\S{0,24}?eval ? \n?)((?:(?!\`\`\`)(?<code>[\S\s]+))|(?:(?:\`\`\`cs|csharp\n)(?<code>[\S\s]+)\n?\`\`\`$))", "$1", RegexOptions.Compiled | RegexOptions.ECMAScript | RegexOptions.Multiline);
        
        var messageResult = await _channels.CreateMessageAsync(_context.GetChannelID(), embeds: new[] {_evaluatingEmbed});
        
        if (!messageResult.IsDefined(out IMessage? msg))
            return Result.FromError(messageResult.Error!);
        
        try
        {
            var globals = new EvalVariables
            {
                UserID         = _context.GetUserID(),
                GuildID        = _context.GuildID.IsDefined(out var guildID) ? guildID : default,
                ChannelID      = _context.GetChannelID(),
                MessageID      = _context.GetMessageID(),
                ReplyMessageID = _context.Message.ReferencedMessage.IsDefined(out var reply) ? reply.ID : default,
                
                Services = _services,
                
                Users           = _users,
                Guilds          = _guilds,
                Channels        = _channels,
                ScheduledEvents = _events,
            };

            var sopts = ScriptOptions.Default;
            sopts = sopts.AddImports(_usings.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
            IEnumerable<Assembly> asm = AppDomain.CurrentDomain
                                                 .GetAssemblies()
                                                 .Where(xa => 
                                                            !xa.IsDynamic &&
                                                            !string.IsNullOrWhiteSpace(xa.Location));
            
            sopts = sopts.WithReferences(asm);

            Script<object> script = CSharpScript.Create(cs, sopts, typeof(EvalVariables));

            ScriptState<object> evalResult = await script.RunAsync(globals);

            if (string.IsNullOrEmpty(evalResult.ReturnValue?.ToString()))
            {
                await _channels.EditMessageAsync(_context.GetChannelID(), msg.ID, "The evaluation returned null or void.", embeds: Array.Empty<IEmbed>());
                return Result.FromSuccess();
            }
            
            if (evalResult.ReturnValue is IEmbed embed)
            {
                var edit = await _channels.EditMessageAsync(_context.GetChannelID(), msg.ID, embeds: new[] { embed });

                if (!edit.IsSuccess)
                {
                    await _channels.EditMessageAsync(_context.GetChannelID(), msg.ID, "Failed to edit message.\n" + edit.Error);
                    return Result.FromError(edit.Error);
                }
            }

            var returnResult = GetHumanFriendlyResultString(evalResult.ReturnValue);
            
            var returnEmbed = new Embed
            {
                Title = "Evaluation Result",
                Description = returnResult ?? "Something went horribly wrong help",
                Colour = Color.MidnightBlue
            };
            
            await _channels.EditMessageAsync(_context.GetChannelID(), msg.ID, embeds: new[] { returnEmbed });
        }
        catch (Exception ex)
        {
            var exEmbed = new Embed
            {
                Title       = "Eval Error!",
                Description = $"**{ex.GetType()}**: {ex.Message.Split('\n')[0]}",
                Colour      = Color.Firebrick
            };
            
            await _channels.EditMessageAsync(_context.GetChannelID(), msg.ID, embeds: new[] { exEmbed });
        }
        
        return Result.FromSuccess();
    }

    private string? GetHumanFriendlyResultString(object? result)
    {
        if (result is null)
            return "null";
        
        var type = result.GetType();

        string? returnResult = result.ToString();

        if (type.IsGenericType && type.GetGenericTypeDefinition() is { } gt && (gt.IsAssignableTo(typeof(IEnumerable<>)) || gt.IsAssignableTo(typeof(IList))))
        {
            returnResult = "{ " + typeof(CollectionExtensions)
                          .GetMethod("Join", BindingFlags.Static | BindingFlags.Public)!
                          .MakeGenericMethod(type.GetGenericArguments()[0])
                          .Invoke(null, new [] { result, ", " }) + " }";
        }
        else if (type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableTo(typeof(Result<>)))
        {
            var error   = type.GetProperty(nameof(Result.Error),          BindingFlags.Public | BindingFlags.Instance)!.GetValue(result)!;
            var success = type.GetProperty(nameof(Result.IsSuccess),      BindingFlags.Public | BindingFlags.Instance)!.GetValue(result)!;
            var entity  = type.GetProperty(nameof(Result<object>.Entity), BindingFlags.Public | BindingFlags.Instance)!.GetValue(result)!;
            
            returnResult = $"Result<{type.GenericTypeArguments[0].Name}>:\n" +
                           $"\u200b\tIsSuccess: {success}\n" +
                           $"\u200b\tEntity: {GetHumanFriendlyResultString(entity)}\n" + // Just in case the entity itself is a result or a collection
                           $"\u200b\tError: {error}";
        }
        else if (type.IsAssignableTo(typeof(IList)))
        {
            returnResult = "{ " + typeof(CollectionExtensions)
                                 .GetMethod("Join", BindingFlags.Static | BindingFlags.Public)!
                                 .MakeGenericMethod(type.GetElementType()!)
                                 .Invoke(null, new [] {result, ", " })! + " }";
        }
        else if (type.IsAssignableTo(typeof(IResult)))
        {
            var res = (IResult)result;
            
            returnResult = $"Result ({type.Name}):\n" +
                           $"\tIsSuccess: {res.IsSuccess}\n" +
                           $"\tError: {res.Error}";
        }
        
        return returnResult;
    }

    public record EvalVariables
    {
        public Snowflake UserID         { get; init; }
        public Snowflake GuildID        { get; init; }
        public Snowflake ChannelID      { get; init; }
        public Snowflake MessageID      { get; init; }
        public Snowflake ReplyMessageID { get; init; }
        
        public IServiceProvider Services { get; init; }
        
        public IDiscordRestUserAPI                Users           { get; init; }
        public IDiscordRestGuildAPI               Guilds          { get; init; }
        public IDiscordRestChannelAPI             Channels        { get; init; }
        public IDiscordRestGuildScheduledEventAPI ScheduledEvents { get; init; }
    }
}