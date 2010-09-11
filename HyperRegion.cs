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
using DotNumerics;
using DotNumerics.LinearAlgebra;
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
        public IEnumerable<VoronoiVertex> VoronoiVertexes { get { return voronoiVertexes; } }
        public IEnumerable<HyperRegion> NeighbourgRegions { get { return lazyConstraintsMap.Values; } }
        public bool IsBoundingRegion { get { return this.Nuclei.IsDelunaiBound; } }

        /*Internal properties*/

        internal int ProblemDimensionality { get { return Nuclei.coordinates.Length; } }
        internal List<VoronoiVertex> voronoiVertexes;

        internal IEnumerable<HyperPlaneConstraint> Constraints { get { return lazyConstraintsMap.Keys; } }
        internal Dictionary<HyperPlaneConstraint, HyperRegion> lazyConstraintsMap
        {
            get
            {
                if (constraints == null)
                {
                    throw new NotImplementedException("chek if this code is ok.");
                    //build the new bound between the matching region centre and the new point
                   /* DefaultConstraint newBoundConstraint = new DefaultConstraint(newRegion.Nuclei.coordinates, matchingRegion.Nuclei.coordinates);
                    newRegion.constraints.Add(newBoundConstraint, matchingRegion);

                    //adding the inverse constraint to the maching region.
                    matchingRegion.constraints.Add(new InverseConstraintDecorator(newBoundConstraint), newRegion);*/

                    //Distinct, maybe they are repeated
                }
                return constraints;
            }
        }
        private Dictionary<HyperPlaneConstraint, HyperRegion> constraints;
		
		
		
		/// <summary>
		/// constructor visibility is restricted to assert dimensionality coherence 
		/// </summary>
		internal HyperRegion (double[] center)
		{
            this.Nuclei = new Nuclei(center, this);
		}
		
		
		private void Permutations(int permutationSize, IEnumerable<HyperPlaneConstraint> ConstraintBag, HyperPlaneConstraint[]CurrentVertexConstraints,List<HyperPlaneConstraint[]> constraintsPermutations)
		{
			if(permutationSize==0)
			{
				constraintsPermutations.Add(CurrentVertexConstraints);
				return;
			}
			else
			{
				//first time
				if(CurrentVertexConstraints==null)
					//to get a vertex we need n constraints where n==dimensionality of the problem
					CurrentVertexConstraints=new HyperPlaneConstraint[ProblemDimensionality];
				   
				foreach (var constr in ConstraintBag)
				{
					CurrentVertexConstraints[permutationSize-1]=constr;
					Permutations(permutationSize-1,ConstraintBag.Except(Enumerable.Repeat(constr,1)),CurrentVertexConstraints,constraintsPermutations);
				}
				
			}
		}
	}
}
