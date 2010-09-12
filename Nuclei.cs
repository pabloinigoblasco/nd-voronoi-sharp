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

namespace ndvoronoisharp
{
    /// <summary>
    /// This is a nuclei in the voronoi Map. They are the center of the Voronoi Regions and the nodes
    /// in the delunai Graph.
    /// </summary>
    /// <remarks>
    /// As it is one of the most stable objects, a lot of data has been set here like lists, etc. 
    /// </remarks>
    public class Nuclei
    {
        public bool IsDelunaiBound { get; internal set; }
        public HyperRegion VoronoiHyperRegion { get; private set; }
        internal List<Nuclei> nucleiNeigbourgs { get; private set; }
        public IEnumerable<Nuclei> NucleiNeigbourgs { get { return nucleiNeigbourgs; } }

        internal List<Simplice> simplices;
        internal double[] coordinates { get; private set; }
        internal Nuclei(double[] coordinates,HyperRegion thisRegion)
        {
            this.coordinates = coordinates;
            this.VoronoiHyperRegion = thisRegion;
            this.simplices = new List<Simplice>();
            this.nucleiNeigbourgs = new List<Nuclei>();
        }
    }
}
