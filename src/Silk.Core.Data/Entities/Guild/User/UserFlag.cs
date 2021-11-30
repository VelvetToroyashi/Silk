using System;

namespace Silk.Core.Data.Entities
{
    [Flags]
    public enum UserFlag
    {
        None                = 0,
        ActivelyMuted       = 2,
        ActivelyBanned      = 4,
        WarnedPrior         = 8,
        KickedPrior         = 16,
        BannedPrior         = 32,
        InfractionExemption = 64,
        [Obsolete($"This flag is no longer used. Use {nameof(InfractionExemption)} instead.")]
        Staff = 128 | InfractionExemption, //1536
        [Obsolete($"This flag is no longer used. Use {nameof(InfractionExemption)} instead.")]
        EscalatedStaff = 256 | Staff // 3584
    }
}