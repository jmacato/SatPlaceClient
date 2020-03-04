using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SatPlaceClient.Models
{
    public class GenericColorComparer : IEqualityComparer<GenericColor>
    {
        public bool Equals([AllowNull] GenericColor x, [AllowNull] GenericColor y)
        {
            return (x.A == y.A) &
                   (x.R == y.R) &
                   (x.G == y.G) &
                   (x.B == y.B);
        }

        public int GetHashCode([DisallowNull] GenericColor obj)
        {
            return int.MaxValue ^ obj.R ^ obj.G ^ obj.B ^ obj.A;
        }
    }
}
