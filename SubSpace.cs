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
using System.Collections.Generic;

namespace ndvoronoisharp
{
	public class SubSpace
	{
		public readonly List<Constraint> constraints;
		
		public SubSpace (IEnumerable<Constraint> constraints)
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
		public SubSpace Verifies(SubSpace subspace)
		{
			//TODO:firstly find if it represent a inconsistent problem
			bool inconsistentSystem=false;
			
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
	}
}
