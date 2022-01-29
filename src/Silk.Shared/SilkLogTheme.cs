using System;
using System.Collections.Generic;
using System.IO;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog.Templates.Themes;

namespace Silk.Shared;

public sealed class SilkLogTemplateTheme : TemplateTheme
{

    public SilkLogTemplateTheme(IReadOnlyDictionary<TemplateThemeStyle, string> ansiStyles) : base(ansiStyles) { }
    public SilkLogTemplateTheme(TemplateTheme                                   baseTheme, IReadOnlyDictionary<TemplateThemeStyle, string> ansiStyles) : base(baseTheme, ansiStyles) { }
}

public sealed class SilkLogTheme : ConsoleTheme
{
    public static TemplateTheme TemplateTheme = new(TemplateTheme.Code, new Dictionary<TemplateThemeStyle, string>
    {
        [TemplateThemeStyle.Number]           = "\x1b[38;5;39m",
        [TemplateThemeStyle.LevelDebug]       = "\x1b[38;5;34m",
        [TemplateThemeStyle.LevelError]       = "\x1b[38;5;160m",
        [TemplateThemeStyle.LevelFatal]       = "\x1b[38;5;52m",
        [TemplateThemeStyle.LevelVerbose]     = "\x1b[38;5;164m",
        [TemplateThemeStyle.LevelWarning]     = "\x1b[38;5;220m",
        [TemplateThemeStyle.SecondaryText]    = "\x1b[38;5;39m",
        [TemplateThemeStyle.LevelInformation] = "\x1b[38;5;255m",
        [TemplateThemeStyle.Scalar]           = "\x1b[38;5;39m",
        [TemplateThemeStyle.TertiaryText]     = "\x1b[38;5;39m",
        [TemplateThemeStyle.Text]             = "\x1b[38;5;220m"
    });

    public override bool CanBuffer => false;

    protected override int ResetCharCount => 0;

    public override void Reset(TextWriter output)
    {
        Console.ResetColor();
    }

    public override int Set(TextWriter output, ConsoleThemeStyle style)
    {
        (ConsoleColor foreground, ConsoleColor background) = style switch
        {
            ConsoleThemeStyle.Scalar           => (ConsoleColor.Blue, ConsoleColor.Black), // I don't actually know what this does. //
            ConsoleThemeStyle.Number           => (ConsoleColor.DarkBlue, ConsoleColor.Black),
            ConsoleThemeStyle.LevelDebug       => (ConsoleColor.Green, ConsoleColor.Black),
            ConsoleThemeStyle.LevelError       => (ConsoleColor.Red, ConsoleColor.Black),
            ConsoleThemeStyle.LevelFatal       => (ConsoleColor.Red, ConsoleColor.Black),
            ConsoleThemeStyle.LevelVerbose     => (ConsoleColor.Magenta, ConsoleColor.Black),
            ConsoleThemeStyle.LevelWarning     => (ConsoleColor.Yellow, ConsoleColor.Black),
            ConsoleThemeStyle.SecondaryText    => (ConsoleColor.DarkBlue, ConsoleColor.Black),
            ConsoleThemeStyle.LevelInformation => (ConsoleColor.White, ConsoleColor.Black),
            _                                  => (ConsoleColor.Yellow, ConsoleColor.Black)
        };
        Console.ForegroundColor = foreground;
        Console.BackgroundColor = background;
        return 0;
    }
}