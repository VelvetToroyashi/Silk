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
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Extensions;
using CollectionExtensions = Silk.Extensions.CollectionExtensions;

namespace Silk.Core.Commands.Bot;

// THIS COMMAND WAS RIPPED FROM Emzi0767#1837. I ONLY MADE IT EVAL INLINE CODE  ~Velvet, as always //
[HelpCategory(Categories.Bot)]
public class EvalCommand : CommandGroup
{
    private readonly ICommandContext _context;
    
    private readonly IDiscordRestUserAPI                _users;
    private readonly IDiscordRestGuildAPI               _guilds;
    private readonly IDiscordRestChannelAPI             _channels;
    private readonly IDiscordRestGuildScheduledEventAPI _events;

    private static readonly IEmbed _evaluatingEmbed = new Embed
    {
        Title  = "Evaluating. Please wait.",
        Colour = Color.HotPink
    };
    public EvalCommand(ICommandContext context, IDiscordRestChannelAPI channels, IDiscordRestUserAPI users, IDiscordRestGuildAPI guilds, IDiscordRestGuildScheduledEventAPI events)
    {
        _context     = context;
        _channels    = channels;
        _users       = users;
        _guilds      = guilds;
        _events = events;
    }

    [Command("eval")]
    public async Task<Result> EvalCS([Greedy] string code)
    {
        var cs = Regex.Replace(code, @"\`\`\`(?:cs(?:harp)?)?\n?(?<code>[\s\S]+)\n?\`\`\`", "$1", RegexOptions.Compiled | RegexOptions.ECMAScript);
        
        var messageResult = await _channels.CreateMessageAsync(_context.ChannelID, embeds: new[] {_evaluatingEmbed});
        
        if (!messageResult.IsDefined(out IMessage? msg))
            return Result.FromError(messageResult.Error!);

        try
        {
            var context = (MessageContext)_context;
            
            var globals = new EvalVariables
            {
                UserID         = context.User.ID,
                GuildID        = context.GuildID.IsDefined(out var guildID) ? guildID : default,
                ChannelID      = context.ChannelID,
                MessageID      = context.MessageID,
                ReplyMessageID = context.Message.ReferencedMessage.IsDefined(out var reply) ? reply.ID : default,
                
                Users           = _users,
                Guilds          = _guilds,
                Channels        = _channels,
                ScheduledEvents = _events,
            };

            var sopts = ScriptOptions.Default;
            sopts = sopts.WithImports("System",
                                      "System.Collections.Generic",
                                      "System.Linq",
                                      "System.Text",
                                      "System.Threading.Tasks",
                                      "Silk.Core",
                                      "Silk.Extensions",
                                      "Microsoft.Extensions.Logging");
            IEnumerable<Assembly> asm = AppDomain.CurrentDomain
                                                 .GetAssemblies()
                                                 .Where(xa => 
                                                            !xa.IsDynamic &&
                                                            !string.IsNullOrWhiteSpace(xa.Location));
            
            sopts = sopts.WithReferences(asm);

            Script<object> script = CSharpScript.Create(cs, sopts, typeof(EvalVariables));
            script.Compile();

            ScriptState<object> evalResult = await script.RunAsync(globals);

            if (string.IsNullOrEmpty(evalResult.ReturnValue?.ToString()))
            {
                await _channels.EditMessageAsync(_context.ChannelID, msg.ID, "The evlaution returned null or void.");
                return Result.FromSuccess();
            }
            
            if (evalResult.ReturnValue is IEmbed embed)
            {
                var edit = await _channels.EditMessageAsync(_context.ChannelID, msg.ID, embeds: new[] { embed });

                if (!edit.IsSuccess)
                {
                    await _channels.EditMessageAsync(_context.ChannelID, msg.ID, "Failed to edit message.\n" + edit.Error.Message);
                }
            }

            var returnResult = GetHumanFriendlyResultString(evalResult.ReturnValue);
            
            var returnEmbed = new Embed()
            {
                Title = "Evaluation Result",
                Description = returnResult ?? "Something went horribly wrong help",
                Colour = Color.MidnightBlue
            };
            
            await _channels.EditMessageAsync(_context.ChannelID, msg.ID, embeds: new[] { returnEmbed });
        }
        catch (Exception ex)
        {
            var exEmbed = new Embed()
            {
                Title       = "Eval Error!",
                Description = $"**{ex.GetType()}**: {ex.Message.Split('\n')[0]}",
                Colour      = Color.Firebrick
            };
            
            await _channels.EditMessageAsync(_context.ChannelID, msg.ID, embeds: new[] { exEmbed });
        }
        
        return Result.FromSuccess();
    }

    private string GetHumanFriendlyResultString(object result)
    {
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
            var success = type.GetProperty(nameof(Result.IsSuccess), BindingFlags.Public      | BindingFlags.Instance)!.GetValue(result)!;
            var error   = type.GetProperty(nameof(Result.Error), BindingFlags.Public          | BindingFlags.Instance)!.GetValue(result)!;
            var entity  = type.GetProperty(nameof(Result<object>.Entity), BindingFlags.Public | BindingFlags.Instance)!.GetValue(result)!;
            
            returnResult = $"Result<{entity.GetType().Name}>:\n" +
                           $"\tIsSuccess: {success}\n" +
                           $"\tEntity: {GetHumanFriendlyResultString(entity)}\n" + // Just in case the entity itself is a result or a collection
                           $"\tError: {error}";
        }
        else if (type.IsAssignableTo(typeof(IEnumerable)))
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
        
        
        public IDiscordRestUserAPI                Users           { get; init; }
        public IDiscordRestGuildAPI               Guilds          { get; init; }
        public IDiscordRestChannelAPI             Channels        { get; init; }
        public IDiscordRestGuildScheduledEventAPI ScheduledEvents { get; init; }
    }
}