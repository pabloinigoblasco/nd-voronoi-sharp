using System;
using System.Collections.Generic;
namespace ndvoronoisharp
{
    public interface IVoronoiRegion
    {
        bool ContainsPoint(double[] point);
        IEnumerable<IVoronoiRegion> NeighbourgRegions { get; }
        INuclei Nuclei { get; }
        IEnumerable<IVoronoiVertex> VoronoiVertexes { get; }
    }
}
