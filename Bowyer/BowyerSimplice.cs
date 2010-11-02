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
using MathNet.Numerics.LinearAlgebra;

namespace ndvoronoisharp.Bowyer
{
    class BowyerSimplice : ISimplice
    {
        internal readonly BowyerSimpliceFacet[] facets;
        internal readonly BowyerNuclei[] nucleis;
        private readonly BowyerVoronoiVertex voronoiVertex;
        private readonly bool infiniteSimplice;
        public int Dimensionality { get; private set; }
        public double Radious { get; private set; }

        public BowyerSimplice(int dimensionality, BowyerVoronoiVertex voronoiVertex, BowyerNuclei[] nucleis)
        {
            this.Dimensionality = dimensionality;
            this.nucleis = nucleis;
            this.voronoiVertex = voronoiVertex;
            infiniteSimplice = this.Dimensionality >= nucleis.Length;

            //if(!this.InfiniteSimplice)
            foreach (var n in nucleis)
                n.AddSimplice(this);

            if (InfiniteSimplice)
            {
                if (dimensionality >= 1)
                    facets = new BowyerSimpliceFacet[1];
                else
                    facets = new BowyerSimpliceFacet[0];
            }
            else
                facets = new BowyerSimpliceFacet[Dimensionality + 1];
        }


        public INuclei[] Nucleis { get { return nucleis; } }
        public IVoronoiVertex VoronoiVertex { get { return voronoiVertex; } }
        public IEnumerable<ISimpliceFacet> Facets { get { return facets; } }
        public bool InfiniteSimplice { get { return infiniteSimplice; } }
        public IEnumerable<ISimplice> NeighbourSimplices
        {
            get { return facets.Select(f => f.External.Simplice); }
        }



        //#warning call this property is not optime in space. Try to avoid it. Anyways it's needed in some cases

        //private BowyerSimpliceFacet[] facets;
        /*      internal BowyerSimpliceFacet[] CalculatePossibleFacets()
              {
                  //if (IsIncomplete)
                  //{
                  //    if (nucleis.Count(n => n != null) == nucleis.Length - 1)
                  //        facets = Enumerable.Repeat(new BowyerSimpliceFacet(nucleis, this), 1).ToArray();
                  //    else
                  //        facets = new BowyerSimpliceFacet[0];
                  //}
                  //else
                  if (nucleis.Length == 1)
                  {
                      return new BowyerSimpliceFacet[0];
                  }
                  else
                  {
                      if (nucleis.Length < Dimensionality + 1)
                          return Helpers.Combinations(nucleis, nucleis.Length).Select(nucs => new BowyerSimpliceFacet(this.VoronoiVertex,null, nucs.ToArray())).ToArray();
                      else
                          return Helpers.Combinations(nucleis, Dimensionality).Select(nucs => new BowyerSimpliceFacet(this.voronoiVertex,null, nucs.ToArray())).ToArray();

                      //facets = voronoiVertex.neighbours.Select(vn => new BowyerSimpliceFacet(this.voronoiVertex, vn)).ToArray();
                  }

            
                 // else if (nucleis.Length < Dimensionality + 1)
                  //    this.facets = Helpers.Combinations(nucleis, nucleis.Length).Select(nucs => new BowyerSimpliceFacet(nucs.ToArray(), this)).ToArray();
                  //else
                   //   this.facets = Helpers.Combinations(nucleis, Dimensionality+1).Select(nucs => new BowyerSimpliceFacet(nucs.ToArray(), this)).ToArray();
            
           
              }*/



        public bool CircumsphereContains(double[] point)
        {
            if (!InfiniteSimplice)
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


        internal double[] CalculateVoronoiVertexCoordinates()
        {
            int problemDimensionality = this.nucleis.First().Coordinates.Length;
            double[] voronoiVertexCoordinates = new double[problemDimensionality];
            if (!InfiniteSimplice)
            {
                if (nucleis.Length == 1)
                {
                    voronoiVertexCoordinates = nucleis.First().Coordinates;
                    this.Radious = double.PositiveInfinity;
                }
                else
                {
                    Helpers.CalculateSimpliceCentroidFromFacets(Nucleis, ref voronoiVertexCoordinates);

                    //now calculate the radious and store it.
                    Vector v = new Vector(voronoiVertexCoordinates.Length);
                    for (int i = 0; i < voronoiVertexCoordinates.Length; i++)
                        v[i] = voronoiVertexCoordinates[i] - nucleis[0].Coordinates[i];

                    this.Radious = v.Norm();
                }
            }
            else
                voronoiVertexCoordinates = Enumerable.Repeat(double.PositiveInfinity, voronoiVertexCoordinates.Length).ToArray();

            return voronoiVertexCoordinates;

        }

        /// <summary>
        /// this is not symetrical. you must add in both sides individually
        /// </summary>
        /// <param name="s"></param>
        internal void AddNeigbourSimpliceForFacets(BowyerSimplice s)
        {
            BowyerNuclei[] facetNucleis = this.nucleis.Intersect(s.nucleis).ToArray();

            int index = Array.IndexOf(facets, null);
            facets[index] = new BowyerSimpliceFacet(this.voronoiVertex, s.voronoiVertex, facetNucleis);
        }

        internal void RemoveNeigbourSimpliceForFacets(BowyerSimplice s)
        {
            bool found = false;
            for (int i = 0; i < facets.Length && !found; i++)
            {
                if (facets[i].External.Simplice == s)
                {
                    facets[i] = null;
                    found = true;
                }
            }
            if (!found)
                throw new ArgumentException("Invalid Simplice");
        }

        /// <summary>
        /// must create the not defined facets to complete the dimension. External Voronoi is supposed to be infinite
        /// and is not defined here but in the caller Voronoi AddInfiniteNeighbours
        /// </summary>
        /// <returns></returns>
        internal void CreateFacetsToCompleteDimension()
        {
            //get nucleis with not enough simplices - candidates
            //create combinations, create a

            var nucs = nucleis.Where(n => n.simplices.Count <= Dimensionality);


            IEnumerable<IEnumerable<BowyerNuclei>> PosibleSimpliceFacets = PosibleSimpliceFacets = Helpers.Combinations(nucs, Dimensionality);
            foreach (var possibleFacet in PosibleSimpliceFacets)
            {
                int index = Array.IndexOf(facets, null);
                facets[index] = new BowyerSimpliceFacet(this.VoronoiVertex, null, possibleFacet.ToArray());
            }




            /*
            if (nucleis.Length == 1)
            {
                return new BowyerSimpliceFacet[0];
            }
            else
            {
                if (nucleis.Length < Dimensionality + 1)
                    return Helpers.Combinations(nucleis, nucleis.Length).Select(nucs => new BowyerSimpliceFacet(this.VoronoiVertex, null, nucs.ToArray())).ToArray();
                else
                    return Helpers.Combinations(nucleis, Dimensionality).Select(nucs => new BowyerSimpliceFacet(this.voronoiVertex, null, nucs.ToArray())).ToArray();

                //facets = voronoiVertex.neighbours.Select(vn => new BowyerSimpliceFacet(this.voronoiVertex, vn)).ToArray();
             * */
        }
    }

}
