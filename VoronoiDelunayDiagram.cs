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
    public class VoronoiDelunayGraph
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
            get { return Simplices.Select(simp => simp.VoronoiVertex); }
        }

        public IEnumerable<Simplice> Simplices
        {
            get
            {
                if (Nucleis.Any())
                    return Nucleis.Select(n => n.simplices as IEnumerable<Simplice>)
                                  .Aggregate((acc, simps) => acc.Union(simps))
                                  .Distinct();
                else
                    return Enumerable.Empty<Simplice>();
            }
        }

        private List<HyperRegion> regions;
        public VoronoiDelunayGraph(int dimensions)
        {
            this.dimensions = dimensions;
            regions = new List<HyperRegion>();
        }



        public HyperRegion AddNewPoint(double[] newPoint)
        {
            return AddNewPoint(null, newPoint);
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
        public HyperRegion AddNewPoint(object data, double[] newPoint)
        {
            if (newPoint == null || newPoint.Length != dimensions)
                throw new ArgumentException("point added null or has invalid dimensionality");

            HyperRegion containerRegion = this.GetMatchingRegion(newPoint);
            HyperRegion newRegion = new HyperRegion(newPoint, data);
            this.regions.Add(newRegion);

            //the very first one region
            if (containerRegion == null)
            {
                newRegion.Nuclei.ConvexBoundary = true;
            }
            //the standard case
            else
            {
                //Get all the simplices of this region and neighbourgs
                //they are candidates to contains this new point
                //after this checks and select the simplicitis of these regions
                //that contains the new point
                //this simplicitis are selected, and latter will be removed since
                //their tesellation will be refactored
                List<Simplice> affectedSimplicies = containerRegion.NeighbourgRegions
                                        .Union(new HyperRegion[] { containerRegion })
                                                .Select(r => r.Nuclei.simplices as IEnumerable<Simplice>)
                                                .Aggregate((acc, simps) => acc.Union(simps))
                                                .Distinct()
                                                .Where(s => s.CheckIsInsideHyperSphere(newPoint))
                                                .ToList();

              

                //standard case
                if (affectedSimplicies.Any())
                {

                    //we have to regenerate a chunk of the delunai map. All the points to remake belongs
                    //to a simplice that match in his hyperSphere the new point.
                    Nuclei[] PointsToRemake = affectedSimplicies.Select(s => s.Nucleis as IEnumerable<Nuclei>)
                                                         .Aggregate((acc, nucs) => acc.Union(nucs))
                                                         .Union(new Nuclei[] { newRegion.Nuclei })
                                                         .Distinct()
                                                         .ToArray();

                    if (!TryBuildTesellation(PointsToRemake, affectedSimplicies))
                    {
                        //theoretically if it's inside of a hypersphere, at least a set of points are rank==dimensions
                        //so this never should happen
                        throw new NotImplementedException("Unexpected Derivation");
                    }
                }
                //THIRD NOT USUAL CASE:
                //
                //It should be    1- an external point of the delunai convex tesellation.
                //                2- a new point in the beginnings and the number of points are not enough to build
                //                   a simplice
                //                      2.1 Enough point to build the very first simplice but they have not enough rank
                //                     rank(existingpoints)<dims
                //                      2.2 Otherwise everyvody is neighbour and bound

                else
                {
                    //CASE 1 - External point
                    //Then try to build a tesellation with bruteForce. This point, is owner region and all his the neighbourg.
                    Nuclei[] PointsToRemake = containerRegion.NeighbourgRegions
                                        .Union(new HyperRegion[] { containerRegion,newRegion })
                                        .Select(r => r.Nuclei)
                                        .ToArray();

                    //select all the simplices related with all nucleis of the current hyperregion
                    affectedSimplicies = PointsToRemake
                                                .Select(n => n.simplices as IEnumerable<Simplice>)
                                                .Aggregate((acc, simps) => acc.Union(simps))
                                                .Distinct()
                                                .ToList();
                     

                    bool achievedTesellation = TryBuildTesellation(PointsToRemake, affectedSimplicies);
                    if (achievedTesellation)
                        Debug.Print("CASE STRANGE 1");

                    //THEN CASE 2 - Beginigs and noth enough to build a simplice
                    else
                    {
                        Debug.Print("CASE STRANGE 2");
                        //this case is only usefull for firsts points when no simplicie exists
                        //and all points can't build a simplicie

                        Debug.Print("We don't like this region. Maybe super-computer requirements?");

                        foreach (var n in containerRegion.NeighbourgRegions.Select(r=>r.Nuclei))
                        {
                            if (!Nuclei.AssertCoLinear(new Nuclei[] { n, newRegion.Nuclei, containerRegion.Nuclei }))
                            {
                                newRegion.Nuclei.nucleiNeigbourgs.Add(n);
                                n.nucleiNeigbourgs.Add(newRegion.Nuclei);
                            }
                        }

                        //of course the new point is neigbhour of the region where it fell
                        containerRegion.Nuclei.nucleiNeigbourgs.Add(newRegion.Nuclei);
                        newRegion.Nuclei.nucleiNeigbourgs.Add(containerRegion.Nuclei);
                        newRegion.Nuclei.ConvexBoundary = true;
                    }

                }
            }
            return newRegion;
        }

        /// <summary>
        /// This function checks all requirements to build a tessellation, basically the number of independent nodes.
        /// If they are enough this method call to the buildTesellation method.
        /// </summary>        
        private bool TryBuildTesellation(Nuclei[] PointsToRemake, List<Simplice> oldSimplices)
        {
            //check enough points
            if (PointsToRemake.Length >= dimensions + 1)
            {
                //Candidate simplices will contain some old simplices
                //and some new maked up simplices
                List<Simplice> candidateSimplices = new List<Simplice>();

                //the only thing we need is a combinatory function about the exploited points
                generateCombinatorySimplicies(0,0, dimensions + 1, PointsToRemake, null, oldSimplices, candidateSimplices);
                candidateSimplices.RemoveAll(s => !Nuclei.AssertRank(s.Nucleis, dimensions));

                //check enough independent points
                if (candidateSimplices.Any())
                {
                    BuildTesellationAndSetNeiborghood(candidateSimplices, PointsToRemake,oldSimplices);
                    return true;
                }
                else
                    return false; //not enough indepndent poitns to build a tessellation in this n-dimensional world
            }
            else
                return false; //not enough points to build a teselation in this n-dimensional world

        }
        private void BuildTesellationAndSetNeiborghood(List<Simplice> candidateSimplices,Nuclei[] pointsToRemake,List<Simplice> oldSimplices)
        {
            List<Simplice> newTesellation = new List<Simplice>();
            foreach (Simplice s in candidateSimplices)
            {
                bool hyperSphereCoversPoint = false;
                foreach (var p in pointsToRemake.Except(s.Nucleis))
                {
                    if (s.CheckIsInsideHyperSphere(p.coordinates))
                    {
                        hyperSphereCoversPoint = true;
                        break;
                    }
                }

                if (!hyperSphereCoversPoint)
                {
                    //if finally some new simplice existed previously
                    //they have not been exploited so don't mark them as new (recreate them)
                    //neither mark them to remove(oldSimplice) them because are stable and usefull simplices.

                    if (oldSimplices.Contains(s))
                        oldSimplices.Remove(s);
                    else
                        newTesellation.Add(s);

                }
            }

            /*var oldSimplices = newTesellation.Select(s => s.Nucleis as IEnumerable<Nuclei>)
                                            .Aggregate((acc, ns) => acc.Union(ns))
                                            .Distinct()
                                            .Select(n => n.simplices as IEnumerable<Simplice>)
                                            .Aggregate((acc, sim) => acc.Union(sim) )
                                            .Distinct()
                                            .Except(newTesellation)
                                            .ToArray();*/

           
                
            foreach (Simplice s in newTesellation)
                foreach (Nuclei n in s.Nucleis)
                {

                    //Deleting refactored nuclei neighbourg for each nuclei.
                    //Assert that we do not remove any neighbour that is connected
                    //thorugh another simplice that won't be removed
                    foreach (var os in oldSimplices.Where(x=>x.Nucleis.Contains(n)))
                    {
                        n.simplices.Remove(os);
                        //remove neighbour not contained in other neighbourgs
                        var neighToRemove = os.Nucleis
                                          .Where(nuc => nuc != n &&
                                                        !n.simplices
                                                        .Any(sim => sim.Nucleis.Contains(nuc)));

                        foreach (var neighRm in neighToRemove)
                            n.nucleiNeigbourgs.Remove(neighRm);
                    }

                    if (n.simplices.Contains(s))
                        throw new Exception();

                    n.simplices.Add(s);
                    foreach (Nuclei newNeigbourg in s.Nucleis)
                        if (newNeigbourg != n && !n.nucleiNeigbourgs.Contains(newNeigbourg))
                            n.nucleiNeigbourgs.Add(newNeigbourg);
                        
                }

            
            

        }

        private void generateCombinatorySimplicies(int auxStartIndex, int workingIndex, int combinationSize, Nuclei[] originalBag, Nuclei[] CurrentNucleiCombination, IEnumerable<Simplice> containersSimplices, List<Simplice> resultingSimplices)
        {

            //first time
            if (CurrentNucleiCombination == null)
                //to get a vertex we need n constraints where n==dimensionality of the problem
                CurrentNucleiCombination = new Nuclei[combinationSize];


            //recursive case
            if (workingIndex<CurrentNucleiCombination.Length)
            {
                //generateCombinatorySimplicies(auxStartIndex + 1, combinationSize - 1, originalBag, CurrentNucleiCombination, containersSimplices, resultingSimplices);
                for (int i = auxStartIndex; i < originalBag.Length && originalBag.Length- i >= combinationSize; i++)
                {
                    CurrentNucleiCombination[workingIndex] = originalBag[i];
                    generateCombinatorySimplicies(i+1,workingIndex+1,combinationSize - 1, originalBag, CurrentNucleiCombination, containersSimplices, resultingSimplices);
                }
            }

            //base case
            else
            {
                var alreadyExistingSimplice = containersSimplices.SingleOrDefault(s => s.Nucleis.All(n => CurrentNucleiCombination.Contains(n)));

                //trying to reuse existing simplices that will be removed
                if (alreadyExistingSimplice != null)
                {
                    resultingSimplices.Add(alreadyExistingSimplice);
                    Debug.Print("Simplice saved: {0}", alreadyExistingSimplice);
                }
                else
                    //creating a very new simplice
                    resultingSimplices.Add(new Simplice(CurrentNucleiCombination.ToArray()));
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
        public HyperRegion GetMatchingRegion(double[] point)
        {
            if (point == null || point.Length != dimensions)
            {
                throw new ArgumentException("point added null or has invalid dimensionality");
            }

            /*This will be a very first approach as a not very efficent algorithm */
            if (!regions.Any())
            {
                return null;
            }
            else if (regions.Count() == 1)
            {
                return regions.Single();
            }
            else
            {
                /*candidate region */
                HyperRegion r = regions.First();

                bool matchAllConstraints = false;
                while (!matchAllConstraints)
                {
                    matchAllConstraints = true;
                    foreach (var constraintInfo in r.lazyConstraintsMap)
                    {
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
