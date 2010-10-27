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
    public class DelunaiFacet : ndvoronoisharp.IDelunaiFacet
    {
        public ISimplice ParentA { get; private set; }
        public ISimplice ParentB{get; private set;} 
        public bool IsBoundingFacet
        {
            get { return ParentB == null; }
        }

        private void calculateParents(IEnumerable<INuclei> Nucleis)
        {
            IEnumerable<ISimplice> parents=Nucleis.First().Simplices;
            foreach (var n in Nucleis.Skip(1))
                parents = parents.Intersect(n.Simplices);

            var en = parents.GetEnumerator();
            if (en.MoveNext())
                ParentA = en.Current;

            if(en.MoveNext())
                ParentB = en.Current;
        }

        public readonly INuclei[] vertexes;
        public INuclei[] Vertexes { get { return vertexes; } }

        public DelunaiFacet(IEnumerable<INuclei> nucleis)
        {
            ParentA = null;
            ParentB = null;
            this.vertexes=nucleis.ToArray();
            calculateParents(this.vertexes);
        }
        
    }
}
