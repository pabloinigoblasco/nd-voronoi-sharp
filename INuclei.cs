using System;
using System.Collections.Generic;

namespace ndvoronoisharp
{
    public interface INuclei
    {
        double[] Coordinates { get; }
        IVoronoiRegion VoronoiHyperRegion { get; }
        IEnumerable<INuclei> Neighbourgs { get; }
        IEnumerable<ISimplice> Simplices { get; }
        bool BelongConvexHull { get; }
    }
}
