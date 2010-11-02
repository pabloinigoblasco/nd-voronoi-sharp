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
    class BowyerSimpliceFacet : ISimpliceFacet
    {
        //dimensions
        public BowyerSimpliceFacet(IVoronoiVertex owner, IVoronoiVertex external, IEnumerable<BowyerNuclei> nucleis)
        {
            this.Owner = owner;
            this.External = external;
            this.nucleis = nucleis.ToArray();

            if (owner == null)
                throw new ArgumentException("it's needed at least a parent voronoi vertex for a simplice facet");

            if (External != null)
                CalculateConstraint();


        }

        public int IsConvexHullFacet
        {
            get
            {
                if (!FullyInitialized)
                    throw new NotSupportedException("Facet no properly initializated. Define the external Voronoi vertex to complete it.");

                if (Owner.Infinity)
                {
                    return 2;
                }
                else
                {
                    if (External.Infinity)
                        return 1;
                    else return 0;
                }
            }
        }

        HyperPlaneConstraint constraint;

        public IVoronoiVertex Owner { get; private set; }
        private IVoronoiVertex external;
        public IVoronoiVertex External
        {
            get { return external; }
            internal set
            {
                external = value;

            }
        }

        public bool FullyInitialized
        {
            get
            {
                return External != null;
            }
        }

        public readonly BowyerNuclei[] nucleis;
        public INuclei[] Nucleis { get { return nucleis; } }
        public bool semiHyperSpaceMatch(double[] point)
        {
            if (!FullyInitialized)
                throw new NotSupportedException("Facet no properly initializated. Define the external Voronoi vertex to complete it.");

            if (constraint == null)
                CalculateConstraint();



            return constraint.semiHyperSpaceMatch(point);
        }

        private void CalculateConstraint()
        {
            //three possible cases, both finites, owner finite/external infnite and viceversa

            if (!Owner.Infinity && !external.Infinity)
            {
                double[] ownerPoint = Owner.Coordinates;
                double[] foreignPoint = external.Coordinates;

                double[] coefficents = new Vector(ownerPoint.Length + 1);
                coefficents[coefficents.Length - 1] = 0;

                //calculating coefficents except the independent coefficent
                for (int i = 0; i < ownerPoint.Length; i++)
                {
                    coefficents[i] = ownerPoint[i] - foreignPoint[i];
                    //calculating the independent coefficent
                    coefficents[coefficents.Length - 1] -= coefficents[i] * ((foreignPoint[i] + ownerPoint[i]) / 2f);
                }
                this.constraint = new HyperPlaneConstraint(coefficents);
            }
            else if (External.Infinity && Owner.Infinity)
            {
                INuclei[] n = External.Simplice.Nucleis.Intersect(Owner.Simplice.Nucleis).ToArray();
                if (n.Length != 2)
                    throw new NotSupportedException();

                IVoronoiFacet vf = n[0].VoronoiHyperRegion.Facets.Single(f => f.External == n[1]);
                double[] coefficents = new Vector(Owner.Coordinates.Length + 1);
                for (int i = 0; i < Owner.Coordinates.Length; i++)
                {
                    coefficents[i] = vf[i];
                }
                this.constraint = new HyperPlaneConstraint(coefficents);

            }
            else if (External.Infinity)
            {
                if (Owner.Simplice.Nucleis.Length > 2)
                {
                    double[] middlePoint = new double[Nucleis[0].Coordinates.Length];
                    Helpers.CalculateSimpliceCentroidFromFacets(Nucleis, ref middlePoint);

                    Vector normal = new Vector(Nucleis[0].Coordinates.Length + 1);
                    double independentTerm = 0;
                    for (int i = 0; i < Nucleis[0].Coordinates.Length; i++)
                    {
                        normal[i] = Owner.Coordinates[i] - middlePoint[i];
                        independentTerm -= normal[i] * middlePoint[i];
                    }
                    normal[normal.Length - 1] = independentTerm;
                    this.constraint = new HyperPlaneConstraint(normal.ToArray());
                }
                else
                {
                    //only two nucleis...is this enough general for n-dimensions?
                    //hope this is only the case base, where a voronoiVertex overlaps a voronoiFacet
                    INuclei n = External.Simplice.Nucleis.Intersect(Owner.Simplice.Nucleis).Single();

                    Vector normal = new Vector(Nucleis[0].Coordinates.Length + 1);
                    double independentTerm = 0;
                    for (int i = 0; i < Nucleis[0].Coordinates.Length; i++)
                    {
                        normal[i] = Owner.Coordinates[i] - n.Coordinates[i];
                        independentTerm -= normal[i] * n.Coordinates[i];
                    }
                    normal[normal.Length - 1] = independentTerm;
                    this.constraint = new HyperPlaneConstraint(normal.ToArray());
                }
            }
            else if (Owner.Infinity)
            {
                if (External.Simplice.Nucleis.Length > 2)
                {
                    double[] middlePoint = new double[Nucleis[0].Coordinates.Length];
                    Helpers.CalculateSimpliceCentroidFromFacets(Nucleis, ref middlePoint);

                    Vector normal = new Vector(Nucleis[0].Coordinates.Length + 1);
                    double independentTerm = 0;
                    for (int i = 0; i < Nucleis[0].Coordinates.Length; i++)
                    {
                        normal[i] = middlePoint[i] - External.Coordinates[i];
                        independentTerm -= normal[i] * middlePoint[i];
                    }
                    normal[normal.Length - 1] = independentTerm;
                    this.constraint = new HyperPlaneConstraint(normal.ToArray());
                }
                else
                {
                    //only two nucleis...is this enough general for n-dimensions?
                    //hope this is only the case base, where a voronoiVertex overlaps a voronoiFacet
                    INuclei n = External.Simplice.Nucleis.Intersect(Owner.Simplice.Nucleis).Single();

                    Vector normal = new Vector(Nucleis[0].Coordinates.Length + 1);
                    double independentTerm = 0;
                    for (int i = 0; i < Nucleis[0].Coordinates.Length; i++)
                    {
                        normal[i] = n.Coordinates[i] - External.Coordinates[i];
                        independentTerm -= normal[i] * n.Coordinates[i];
                    }
                    normal[normal.Length - 1] = independentTerm;
                    this.constraint = new HyperPlaneConstraint(normal.ToArray());
                }
            }
            else //both infinities
                throw new NotFiniteNumberException();

        }


        public double this[int coefficentIndex]
        {
            get
            {
                if (!FullyInitialized)
                    throw new NotSupportedException("Facet no properly initializated. Define the external Voronoi vertex to complete it.");

                if (constraint == null)
                    CalculateConstraint();

                return constraint.coefficents[coefficentIndex];
            }
        }
    }
}
