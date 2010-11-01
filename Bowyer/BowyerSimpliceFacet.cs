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
using MathNet.Numerics.LinearAlgebra;
using ndvoronoisharp.Common;

namespace ndvoronoisharp.Bowyer
{
    public class BowyerSimpliceFacet:ISimpliceFacet
    {
        //dimensions
        public BowyerSimpliceFacet(INuclei[] nucleis, ISimplice parentA)
        {
            this.Vertexes = nucleis;
            this.ParentA=parentA;
        }

        public bool IsConvexHullFacet
        {
            get 
            {
                return ParentA == null || ParentB == null;
            }
        }

        HyperPlaneConstraint constraint;

        public ISimplice ParentA { get; private set;}
        private ISimplice parentB;

#warning this can get obsolete
        public ISimplice ParentB { get 
        {
            if(parentB==null)
                return ParentA.NeighbourSimplices.SingleOrDefault(neigh => this.Vertexes.All(v => neigh.Nucleis.Contains(v)));
            return parentB;
        }
        }

        public INuclei[] Vertexes { get; private set;}
        public bool semiHyperSpaceMatch(double[] point)
        {
            if (constraint == null)
            {
                if (Vertexes[0].Coordinates.Length == 2)
                {
                    double[] normalVector = new double[] { Vertexes[0].Coordinates[0] };
                }
                else
                {

                    //calculating the normal vector with a n-dimensional cross-product

                    Vector firstVector = new Vector(point.Length);
                    Vector secondVector = new Vector(point.Length);
                    for (int i = 0; i < Vertexes[0].Coordinates.Length; i++)
                    {
                        firstVector[0] = Vertexes[0].Coordinates[i] - Vertexes[1].Coordinates[i];
                        secondVector[0] = Vertexes[0].Coordinates[i] - Vertexes[2].Coordinates[i];
                    }


#warning the direction of the generated normal vector can be whatever.. uncontrolled
                    Vector totalCrossProduct = Vector.CrossProduct(firstVector, secondVector);
                    for (int i = 3; i < Vertexes[0].Coordinates.Length; i++)
                    {
                        Vector currentVector = new Vector(point.Length);
                        for (int j = 0; j < point.Length; j++)
                            currentVector[j] = Vertexes[0].Coordinates[j] - Vertexes[i].Coordinates[j];

                        totalCrossProduct = Vector.CrossProduct(totalCrossProduct, currentVector);
                    }

                    double[] coefficents = new double[point.Length + 1];

                    //using one arbitrary point for the independent term
                    for (int i = 0; i < Vertexes[0].Coordinates.Length; i++)
                    {
                        coefficents[0] += totalCrossProduct[i] * Vertexes[0].Coordinates[i];
                        coefficents[i] = totalCrossProduct[i];
                    }

                    this.constraint = new HyperPlaneConstraint(coefficents);

                    if (this.constraint.semiHyperSpaceMatch(parentB.VoronoiVertex.Coordinates))
                        this.constraint.Inverse();
                }
            }
            return constraint.semiHyperSpaceMatch(point); 
        }
    }
}
