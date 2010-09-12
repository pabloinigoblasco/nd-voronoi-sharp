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
using ndvoronoisharp.implementations;

namespace ndvoronoisharp
{
	/// <summary>
	/// This class represent a n-dimensional voronoi region
	/// </summary>
	public class HyperRegion
	{
		/*Public properties*/

		public Nuclei Nuclei{get; private set;}
        public IEnumerable<SimpliceCentroid> VoronoiVertexes { get { return Nuclei.simplices.Select(s => s.VoronoiVertex); } }
        public IEnumerable<HyperRegion> NeighbourgRegions { get { return Nuclei.NucleiNeigbourgs.Select(n => n.VoronoiHyperRegion); } }
        public bool IsBoundingRegion { get { return this.Nuclei.IsDelunaiBound; } }

        /*Internal properties*/
        internal int ProblemDimensionality { get { return Nuclei.coordinates.Length; } }

        internal Dictionary<HyperRegion, HyperPlaneConstraint> lazyConstraintsMap
        {
            get
            {
                var oldAndDeletedNeighbourgs = neighboursConstraintMap.Keys.Except(NeighbourgRegions);
                var newNeighbourgs=NeighbourgRegions.Except(neighboursConstraintMap.Keys);

                foreach (var toDelete in oldAndDeletedNeighbourgs)
                    neighboursConstraintMap.Remove(toDelete);

                foreach(var newNeighbour in newNeighbourgs)
                {
                    HyperPlaneConstraint constraint=new DefaultConstraint(this.Nuclei.coordinates,newNeighbour.Nuclei.coordinates);
                    neighboursConstraintMap.Add(newNeighbour, constraint);
                    newNeighbour.neighboursConstraintMap.Add(this, new InverseConstraintDecorator(constraint));
                 }

                return neighboursConstraintMap;
            }
        }
        private Dictionary<HyperRegion,HyperPlaneConstraint> neighboursConstraintMap;

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
		
		
		/// <summary>
		/// constructor visibility is restricted to assert dimensionality coherence 
		/// </summary>
		internal HyperRegion (double[] center)
		{
            this.Nuclei = new Nuclei(center, this);
            neighboursConstraintMap = new Dictionary<HyperRegion, HyperPlaneConstraint>();
		}
		
		
		
	}
}
