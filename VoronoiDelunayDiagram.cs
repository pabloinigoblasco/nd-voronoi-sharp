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
	public class VoronoiDelunayDiagram
	{
        public readonly int dimensions;
		public IEnumerable<HyperRegion> Regions 
        {
			get { return regions; }
		}

        public IEnumerable<Nuclei> Nucleis
        {
            get { return regions.Select(r => r.Nuclei); }
        }
        
        public IEnumerable<SimpliceCentroid> VoronoiVertexes
        {
            get { return regions.Select(r=>r.VoronoiVertexes)
                         .Aggregate((acc,vs)=>acc.Union(vs))
                         .Distinct();}
        }

		private List<HyperRegion> regions;
		public VoronoiDelunayDiagram (int dimensions)
		{
			this.dimensions = dimensions;
			regions = new List<HyperRegion> ();
		}

        public HyperRegion AddNewPointCoordinates(params double[] coordinates)
        {
            return AddNewPoint((double[])coordinates);
        }

		/// <summary>
		/// Adds a new point to the diagram and returns the generated region.
		/// </summary>
		/// <param name="newPoint">
		/// point coordinates. Dimensions must match.
		/// A <see cref="System.Double[]"/>
		/// </param>
		/// <returns>
		/// generated region that represent the set of pooints that has newPoint as the nearest neigbourgh.
		/// A <see cref="Region"/>
		/// </returns>
		public HyperRegion AddNewPoint (double[] newPoint)
		{
			if (newPoint == null || newPoint.Length != dimensions)
				throw new ArgumentException ("point added null or has invalid dimensionality");
			
			HyperRegion matchingRegion = this.GetMatchingRegion (newPoint);
            HyperRegion newRegion = new HyperRegion(newPoint);

            if (matchingRegion == null)
            {
                //this is the very first one vertex
                //just addit to the bounding

                newRegion.Nuclei.IsDelunaiBound = true;
                this.regions.Add(newRegion);
            }
            else
            {
                //Get all the simplices of this region and neighbourgs
                //they are candidates to contains this new point
                //after this checks and select the simplicitis of these regions
                //that contains the new point
                //this simplicitis are selected, and latter will be removed since
                //their tesellation will be refactored
                var matchingSimplicies = matchingRegion.NeighbourgRegions
                                        .Union(new HyperRegion[] { matchingRegion })
                                                .Select(r => r.Nuclei.simplices as IEnumerable<Simplice>)
                                                .Aggregate((acc, simps) => acc.Union(simps))
                                                .Distinct()
                                                .Where(s=>s.MatchInsideHyperSphere(newPoint));


                //we have to regenerate a chunk of the delunai map. All the points to remake belongs
                //to a simplice that match in his hyperSphere the new point.
                var PointsToRemake = matchingSimplicies.Select(s => s.Nucleis as IEnumerable<Nuclei>)
                                                     .Aggregate((acc, nucs) => acc.Union(nucs))
                                                     .Distinct()
                                                     .Union(new Nuclei[]{newRegion.Nuclei});

                
                List<Simplice> candidateSimplices = new List<Simplice>();
                generateCombinatorySimplicies(dimensions + 1, PointsToRemake, null, candidateSimplices);

                List<Simplice> newTesellation = new List<Simplice>();
                foreach (Simplice s in candidateSimplices)
                {
                    bool containsAnyPoint=false;
                    foreach (var p in PointsToRemake.Except(s.Nucleis))
                    {
                        if (s.MatchInsideHyperSphere(p.coordinates))
                        {
                            containsAnyPoint = true;
                            break;
                        }
                    }

                    if (!containsAnyPoint)
                        newTesellation.Add(s);
                }

                //TODO: Apply the new Teselation in the data Structures.



            }
			
			return newRegion;
		}

        private void generateCombinatorySimplicies(int permutationSize, IEnumerable<Nuclei> AllNucleiBag, Nuclei[] CurrentNucleiCombination, List<Simplice> resultingSimplices)
        {
            if (permutationSize == 0)
            {
                Simplice alreadyExistingSimplice=AllNucleiBag.Select(nc=>nc.simplices as IEnumerable<Simplice>)
                                                .Aggregate((acc,simps)=>acc.Union(simps))
                                                .Distinct()
                                                .FirstOrDefault(s=>s.Nucleis.All(n=>CurrentNucleiCombination.Contains(n)));
                if(alreadyExistingSimplice!=null)
                    resultingSimplices.Add(alreadyExistingSimplice);
                else
                    resultingSimplices.Add(new Simplice(CurrentNucleiCombination));
                
            }
            else
            {
                //first time
                if (CurrentNucleiCombination == null)
                    //to get a vertex we need n constraints where n==dimensionality of the problem
                    CurrentNucleiCombination = new Nuclei[this.dimensions];

                foreach (var currNuclei in AllNucleiBag)
                {
                    CurrentNucleiCombination[permutationSize - 1] = currNuclei;
                    generateCombinatorySimplicies(permutationSize - 1, AllNucleiBag.Except(Enumerable.Repeat(currNuclei, 1)), CurrentNucleiCombination, resultingSimplices);
                }

            }
        }


		/// <summary>
		/// Look up the region that match point. It uses the Gradient Descendent Method.
		/// </summary>
		/// <param name="point">
		/// point that will be checked
		/// A <see cref="System.Double[]"/>
		/// </param>
		/// <returns>
		/// Region that contains point.
		/// A <see cref="Region"/>
		/// </returns>
		public HyperRegion GetMatchingRegion (double[] point)
		{
			if (point == null || point.Length != dimensions) {
				throw new ArgumentException ("point added null or has invalid dimensionality");
			}
			
			/*This will be a very first approach as a not very efficent algorithm */
			if (!regions.Any ())
			{
				return null; 
			}
			else if (regions.Count () == 1)
			{
				return regions.Single ();
			}
			else {
				/*candidate region */				
				HyperRegion r = regions.First ();
				
				bool matchAllConstraints = false;
				while (!matchAllConstraints) {
					matchAllConstraints = false;
					foreach (var constraintInfo in r.lazyConstraintsMap) {
						var constraint = constraintInfo.Value;
						var foreingRegion = constraintInfo.Key;
						
						if (!constraint.semiHyperSpaceMatch (point))
							r = foreingRegion;
					}
                    //this is the region.
					matchAllConstraints = true;
				}
				
				return r;
			}
		}
	}
}
