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
    public class HyperPlaneConstraint
    {
        /// <summary>
        /// Example in the plane Ax+By+Cz<D in R3, coefficents would be [A,B,C,D]
        /// </summary>
        public readonly Vector coefficents;
        bool inverse;

        public HyperPlaneConstraint(double[] coefficents)
        {
            // TODO: Complete member initialization
            this.coefficents = new Vector(coefficents);
        }
        /// <summary>
        /// Checks if the point belong to the owner hyperplane
        /// </summary>
        public bool semiHyperSpaceMatch(double[] point)
        {
            //here should be a verification for the dimensionality. But we're simplifying and looking for efficency.
            double res = Enumerable.Range(0, point.Length).Sum(i => point[i] * coefficents[i]);

            if (inverse)
                return res < - coefficents.Last();
            else
                return res > - coefficents.Last();
        }


        public int SpaceDimesionality
        {
            get { return coefficents.Length - 1; }
        }

        internal void Inverse()
        {
            this.inverse = !this.inverse;
        }
    }
}
