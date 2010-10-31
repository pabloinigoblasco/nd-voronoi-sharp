/*  Copyright (C) 2010  Pablo Iñigo Blasco. 
    Computer Architecture and Technology Department.
    Universidad de Sevilla. 

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.*/

using System;
using System.Collections.Generic;
using System.Linq;
using ndvoronoisharp.CustomImp;
using ndvoronoisharp.Common;
using ndvoronoisharp.Common.implementations;

namespace ndvoronoisharp.CustomImp
{
	/// <summary>
	/// This class represent a n-dimensional voronoi region
	/// </summary>
	public class HyperRegion : ndvoronoisharp.IVoronoiRegion
	{
        /// <summary>
        /// constructor visibility is restricted to assert dimensionality coherence 
        /// </summary>
        internal HyperRegion(double[] center, object data)
        {
            this.Nuclei = new Nuclei(center, this, data);
            neighboursConstraintMap = new Dictionary<IVoronoiRegion, IVoronoiFacet>();
        }

		/*Public properties*/
		public INuclei Nuclei{get; private set;}
        public IEnumerable<IVoronoiVertex> Vertexes { get { return Nuclei.Simplices.Select(s => s.VoronoiVertex); } }
        public IEnumerable<IVoronoiRegion> NeighbourgRegions { get { return Nuclei.Neighbourgs.Select(n => n.VoronoiHyperRegion); } }
        public bool IsInfiniteRegion { get { return this.Nuclei.BelongConvexHull; } }

        /*Internal properties*/
        internal int ProblemDimensionality { get { return Nuclei.Coordinates.Length; } }

        internal Dictionary<IVoronoiRegion, IVoronoiFacet> lazyConstraintsMap
        {
            get
            {
                var oldAndDeletedNeighbourgs = neighboursConstraintMap.Keys.Except(NeighbourgRegions).ToArray();
                var newNeighbourgs=NeighbourgRegions.Except(neighboursConstraintMap.Keys).ToArray();

                foreach (var toDelete in oldAndDeletedNeighbourgs)
                    neighboursConstraintMap.Remove(toDelete);

                foreach(var newNeighbour in newNeighbourgs)
                {
                    DefaultVoronoiFacet constraint=new DefaultVoronoiFacet(this.Nuclei,newNeighbour.Nuclei);
                    neighboursConstraintMap.Add(newNeighbour, constraint);
                    ((HyperRegion)newNeighbour).neighboursConstraintMap.Add(this, new InverseDefaultVoronoiFacet(constraint));
                 }

                return neighboursConstraintMap;
            }
        }
        private Dictionary<IVoronoiRegion,IVoronoiFacet> neighboursConstraintMap;

        /// <summary>
        /// This function checks if a point in the n-dimensional space is contained in this
        /// Voronoi-HypeRegion
        /// </summary>
        public bool ContainsPoint(double[] point)
        {
            if (point.Length != this.ProblemDimensionality)
                throw new ArgumentException("Invalid dimensionality of the point");

            foreach (var constraintInfo in lazyConstraintsMap)
            {
                var constraint = constraintInfo.Value;
                var foreingRegion = constraintInfo.Key;

                if (!constraint.semiHyperSpaceMatch(point))
                    return false;
            }
            return true;
        }

        public IEnumerable<IVoronoiFacet> Facets
        {
            get { return neighboursConstraintMap.Values.Distinct(); }
        }

    }
}
