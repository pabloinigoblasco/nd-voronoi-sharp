using System;
using System.Collections.Generic;
namespace ndvoronoisharp
{
    public interface IVoronoiDelunayGraph
    {
        HyperRegion AddNewPoint(object data, double[] newPoint);
        HyperRegion AddNewPoint(double[] newPoint);
        IEnumerable<INuclei> Nucleis { get; }
        IEnumerable<ISimplice> Simplices { get; }
        IEnumerable<IVoronoiRegion> VoronoiRegions { get; }
        IEnumerable<IVoronoiVertex> VoronoiVertexes { get; }
    }
}
