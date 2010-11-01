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
    class BowyerSimplice:ISimplice
    {
        public BowyerSimplice(int dimensionalyty,BowyerVoronoiVertex voronoiVertex,BowyerNuclei[] nucleis)
        {
            this.nucleis = nucleis;
            this.voronoiVertex = voronoiVertex;
            IsIncomplete = nucleis.Any(n => n == null);
        }

        private readonly BowyerNuclei[] nucleis;
        public INuclei[] Nucleis { get { return nucleis; }}

        private readonly BowyerVoronoiVertex voronoiVertex;
        public IVoronoiVertex VoronoiVertex{get { return voronoiVertex; }}

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

#warning call this property is not optime in space. Try to avoid it. Anyways it's needed in some cases

        private BowyerSimpliceFacet[] facets;
        public ISimpliceFacet[] Facets
        {
            get 
            {
                if (facets == null)
                {
                    if (IsIncomplete)
                    {
                        if (nucleis.Count(n => n != null) == nucleis.Length - 1)
                            facets = Enumerable.Repeat(new BowyerSimpliceFacet(nucleis, this), 1).ToArray();
                        else
                            facets = new BowyerSimpliceFacet[0];
                    }
                    else
                        this.facets = Helpers.Combinations(nucleis, nucleis.Length - 1).Select(nucs => new BowyerSimpliceFacet(nucs.ToArray(), this)).ToArray();
                }

                return facets;
            }
        }

        public bool CircumsphereContains(double[] point)
        {
            if (!IsIncomplete)
            {
                double sum = 0;
                for (int i = 0; i < point.Length; i++)
                {
                    double diff = point[i] - this.VoronoiVertex.Coordinates[i];
                    sum += (diff * diff);
                }

#warning optimizable
                return sum <= Radious * Radious;
            }
            else
            {
                //check restricions
                //foreach facets verify restriction. It's only possible to have zero or one facet
                //if no facet is made. Always CircumHyperSphere contains the new point
                if (!Facets.Any())
                    return true;
                else
                {
                    ISimpliceFacet f = Facets.Single();
                    return f.semiHyperSpaceMatch(point);
                }
                
            }
        }


        public void CalculateVoronoiVertexCoordinates(ref double[] voronoiVertexCoordinates)
        {
            if (!IsIncomplete)
                Helpers.CalculateSimpliceCentroid(null, this.voronoiVertex);
            else
                voronoiVertexCoordinates= Enumerable.Repeat(double.PositiveInfinity, voronoiVertexCoordinates.Length).ToArray();
                
        }
    }
}
