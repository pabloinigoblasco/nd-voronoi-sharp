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
        internal static void CalculateSimpliceCentroid(IEnumerable<double[]> Points, ref double[] vectorOut)
        {

            IEnumerator<double[]> currentPoint = Points.GetEnumerator();
            currentPoint.MoveNext();
            double[] firstPoint = currentPoint.Current;
            int Dimensionality = firstPoint.Length;

            Matrix mA = new Matrix(Dimensionality, Dimensionality);
            Matrix mb = new Matrix(Dimensionality, 1);
            for (int i = 0; currentPoint.MoveNext(); i++)
            {
                mb[i, 0] = 0;
                for (int j = 0; j < Dimensionality; j++)
                {
                    mA[i, j] = firstPoint[j] - currentPoint.Current[j];
                    mb[i, 0] += mA[i, j] * ((firstPoint[j] + currentPoint.Current[j]) / 2.0);
                }

            }

            Matrix result = mA.Solve(mb);
            for (int i = 0; i < vectorOut.Length; i++)
                vectorOut[i] = result[i, 0];
        }

        /// <summary>
        /// This is a lazy calculation of the voronoi Vertex, its not calculated if it isn't required.
        /// </summary>
        internal static void CalculateSimpliceCentroidFromFacets(IEnumerable<INuclei> Nucleis, int simpliceDimensions, ref double[] vectorOut)
        {
            INuclei firstNuclei = Nucleis.First();


            int problemDimensionality = Nucleis.First().Coordinates.Length;

            IVoronoiFacet[] voronoiFacets = firstNuclei.VoronoiHyperRegion.Facets.Where(f => Nucleis.Contains(f.External)).ToArray();

            int Dof = problemDimensionality - simpliceDimensions;
            //we have facets.Length restrictions, and we have to create Dof new restrictions.
            if (simpliceDimensions == problemDimensionality)
                CalculateSimpliceCentroid(Nucleis.Select(n => n.Coordinates), ref vectorOut);
            else
                //we have to solve a facets.Length problem, to get the parameters of the problem.
                //ie: two facets, two ecuations. constraint with the current space formed by nucleiVectors with 2 parameters (unknowns)
                //this is a two ecuations/two unknowns problem.
                if (simpliceDimensions == 1)
                {
                    IVoronoiFacet hpConstraint = voronoiFacets[0];
                    double tCoeff = 0;
                    double independentTerm = hpConstraint[problemDimensionality];
                    Vector[] vectors = VectorsFromPoints(Nucleis, simpliceDimensions + 1);
                    for (int i = 0; i < problemDimensionality; i++)
                    {
                        tCoeff += hpConstraint[i] * vectors[0][i];
                        independentTerm += hpConstraint[i] * firstNuclei.Coordinates[i];
                    }
                    double t = (-independentTerm) / tCoeff;

                    //solve the system
                    for (int i = 0; i < problemDimensionality; i++)
                    {
                        vectorOut[i] = firstNuclei.Coordinates[i] + t * vectors[0][i];
                    }
                }
                else
                {
                    //parameters matrix
                    Matrix mA = new Matrix(voronoiFacets.Length, voronoiFacets.Length);
                    Vector[] vectors = VectorsFromPoints(Nucleis, simpliceDimensions + 1);
                    Matrix mb = new Matrix(voronoiFacets.Length, 1);

                    //mounting parameters matrix
                    for (int row = 0; row < voronoiFacets.Length; row++)
                    {
                        IVoronoiFacet hpConstraint = voronoiFacets[row];
                        double independentTerm = hpConstraint[problemDimensionality];
                        for (int col = 0; col < voronoiFacets.Length; col++)
                        {
                            double tCoeff_col = 0;
                            for (int j = 0; j < problemDimensionality; j++)
                            {
                                tCoeff_col += hpConstraint[j] * vectors[col][j];
                                independentTerm += hpConstraint[j] * firstNuclei.Coordinates[j];
                            }
                            mA[row, col] = tCoeff_col;
                        }
                        mb[row, 0] = independentTerm;
                    }

                    //solving parameters matrix
                    Matrix parametersRes = mA.Solve(mb);
                    for (int i = 0; i < vectorOut.Length; i++)
                    {
                        double increment = 0;
                        for (int j = 0; j < voronoiFacets.Length; j++)
                            increment = parametersRes[j, 0] * vectors[j][i];

                        vectorOut[i] = firstNuclei.Coordinates[i] + increment;
                    }

                }

        }

        internal static int CalculatePointsRank(IEnumerable<INuclei> points)
        {
            INuclei first = points.First();
            int pointsCount = first.Simplices.First().Rank + 2;
            Matrix vectors = new Matrix(pointsCount - 1, first.Coordinates.Length);
            IEnumerator<INuclei> currPoint = points.GetEnumerator();
            currPoint.MoveNext();

            for (int i = 0; i < vectors.RowCount && currPoint.MoveNext(); i++)
            {
                for (int j = 0; j < vectors.ColumnCount; j++)
                    vectors[i, j] = first.Coordinates[j] - currPoint.Current.Coordinates[j];

            }

            return vectors.Rank();
        }

        internal static Vector[] VectorsFromPoints(IEnumerable<INuclei> points, int pointsCount)
        {
            Vector[] vectors = new Vector[pointsCount - 1];
            IEnumerator<INuclei> currentPoint = points.GetEnumerator();
            currentPoint.MoveNext();
            INuclei first = currentPoint.Current;

            int dimensionality = first.Coordinates.Length;
            for (int i = 0; currentPoint.MoveNext(); i++)
            {
                vectors[i] = new Vector(dimensionality);
                for (int j = 0; j < first.Coordinates.Length; j++)
                    vectors[i][j] = currentPoint.Current.Coordinates[j] - first.Coordinates[j];

            }
            return vectors;
        }
    }
}