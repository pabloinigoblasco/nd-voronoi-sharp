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
using ndvoronoisharp.Common.implementations;

namespace ndvoronoisharp.Bowyer
{
    class BowyerNuclei : INuclei,IVoronoiRegion
    {
        //state
        private readonly double[] coordinates;
        internal List<BowyerSimplice> simplices; //redundante with VoronoiGraph
        Dictionary<INuclei, IVoronoiFacet> voronoiFacets;
        public object Data { get; set; }

        public BowyerNuclei(double[] Coordinates)
        {
            this.coordinates = Coordinates;
            simplices = new List<BowyerSimplice>();
            this.voronoiFacets = new Dictionary<INuclei, IVoronoiFacet>();
        }

     
        public double[] Coordinates
        {
            get { return coordinates; }
        }
        public IVoronoiRegion VoronoiHyperRegion
        {
            get{return this;}
        }
        
        public IEnumerable<INuclei> Neighbourgs
        {
            get 
            {
                return voronoiFacets.Keys;
            }
        }
        public IEnumerable<ISimplice> Simplices { get { return simplices.Cast<ISimplice>(); } }
        public bool BelongConvexHull
        {
            get 
            {
                return this.coordinates.Length> Simplices.First().Rank || Simplices.Any(s => ((BowyerSimplice)s).InfiniteSimplice);
            }
        }


        internal void RemoveSimplice(BowyerSimplice toRemove)
        {
            simplices.Remove(toRemove);
            //remove voronoi facets, except those used in other simplices
            foreach(var n in toRemove.nucleis.Except(simplices.SelectMany(s=>s.nucleis)))
                voronoiFacets.Remove(n);
        }
        internal void AddSimplice(BowyerSimplice toAdd)
        {
            simplices.Add(toAdd);
            //add also simplice Facets
            if (!toAdd.InfiniteSimplice)
            {
                foreach (var n in toAdd.nucleis)
                {
                    if (n != this && !voronoiFacets.ContainsKey(n))
                        voronoiFacets.Add(n, new Common.implementations.DefaultVoronoiFacet(this, n));
                }
            }
        }

        public IEnumerable<IVoronoiFacet> Facets { 
            get 
            {
                return voronoiFacets.Values;
            } 
        }
        bool IVoronoiRegion.IsInfiniteRegion
        {
            get { return BelongConvexHull; }
        }
        bool IVoronoiRegion.ContainsPoint(double[] point)
        {
            if (point.Length != this.Coordinates.Length)
                throw new ArgumentException("Invalid dimensionality of the point");

            foreach (var facet in voronoiFacets.Values)
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
        IEnumerable<IVoronoiRegion> IVoronoiRegion.NeighbourgRegions
        {
            get { return Neighbourgs.Select(n => n.VoronoiHyperRegion); }
        }
        INuclei IVoronoiRegion.Nuclei { get { return this; } }

        IEnumerable<IVoronoiVertex> IVoronoiRegion.Vertexes
        {
            get { return Simplices.Select(n => n.VoronoiVertex); }
        }

       
    }

}
