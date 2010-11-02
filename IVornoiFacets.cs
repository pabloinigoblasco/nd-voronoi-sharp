using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ndvoronoisharp
{
    /// <summary>
    /// This interface represent a semi-hyperSpace whose bounds are defined by a hyperplane. The normal
    /// vector of the hyperplane defines the owned semi-hyperSpace. 
    /// It is used to define bounds between voronoi regions.
    /// </summary>
    public interface IVoronoiFacet
    {
        /// <summary>
        /// Checks if the point belong to the owner hyperplane
        /// </summary>
        bool semiHyperSpaceMatch(double[] point);

        /// <summary>
        /// coefficents
        /// </summary>
        /// <param name="coefficentIndex"></param>
        /// <returns></returns>
        double this[int coefficentIndex]
        {
            get;
        }

        int EuclideanSpaceDimensionality { get; }
        INuclei Owner { get; }
        INuclei External { get; }
    }
}
