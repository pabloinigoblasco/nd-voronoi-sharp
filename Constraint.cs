/*Copyright (C) 2010  Pablo Iñigo Blasco

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
namespace ndvoronoisharp
{
	public class Constraint
	{
		/// <summary>
		/// Example in the plane Ax+By+Cz<D, coefficents would be [A,B,C,D]
		/// </summary>
		public readonly double[] coefficents;

		/// <summary>
		/// Constraint is created as a bound between these two points. A line-bound in 2D case, a Plane in the 3D case and a hyperplane in ND case.
		/// It represents a single inequality that checks if a sample point belong to the positive or negative subspaces.
		/// </summary>
		/// </param>
		public Constraint (double[] ownerPoint, double[] foreignPoint)
		{
			double[] middlePoint = new double[ownerPoint.Length];
			coefficents = new double[ownerPoint.Length + 1];
			
			//calculating coefficents except the independent coefficent
			for (int i = 0; i < ownerPoint.Length; i++) {
				middlePoint[i] = (foreignPoint[i] + ownerPoint[i]) / 2f;
				coefficents[i] = foreignPoint[i] - ownerPoint[i];
			}
			
			//calculating the independent coefficent
			coefficents[coefficents.Length - 1] = Enumerable.Range (0, coefficents.Length - 1).Sum (i => middlePoint[i] * coefficents[i]);
			
		}

		/// <summary>
		/// Checks if the point belong to the owner hyperplane
		/// </summary>
		public bool Verifies (double[] point)
		{
			//here should be a verification for the dimensionality. But we're simplifying and looking for efficency.
			double res = Enumerable.Range (0, point.Length).Sum (i=> point[i] * coefficents[i]);
			
			return res > coefficents[coefficents.Length - 1];
		}
	}
}
