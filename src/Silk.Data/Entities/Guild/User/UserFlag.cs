using System;

namespace Silk.Data.Entities;

[Flags]
[Obsolete("Silk does not use these flags.")]
public enum UserFlag
{
    None        = 0,
    WarnedPrior = 2,
    KickedPrior = 4,
    BannedPrior = 8,
}