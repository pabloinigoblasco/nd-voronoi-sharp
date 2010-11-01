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
using ndvoronoisharp.Common.implementations;

namespace ndvoronoisharp.Bowyer
{
    class BowyerHyperRegion:IVoronoiRegion
    {

#warning this can got incoherent when new nucleis are added
        IEnumerable<IVoronoiFacet> voronoiFacets;
        public IEnumerable<IVoronoiFacet> Facets { 
            get 
            {
                if(voronoiFacets==null)
                   voronoiFacets= nuclei.Neighbourgs.Select(n => (IVoronoiFacet)(new DefaultVoronoiFacet(nuclei, n))); 

                return voronoiFacets; 
            } 
        }

        public BowyerHyperRegion(BowyerNuclei nuclei)
        {
            this.nuclei = nuclei;
        }
 
        public bool IsInfiniteRegion
        {
            get { return Nuclei.BelongConvexHull; }
        }

        public bool ContainsPoint(double[] point)
        {
            if (point.Length != this.nuclei.Coordinates.Length)
                throw new ArgumentException("Invalid dimensionality of the point");

            foreach (var facet in voronoiFacets)
            {
                if (!facet.semiHyperSpaceMatch(point))
                    return false;
            }
            return true;
        }

        private static bool VerifyRestriction(IVoronoiVertex a, IVoronoiVertex b)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IVoronoiRegion> NeighbourgRegions
        {
            get { return Nuclei.Neighbourgs.Select(n => n.VoronoiHyperRegion); }
        }

        private readonly BowyerNuclei nuclei;
        public INuclei Nuclei { get { return nuclei; } }

        public IEnumerable<IVoronoiVertex> Vertexes
        {
            get { return Nuclei.Simplices.Select(n => n.VoronoiVertex); }
        }
    }
}
