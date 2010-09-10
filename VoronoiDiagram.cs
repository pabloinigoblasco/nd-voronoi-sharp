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
	public class VoronoiDiagram
	{
		IEnumerable<Region> Regions {
			get { return regions; }
		}
		List<Region> regions;

		public readonly double dimensions;

		public VoronoiDiagram (int dimensions)
		{
			this.dimensions = dimensions;
			regions = new List<Region> ();
		}

		/// <summary>
		/// Adds a new point to the diagram and returns the generated region.
		/// </summary>
		/// <param name="newPoint">
		/// point coordinates. Dimensions must match.
		/// A <see cref="System.Double[]"/>
		/// </param>
		/// <returns>
		/// generated region that represent the set of pooints that has newPoint as the nearest neigbourgh.
		/// A <see cref="Region"/>
		/// </returns>
		public Region AddNewPoint (double[] newPoint)
		{
			if (newPoint == null || newPoint.Length != dimensions)
				throw new ArgumentException ("point added null or has invalid dimensionality");
			
			Region matchingRegion = this.GetMatchingRegion (newPoint);
			
			Region newRegion = new Region (newPoint);
			this.regions.Add (newRegion);
			
			if (matchingRegion != null) {
				//build the new bound between the matching region centre and the new point
				DefaultConstraint newBoundConstraint = new DefaultConstraint (newRegion.Center, matchingRegion.Center);
				newRegion.constraints.Add (newBoundConstraint, matchingRegion);
				
				//adding the inverse constraint to the maching region.
				matchingRegion.CalculateVertexes (new InverseConstraintDecorator(newBoundConstraint),newRegion);
				
				//getting affected regions and their affected vertexes.
				var affectedRegions=matchingRegion.NeighbourgRegions
												  .Select(nr=> new{region=nr,
															   affectedVertexes=nr.Vertexes.Where(v=>newBoundConstraint.Verifies(v.point))})
																.Where(tuple=>tuple.affectedVertexes.Any());
				
				foreach(var affectedRegionInfo in affectedRegions)
				{
					Region affectedRegion=affectedRegionInfo.region;
					DefaultConstraint colateralBound=new DefaultConstraint(newRegion.Center,affectedRegion.Center);
					newRegion.constraints.Add(colateralBound,affectedRegion);
					
					affectedRegion.constraints.Add(new InverseConstraintDecorator(colateralBound),affectedRegion);
					foreach(var removingVertex in affectedRegionInfo.affectedVertexes)
						affectedRegion.vertexes.Remove(removingVertex);
					
					affectedRegion.CalculateVertexes();
				}
				
				newRegion.CalculateVertexes();                                                   
			}
			
			return newRegion;
		}

		/// <summary>
		/// Look up the region that match point
		/// </summary>
		/// <param name="point">
		/// point that will be checked
		/// A <see cref="System.Double[]"/>
		/// </param>
		/// <returns>
		/// Region that contains point.
		/// A <see cref="Region"/>
		/// </returns>
		public Region GetMatchingRegion (double[] point)
		{
			if (point == null || point.Length != dimensions) {
				throw new ArgumentException ("point added null or has invalid dimensionality");
			}
			
						/*This will be a very first approach as a not very efficent algorithm */


			if (!regions.Any ())
			{
				return null; 
			}
			else if (regions.Count () == 1)
			{
				return regions.Single ();
			}
			else {
				/*candidate region */				
				Region r = regions.First ();
				
				bool matchAllConstraints = false;
				while (!matchAllConstraints) {
					matchAllConstraints = false;
					foreach (var constraintInfo in r.constraints) {
						var constraint = constraintInfo.Key;
						var foreingRegion = constraintInfo.Value;
						
						if (!constraint.Verifies (point))
							r = foreingRegion;
					}
					matchAllConstraints = true;
				}
				
				return r;
			}
		}
	}
}
