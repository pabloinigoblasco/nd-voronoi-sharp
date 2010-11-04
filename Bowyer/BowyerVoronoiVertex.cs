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
using ndvoronoisharp.Common;

namespace ndvoronoisharp.Bowyer
{
    class BowyerVoronoiVertex : IVoronoiVertex
    {
        /*Delunay Simplices whose facets intersects in the vorono Vertex*/
        /// <summary>
        /// warning to all the calls to these property
        /// </summary>
        
        readonly internal BowyerSimplice simplice;

        public BowyerVoronoiVertex(int dimensionality, BowyerNuclei[] nucleis, HyperSphereConstraint hyperSphereConstraint)
        {
            simplice = new BowyerSimplice(hyperSphereConstraint, dimensionality, this, nucleis);
        }


        public double[] Coordinates { get { return simplice.Centroid; } }
        public ISimplice Simplice { get { return simplice; } }
        int dimensionality { get { return simplice.Rank; } }

        public IEnumerable<IVoronoiVertex> Neighbours
        {
            get { return this.simplice.facets.Where(f => f.External != null).Select(f => f.External); } 
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
        internal void AddNeighbour(BowyerSimpliceFacet neighbourg)
        {
            this.simplice.UpdateFace(neighbourg);
        }

        /// <summary>
        /// This is not bidirectional. You must do explicitly the two symetrics removes
        /// </summary>
        internal void RemoveNeighbour(BowyerSimpliceFacet neighbourg)
        {
            this.simplice.RemoveFacet(neighbourg);
        }


        internal void GenerateInfiniteNeighbousr()
        {
#if DEBUG
            int expectedNeighbours = (simplice.Rank + 1);
            int toCreate = expectedNeighbours - this.simplice.facets.Length;
#endif

            //this.simplice.CreateFacetsToCompleteDimension();

            foreach (var fn in this.simplice.facets.Where(f => !f.FullyInitialized))
            {
                //why infinite neibhours only one facet?
                //I think it's true for 1,2 and 3D
                var newInfiniteVornoiNeigbour = new BowyerVoronoiVertex(this.dimensionality, fn.nucleis,null);

                //initialize this facet
                fn.External = newInfiniteVornoiNeigbour;                
                //initialize the other facet
                newInfiniteVornoiNeigbour.simplice.UpdateFace(fn);


                /*if (fac.IsConvexHullFacet >= 1)
                    this.AddNeighbour(new BowyerVoronoiVertex(this.dimensionality, fac.Vertexes.Cast<BowyerNuclei>().ToArray()));
                else if(fac.IsConvexHullFacet==2)
                {
                    this.AddNeighbour(new BowyerVoronoiVertex(this.dimensionality, fac.Vertexes.Cast<BowyerNuclei>().ToArray()));                    
                }*/
            }
        }

        public bool IsTrash { get; set; }

        internal void Dispose()
        {
                simplice.Dispose();
            
        }
    }
}
