/*  Copyright (C) 2010  Pablo I�igo Blasco. 
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
using NUnit.Framework;
using ndvoronoisharp;
using ndvoronoisharp.implementations;
using System.Linq;

namespace Tests
{

	[TestFixture]
	public class Constraints
	{

		[Test()]
		public void ConstraintAndSinglePoints ()
		{
			HyperPlaneConstraint c=new DefaultConstraint(new double[]{0,0,-1},new double[]{0,0,1});
			
            //plane z=0
			Assert.IsFalse(c.semiHyperSpaceMatch(new double[]{0.4,0.4,0.4}));
			Assert.IsTrue(c.semiHyperSpaceMatch(new double[]{-0.5,-0.5,-0.5}));
			Assert.IsFalse(c.semiHyperSpaceMatch(new double[]{1,1,2}));

            c = new InverseConstraintDecorator(c);

            Assert.IsTrue(c.semiHyperSpaceMatch(new double[] { 0.4, 0.4, 0.4 }));
            Assert.IsFalse(c.semiHyperSpaceMatch(new double[] { -0.5, -0.5, -0.5 }));
            Assert.IsTrue(c.semiHyperSpaceMatch(new double[] { 1, 1, 2 }));

            c = new DefaultConstraint(new double[] { 10, 0, 10 }, new double[] { 20, 0, 15 });
            Assert.IsTrue(c.semiHyperSpaceMatch(new double[] { 1, 1, 1 }));
            Assert.IsFalse(c.semiHyperSpaceMatch(new double[]{50,40,20}));
		}

        [Test]
        public void BasicVoronoiAddOne()
        {
            VoronoiDelunayDiagram gdv = new VoronoiDelunayDiagram(4);
            HyperRegion reg= gdv.AddNewPointCoordinates(10, 3, 45, 2);

            Assert.AreEqual(gdv.VoronoiRegions.Count(), 1);
            Assert.AreEqual(gdv.VoronoiRegions.Single(), reg);

            Assert.IsFalse(reg.NeighbourgRegions.Any());
            Assert.IsTrue(reg.IsBoundingRegion);
            Assert.IsFalse(reg.VoronoiVertexes.Any());
        }

        [Test]
        public void BasicVoronoiAddOne_RegionBasicFunctionality()
        {
            VoronoiDelunayDiagram gdv = new VoronoiDelunayDiagram(4);
            HyperRegion reg = gdv.AddNewPointCoordinates(10, 3, 45, 2);

            Assert.IsTrue(reg.ContainsPoint(new double[] { 1, 2, 3, 4 }));
        }

        [Test]
        public void BasicVoronoiAddTwo()
        {
            VoronoiDelunayDiagram gdv = new VoronoiDelunayDiagram(4);
            HyperRegion reg = gdv.AddNewPointCoordinates(10, 3, 45, 2);
            HyperRegion regB = gdv.AddNewPointCoordinates(10, 50, 45, 50);

            Assert.AreEqual(gdv.VoronoiRegions.Count(), 2);
            Assert.IsTrue(gdv.VoronoiRegions.Contains(reg));
            Assert.IsTrue(gdv.VoronoiRegions.Contains(regB));

            Assert.AreEqual(reg.NeighbourgRegions.Count(),1);
            Assert.IsTrue(reg.IsBoundingRegion);
            Assert.IsFalse(reg.VoronoiVertexes.Any());

            Assert.AreEqual(regB.NeighbourgRegions.Count(), 1);
            Assert.IsTrue(regB.IsBoundingRegion);
            Assert.IsFalse(regB.VoronoiVertexes.Any());
        }

        [Test]
        public void BasicVoronoiAddTwo_BasicFunctionality()
        {
            VoronoiDelunayDiagram gdv = new VoronoiDelunayDiagram(4);
            HyperRegion reg = gdv.AddNewPointCoordinates(10, 3, 45, 2);
            HyperRegion regB = gdv.AddNewPointCoordinates(10, 50, 45, 50);
            double[] testingPoint =new double[]{10,4,43,0};

            Assert.IsTrue(reg.ContainsPoint(testingPoint));
            Assert.IsFalse(regB.ContainsPoint(testingPoint));
            Assert.AreEqual(gdv.GetMatchingRegion(testingPoint),reg);

        }
        [Test]
        public void BasicVoronoiAddThree()
        {
            VoronoiDelunayDiagram gdv = new VoronoiDelunayDiagram(4);
            HyperRegion reg = gdv.AddNewPointCoordinates(10, 3, 45, 2);
            HyperRegion regB = gdv.AddNewPointCoordinates(10, 50, 45, 50);
            HyperRegion regC = gdv.AddNewPointCoordinates(10, 50, -45, -1);

            Assert.AreEqual(gdv.VoronoiRegions.Count(), 3);
            Assert.IsTrue(gdv.VoronoiRegions.Contains(reg));
            Assert.IsTrue(gdv.VoronoiRegions.Contains(regB));
            Assert.IsTrue(gdv.VoronoiRegions.Contains(regC));

            Assert.AreEqual(reg.NeighbourgRegions.Count(), 2);
            Assert.IsTrue(reg.IsBoundingRegion);
            Assert.IsFalse(reg.VoronoiVertexes.Any());

            Assert.AreEqual(regB.NeighbourgRegions.Count(), 2);
            Assert.IsTrue(regB.IsBoundingRegion);
            Assert.IsFalse(regB.VoronoiVertexes.Any());

            Assert.AreEqual(regC.NeighbourgRegions.Count(), 2);
            Assert.IsTrue(regC.IsBoundingRegion);
            Assert.IsFalse(regC.VoronoiVertexes.Any());
        }

        [Test]
        public void BasicVoronoiAddThree_BasicFunctionality()
        {
            VoronoiDelunayDiagram gdv = new VoronoiDelunayDiagram(4);
            HyperRegion reg = gdv.AddNewPointCoordinates(10, 3, 45, 2);
            HyperRegion regB = gdv.AddNewPointCoordinates(10, 50, 45, 50);
            HyperRegion regC = gdv.AddNewPointCoordinates(10, 50, -45, -1);

            double[] testingPoint = new double[] { 10, 50, 40, 50 };

            Assert.IsFalse(reg.ContainsPoint(testingPoint));
            Assert.True(regB.ContainsPoint(testingPoint));
            Assert.IsFalse(regC.ContainsPoint(testingPoint));
            Assert.AreEqual(gdv.GetMatchingRegion(testingPoint), regB);
        }

        [Test]
        public void BasicVoronoiAddFour()
        {
            VoronoiDelunayDiagram gdv = new VoronoiDelunayDiagram(4);
            HyperRegion reg = gdv.AddNewPointCoordinates(10, 3, 45, 2);
            HyperRegion regB = gdv.AddNewPointCoordinates(10, 50, 45, 50);
            HyperRegion regC = gdv.AddNewPointCoordinates(10, 50, -45, -1);
            HyperRegion regD = gdv.AddNewPointCoordinates(10, 0, -45, -21);

            Assert.AreEqual(gdv.VoronoiRegions.Count(), 4);
            Assert.IsTrue(gdv.VoronoiRegions.Contains(reg));
            Assert.IsTrue(gdv.VoronoiRegions.Contains(regB));
            Assert.IsTrue(gdv.VoronoiRegions.Contains(regC));
            Assert.IsTrue(gdv.VoronoiRegions.Contains(regD));

            Assert.AreEqual(reg.NeighbourgRegions.Count(), 3);
            Assert.IsTrue(reg.IsBoundingRegion);
            Assert.IsFalse(reg.VoronoiVertexes.Any());

            Assert.AreEqual(regB.NeighbourgRegions.Count(), 3);
            Assert.IsTrue(regB.IsBoundingRegion);
            Assert.IsFalse(regB.VoronoiVertexes.Any());

            Assert.AreEqual(regC.NeighbourgRegions.Count(), 3);
            Assert.IsTrue(regC.IsBoundingRegion);
            Assert.IsFalse(regC.VoronoiVertexes.Any());

            Assert.AreEqual(regD.NeighbourgRegions.Count(), 3);
            Assert.IsTrue(regD.IsBoundingRegion);
            Assert.IsFalse(regD.VoronoiVertexes.Any());
        }

        [Test]
        public void BasicVoronoiAddFour_BasicFunctionality()
        {
            VoronoiDelunayDiagram gdv = new VoronoiDelunayDiagram(4);
            HyperRegion reg = gdv.AddNewPointCoordinates(10, 3, 45, 2);
            HyperRegion regB = gdv.AddNewPointCoordinates(10, 50, 45, 50);
            HyperRegion regC = gdv.AddNewPointCoordinates(10, 50, -45, -1);
            HyperRegion regD = gdv.AddNewPointCoordinates(10, 0, -45, -21);

            double[] testingPoint = new double[] { 10, 50, -40, 5 };

            Assert.IsFalse(reg.ContainsPoint(testingPoint));
            Assert.IsFalse(regB.ContainsPoint(testingPoint));
            Assert.IsTrue(regC.ContainsPoint(testingPoint));
            Assert.IsFalse(regD.ContainsPoint(testingPoint));
            Assert.AreEqual(gdv.GetMatchingRegion(testingPoint), regC);
        }

      


        [Test]
        public void Vertex_WithoutSimplice2D()
        {
            //interesting to assert the correct neiboorhood
            Assert.Fail();
        }

        [Test]
        public void Vertex_WithoutSimplice3D()
        {
            //interesting to assert the correct neiboorhood
            Assert.Fail();
        }

        [Test]
        public void Vertex_WithoutSimplice4D()
        {
            //interesting to assert the correct neiboorhood
            Assert.Fail();
        }

        [Test]
        public void BowPoints()
        {
            //interesting to assert the correct neiboorhood
            Assert.Fail();
        }

        [Test]
        public void NoVertex_Build_SameLinePoints2D()
        {
            //interesting to assert the correct neiboorhood
            Assert.Fail();
        }

        [Test]
        public void NoVertex_Build_SameLinePoints3D()
        {
            //interesting to assert the correct neiboorhood
            Assert.Fail();
        }
        [Test]
        public void NoVertex_Build_SameLinePoints4D()
        {
            //interesting to assert the correct neiboorhood
            Assert.Fail();
        }

        [Test]
        public void NoVertex_Build_SamePlanePoints3D()
        {
            //interesting to assert the correct neiboorhood
            Assert.Fail();
        }

        [Test]
        public void NoVertex_Build_SamePlanePoints4D()
        {
            //interesting to assert the correct neiboorhood
            Assert.Fail();
        }

        [Test]
        public void SameSpacePoints4D()
        {
            //interesting to assert the correct neiboorhood
            Assert.Fail();
        }



	}
}
