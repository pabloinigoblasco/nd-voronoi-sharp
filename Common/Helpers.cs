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
using ndvoronoisharp;
using MathNet.Numerics.LinearAlgebra;

namespace ndvoronoisharp.Common
{
    internal static class Helpers
    {
        internal static IEnumerable<IEnumerable<T>> Combinations<T>(IEnumerable<T> elements, int setLenght)
        {
            int elementLenght = elements.Count();
            if (setLenght == 1)
                return elements.Select(e => Enumerable.Repeat(e, 1));
            else if (setLenght == elementLenght)
                return Enumerable.Repeat(elements, 1);
            else
            {
                return Combinations(elements.Skip(1), setLenght - 1)
                                .Select(tail => Enumerable.Repeat(elements.First(), 1).Union(tail))
                                .Union(Combinations(elements.Skip(1), setLenght));
            }
        }

        /// <summary>
        /// This is a lazy calculation of the voronoi Vertex, its not calculated if it isn't required.
        /// </summary>
        internal static void CalculateSimpliceCentroid(INuclei[] Nucleis, ref double[] vectorOut)
        {
            int Dimensionality = Nucleis.First().Coordinates.Length;

            Matrix mA = new Matrix(Dimensionality, Dimensionality);
            Matrix mb = new Matrix(Dimensionality, 1);
            for (int i = 0; i < Dimensionality; i++)
            {
                mb[i, 0] = 0;
                for (int j = 0; j < Dimensionality; j++)
                {
                    mA[i, j] = Nucleis[0].Coordinates[j] - Nucleis[i + 1].Coordinates[j];
                    mb[i, 0] += mA[i, j] * ((Nucleis[0].Coordinates[j] + Nucleis[i + 1].Coordinates[j]) / 2.0);
                }

            }

            Matrix result = mA.Solve(mb);
            for (int i = 0; i < vectorOut.Length; i++)
                vectorOut[i] = result[0, 1];
        }
        /// <summary>
        /// This is a lazy calculation of the voronoi Vertex, its not calculated if it isn't required.
        /// </summary>
        internal static void CalculateSimpliceCentroidFromFacets(INuclei[] Nucleis, ref double[] vectorOut)
        {
            IVoronoiFacet[] voronoiFacets = Nucleis[0].VoronoiHyperRegion.Facets.Where(f => Nucleis.Contains(f.External)).ToArray();

            int Dimensionality = Nucleis.First().Coordinates.Length;
            if (Dimensionality == voronoiFacets.Length) //SVD
                CalculateSimpliceCentroid(Nucleis, ref vectorOut);
            else
            {
                int SimpliceDimensions= Nucleis.Length - 1;
                int Dof = Dimensionality - SimpliceDimensions;
                //we have facets.Length restrictions, and we have to create Dof new restrictions.


                //we have to solve a facets.Length problem, to get the parameters of the problem.
                //ie: two facets, two ecuations. constraint with the current space formed by nucleiVectors with 2 parameters (unknowns)
                //this is a two ecuations/two unknowns problem.
                if (SimpliceDimensions == 1)
                {
                    IVoronoiFacet hpConstraint = voronoiFacets[0];
                    double tCoeff = 0;
                    double independentTerm = hpConstraint[Dimensionality ];
                    Vector[] vectors = VectorsFromPoints(Nucleis);
                    for (int i = 0; i < Dimensionality; i++)
                    {
                        tCoeff += hpConstraint[i] * vectors[0][i];
                        independentTerm += hpConstraint[i] * Nucleis[0].Coordinates[i];
                    }
                    double t =(-independentTerm) / tCoeff;

                    //solve the system
                    for (int i = 0; i < Dimensionality; i++)
                    {
                        vectorOut[i] = Nucleis[0].Coordinates[i] + t * vectors[0][i];
                    }
                }
                else
                {
                    //parameters matrix
                    Matrix mA = new Matrix(voronoiFacets.Length, voronoiFacets.Length);
                    Vector[] vectors = VectorsFromPoints(Nucleis);
                    Matrix mb = new Matrix(voronoiFacets.Length, 1);

                    //mounting parameters matrix
                    for (int row = 0; row < voronoiFacets.Length; row++)
                    {
                        IVoronoiFacet hpConstraint = voronoiFacets[row];
                        double independentTerm = hpConstraint[Dimensionality ];
                        for (int col = 0; col < voronoiFacets.Length; col++)
                        {
                            double tCoeff_col = 0;
                            for (int j = 0; j < Dimensionality; j++)
                            {
                                tCoeff_col += hpConstraint[j] * vectors[col][j];
                                independentTerm += hpConstraint[j] * Nucleis[0].Coordinates[j];
                            }
                            mA[row, col] = tCoeff_col;
                        }
                        mb[row, 0] = independentTerm;
                    }

                    //solving parameters matrix
                    Matrix parametersRes=mA.Solve(mb);
                    for (int i = 0; i < vectorOut.Length; i++)
                    {
                        double increment=0;
                        for (int j = 0; j < voronoiFacets.Length; j++)
                            increment=parametersRes[j,0]*vectors[j][i];

                        vectorOut[i] = Nucleis[0].Coordinates[i] + increment;
                    }

                }
            }
        }



        internal static int CalculatePointsRank(IEnumerable<INuclei> points)
        {
            INuclei first = points.First();
            Matrix vectors = new Matrix(points.Count() - 1, first.Coordinates.Length);
            IEnumerator<INuclei> currPoint=points.GetEnumerator();
            currPoint.MoveNext();
            
            for (int i = 0; i < vectors.RowCount && currPoint.MoveNext(); i++)
            {
                for (int j = 0; j < vectors.ColumnCount; j++)
                {
                    vectors[i, j] = first.Coordinates[j] - currPoint.Current.Coordinates[j];
                }
            }

            return vectors.Rank();
        }

        internal static Vector[] VectorsFromPoints(INuclei[] points)
        {
            Vector[] vectors = new Vector[points.Length - 1];
            int dimensionality = points[0].Coordinates.Length;
            for (int i = 0; i < vectors.Length; i++)
            {
                vectors[i] = new Vector(dimensionality);
                for (int j = 0; j < points[0].Coordinates.Length; j++)
                    vectors[i][j] = points[i + 1].Coordinates[j] - points[0].Coordinates[j];

            }
            return vectors;
        }
    }
}