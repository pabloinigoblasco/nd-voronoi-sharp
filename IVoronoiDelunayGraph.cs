﻿/*  Copyright (C) 2010  Pablo Iñigo Blasco. 
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
namespace ndvoronoisharp
{
    public interface IVoronoiDelunayGraph
    {
        IVoronoiRegion AddNewPoint(object data, double[] newPoint);
        IVoronoiRegion AddNewPoint(double[] newPoint);
        IEnumerable<INuclei> Nucleis { get; }

        /// <summary>
        /// All existing simplices in the diagram. This not include infinite simplices (simplices of infinite voronoi vertexes)
        /// </summary>
        IEnumerable<ISimplice> Simplices { get; }
        IEnumerable<IVoronoiRegion> VoronoiRegions { get; }
        IEnumerable<IVoronoiVertex> VoronoiVertexes { get; }
        int ProblemDimensionality { get; }
        int Rank { get; }
        IVoronoiRegion GetMatchingRegion(double[] point);

        IEnumerable<ISimpliceFacet> GetFacetOrNull(INuclei n1, INuclei n2);
    }
}
