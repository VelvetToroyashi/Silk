using Prometheus;

namespace Silk.Utilities;

public static class SilkMetric
{
    public static readonly Gauge LoadedPhishingLinks = Metrics.CreateGauge("loaded_phishing_links", "Number of loaded phishing links");
    
    public static readonly Counter SeenPhishingLinks = Metrics.CreateCounter("seen_phishing_links", "Number of seen phishing links", "domain");
    
    public static readonly Counter AutoPhishingBan = Metrics.CreateCounter("auto_phishing_ban", "Number of auto phishing bans", "reason");
    
    public static readonly Gauge LoadedReminders = Metrics.CreateGauge("loaded_reminders", "Number of loaded reminders");
    
    public static readonly Gauge LoadedInfractions = Metrics.CreateGauge("loaded_infractions", "Number of loaded infractions");
    
    public static readonly Gauge InfractionDispatchTime = Metrics.CreateGauge("infraction_dispatch_time", "Time spent dispatching infractions", "type");
    
    public static readonly Gauge ReminderDispatchTime = Metrics.CreateGauge("reminder_dispatch_time", "Time spent dispatching reminders");
    
    public static readonly Counter SeenCommands = Metrics.CreateCounter("seen_commands", "Number of seen commands", "type");
    
    public static readonly Gauge EvaluationExemptionTime = Metrics.CreateGauge("evaluation_exemption_time", "Time spent evaluating exemption", "type");
    
    public static readonly Counter GatewayEventReceieved = Metrics.CreateCounter("gateway_event_received", "Number of gateway events received", "type");
    
}