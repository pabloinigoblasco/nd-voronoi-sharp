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

namespace ndvoronoisharp
{

	/// <summary>
	/// This interface represent a semi-hyperSpace whose bounds are defined by a hyperplane. The normal
	/// vector of the hyperplane defines the owned semi-hyperSpace. 
	/// It is used to define bounds between voronoi regions.
	/// </summary>
	public interface HyperPlaneConstraint
	{
		/// <summary>
		/// Checks if the point belong to the owner hyperplane
		/// </summary>
		bool semiHyperSpaceMatch (double[] point);

		double this[int coordinate] {
			get;
		}

		int EuclideanSpaceDimensionality { get; }
	}

	namespace implementations
	{
		
	/// <summary>
	/// This class represent a hyperplane that subdivides the ndimensional space in two subspaces. 
	/// It is used to define bounds between voronoi regions.
	/// </summary>
	public class DefaultConstraint : HyperPlaneConstraint
	{
		/// <summary>
		/// Example in the plane Ax+By+Cz<D in R3, coefficents would be [A,B,C,D]
		/// </summary>
		private readonly Vector coefficents;

		public double this[int coordinateIndex] {
			get { return coefficents[coordinateIndex]; }
		}

		/// <summary>
		/// Constraint is created as a bound between these two points. A line-bound in 2D case, a Plane in the 3D case and a hyperplane in ND case.
		/// It represents a single inequality that checks if a sample point belong to the positive or negative subspaces.
		/// </summary>
		/// </param>
		public DefaultConstraint (double[] ownerPoint, double[] foreignPoint)
		{
            coefficents = new Vector(ownerPoint.Length + 1) ;
            coefficents[coefficents.Length - 1] = 0;

			//calculating coefficents except the independent coefficent
			for (int i = 0; i < ownerPoint.Length; i++) {
                coefficents[i] = ownerPoint[i] - foreignPoint[i];
                //calculating the independent coefficent
                coefficents[coefficents.Length - 1] += coefficents[i] * ((foreignPoint[i] + ownerPoint[i]) / 2f);
			}
			
			
			
		}

        /// <summary>
        /// This is a direct constructor where coefficents are setted directly.
        /// </summary>
        /// <remarks>
        /// Remember that a nd constraint has a nd+1 coefficents taking into account the independent term.
        /// </remarks>
        /// <param name="coefficents"></param>
        internal DefaultConstraint(double[] coefficents)
        {
            this.coefficents = new Vector(coefficents);
        }

		/// <summary>
		/// Checks if the point belong to the owner hyperplane
		/// </summary>
		public bool semiHyperSpaceMatch (double[] point)
		{
			//here should be a verification for the dimensionality. But we're simplifying and looking for efficency.
			double res = Enumerable.Range (0, point.Length).Sum (i => point[i] * coefficents[i]);
			
			return res > coefficents.Last();
		}


		public int EuclideanSpaceDimensionality {
			get { return coefficents.Length - 1; }
		}
	}

	
		/// <summary>
		/// This decorator is useful to view a constraint from the inverse point of view in terms of regions.
		/// It is not nice waste memmory in n-dimensional spaces.
		/// </summary>
		public class InverseConstraintDecorator : HyperPlaneConstraint
		{
			readonly HyperPlaneConstraint decorated;
			public InverseConstraintDecorator (HyperPlaneConstraint constraint)
			{
				if (constraint == null || !(constraint is DefaultConstraint))
					throw new ArgumentException ("invalid constraint.");
				
				decorated = constraint;
			}

			public bool semiHyperSpaceMatch (double[] point)
			{
				return !decorated.semiHyperSpaceMatch (point);
			}

			public double this[int coordinate] {
				get { return -decorated[coordinate]; }
			}
			public int EuclideanSpaceDimensionality {
				get { return decorated.EuclideanSpaceDimensionality; }
			}
			
		}
	}
}
