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
    public class Simplice
    {
        public Nuclei[] Nucleis { get; private set; }
        public IEnumerable<Simplice> NeighbourSimplices 
        {
            get 
            {
                var a=this.Nucleis.SelectMany(p => p.NucleiSimplices).Distinct();
                var b = a.Where(simp => simp != this); //vértices de voronoi candidatos
                var c= b.Where(simp => simp.Nucleis.Intersect(this.Nucleis).Count()>=Dimensionality);

                return c;
            }
        }

        public IEnumerable<Facet> Facets
        {
            get 
            { 
                //combinations of ndimension+1 sets of nuclei elements
                throw new NotImplementedException();

            }
        }


        /// <summary>
        /// Returns the set of nucleis that forms the infinite Neighbour área
        /// </summary>
        public Nuclei[] InfiniteNeighbourVoronoiVertexes
        {
            get 
            {
                if (NeighbourSimplices.Count() == Dimensionality + 1) //maxium numbers of facets
                    return null;
                else
                {
                    //can only they have one infinite Neighbour?

                    //foreach nuclei pair in the Simplice nucleis
                    //if they don't share another simplice means that they are
                    //delunay-graph hull vertexes and then contains a infinite voronoi vertex

                    List<Nuclei> notSharing=new List<Nuclei>();
                    foreach (var n1 in Nucleis)
                    {
                        foreach (var n2 in n1.nucleiNeigbourgs)
                        {
                            if (!n1.simplices.Except(Enumerable.Repeat(this,1)).Intersect(n2.simplices).Any())
                            {
                                notSharing.Add(n2);
                                notSharing.Add(n1);
                            }
                        }
                    }
                    return notSharing.Distinct().ToArray();
                      
                }
            }
        }

        public int Dimensionality { get { return Nucleis.First().coordinates.Length ; } }

        private SimpliceCentroid voroniVertex;
        private double squaredDistance;
        public double Radious { get { return Math.Sqrt(squaredDistance); } }

        /// <summary>
        /// This method calculates the n-dimensional centroid of the hyper-sphere that bounds all his Nucleis.
        /// </summary>
        /// <returns></returns>
        public SimpliceCentroid VoronoiVertex
        {
            get 
            {
                if (voroniVertex == null)
                    CalculateSimpliceCentroid();

                return voroniVertex;
            }
        }

        /// <summary>
        /// The number of nodes must be n+1 dimensional where n is the dimensions of the problem
        /// </summary>
        /// <param name="nucleis"></param>
        public Simplice(Nuclei[] nucleis)
        {
            this.Nucleis = nucleis;
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
                double diff=point[i] - this.voroniVertex.coordinates[i];
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

            if (Nucleis.Any(n => n.coordinates.Length != Dimensionality))
                throw new ArgumentException("Incorrect dimensionality in the nucleis. some nucleis have no the right dimensionality");

            Matrix mA = new Matrix(Dimensionality,Dimensionality);
            Matrix mb=new Matrix(Dimensionality,1);
            for (int i = 0; i < Dimensionality; i++)
            {
                mb[i,0]=0;
                for(int j=0;j<Dimensionality;j++)
                {
                    mA[i,j] = Nucleis[0].coordinates[j] - Nucleis[i+1].coordinates[j];
                    mb[i,0]+= mA[i,j]*((Nucleis[0].coordinates[j]+Nucleis[i+1].coordinates[j])/2.0);
                }

            }

            Matrix result = mA.Solve(mb);
            squaredDistance = 0;
            double[] dataResult=result.GetColumnVector(0).ToArray();
            voroniVertex = new SimpliceCentroid(dataResult);
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