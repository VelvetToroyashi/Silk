using DSharpPlus.SlashCommands;

namespace Silk.Types;

public enum ReminderTypeOption
{
    [ChoiceName("Once")]
    Once,
    [ChoiceName("Hourly")]
    Hourly,
    [ChoiceName("Daily")]
    Daily,
    [ChoiceName("Weekly")]
    Weekly,
    [ChoiceName("Monthly")]
    Monthly
}