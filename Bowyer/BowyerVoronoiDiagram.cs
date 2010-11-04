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
using System.Linq;
using System.Collections.Generic;
using ndvoronoisharp.Common;
using System.Diagnostics;
using System.Text;

namespace ndvoronoisharp.Bowyer
{
    public class BowyerVoronoiDelunayGraph : IVoronoiDelunayGraph
    {

        internal readonly List<BowyerVoronoiVertex> voronoiVertexes;
        internal readonly List<INuclei> nucleis;

        public int ProblemDimensionality { get; private set; }
        int nucleisRank;


        public int Rank { get { return nucleisRank; } }

        public BowyerVoronoiDelunayGraph(int spaceDimensionality)
        {
            voronoiVertexes = new List<BowyerVoronoiVertex>();
            nucleis = new List<INuclei>();
            ProblemDimensionality = spaceDimensionality;
            nucleisRank = 0;
        }

        public IVoronoiRegion AddNewPoint(object data, double[] newPoint)
        {
            if (newPoint == null || newPoint.Length != ProblemDimensionality)
                throw new ArgumentException("point added null or has invalid dimensionality");

            BowyerNuclei newNuclei = new BowyerNuclei(newPoint);
            newNuclei.Data = data;

            //SPECIAL CASE FOR FIRST ADD
            if (!voronoiVertexes.Any())
            {
                //no voronoiVertexes
                //no simplices
                //no regions

                BowyerNuclei[] nucleis = new BowyerNuclei[1];
                newNuclei = nucleis[0] = new BowyerNuclei(newPoint);
                newNuclei.Data = data;

                //create a VoronoiVertex in the infinite
                var voronoiVertex = new BowyerVoronoiVertex(0, nucleis, new HyperSphereConstraint(newNuclei.Coordinates, double.PositiveInfinity));

                this.voronoiVertexes.Add(voronoiVertex);
                this.nucleis.Add(newNuclei);

                //create a not formed Simplice
                return voronoiVertexes.First().Simplice.Nucleis.First().VoronoiHyperRegion;
            }
            else
            {

                //--------------- SITUATION ANALYSIS - READ ONLY--------------

#warning optimizable: not variable list without cast, external auxiliar attribute
                List<BowyerVoronoiVertex> affectedVertexes = new List<BowyerVoronoiVertex>();
#warning optimizable: not variable list without cast, external auxiliar attribute
                List<BowyerNuclei> affectedNucleis = new List<BowyerNuclei>();


                IEnumerable<INuclei> newpointSet = new INuclei[] { newNuclei };

#warning optimizable: cant be this an auxiliar attribute?
                var secondaryAffecteVertexes = new List<BowyerSimpliceFacet>();


                if (nucleisRank < ProblemDimensionality && Helpers.CalculatePointsRank(Simplices.First().Nucleis.Union(newpointSet)) > nucleisRank)
                {
                    affectedVertexes = this.voronoiVertexes;
                    nucleisRank++;
                    foreach (var v in affectedVertexes)
                        v.IsTrash = true;

#warning optimize, eliminate the cast and create a nuclei List
                    affectedNucleis.AddRange(this.Nucleis.Cast<BowyerNuclei>());
                }
                else
                {
                    IVoronoiRegion r = GetMatchingRegion(newPoint);
                    // and use r.Vertexes
                    foreach (BowyerVoronoiVertex v in r.Vertexes)
                        if (v.Simplice.CircumsphereContains(newPoint))
                        {
                            if (!affectedVertexes.Contains(v))
                            {
                                affectedVertexes.Add(v);
                                v.IsTrash = true;
                                foreach (var affectedNuclei in v.simplice.nucleis)
                                    if (!affectedNucleis.Contains(affectedNuclei))
                                        affectedNucleis.Add(affectedNuclei);
                            }

                            foreach (var vaffected in v.simplice.facets.Where(f => f.External.Infinity).Select(f => (BowyerVoronoiVertex)f.External))
                            {
                                if (!affectedVertexes.Contains(vaffected))
                                {
                                    affectedVertexes.Add(vaffected);
                                    vaffected.IsTrash = true;
                                }

                            }

                            foreach (var otherAffectedFace in v.simplice.facets.Where(f => !f.External.Infinity))
                            {
                                if (!affectedVertexes.Contains((BowyerVoronoiVertex)otherAffectedFace.External) && !secondaryAffecteVertexes.Contains(otherAffectedFace))
                                    secondaryAffecteVertexes.Add(otherAffectedFace);
                            }


                            //add also all the infinite vertexes if it or neighbours who contains it
                            //foreach (var vneigh2 in v.simplice.facets
                            //                                      .Where(f => !affectedVertexes.Contains((BowyerVoronoiVertex)f.External)
                            //                                                  && (f.External.Infinity
                            //                                                     || f.External.Simplice.CircumsphereContains(newPoint)))
                            //                                      .Select(f => (BowyerVoronoiVertex)f.External))
                            //{
                            //    vneigh2.IsTrash = true;
                            //    affectedVertexes.Add(vneigh2);

                            //}
                        }
                }



                //if (!affectedVertexes.Any())
                //{
                //    //if no normal simplices contains the new point
                //    //we will to try it in infinite vertexs of this region
                //    foreach (BowyerVoronoiVertex v in r.Vertexes.Where(v => v.Infinity))
                //        if (v.Simplice.CircumsphereContains(newPoint))
                //        {
                //            affectedVertexes.Add(v);
                //            v.IsTrash = true;
                //            affectedNucleis.AddRange(v.simplice.nucleis);

                //            //add to affected vertexes also the only neighbour vertex of
                //            //the current infinite vertex
                //            BowyerVoronoiVertex vnormal = (BowyerVoronoiVertex)v.simplice.facets.Single().External;
                //            secondaryAffecteVertexes.Add(vnormal);

                //            //vnormal.IsTrash = true;
                //            //affectedVertexes.Add(vnormal);
                //            ////add also all the infinite vertexes if it or neighbours who contains it
                //            //foreach (var vneigh2 in vnormal.simplice.facets.Select(f => (BowyerVoronoiVertex)f.External)
                //            //                                               .Where(v2 => v2 != v && !affectedVertexes.Contains(v2)
                //            //                                                   && (v2.Infinity || v2.simplice.CircumsphereContains(newPoint))))
                //            //{
                //            //    vneigh2.IsTrash = true;
                //            //    affectedVertexes.Add(vneigh2);
                //            //}

                //      }
                //  }

                // }

#if DEBUG
                Debug.Print(string.Format("{0} ||| adding new point. Affected vertexes: {1}. Secondary Affected vertexes: {2}", this.ToString(), affectedVertexes.Count, secondaryAffecteVertexes.Count));

                if (!affectedVertexes.Any())
                    throw new ArgumentException("this case is not possible and has not been contemplated");

                if (affectedVertexes.Distinct().Count() != affectedVertexes.Count)
                    throw new ArgumentException("Incoherence in the algorithm");

                if (affectedNucleis.Distinct().Count() != affectedNucleis.Count)
                    throw new ArgumentException("Incoherence in the algorithm");

                if (secondaryAffecteVertexes.Distinct().Count() != secondaryAffecteVertexes.Count)
                    throw new ArgumentException("Incoherence in the algorithm");

#endif


                //if any candidate vertex sismplice has the maxium dimensionality, the postgenerated tesellation will also have this maxium dimensionality

                //if (affectedVertexes.First().Simplice.Dimensionality == ProblemDimensionality)
                //    nucleisRank = ProblemDimensionality;
                //else
                //    nucleisRank = Helpers.CalculatePointsRank(affectedNucleisArray);



                //--------------------- CLEARING EXISTING DATA -------------------------------------- 

                foreach (var f in secondaryAffecteVertexes)
                    ((BowyerVoronoiVertex)f.Owner).RemoveNeighbour(f);


                //Removing affected voronoi vertexes
                //Removing affected voronoi facets in nucleis
                foreach (var v in affectedVertexes)
                    v.Dispose();


                //Removing affected simplices
                voronoiVertexes.RemoveAll(v => v.IsTrash);

#if DEBUG
                if (secondaryAffecteVertexes.Any(f => f.FullyInitialized))
                    throw new NotSupportedException("Incoherence in the problem");
#endif
                affectedNucleis.Add(newNuclei);

                //--------------------- BUILDING NEW MESH -------------------------------------- 
                //build tesellation and check some neighbourhood with secondary
                var generatedVertexes = BuildTesellation(affectedNucleis, secondaryAffecteVertexes, nucleisRank);

                this.nucleis.Add(newNuclei);
                return newNuclei.VoronoiHyperRegion;

            }
        }

        private IEnumerable<BowyerVoronoiVertex> BuildTesellation(IEnumerable<BowyerNuclei> affectedNucleis, IEnumerable<BowyerSimpliceFacet> aloneOldFacets, int groupRank)
        {
            int previousVoronoiVertexSize = voronoiVertexes.Count;
            var newVoronoiVertexes = Helpers.Combinations(affectedNucleis, groupRank + 1);

#warning optimizable: cant this be an reusable attribute?
            foreach (var nucleiGroup in newVoronoiVertexes)
            {
                BowyerNuclei[] nucleiGroupArray = nucleiGroup.ToArray();

                bool validSimplice = false;

                BowyerVoronoiVertex v = null;
                //if (groupRank == ProblemDimensionality)
                //{ //this is the usual case
                //    HyperSphereConstraint hyperSphereConstraint = new HyperSphereConstraint(this.ProblemDimensionality);
                //    hyperSphereConstraint.Calculate(nucleiGroupArray, ProblemDimensionality, groupRank+1);
                //    if (affectedNucleis.Except(nucleiGroupArray)
                //    .All(nuc => !hyperSphereConstraint.CircumsphereContains(nuc.Coordinates)))
                //    {
                //        validSimplice = true;
                //        v = new BowyerVoronoiVertex(groupRank, nucleiGroupArray, hyperSphereConstraint);

                //    }
                //}
                //else
                //{
                HyperSphereConstraint hyperSphereConstraint = new HyperSphereConstraint(this.ProblemDimensionality);
                v = new BowyerVoronoiVertex(groupRank, nucleiGroupArray, hyperSphereConstraint);
                hyperSphereConstraint.Calculate(nucleiGroupArray, ProblemDimensionality, groupRank + 1);

#warning this is very little optime!!!
                if (//nucleis.Except(nucleiGroupArray)
                    affectedNucleis.Except(nucleiGroupArray)
                    .All(nuc => !v.simplice.CircumsphereContains(nuc.Coordinates)))
                {
                    validSimplice = true;

                }
                else
                    v.Dispose();
                //}

                if (validSimplice)
                {
                    this.voronoiVertexes.Add(v);

                    //select those who share a face with the new voronoiVertex



                    //foreach (var s in stableNeighbours)
                    //{
                    //    foreach (var f in s.simplice.facets.Where(f => !f.FullyInitialized))
                    //    {
                    //        if (v.simplice.nucleis.All(n => f.nucleis.Contains(n)))
                    //        {
                    //            v.AddNeighbour(s);
                    //            s.AddNeighbour(v);
                    //        }
                    //    }
                    //}


                }
            }


            //all generated vertexes are neighbour
            IEnumerable<BowyerVoronoiVertex> generatedVertexes = voronoiVertexes.Skip(previousVoronoiVertexSize);

            IEnumerable<BowyerVoronoiVertex> createdInfiniteVoronoiVertexes = Enumerable.Empty<BowyerVoronoiVertex>();
           

            foreach (BowyerVoronoiVertex v in generatedVertexes)
            {
                foreach (var v2 in generatedVertexes)
                    if (v != v2)
                    {
#warning 3level loop ... this shouldn't be to optime
                        foreach (var externalFacet in v2.simplice.facets.Where(f => !f.FullyInitialized))
                        {
                            BowyerSimpliceFacet thisFacet = v.simplice.facets.SingleOrDefault(f => externalFacet.nucleis.All(n => f.nucleis.Contains(n)));
                            if (thisFacet != null)
                            {
                                externalFacet.External = v;
                                v.AddNeighbour(thisFacet);
                            }
                        }
                    }

                foreach (BowyerSimpliceFacet externalFacet in aloneOldFacets.Where(f => !f.FullyInitialized))
                {
                    BowyerSimpliceFacet thisFacet = v.simplice.facets.SingleOrDefault(f => externalFacet.nucleis.All(n => f.nucleis.Contains(n)));
                    if (thisFacet != null)
                    {
                        externalFacet.External = v;
                        v.AddNeighbour(thisFacet);
                    }
                }

                //this vertex need nucleisRank+1 neighbours
                if (v.simplice.facets.Count(f => f.FullyInitialized) <= groupRank)
                {
                    v.GenerateInfiniteNeighbousr();
                    //add the new created voronoi vertex neighbours
                    createdInfiniteVoronoiVertexes = createdInfiniteVoronoiVertexes.Union(v.simplice.facets.Where(f => f.External.Infinity).Select(f => (BowyerVoronoiVertex)f.External));
                }

#if DEBUG
                else if (v.simplice.facets.Any(f => !f.FullyInitialized))
                    throw new NotSupportedException("This case has not been contemplated");
#endif
            }

#if DEBUG
            if (aloneOldFacets.Any(f => !f.FullyInitialized))
                throw new NotSupportedException("Incoherence in the problem. All old facets should be filled.");
#endif

            voronoiVertexes.AddRange(createdInfiniteVoronoiVertexes);

            return generatedVertexes;
        }

        public IVoronoiRegion AddNewPoint(double[] newPoint)
        {
            return AddNewPoint(null, newPoint);
        }
        /// <summary>
        /// Not to much optime (full main structure loop and check distinct)
        /// </summary>
        public IEnumerable<INuclei> Nucleis
        {
            get
            {
                return nucleis;
            }
        }

        public IEnumerable<ISimplice> Simplices
        {
            get
            {
                return voronoiVertexes.Where(v => !v.Infinity)
                                      .Select(v => v.Simplice);
            }
        }

        public IEnumerable<IVoronoiRegion> VoronoiRegions
        {
            get { return Nucleis.Select(n => n.VoronoiHyperRegion); }
        }

        public IEnumerable<IVoronoiVertex> VoronoiVertexes
        {
            get { return voronoiVertexes.Cast<IVoronoiVertex>(); }
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
        public IVoronoiRegion GetMatchingRegion(double[] point)
        {
            if (point == null || point.Length != ProblemDimensionality)
            {
                throw new ArgumentException("point added null or has invalid dimensionality");
            }

            /*This will be a very first approach as a not very efficent algorithm */
            if (!VoronoiRegions.Any())
            {
                return null;
            }
            else
            {
                /*candidate region */
                IVoronoiRegion r = VoronoiRegions.First();

                bool matchAllConstraints = false;
                while (!matchAllConstraints)
                {
                    matchAllConstraints = true;
                    foreach (var regionFacet in r.Facets)
                    {
                        if (!regionFacet.semiHyperSpaceMatch(point))
                        {
                            r = regionFacet.External.VoronoiHyperRegion;
                            matchAllConstraints = false;
                            break;
                        }
                    }
                }

                return r;
            }
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("VDiagram - Rank: " + this.Rank);
            sb.AppendLine("Number of Simplices: " + this.Simplices.Count());
            sb.AppendLine("Number of Nucleis: " + this.Nucleis.Count());
            return base.ToString();
        }


        public IEnumerable<ISimpliceFacet> GetFacetOrNull(INuclei n1, INuclei n2)
        {
            IEnumerable<BowyerSimplice> intersectionSimplices=((BowyerNuclei)n1).simplices.Where(s => s.nucleis.Contains(n2));
            var facetsA=intersectionSimplices.Select(s => s.Facets.SingleOrDefault(f => f.Nucleis.Contains(n2) && f.Nucleis.Contains(n1)))
                                              .Where(f=>f!=null);

            return facetsA;

        }
    }
}