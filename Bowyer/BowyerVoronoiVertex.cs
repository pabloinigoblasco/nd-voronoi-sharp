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
using System.Text;

namespace ndvoronoisharp.Bowyer
{
    class BowyerVoronoiVertex:IVoronoiVertex
    {
        /*Delunay Simplices whose facets intersects in the vorono Vertex*/
        /// <summary>
        /// warning to all the calls to these property
        /// </summary>
        internal List<BowyerVoronoiVertex> neighbours { get; set; }
        internal readonly double[] coordinates;
        readonly internal BowyerSimplice simplice;

        public BowyerVoronoiVertex(int dimensionality,BowyerNuclei[] nucleis)
        {
            neighbours = new List<BowyerVoronoiVertex>();
            simplice = new BowyerSimplice(dimensionality, this, nucleis);
            this.coordinates = simplice.CalculateVoronoiVertexCoordinates();
        }

        
        public double[] Coordinates{get { return coordinates; }}
        public ISimplice Simplice { get { return simplice; } }
        int dimensionality { get { return simplice.Dimensionality; } }
        public IEnumerable<IVoronoiVertex> Neighbours
        {       
            get { return neighbours.Cast<IVoronoiVertex>(); }
        }
        public bool Infinity
        {
            get 
            {
                return simplice.InfiniteSimplice;
            }
        }


        /// <summary>
        /// This is not bidirectional. You must do explicitly the two symetrics adds
        /// </summary>
        /// <param name="neighbourg"></param>
        internal void AddNeighbour(BowyerVoronoiVertex neighbourg)
        {
            this.neighbours.Add(neighbourg);
            this.simplice.AddNeigbourSimpliceForFacets(neighbourg.simplice);
        }

        /// <summary>
        /// This is not bidirectional. You must do explicitly the two symetrics removes
        /// </summary>
        internal void RemoveNeighbour(BowyerVoronoiVertex neighbourg)
        {
            this.neighbours.Remove(neighbourg);
            this.simplice.RemoveNeigbourSimpliceForFacets(neighbourg.simplice);
        }

        internal void GenerateInfiniteNeighbousr()
        {
            int expectedNeighbours = (simplice.Dimensionality + 1);
            int toCreate = expectedNeighbours - this.neighbours.Count;
            
            this.simplice.CreateFacetsToCompleteDimension();
         
            foreach (var fn in this.simplice.facets.Where(f=>!f.FullyInitialized))
            {
                //why infinite neibhours only one facet?
                //I think it's true for 1,2 and 3D
                var newInfiniteVornoiNeigbour = new BowyerVoronoiVertex(this.dimensionality, fn.nucleis);

                fn.External = newInfiniteVornoiNeigbour;
                this.neighbours.Add(newInfiniteVornoiNeigbour);

                newInfiniteVornoiNeigbour.neighbours.Add(this);
                newInfiniteVornoiNeigbour.simplice.AddNeigbourSimpliceForFacets(this.simplice);

                /*if (fac.IsConvexHullFacet >= 1)
                    this.AddNeighbour(new BowyerVoronoiVertex(this.dimensionality, fac.Vertexes.Cast<BowyerNuclei>().ToArray()));
                else if(fac.IsConvexHullFacet==2)
                {
                    this.AddNeighbour(new BowyerVoronoiVertex(this.dimensionality, fac.Vertexes.Cast<BowyerNuclei>().ToArray()));                    
                }*/
            }
        }

        public bool IsTrash { get; set; }
    }
}
