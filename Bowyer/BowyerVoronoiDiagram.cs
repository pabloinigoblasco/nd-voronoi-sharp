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

namespace ndvoronoisharp.Bowyer
{
    public class BowyerVoronoiDelunayGraph:IVoronoiDelunayGraph
    {
   
        internal readonly List<BowyerVoronoiVertex> voronoiVertexes;
        private readonly int dimensionality;
        public int Dimensionality { get { return dimensionality; } }
        
        public BowyerVoronoiDelunayGraph(int spaceDimensionality)
        {
            voronoiVertexes = new List<BowyerVoronoiVertex>();
            dimensionality = spaceDimensionality;
        }

        public IVoronoiRegion AddNewPoint(object data, double[] newPoint)
        {
            if (newPoint == null || newPoint.Length != Dimensionality)
                throw new ArgumentException("point added null or has invalid dimensionality");


            if (!voronoiVertexes.Any())
            {
                //no voronoiVertexes
                //no simplices
                //no regions

                BowyerNuclei[] nucleis=new BowyerNuclei[dimensionality+1];
                INuclei nuclei=nucleis[0]=new BowyerNuclei(newPoint);

                //create a VoronoiVertex in the infinite
                var voronoiVertex = new BowyerVoronoiVertex(dimensionality,nucleis);
                nucleis[0].referenceVoronoiVertex = voronoiVertex;

                this.voronoiVertexes.Add(voronoiVertex);

                //create a not formed Simplice
                return voronoiVertexes.First().Simplice.Nucleis.First().VoronoiHyperRegion;
            }
            else
            {
                INuclei n=new BowyerNuclei(newPoint);
                IVoronoiRegion r = GetMatchingRegion(newPoint);
                var candidateVertexes = r.Vertexes.Where(v => v.Simplice.CircumsphereContains(newPoint));
                candidateVertexes=candidateVertexes.Union(candidateVertexes.SelectMany(v => v.Neighbours.Where(v2 => v2.Simplice.CircumsphereContains(newPoint)))).Distinct().ToArray();
               
                IEnumerable<INuclei> affectedNucleis = candidateVertexes.SelectMany(v => v.Simplice.Nucleis.Where(nuc=>nuc!=null)).Union(Enumerable.Repeat(n,1)).Distinct();

                if (affectedNucleis.Count() > dimensionality + 1)
                {
                    //añadir nuevo punto a voronoi no tiene sentido
                    IEnumerable<IVoronoiVertex> newVoronoiVertexes = BuildTesellation(affectedNucleis);
                    removeOldVertexes(candidateVertexes);
                }

                return n.VoronoiHyperRegion;

            }
        }

        private void removeOldVertexes(IEnumerable<IVoronoiVertex> candidateVertexes)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<IVoronoiVertex> BuildTesellation(IEnumerable<INuclei> affectedNucleis)
        {
            throw new NotImplementedException();
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
            get { return voronoiVertexes.SelectMany(v => v.Simplice.Nucleis.Where(n=>n!=null)).Distinct(); }
        }

        public IEnumerable<ISimplice> Simplices
        {
            get { return voronoiVertexes.Select(v => v.Simplice); }
        }

        public IEnumerable<IVoronoiRegion> VoronoiRegions
        {
            get { return Nucleis.Select(n=>n.VoronoiHyperRegion); }
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
            if (point == null || point.Length != dimensionality)
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