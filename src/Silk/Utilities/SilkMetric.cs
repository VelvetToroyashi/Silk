using Prometheus;

namespace Silk.Utilities;

public static class SilkMetric
{
    public static Gauge LoadedPhishingLinks = Metrics.CreateGauge("loaded_phishing_links", "Number of loaded phishing links");
    
    public static Counter SeenPhishingLinks = Metrics.CreateCounter("seen_phishing_links", "Number of seen phishing links");
    
    public static Counter AutoPhishingBan = Metrics.CreateCounter("auto_phishing_ban", "Number of auto phishing bans", "reason");
    
    public static Gauge LoadedReminders = Metrics.CreateGauge("loaded_reminders", "Number of loaded reminders");
    
    public static Gauge LoadedInfractions = Metrics.CreateGauge("loaded_infractions", "Number of loaded infractions");
    
    public static Summary InfractionDispatchTime = Metrics.CreateSummary("infraction_dispatch_time", "Time spent dispatching infractions", "type");
    
    public static Summary ReminderDispatchTime = Metrics.CreateSummary("reminder_dispatch_time", "Time spent dispatching reminders");
    
    public static Counter SeenCommands = Metrics.CreateCounter("seen_commands", "Number of seen commands", "type");
    
    public static Summary EvaluationExemptionTime = Metrics.CreateSummary("evaluation_exemption_time", "Time spent evaluating exemption", "type");
    
    public static Counter GatewayEventReceieved = Metrics.CreateCounter("gateway_event_received", "Number of gateway events received", "type");
    
}