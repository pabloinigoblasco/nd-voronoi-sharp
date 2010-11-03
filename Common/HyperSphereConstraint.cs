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
using MathNet.Numerics.LinearAlgebra;

namespace ndvoronoisharp.Common
{
    class HyperSphereConstraint
    {
        public double Radious;
        public double[] Center;

        public HyperSphereConstraint(int dimensionality)
        {
           Center=new double[dimensionality];
           this.Radious = double.NaN;
        }

        public HyperSphereConstraint(double[] center,double Radious)
        {
            Center = center;
            this.Radious = Radious;
        }

        public bool CircumsphereContains(double[] point)
        {
            double sum = 0;
            for (int i = 0; i < point.Length; i++)
            {
                double diff = point[i] - Center[i];
                sum += (diff * diff);
            }

#warning optimizable
            return sum <= Radious * Radious;
        }

        internal void Calculate(IEnumerable<INuclei> Nucleis,int problemDimensionality, int nucleisCount)
        {
            Helpers.CalculateSimpliceCentroidFromFacets(Nucleis, nucleisCount -1 , ref Center);
            INuclei nuclei = Nucleis.First();

            //now calculate the radious and store it.
            Vector v = new Vector(problemDimensionality);
            for (int i = 0; i<problemDimensionality; i++)
                v[i] = Center[i] - nuclei.Coordinates[i];

            this.Radious = v.Norm();
        }
    }
}
