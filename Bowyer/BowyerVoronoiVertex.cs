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
        private IVoronoiVertex[] NeighbourVertexes { get; set; }
        int dimensionality { get { return NeighbourVertexes.Length; } }

        public BowyerVoronoiVertex(int dimensionality,BowyerNuclei[] nucleis)
        {
            simplice=new BowyerSimplice(dimensionality+1,this, nucleis);
            NeighbourVertexes = new IVoronoiVertex[dimensionality + 1];
            this.coordinates=new double[dimensionality];
            simplice.CalculateVoronoiVertexCoordinates(ref coordinates);
        }

        private readonly double[] coordinates;
        public double[] Coordinates{get { return coordinates; }}

        readonly private BowyerSimplice simplice;
        public ISimplice Simplice { get { return simplice; } }

        
        public IEnumerable<IVoronoiVertex> Neighbours
        {
            get { return NeighbourVertexes.Where(n=>n!=null); }
        }

        public bool Infinity
        {
            get 
            {
                return simplice.IsIncomplete;
            }
        }

    }
}
