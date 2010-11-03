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
        public int Rank { get; private set; }
        private HyperSphereConstraint containConstraint;

        public BowyerSimplice(HyperSphereConstraint containConstraint, int rank, BowyerVoronoiVertex voronoiVertex, BowyerNuclei[] nucleis)
        {
            this.Rank = rank;
            this.nucleis = nucleis;
            this.voronoiVertex = voronoiVertex;
            this.containConstraint = containConstraint;
            infiniteSimplice = this.Rank == nucleis.Length;

            //if(!this.InfiniteSimplice)
            foreach (var n in nucleis)
                n.AddSimplice(this);

            if (InfiniteSimplice || rank == 0)
            {
                facets = new BowyerSimpliceFacet[] { new BowyerSimpliceFacet(this.voronoiVertex, null, nucleis) };
            }
            else
            {
                facets = new BowyerSimpliceFacet[Rank + 1];
                IEnumerable<IEnumerable<BowyerNuclei>> fs = Helpers.Combinations(nucleis, rank);
                int i = 0;

                foreach (var f in fs)
                    facets[i++] = new BowyerSimpliceFacet(this.voronoiVertex, null, f);
            }
        }


        public INuclei[] Nucleis { get { return nucleis; } }
        public IVoronoiVertex VoronoiVertex { get { return voronoiVertex; } }
        public IEnumerable<ISimpliceFacet> Facets { get { return facets; } }
        public bool InfiniteSimplice { get { return infiniteSimplice; } }
        public IEnumerable<ISimplice> NeighbourSimplices
        {
            get { return facets.Where(f => f.Rank > 0 && !f.IsConvexHullFacet && !f.External.Infinity).Select(f => f.External.Simplice); }
        }
        public double[] Centroid
        {
            get
            {
                if (containConstraint == null)
                    return null;
                else
                    return containConstraint.Center;

            }
        }
        public double Radious
        {
            get
            {
                if (containConstraint == null)
                    return double.PositiveInfinity;
                else
                    return containConstraint.Radious;
            }
        }

        public bool CircumsphereContains(double[] point)
        {
            if (!InfiniteSimplice)
                return containConstraint.CircumsphereContains(point);
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

        /*
        internal void CalculateVoronoiVertexCoordinates()
        {
            int problemDimensionality = this.nucleis.First().Coordinates.Length;
            double[] voronoiVertexCoordinates = new double[problemDimensionality];
            if (!InfiniteSimplice && nucleis.Length >= 2)
            {
                //if (nucleis.Length == 1)
                //{
                //    voronoiVertexCoordinates = nucleis.First().Coordinates;
                //    this.Radious = double.PositiveInfinity;
                //}
                //else
                containConstraint.CalculateCentroid(nucleis.Select(n => n.Coordinates));
            }
            //in other case it is supposed that the coordinates has been precalculated

            //else
            //    voronoiVertexCoordinates = Enumerable.Repeat(double.PositiveInfinity, voronoiVertexCoordinates.Length).ToArray();

            //return voronoiVertexCoordinates;
        }*/




        ///// <summary>
        ///// this is not symetrical. you must add in both sides individually
        ///// </summary>
        ///// <param name="s"></param>
        //private void AddNeigbourSimpliceForFacets(BowyerSimplice s)
        //{
        //    BowyerNuclei[] facetNucleis = this.nucleis.Intersect(s.nucleis).ToArray();

        //    int index = Array.IndexOf(facets, null);
        //    facets[index] = new BowyerSimpliceFacet(this.voronoiVertex, s.voronoiVertex, facetNucleis);
        //}

        //private void RemoveNeigbourSimpliceForFacets(BowyerSimplice s)
        //{
        //    bool found = false;
        //    for (int i = 0; i < facets.Length && !found; i++)
        //    {
        //        if (facets[i].External.Simplice == s)
        //        {
        //            facets[i] = null;
        //            found = true;
        //        }
        //    }
        //    if (!found)
        //        throw new ArgumentException("Invalid Simplice");
        //}

        internal void UpdateFace(BowyerSimplice bowyerSimplice)
        {
            BowyerSimpliceFacet facet = this.facets.Single(f => f.nucleis.Intersect(bowyerSimplice.nucleis).Count() == this.Rank);
            facet.External = bowyerSimplice.voronoiVertex;
        }

        internal void RemoveFacet(BowyerSimplice bowyserSimplice)
        {
#if DEBUG
            BowyerSimpliceFacet facet = facets.Single(f => f.External == bowyserSimplice.voronoiVertex);
            facet.External = null;
#else
            BowyerSimpliceFacet facet = facets.First(f => f.External == bowyserSimplice.voronoiVertex);
            facet.External = null;
#endif
        }

        /// <summary>
        /// must create the not defined facets to complete the dimension. External Voronoi is supposed to be infinite
        /// and is not defined here but in the caller Voronoi AddInfiniteNeighbours
        /// </summary>
        /// <returns></returns>
        //        internal void CreateFacetsToCompleteDimension()
        //        {
        //            //get nucleis with not enough simplices - candidates
        //            //create combinations, create a

        //            if (Rank > 1)
        //            {

        //                IEnumerable<BowyerNuclei> nucs = nucleis.Where(n => !facets.Any(f => f != null && f.nucleis.Contains(n)));
        //#error BAD
        //                IEnumerable<IEnumerable<BowyerNuclei>> PosibleSimpliceFacets = Helpers.Combinations(nucs, Rank - 1);
        //                foreach (var posibleFacet in PosibleSimpliceFacets)
        //                {
        //                    int index = Array.IndexOf(facets, null);
        //                    facets[index] = new BowyerSimpliceFacet(this.VoronoiVertex, null, posibleFacet.ToArray());
        //                }
        //            }
        //            //dimension 1 simplice
        //            else
        //            {
        //                for(int i=0;i<nucleis.Length;i++)
        //                    facets[i] = new BowyerSimpliceFacet(this.voronoiVertex, null, new BowyerNuclei[]{ this.nucleis[i]});
        //            }



        //            //var nucs = nucleis.Where(n => facets.Count(f=>f.nucleis.Contains(n)) <= Dimensionality)
        //            //IEnumerable<IEnumerable<BowyerNuclei>> PosibleSimpliceFacets  = Helpers.Combinations(nucs, Dimensionality);
        //            //foreach (var possibleFacet in PosibleSimpliceFacets)
        //            //{
        //            //    int index = Array.IndexOf(facets, null);
        //            //    facets[index] = new BowyerSimpliceFacet(this.VoronoiVertex, null, possibleFacet.ToArray());
        //            //}






        //            /*
        //            if (nucleis.Length == 1)
        //            {
        //                return new BowyerSimpliceFacet[0];
        //            }
        //            else
        //            {
        //                if (nucleis.Length < Dimensionality + 1)
        //                    return Helpers.Combinations(nucleis, nucleis.Length).Select(nucs => new BowyerSimpliceFacet(this.VoronoiVertex, null, nucs.ToArray())).ToArray();
        //                else
        //                    return Helpers.Combinations(nucleis, Dimensionality).Select(nucs => new BowyerSimpliceFacet(this.voronoiVertex, null, nucs.ToArray())).ToArray();

        //                //facets = voronoiVertex.neighbours.Select(vn => new BowyerSimpliceFacet(this.voronoiVertex, vn)).ToArray();
        //             * */
        //        }



        internal void Dispose()
        {
            for (int i = 0; i < facets.Length; i++)
                facets[i].Dispose();
            
        }
    }

}
