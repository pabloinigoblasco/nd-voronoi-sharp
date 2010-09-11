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
using ndvoronoisharp.implementations;

namespace ndvoronoisharp
{
	public class SubSpace
	{
        public static SubSpace FromPoint(params double[] coordinates)
        {
            //foreach coordinate, one constraint x[i]=coordinate[y], and the term??
            Constraint[] constraints=new Constraint[coordinates.Length];
            for (int i = 0; i < coordinates.Length; i++)
            {
                double[] constraintCoeffs=new double[coordinates.Length];
                constraintCoeffs[i] = coordinates[i];
                constraints[i] = new DefaultConstraint(constraintCoeffs);
            }
            return new SubSpace(constraints);
        }

        public readonly List<Constraint> constraints;
		

		private SubSpace (IEnumerable<Constraint> constraints)
		{
			this.constraints=new List<Constraint>(constraints);

		}
		
		public double[] CalculateSolution()
		{
			//if inconsistent o consistent-indeterminated
			throw new NotSupportedException();
		}
		
		/// <summary>
		/// Calculates if this subspaces has any intersection with thie hyperplane that represents this constraint
		/// </summary>
		/// <returns>
		/// The intersection Subspace 
		///</returns>	
		public SubSpace Intersects(SubSpace subspace)
		{
			//TODO:firstly find if it represent a inconsistent problem
			bool inconsistentSystem=false;
			DotNumerics.LinearAlgebra.CSLapack.l

			if(inconsistentSystem)
			{
				//If so, both subspaces (the hyperplane Constraint and this subspace) are pararell, and that means that it can verifies the constraint or not
				//The question is ¿this pararell subspace verifies the constraint?
				//TODO: just check if any point of the subspace verifies it.
				
				throw new NotImplementedException();
			}
			else
			{
				//Just make the intersection subspace...
				//but where will be the semi-hyperSpace Constraints??
				
				throw new NotImplementedException();
			}
			
			return null;
		}
		
		/// <summary>
		/// Degree of freedom of the subspace
		/// </summary>
		public int DOF
		{
			get
			{
				return constraints[0].EuclideanSpaceDimensionality -constraints.Count;
			}
		}

        public static SubSpace FromConstraints(params Constraint[] constraints)
        {
            return new SubSpace(constraints);
        }
    }
}

