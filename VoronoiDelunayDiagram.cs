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
using System.Diagnostics;

namespace ndvoronoisharp
{
	public class VoronoiDelunayDiagram
	{
        public readonly int dimensions;
		public IEnumerable<HyperRegion> VoronoiRegions 
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
            this.regions.Add(newRegion);

            //the very first one region
            if (matchingRegion == null)
            {
                newRegion.Nuclei.IsDelunaiBound = true;
            }

            //the second region
            else if (matchingRegion != null && regions.Count == 2)
            {
                newRegion.Nuclei.IsDelunaiBound = true;
                matchingRegion.Nuclei.nucleiNeigbourgs.Add(newRegion.Nuclei);
                newRegion.Nuclei.nucleiNeigbourgs.Add(matchingRegion.Nuclei);
                
            }
            
            //three or more regions
            else
            {
                //Get all the simplices of this region and neighbourgs
                //they are candidates to contains this new point
                //after this checks and select the simplicitis of these regions
                //that contains the new point
                //this simplicitis are selected, and latter will be removed since
                //their tesellation will be refactored
                var affectedSimplicies = matchingRegion.NeighbourgRegions
                                        .Union(new HyperRegion[] { matchingRegion })
                                                .Select(r => r.Nuclei.simplices as IEnumerable<Simplice>)
                                                .Aggregate((acc, simps) => acc.Union(simps))
                                                .Distinct()
                                                .Where(s => s.CheckIsInsideHyperSphere(newPoint));
                //standard case
                if (affectedSimplicies.Any())
                {

                    //we have to regenerate a chunk of the delunai map. All the points to remake belongs
                    //to a simplice that match in his hyperSphere the new point.
                    IEnumerable<Nuclei> PointsToRemake = affectedSimplicies.Select(s => s.Nucleis as IEnumerable<Nuclei>)
                                                         .Aggregate((acc, nucs) => acc.Union(nucs))
                                                         .Distinct()
                                                         .Union(new Nuclei[] { newRegion.Nuclei });


                    List<Simplice> candidateSimplices = new List<Simplice>();
                    generateCombinatorySimplicies(dimensions + 1, PointsToRemake, null, candidateSimplices);
                    candidateSimplices.RemoveAll(s => Nuclei.AssertRank(s.Nucleis, dimensions));

                    if (!candidateSimplices.Any())
                        BuildTesellationAndSetNeiborghood(candidateSimplices);
                    else
                        throw new NotImplementedException("Is this case possible?");
                }
                //strage case
                else
                {
                    //This path must check aligned and neigbour points.
                    newRegion.Nuclei.IsDelunaiBound = true;

                    //this selection is distinct. I don't just search in neiborghood but in all unconnected points
                    IEnumerable<Nuclei> notSimpliceNuclei = this.Nucleis.Where(n => !n.simplices.Any())
                                                                .Union(new Nuclei[] { newRegion.Nuclei });
                    Debug.Print("Isolated point {0}. Nº total IsolatedPoints:{1}", newRegion, notSimpliceNuclei.Count());

                    bool achievedTesellation = false;
                    if (notSimpliceNuclei.Count() > dimensions)
                    {
                        List<Simplice> candidateSimplices = new List<Simplice>();
                        generateCombinatorySimplicies(dimensions + 1, notSimpliceNuclei, null, candidateSimplices);
                        candidateSimplices.RemoveAll(s => Nuclei.AssertRank(s.Nucleis, dimensions));
                        if (candidateSimplices.Any())
                        {
                            BuildTesellationAndSetNeiborghood(candidateSimplices);
                            achievedTesellation = false;
                        }
                    }
                    
                    if(!achievedTesellation)
                    {
                        //this case is only usefull for firsts points when no simplicie exists
                        //and all points can't build a simplicie.

                        matchingRegion.Nuclei.nucleiNeigbourgs.Add(newRegion.Nuclei);
                        newRegion.Nuclei.nucleiNeigbourgs.Add(matchingRegion.Nuclei);
                        //there are points aligned. Mark As Neigbourg all oldRegion
                        //Neighbourg that verifies
                        HyperPlaneConstraint contraint = newRegion.lazyConstraintsMap[matchingRegion];
                        foreach (var mRegNeighbour in matchingRegion.NeighbourgRegions)
                        {
#warning is this assertion correct? colinear, and what about coplanar?

                            if (mRegNeighbour!=newRegion
 
                                && !Nuclei.AssertCoLinear(matchingRegion.Nuclei,newRegion.Nuclei,mRegNeighbour.Nuclei))
                            {
                                mRegNeighbour.Nuclei.nucleiNeigbourgs.Add(newRegion.Nuclei);
                                newRegion.Nuclei.nucleiNeigbourgs.Add(mRegNeighbour.Nuclei);
                            }
                        }
                    }

                }
            }
			
			return newRegion;
		}

        private void BuildTesellationAndSetNeiborghood(List<Simplice> candidateSimplices)
        {
            List<Simplice> newTesellation = new List<Simplice>();
            IEnumerable<Nuclei> pointsToRemake = candidateSimplices.Select(s => s.Nucleis as IEnumerable<Nuclei>)
                                                                 .Aggregate((acc, nuc) => acc.Union(nuc));


            foreach (Simplice s in candidateSimplices)
            {
                bool emptyHyperSphere = false;
                foreach (var p in pointsToRemake.Except(s.Nucleis))
                {
                    if (s.CheckIsInsideHyperSphere(p.coordinates))
                    {
                        emptyHyperSphere = true;
                        break;
                    }
                }

                if (!emptyHyperSphere)
                {
                    newTesellation.Add(s);
                    //refactor bounds
                    //aquellos nuclieis con alguna cara no compartida
                    //es frontera.
                }
            }
            //who are bounds? those that have less than dimensionality simplicities
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
					matchAllConstraints = true;
					foreach (var constraintInfo in r.lazyConstraintsMap) {
						var constraint = constraintInfo.Value;
						var foreingRegion = constraintInfo.Key;

                        if (!constraint.semiHyperSpaceMatch(point))
                        {
                            r = foreingRegion;
                            matchAllConstraints = false;
                            break;
                        }
					}
				}
				
				return r;
			}
		}
	}
}
