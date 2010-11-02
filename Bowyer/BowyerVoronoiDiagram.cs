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

namespace ndvoronoisharp.Bowyer
{
    public class BowyerVoronoiDelunayGraph : IVoronoiDelunayGraph
    {

        internal readonly List<BowyerVoronoiVertex> voronoiVertexes;
        public int ProblemDimensionality { get; private set; }
        int nucleisRank;

        public BowyerVoronoiDelunayGraph(int spaceDimensionality)
        {
            voronoiVertexes = new List<BowyerVoronoiVertex>();
            ProblemDimensionality = spaceDimensionality;
            nucleisRank = 0;
        }

        public IVoronoiRegion AddNewPoint(object data, double[] newPoint)
        {
            if (newPoint == null || newPoint.Length != ProblemDimensionality)
                throw new ArgumentException("point added null or has invalid dimensionality");


            //SPECIAL CASE FOR FIRST ADD
            if (!voronoiVertexes.Any())
            {
                //no voronoiVertexes
                //no simplices
                //no regions

                BowyerNuclei[] nucleis = new BowyerNuclei[1];
                BowyerNuclei nuclei = nucleis[0] = new BowyerNuclei(newPoint);

                //create a VoronoiVertex in the infinite
                var voronoiVertex = new BowyerVoronoiVertex(0, nucleis);

                this.voronoiVertexes.Add(voronoiVertex);

                //create a not formed Simplice
                return voronoiVertexes.First().Simplice.Nucleis.First().VoronoiHyperRegion;
            }
            else
            {

                //--------------- SITUATION ANALYSIS - READ ONLY--------------
                BowyerNuclei newNuclei = new BowyerNuclei(newPoint);
#warning optimizable: not variable list without cast
                List<BowyerVoronoiVertex> affectedVertexes = new List<BowyerVoronoiVertex>();

                BowyerNuclei[] newpointSet = new BowyerNuclei[] { newNuclei };


                if (nucleisRank < ProblemDimensionality && Helpers.CalculatePointsRank(Simplices.First().Nucleis.Union(newpointSet)) > nucleisRank)
                {
                    affectedVertexes = this.voronoiVertexes;
                    nucleisRank++;
                    foreach (var v in affectedVertexes)
                        v.IsTrash = true;
                }
                else
                {
                    IVoronoiRegion r = GetMatchingRegion(newPoint);
                    // and use r.Vertexes
                    foreach (BowyerVoronoiVertex v in r.Vertexes)
                        if (v.Simplice.CircumsphereContains(newPoint))
                        {
                            affectedVertexes.Add(v);
                            v.IsTrash = true;
                        }

                    affectedVertexes = affectedVertexes.Union(affectedVertexes.SelectMany(v => v.neighbours.Except(affectedVertexes).Where(n => n.Simplice.CircumsphereContains(newPoint)))).ToList();
                }


                //select affected nucleis to build a new mesh
                IEnumerable<BowyerNuclei> affectedNucleis = affectedVertexes
                                                                       .SelectMany(v => v.simplice.nucleis)
                                                                       .Distinct();

                BowyerNuclei[] affectedNucleisArray = affectedNucleis.Union(newpointSet).ToArray();

                //if any candidate vertex sismplice has the maxium dimensionality, the postgenerated tesellation will also have this maxium dimensionality

                //if (affectedVertexes.First().Simplice.Dimensionality == ProblemDimensionality)
                //    nucleisRank = ProblemDimensionality;
                //else
                //    nucleisRank = Helpers.CalculatePointsRank(affectedNucleisArray);

#warning optimizable: cant be this an auxiliar attribute?
                List<BowyerVoronoiVertex> secondaryAffecteVertexes = new List<BowyerVoronoiVertex>();

                //--------------------- BUILD NEW MESH -------------------------------------- 

               

                
                //Removing affected voronoi vertexes
                //Removing affected voronoi facets in nucleis
                foreach (var v in affectedVertexes)
                {
                    foreach (var n in v.simplice.nucleis)
                        //theoretically this also removes the associated facets.
                        n.RemoveSimplice(v.simplice);

                    foreach (BowyerVoronoiVertex neigh in v.Neighbours)
                    {
                        //if (neigh.Infinity)//dependent voronoiVertex
                        //{
                        //    voronoiVertexes.Remove(neigh);
                        //    foreach (var n in neigh.simplice.nucleis)
                        //        n.RemoveSimplice(neigh.simplice);

                        //}
                        //else
                        //{

#warning these neighbours needs a new neighbour. is it assured?
                        if (!neigh.IsTrash)
                        {
                            neigh.RemoveNeighbour(v);
                            secondaryAffecteVertexes.Add(neigh);
                        }
                        //}
                    }
                }
                //Removing affected simplices
                voronoiVertexes.RemoveAll(v => v.IsTrash);
                

                //build tesellation and check some neighbourhood with secondary
                var generatedVertexes = BuildTesellation(affectedNucleisArray, secondaryAffecteVertexes, nucleisRank);

                return newNuclei.VoronoiHyperRegion;

            }
        }

        private IEnumerable<BowyerVoronoiVertex> BuildTesellation(IEnumerable<BowyerNuclei> affectedNucleis, IEnumerable<BowyerVoronoiVertex> stableNeighbours, int groupRank)
        {
            int previousVoronoiVertexSize = voronoiVertexes.Count;
            var newVoronoiVertexes = Helpers.Combinations(affectedNucleis, groupRank + 1);

#warning optimizable: cant this be an reusable attribute?
            foreach (var nucleiGroup in newVoronoiVertexes)
            {
                BowyerNuclei[] nucleiGroupArray = nucleiGroup.ToArray();
                //all generated simplices contains a voronoiVertex
                BowyerVoronoiVertex v = new BowyerVoronoiVertex(groupRank, nucleiGroupArray);
                this.voronoiVertexes.Add(v);

                //select those who share a face with the new voronoiVertex
#warning optimizable?
                var existingNeigbours = stableNeighbours.Select(candidateNeigbhour => new { candidateNeigbour = candidateNeigbhour, NucleiIntersection = candidateNeigbhour.simplice.nucleis.Intersect(nucleiGroup) })
                                       .Where(t => t.NucleiIntersection.Count() == groupRank);

                foreach (var t in existingNeigbours)
                {
                    t.candidateNeigbour.AddNeighbour(v);
                    v.AddNeighbour(t.candidateNeigbour);

                }
            }

            //all generated vertexes are neighbour
            IEnumerable<BowyerVoronoiVertex> generatedVertexes = voronoiVertexes.Skip(previousVoronoiVertexSize);

            IEnumerable<BowyerVoronoiVertex> createdInfiniteVoronoiVertexes = Enumerable.Empty<BowyerVoronoiVertex>();
            foreach (BowyerVoronoiVertex v in generatedVertexes)
            {
                foreach (var v2 in generatedVertexes)
                    if (v != v2)
                        v.AddNeighbour(v2);

                //this vertex need nucleisRank+1 neighbours
                if (v.neighbours.Count <= groupRank)
                {
                    v.GenerateInfiniteNeighbousr();
                    createdInfiniteVoronoiVertexes = createdInfiniteVoronoiVertexes.Union(v.neighbours.Where(vneigh => vneigh.Infinity));
                }

#if DEBUG
                else if (v.neighbours.Count < groupRank)
                    throw new NotSupportedException("This case has not been contemplated");
#endif
            }
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
            get { return voronoiVertexes.SelectMany(v => v.Simplice.Nucleis.Where(n => n != null)).Distinct(); }
        }

        public IEnumerable<ISimplice> Simplices
        {
            get { return voronoiVertexes.Where(v => !v.Infinity).Select(v => v.Simplice); }
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

    }
}