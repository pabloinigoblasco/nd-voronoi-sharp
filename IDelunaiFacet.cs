using System;
namespace ndvoronoisharp
{
    public interface IDelunaiFacet
    {
        bool IsBoundingFacet { get; }
        ISimplice ParentA { get; }
        ISimplice ParentB { get; }
        INuclei[] Vertexes { get; }
    }
}
