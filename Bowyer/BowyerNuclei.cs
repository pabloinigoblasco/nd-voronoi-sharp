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
    class BowyerNuclei : INuclei
    {

        internal BowyerVoronoiVertex referenceVoronoiVertex { get; set; }
        public BowyerNuclei(double[] Coordinate)
        {
            this.coordinates = Coordinates;
            this.VoronoiHyperRegion = new BowyerHyperRegion(this);
        }
        

        public void UpdateNucleiData()
        {
#warning fill this method when a voronoi region removed or added, actuallize the referenceVoronoiVertex

        }
        private readonly double[] coordinates;
        public double[] Coordinates
        {
            get { return coordinates; }
        }
        public IVoronoiRegion VoronoiHyperRegion
        {
            get;
            private set;
        }

#warning call this property is not optime in space. Try to avoid it. Anyways it's needed in some cases
        public IEnumerable<INuclei> Neighbourgs
        {
            get 
            {

                return Simplices.SelectMany(s => s.Nucleis.Where(n=>n!=null)).Except(Enumerable.Repeat<INuclei>(this, 1)).Distinct();
            }
        }
        public IEnumerable<ISimplice> Simplices
        {
            get
            {
#warning is this right? we could check all the graph table, but this can be a good optimization

                var simps = referenceVoronoiVertex.Neighbours.Where(v => v!=null && v.Simplice.Nucleis.Contains(this)).Select(v => v.Simplice);
                simps = simps.Union(Enumerable.Repeat(referenceVoronoiVertex.Simplice, 1)).ToArray();
                return simps;
            }
        }
        public bool BelongConvexHull
        {
            get 
            {
                return Simplices.Any(s => ((BowyerSimplice)s).IsIncomplete);
            }
        }
    }
}
