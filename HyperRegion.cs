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
using System.Linq;
using DotNumerics;
using DotNumerics.LinearAlgebra;

namespace ndvoronoisharp
{
	/// <summary>
	/// This class represent a n-dimensional voronoi region
	/// </summary>
	public class HyperRegion
	{
		private int ProblemDimensionality{get{return Center.Length;}}
		public double[] Center{get; private set;}
		
		internal Dictionary<Constraint,HyperRegion> constraints;
		internal Dictionary<SubSpace,IEnumerable<HyperRegion>> subspaces;
		public IEnumerable<SubSpace> Subspaces{get{return subspaces.Keys;}}
		
		public IEnumerable<Constraint> Constraints{get{return constraints.Keys;}}
		public IEnumerable<HyperRegion> NeighbourgRegions{get{return constraints.Values;}}
		
		/// <summary>
		/// constructor visibility is restricted to assert dimensionality coherence 
		/// </summary>
		internal HyperRegion (double[] center)
		{
			this.Center=center;
		}
		
		public void CalculateSubspaces()
		{
			
			/*List<Constraint[]> subspacesCandidates=new List<Constraint[]>();
			subspacesCandidates.RemoveAll(tuples=>tuples.All(c=>this.Subspaces.Contains(c)));
			
			foreach(var vertexCandidateConstraints in subspacesCandidates)
			{
				
				Matrix A = new Matrix(ProblemDimensionality,ProblemDimensionality);
				LinearEquations leq = new LinearEquations();
				for(int i=0;i<ProblemDimensionality;i++)
				{
					Constraint c=vertexCandidateConstraints[i];
					for(int j=0;j<ProblemDimensionality;j++)
						A[i,j]=c.coefficents[j];
				}
				
				Matrix B = new Matrix(ProblemDimensionality, 1);
				for(int i=0;i<ProblemDimensionality;i++)
				{
					Constraint c=vertexCandidateConstraints[i];
					B[i,0]=c.coefficents[ProblemDimensionality];
				}
				
				
				//vertex info
            	Matrix X = leq.Solve(A, B);
				
			}*/
			
			
		}
		private void Permutations(int permutationSize, IEnumerable<Constraint> ConstraintBag, Constraint[]CurrentVertexConstraints,List<Constraint[]> constraintsPermutations)
		{
			if(permutationSize==0)
			{
				constraintsPermutations.Add(CurrentVertexConstraints);
				return;
			}
			else
			{
				//first time
				if(CurrentVertexConstraints==null)
					//to get a vertex we need n constraints where n==dimensionality of the problem
					CurrentVertexConstraints=new Constraint[this.Center.Length];
				   
				foreach (var constr in ConstraintBag)
				{
					CurrentVertexConstraints[permutationSize-1]=constr;
					Permutations(permutationSize-1,ConstraintBag.Except(Enumerable.Repeat(constr,1)),CurrentVertexConstraints,constraintsPermutations);
				}
				
			}
		}
	}
}
