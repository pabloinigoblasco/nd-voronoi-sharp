using System;
using System.Collections.Generic;
namespace ndvoronoisharp
{
    public interface ISimplice
    {
        IDelunaiFacet[] Facets { get; }
        INuclei[] Nucleis { get; }
        IEnumerable<ISimplice> NeighbourSimplices { get; }
        IVoronoiVertex VoronoiVertex { get; }
        double Radious { get; }
    }
}
