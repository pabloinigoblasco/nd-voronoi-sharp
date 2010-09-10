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

namespace ndvoronoisharp
{
	/// <summary>
	/// This class represent a n-dimensional voronoi region
	/// </summary>
	public class Region
	{
		public double[] Center{get; private set;}
		internal Dictionary<Constraint,Region> constraints;
		internal Dictionary<BoundingVertex,IEnumerable<Region>> vertexes;
		public IEnumerable<BoundingVertex> Vertexes{get{return vertexes.Keys;}}
		
		public IEnumerable<Constraint> Constraints{get{return constraints.Keys;}}
		public IEnumerable<Region> NeighbourgRegions{get{return constraints.Values;}}
		
		
		
		/// <summary>
		/// constructor visibility is restricted to assert dimensionality coherence 
		/// </summary>
		internal Region (double[] center)
		{
			this.Center=center;
		}
		
		public void CalculateVertexes()
		{
			
			
			
			
		}
	}
}
