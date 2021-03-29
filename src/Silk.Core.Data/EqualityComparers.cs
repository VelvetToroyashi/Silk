using System.Collections.Generic;
using Silk.Core.Data.Models;

namespace Silk.Core.Data
{
    internal sealed class EqualityComparers
    {
        internal class SelfAssignableRoleComparer : IEqualityComparer<SelfAssignableRole>
        {
            public bool Equals(SelfAssignableRole? x, SelfAssignableRole? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Id == y.Id;
            }
            public int GetHashCode(SelfAssignableRole obj)
            {
                return obj.Id.GetHashCode();
            }
        }
    }
}