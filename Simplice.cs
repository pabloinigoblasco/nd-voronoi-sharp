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
        public int Dimensionality { get { return Nucleis.First().coordinates.Length - 1; } }

        private SimpliceCentroid voroniVertex;
        private double squaredDistance;

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
                sum += point[i] * point[i];

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
            Matrix mb=new Matrix(Nucleis.Length,1);
            int cont=1;
            for (int i = 0; i < Nucleis.Length; i++,cont++)
            {
                mb[i,0]=0;
                for(int j=0;j<Nucleis.Length;j++)
                {
                    mA[i,j] = Nucleis[0].coordinates[j] - Nucleis[cont].coordinates[j];
                    mb[i,0]+= mA[i,j]*((Nucleis[0].coordinates[j]+Nucleis[cont].coordinates[j])/2.0);
                }

            }

            Matrix result = mA.Solve(mb);
            squaredDistance = 0;
            double[] dataResult=result.GetColumnVector(0).ToArray();
            voroniVertex = new SimpliceCentroid(dataResult);
            for (int i = 0; i < dataResult.Length; i++)
                squaredDistance += dataResult[i] * dataResult[i];
            
        }
    }
}