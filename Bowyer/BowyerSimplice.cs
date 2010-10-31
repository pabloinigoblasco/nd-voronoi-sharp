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
    class BowyerSimplice:ISimplice
    {
        public BowyerSimplice(int dimensionalyty,BowyerVoronoiVertex voronoiVertex)
        {
            this.nucleis = new BowyerNuclei[dimensionalyty + 1];
            this.voronoiVertex = voronoiVertex;
            IsIncomplete = true;
        }

        private readonly BowyerNuclei[] nucleis;
        public INuclei[] Nucleis { get { return nucleis; }}

        private readonly BowyerVoronoiVertex voronoiVertex;
        public IVoronoiVertex VoronoiVertex{get { return voronoiVertex; }}

        public void AddNewNuclei(BowyerNuclei nuclei)
        {
            int indexof = Array.IndexOf(nucleis, null);
            nucleis[indexof] = nuclei;

            if (indexof == nucleis.Length - 1)
            {
                IsIncomplete = false;
                Helpers.CalculateSimpliceCentroid(Nucleis, this.VoronoiVertex);
                    
            }
        }

        public bool IsIncomplete
        {
            get;
            private set;
        }


        public IEnumerable<ISimplice> NeighbourSimplices
        {
            get {return  Nucleis.SelectMany(n => n.Simplices).Distinct(); }
        }

        public double Radious
        {
            get { throw new NotImplementedException(); }
        }

        public IDelunaiFacet[] Facets
        {
            get { throw new NotImplementedException(); }
        }

        public bool CircumsphereContains(double[] point)
        {
            double sum = 0;
            for (int i = 0; i < point.Length; i++)
            {
                double diff = point[i] - this.VoronoiVertex.Coordinates[i];
                sum += (diff * diff);
            }

#warning optimizable
            return sum <= Radious*Radious;
        }

    }
}
