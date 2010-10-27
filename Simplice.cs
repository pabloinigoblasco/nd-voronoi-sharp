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
using MathNet.Numerics.LinearAlgebra;
using ndvoronoisharp.implementations;
using System.Diagnostics;
using System.Collections.Generic;

namespace ndvoronoisharp
{
    /// <summary>
    /// This class represent the minimun convex cell politope in an politope n-dimensional world.
    /// This politope must have n+1 nodes where rank(nodes)==n
    /// For example. In a 2-Dimensional World, this class represent a triangle.
    /// In a 3-Dimensional World this class represent a poliedra.
    /// </summary>
    public class Simplice : ndvoronoisharp.ISimplice
    {
        public INuclei[] Nucleis { get; private set; }
        public IEnumerable<ISimplice> NeighbourSimplices 
        {
            get 
            {
                return Facets.Where(f => !f.IsBoundingFacet).Select(f => f.ParentA == this ? f.ParentB : f.ParentB).Cast<ISimplice>();
            }
        }
        public double Radious { get { return Math.Sqrt(squaredDistance); } }
        public IDelunaiFacet[] Facets
        {
            get
            {
                //combinations of ndimension+1 sets of nuclei elements
                if (facets == null)
                    facets = Helpers.Combinations(this.Nucleis, Dimensionality).
                                Select(nucs => new DelunaiFacet(nucs)).ToArray();
                return facets;

            }
        }

        /// <summary>
        /// This method calculates the n-dimensional centroid of the hyper-sphere that bounds all his Nucleis.
        /// </summary>
        /// <returns></returns>
        public IVoronoiVertex VoronoiVertex
        {
            get
            {
                if (voroniVertex == null)
                    CalculateSimpliceCentroid();

                return voroniVertex;
            }
        }

        private IDelunaiFacet[] facets;
     
        public int Dimensionality { get { return Nucleis.First().Coordinates.Length ; } }
        private IVoronoiVertex voroniVertex;
        private double squaredDistance;
        

    

        /// <summary>
        /// The number of nodes must be n+1 dimensional where n is the dimensions of the problem
        /// </summary>
        /// <param name="nucleis"></param>
        public Simplice(INuclei[] nucleis)
        {
            this.Nucleis = nucleis;
        }

        internal void RaiseRefreshNeighbours()
        {
            var a = this.Nucleis.SelectMany(p => p.Simplices).Distinct();
            var b = a.Where(simp => simp != this); //vértices de voronoi candidatos
            var neighbours = b.Where(simp => simp.Nucleis.Intersect(this.Nucleis).Count() >= Dimensionality);

            foreach (Simplice neighbour in neighbours)
                neighbour.RaiseNeighbourRefresh();
        }

        private void RaiseNeighbourRefresh()
        {
            this.facets = null;
        }

        /// <summary>
        /// This method asserts if a point in the n-dimensional space is inside or intersecting with the 
        /// circum-hypersphere of this siplice
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool CheckIsInsideHyperSphere(double[] point)
        {
            if (voroniVertex == null)
                CalculateSimpliceCentroid();

            double sum = 0;
            for (int i = 0; i < point.Length; i++)
            {
                double diff=point[i] - this.voroniVertex.Coordinates[i];
                sum += (diff*diff);
            }

            return sum <= squaredDistance;
        }

        /// <summary>
        /// This is a lazy calculation of the voronoi Vertex, its not calculated if it isn't required.
        /// </summary>
        private void CalculateSimpliceCentroid()
        {
            Debug.Print("Calculating Simplice centroid");

            if (Nucleis.Any(n => n.Coordinates.Length != Dimensionality))
                throw new ArgumentException("Incorrect dimensionality in the nucleis. some nucleis have no the right dimensionality");

            Matrix mA = new Matrix(Dimensionality,Dimensionality);
            Matrix mb=new Matrix(Dimensionality,1);
            for (int i = 0; i < Dimensionality; i++)
            {
                mb[i,0]=0;
                for(int j=0;j<Dimensionality;j++)
                {
                    mA[i,j] = Nucleis[0].Coordinates[j] - Nucleis[i+1].Coordinates[j];
                    mb[i,0]+= mA[i,j]*((Nucleis[0].Coordinates[j]+Nucleis[i+1].Coordinates[j])/2.0);
                }

            }

            Matrix result = mA.Solve(mb);
            squaredDistance = 0;
            double[] dataResult=result.GetColumnVector(0).ToArray();
            voroniVertex = new VoronoiVertex(dataResult);
            for (int i = 0; i < dataResult.Length; i++)
            {
                double diff=dataResult[i]-Nucleis.First().Coordinates[i];
                squaredDistance += diff * diff;
            }
            
        }

        public override string ToString()
        {
            return GetHashCode()+"||"+string.Join(", ", Nucleis.Select(nc => nc.ToString()).ToArray());
        }


    }
}