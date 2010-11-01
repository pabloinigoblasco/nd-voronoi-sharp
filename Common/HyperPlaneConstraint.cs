using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace ndvoronoisharp.Common
{
    public class HyperPlaneConstraint
    {
        /// <summary>
        /// Example in the plane Ax+By+Cz<D in R3, coefficents would be [A,B,C,D]
        /// </summary>
        public readonly Vector coefficents;
        bool inverse;

        public HyperPlaneConstraint(double[] coefficents)
        {
            // TODO: Complete member initialization
            this.coefficents = new Vector(coefficents);
        }
        /// <summary>
        /// Checks if the point belong to the owner hyperplane
        /// </summary>
        public bool semiHyperSpaceMatch(double[] point)
        {
            //here should be a verification for the dimensionality. But we're simplifying and looking for efficency.
            double res = Enumerable.Range(0, point.Length).Sum(i => point[i] * coefficents[i]);

            if (inverse)
                return res < coefficents.Last();
            else
                return res > coefficents.Last();
        }


        public int SpaceDimesionality
        {
            get { return coefficents.Length - 1; }
        }

        internal void Inverse()
        {
            this.inverse = !this.inverse;
        }
    }
}
