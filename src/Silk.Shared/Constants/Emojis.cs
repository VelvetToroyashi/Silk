namespace Silk.Shared.Constants;

public static class Emojis
{
    public static ulong ConfirmId { get; set; } = 777724297627172884;
    public static string ConfirmEmoji => $"<:_:{ConfirmId}>";
    
    public static ulong DeclineId { get; set; } = 777724316115796011;
    public static string DeclineEmoji => $"<:_:{DeclineId}>";
    
    public static ulong LoadingId { get; set; } = 841020747577163838;
    public static string LoadingEmoji => $"<:_:{LoadingId}>";
    
    public static ulong OnlineId       { get; set; } = 743339430672203796;
    public static string OnlineEmoji    => $"<:_:{OnlineId}>";

    public static ulong AwayId         { get; set; } = 743339431720910889;
    public static string AwayEmoji      => $"<:_:{AwayId}>";

    public static ulong DoNotDisturbId { get; set; } = 743339431632568450;
    public static string DoNotDisturbEmoji => $"<:_:{DoNotDisturbId}>";

    public static ulong OfflineId      { get; set; } = 743339431905198100;
    public static string OfflineEmoji   => $"<:_:{OfflineId}>";
}