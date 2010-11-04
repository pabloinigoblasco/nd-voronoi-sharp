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
using NUnit.Framework;
using ndvoronoisharp.Common;
using System.Linq;
using ndvoronoisharp;
using ndvoronoisharp.Bowyer;
using ndvoronoisharp.Common.implementations;

namespace Tests
{

    public class stubNuclei : INuclei
    {
        #region Miembros de INuclei
        public stubNuclei(params double[] Coordinates)
        {
            this.Coordinates = Coordinates;
        }
        public double[] Coordinates
        {
            get;
            private set;
        }

        public IVoronoiRegion VoronoiHyperRegion
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.Generic.IEnumerable<INuclei> Neighbourgs
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.Generic.IEnumerable<ISimplice> Simplices
        {
            get { throw new NotImplementedException(); }
        }

        public bool BelongConvexHull
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }

    [TestFixture]
    public class Constraints
    {
        static IVoronoiDelunayGraph createNewVoronoiDiagram(int dimensions)
        {
            return new BowyerVoronoiDelunayGraph(dimensions);
        }
        public void CheckGeneralDiagramCoherence(IVoronoiDelunayGraph diagram)
        {
            Assert.IsTrue(diagram.Simplices.All(s => s.Rank == diagram.Simplices.Max(s2 => s2.Rank)));

            Assert.IsTrue(diagram.Simplices.All(s=>s.Facets.Count()==s.Rank+1));
            Assert.IsTrue(diagram.VoronoiVertexes.Where(v=>v.Infinity).All(v=>v.Simplice.Facets.Count()==1));
            Assert.IsTrue(diagram.Simplices.All(s=>s.Facets.All(f=>f.Rank+1==s.Rank)));

            Assert.IsTrue(diagram.Nucleis.All(n => n.VoronoiHyperRegion.Facets.All(f => f.semiHyperSpaceMatch(n.Coordinates))));
            Assert.IsTrue(diagram.Simplices.All(s => s.Facets.All(f => f.semiHyperSpaceMatch(s.VoronoiVertex.Coordinates))));
        }

        [Test()]
        public void ConstraintAndSinglePoints()
        {
            IVoronoiFacet c = new DefaultVoronoiFacet(new stubNuclei(0, 0, -1), new stubNuclei(0, 0, 1));

            //plane z=0
            Assert.IsFalse(c.semiHyperSpaceMatch(new double[] { 0.4, 0.4, 0.4 }));
            Assert.IsTrue(c.semiHyperSpaceMatch(new double[] { -0.5, -0.5, -0.5 }));
            Assert.IsFalse(c.semiHyperSpaceMatch(new double[] { 1, 1, 2 }));

            c = new InverseDefaultVoronoiFacet(c);

            Assert.IsTrue(c.semiHyperSpaceMatch(new double[] { 0.4, 0.4, 0.4 }));
            Assert.IsFalse(c.semiHyperSpaceMatch(new double[] { -0.5, -0.5, -0.5 }));
            Assert.IsTrue(c.semiHyperSpaceMatch(new double[] { 1, 1, 2 }));

            c = new DefaultVoronoiFacet(new stubNuclei(10, 0, 10), new stubNuclei(20, 0, 15));
            Assert.IsTrue(c.semiHyperSpaceMatch(new double[] { 1, 1, 1 }));
            Assert.IsFalse(c.semiHyperSpaceMatch(new double[] { 50, 40, 20 }));
        }

        [Test]
        public void BasicVoronoiAddOne()
        {
            IVoronoiDelunayGraph gdv = createNewVoronoiDiagram(4);
            IVoronoiRegion reg = gdv.AddNewPoint(new double[] { 10, 3, 45, 2 });

            Assert.AreEqual(gdv.VoronoiRegions.Count(), 1);
            Assert.AreEqual(gdv.VoronoiRegions.Single(), reg);

            Assert.IsFalse(reg.NeighbourgRegions.Any());
            Assert.IsTrue(reg.Nuclei.BelongConvexHull);

            CheckGeneralDiagramCoherence(gdv);
        }

        [Test]
        public void BasicVoronoiAddOne_RegionBasicFunctionality()
        {
            IVoronoiDelunayGraph gdv = createNewVoronoiDiagram(4);
            IVoronoiRegion reg = gdv.AddNewPoint(new double[] { 10, 3, 45, 2 });

            Assert.IsTrue(reg.ContainsPoint(new double[] { 1, 2, 3, 4 }));
            CheckGeneralDiagramCoherence(gdv);
        }

        [Test]
        public void BasicVoronoiAddTwo()
        {
            IVoronoiDelunayGraph gdv = createNewVoronoiDiagram(4);
            IVoronoiRegion reg = gdv.AddNewPoint(new double[] { 10, 3, 45, 2 });
            IVoronoiRegion regB = gdv.AddNewPoint(new double[] { 10, 50, 45, 50 });

            Assert.AreEqual(gdv.VoronoiRegions.Count(), 2);
            Assert.AreEqual(gdv.Simplices.Count(s => s.Rank == 1), 1);
            Assert.IsTrue(!gdv.Simplices.Any(s => s.Rank > 1));
            Assert.IsTrue(gdv.Simplices.Single().Facets.All(f => f.semiHyperSpaceMatch(gdv.Simplices.Single().VoronoiVertex.Coordinates)));

            Assert.IsTrue(gdv.VoronoiRegions.Contains(reg));
            Assert.IsTrue(gdv.VoronoiRegions.Contains(regB));

            Assert.AreEqual(reg.NeighbourgRegions.Count(), 1);
            Assert.IsTrue(reg.IsInfiniteRegion);
            Assert.AreEqual(reg.Vertexes.Count(), 2);

            Assert.AreEqual(regB.NeighbourgRegions.Count(), 1);
            Assert.IsTrue(regB.IsInfiniteRegion);
            Assert.AreEqual(regB.Vertexes.Count(), 2);

            CheckGeneralDiagramCoherence(gdv);
        }

        [Test]
        public void BasicVoronoiAddTwo_BasicFunctionality()
        {
            IVoronoiDelunayGraph gdv = createNewVoronoiDiagram(4);
            IVoronoiRegion reg = gdv.AddNewPoint(new double[] { 10, 3, 45, 2 });
            IVoronoiRegion regB = gdv.AddNewPoint(new double[] { 10, 50, 45, 50 });
            double[] testingPoint = new double[] { 10, 4, 43, 0 };

            Assert.IsTrue(reg.ContainsPoint(testingPoint));
            Assert.IsFalse(regB.ContainsPoint(testingPoint));
            Assert.AreEqual(gdv.GetMatchingRegion(testingPoint), reg);
            CheckGeneralDiagramCoherence(gdv);

        }
        [Test]
        public void BasicVoronoiAddThree()
        {
            IVoronoiDelunayGraph gdv = createNewVoronoiDiagram(4);
            IVoronoiRegion reg = gdv.AddNewPoint(new double[] { 10, 3, 45, 2 });
            IVoronoiRegion regB = gdv.AddNewPoint(new double[] { 10, 50, 45, 50 });
            IVoronoiRegion regC = gdv.AddNewPoint(new double[] { 10, 50, -45, -1 });

            Assert.AreEqual(gdv.VoronoiRegions.Count(), 3);

            Assert.AreEqual(gdv.Simplices.Count(s => s.Rank == 2), 1);
            Assert.IsTrue(gdv.Simplices.Single().Facets.All(f => f.semiHyperSpaceMatch(gdv.Simplices.Single().VoronoiVertex.Coordinates)));
            Assert.IsTrue(!gdv.Simplices.Any(s => s.Rank > 2));

            Assert.IsTrue(gdv.VoronoiRegions.Contains(reg));
            Assert.IsTrue(gdv.VoronoiRegions.Contains(regB));
            Assert.IsTrue(gdv.VoronoiRegions.Contains(regC));

            Assert.AreEqual(reg.NeighbourgRegions.Count(), 2);
            Assert.IsTrue(reg.IsInfiniteRegion);
            Assert.AreEqual(reg.Vertexes.Count(), 3);
            Assert.AreEqual(reg.Vertexes.Count(v => v.Infinity), 2);
            Assert.AreEqual(reg.Facets.Count(), 2);
            Assert.IsTrue(reg.Facets.All(f => f.semiHyperSpaceMatch(reg.Nuclei.Coordinates)));

            Assert.AreEqual(regB.NeighbourgRegions.Count(), 2);
            Assert.IsTrue(regB.IsInfiniteRegion);
            Assert.AreEqual(regB.Vertexes.Count(), 3);
            Assert.AreEqual(regB.Vertexes.Count(v => v.Infinity), 2);
            Assert.AreEqual(regB.Facets.Count(), 2);
            Assert.IsTrue(regB.Facets.All(f => f.semiHyperSpaceMatch(regB.Nuclei.Coordinates)));

            Assert.AreEqual(regC.NeighbourgRegions.Count(), 2);
            Assert.IsTrue(regC.IsInfiniteRegion);
            Assert.AreEqual(regC.Vertexes.Count(), 3);
            Assert.AreEqual(regC.Vertexes.Count(v => v.Infinity), 2);
            Assert.AreEqual(regC.Facets.Count(), 2);
            Assert.IsTrue(regC.Facets.All(f => f.semiHyperSpaceMatch(regC.Nuclei.Coordinates)));

            CheckGeneralDiagramCoherence(gdv);
        }

        [Test]
        public void BasicVoronoiAddThree_BasicFunctionality()
        {
            IVoronoiDelunayGraph gdv = createNewVoronoiDiagram(4);
            IVoronoiRegion reg = gdv.AddNewPoint(new double[] { 10, 3, 45, 2 });
            IVoronoiRegion regB = gdv.AddNewPoint(new double[] { 10, 50, 45, 50 });
            IVoronoiRegion regC = gdv.AddNewPoint(new double[] { 10, 50, -45, -1 });

            double[] testingPoint = new double[] { 10, 50, 40, 50 };

            Assert.IsFalse(reg.ContainsPoint(testingPoint));
            Assert.True(regB.ContainsPoint(testingPoint));
            Assert.IsFalse(regC.ContainsPoint(testingPoint));
            Assert.AreEqual(gdv.GetMatchingRegion(testingPoint), regB);

            CheckGeneralDiagramCoherence(gdv);
        }

        [Test]
        public void BasicVoronoiAddFour()
        {
            IVoronoiDelunayGraph gdv = createNewVoronoiDiagram(4);
            IVoronoiRegion reg = gdv.AddNewPoint(new double[] { 10, 3, 45, 2 });
            IVoronoiRegion regB = gdv.AddNewPoint(new double[] { 10, 50, 45, 50 });
            IVoronoiRegion regC = gdv.AddNewPoint(new double[] { 10, 50, -45, -1 });
            IVoronoiRegion regD = gdv.AddNewPoint(new double[] { 10, 0, -45, -21 });

            Assert.AreEqual(gdv.VoronoiRegions.Count(), 4);
            Assert.IsTrue(gdv.VoronoiRegions.Contains(reg));
            Assert.IsTrue(gdv.VoronoiRegions.Contains(regB));
            Assert.IsTrue(gdv.VoronoiRegions.Contains(regC));
            Assert.IsTrue(gdv.VoronoiRegions.Contains(regD));

            Assert.AreEqual(reg.NeighbourgRegions.Count(), 3);
            Assert.IsTrue(reg.IsInfiniteRegion);


            Assert.AreEqual(regB.NeighbourgRegions.Count(), 3);
            Assert.IsTrue(regB.IsInfiniteRegion);


            Assert.AreEqual(regC.NeighbourgRegions.Count(), 3);
            Assert.IsTrue(regC.IsInfiniteRegion);


            Assert.AreEqual(regD.NeighbourgRegions.Count(), 3);
            Assert.IsTrue(regD.IsInfiniteRegion);

            CheckGeneralDiagramCoherence(gdv);
        }

        [Test]
        public void BasicVoronoiAddFour_BasicFunctionality()
        {
            IVoronoiDelunayGraph gdv = createNewVoronoiDiagram(4);
            IVoronoiRegion reg = gdv.AddNewPoint(new double[] { 10, 3, 45, 2 });
            IVoronoiRegion regB = gdv.AddNewPoint(new double[] { 10, 50, 45, 50 });
            IVoronoiRegion regC = gdv.AddNewPoint(new double[] { 10, 50, -45, -1 });
            IVoronoiRegion regD = gdv.AddNewPoint(new double[] { 10, 0, -45, -21 });

            double[] testingPoint = new double[] { 10, 50, -40, 5 };

            Assert.IsFalse(reg.ContainsPoint(testingPoint));
            Assert.IsFalse(regB.ContainsPoint(testingPoint));
            Assert.IsTrue(regC.ContainsPoint(testingPoint));
            Assert.IsFalse(regD.ContainsPoint(testingPoint));
            Assert.AreEqual(gdv.GetMatchingRegion(testingPoint), regC);

            CheckGeneralDiagramCoherence(gdv);
        }

        [Test]
        public void BuildOneSimplice_2D()
        {
            IVoronoiDelunayGraph gdv = createNewVoronoiDiagram(2);
            IVoronoiRegion reg = gdv.AddNewPoint("Cordoba", new double[] { 20, 5 });
            IVoronoiRegion regB = gdv.AddNewPoint("Huelva", new double[] { 1, 1 });
            IVoronoiRegion regC = gdv.AddNewPoint("Cadiz", new double[] { 40, 1 });

            double[] testingPoint = new double[] { 4, 4 };

            Assert.IsFalse(reg.ContainsPoint(testingPoint));
            Assert.True(regB.ContainsPoint(testingPoint));
            Assert.IsFalse(regC.ContainsPoint(testingPoint));
            Assert.AreEqual(gdv.GetMatchingRegion(testingPoint), regB);

            Assert.IsTrue(gdv.Nucleis.All(n => n.BelongConvexHull));
            Assert.IsTrue(gdv.Simplices.Count() == 1);
            Assert.IsTrue(gdv.Simplices.Single().Nucleis.Intersect(gdv.Nucleis).Count() == 3);

            Assert.IsTrue(gdv.Nucleis.All(n => n.Simplices.Contains(gdv.Simplices.Single())));

            CheckGeneralDiagramCoherence(gdv);
        }

        [Test]
        public void BuildOneSimplice_AndCheckVoronoiFeatures_2D()
        {
            IVoronoiDelunayGraph gdv = createNewVoronoiDiagram(2);
            IVoronoiRegion reg = gdv.AddNewPoint("Cordoba", new double[] { 20, 5 });
            IVoronoiRegion regB = gdv.AddNewPoint("Huelva", new double[] { 1, 1 });
            IVoronoiRegion regC = gdv.AddNewPoint("Cadiz", new double[] { 40, 1 });

            double[] testingPoint = new double[] { 4, 4 };

            Assert.IsTrue(gdv.VoronoiVertexes.Count() == 1);
            Assert.IsTrue(gdv.VoronoiRegions.All(r => r.Vertexes.Count() == 1));

            CheckGeneralDiagramCoherence(gdv);
        }

        [Test]
        public void BuildAndRefactorOneSimpliceInTwoSimplices_2D()
        {
            IVoronoiDelunayGraph gdv = createNewVoronoiDiagram(2);
            
            IVoronoiRegion reg = gdv.AddNewPoint("Cordoba", new double[] { 20, 5 });
            CheckGeneralDiagramCoherence(gdv);
            Assert.IsTrue(gdv.Simplices.Count() == 1);

            IVoronoiRegion regB = gdv.AddNewPoint("Huelva", new double[] { 1, 1 });
            CheckGeneralDiagramCoherence(gdv);
            Assert.IsTrue(gdv.Simplices.Count() == 1);

            IVoronoiRegion regC = gdv.AddNewPoint("Cadiz", new double[] { 40, 1 });
            CheckGeneralDiagramCoherence(gdv);
            Assert.IsTrue(gdv.Simplices.Count() == 1);

            IVoronoiRegion regD = gdv.AddNewPoint("Sevilla", new double[] { 10, -10 });
            CheckGeneralDiagramCoherence(gdv);
            Assert.IsTrue(gdv.Simplices.Count() == 2);

            Assert.IsTrue(gdv.VoronoiRegions.Count() == 4);
            Assert.IsTrue(gdv.VoronoiVertexes.Count() == 6);

            Assert.IsTrue(regD.Nuclei.Simplices.Count() == 2);
            Assert.IsTrue(reg.Nuclei.Simplices.Count() == 2);
            Assert.IsTrue(regB.Nuclei.Simplices.Count() == 1);
            Assert.IsTrue(regC.Nuclei.Simplices.Count() == 1);

            CheckGeneralDiagramCoherence(gdv);
        }

        [Test]
        public void BuildAndRefactorTwoSimpliceInTwoSimplices_2D()
        {
            IVoronoiDelunayGraph gdv = createNewVoronoiDiagram(2);
            IVoronoiRegion cordoba = gdv.AddNewPoint("Cordoba", new double[] { 20, 5 });
            IVoronoiRegion huelva = gdv.AddNewPoint("Huelva", new double[] { 1, 1 });
            IVoronoiRegion cadiz = gdv.AddNewPoint("Cadiz", new double[] { 5, -13 });

            Assert.IsTrue(cordoba.NeighbourgRegions.Contains(huelva));
            Assert.IsTrue(cordoba.NeighbourgRegions.Contains(cadiz));
            Assert.IsTrue(huelva.NeighbourgRegions.Contains(cordoba));
            Assert.IsTrue(huelva.NeighbourgRegions.Contains(cadiz));
            Assert.IsTrue(cadiz.NeighbourgRegions.Contains(cordoba));
            Assert.IsTrue(cadiz.NeighbourgRegions.Contains(huelva));



            IVoronoiRegion malaga = gdv.AddNewPoint("Malaga", new double[] { 20, -20 });

            Assert.IsTrue(cordoba.NeighbourgRegions.Contains(huelva));
            Assert.IsTrue(cordoba.NeighbourgRegions.Contains(cadiz));
            Assert.IsTrue(huelva.NeighbourgRegions.Contains(cordoba));
            Assert.IsTrue(huelva.NeighbourgRegions.Contains(cadiz));
            Assert.IsTrue(cadiz.NeighbourgRegions.Contains(cordoba));
            Assert.IsTrue(cadiz.NeighbourgRegions.Contains(huelva));

            IVoronoiRegion sevilla = gdv.AddNewPoint("Sevilla", new double[] { 10, -10 });

            Assert.AreEqual(gdv.VoronoiRegions.Count(), 5);
            Assert.AreEqual(gdv.Simplices.Count(), 4);
            Assert.AreEqual(gdv.VoronoiVertexes.Count(), 4);

            Assert.IsFalse(huelva.NeighbourgRegions.Contains(malaga));
            Assert.IsFalse(malaga.NeighbourgRegions.Contains(huelva));
            Assert.IsFalse(cordoba.NeighbourgRegions.Contains(cadiz));
            Assert.IsFalse(cadiz.NeighbourgRegions.Contains(cordoba));


            Assert.AreEqual(sevilla.Nuclei.Simplices.Count(), 4);
            Assert.AreEqual(cordoba.Nuclei.Simplices.Count(), 2);
            Assert.AreEqual(huelva.Nuclei.Simplices.Count(), 2);
            Assert.AreEqual(malaga.Nuclei.Simplices.Count(), 2);

            CheckGeneralDiagramCoherence(gdv);
        }


        [Test]
        public void BuildOneSimplice_3D()
        {
            Assert.Fail();
        }

        [Test]
        public void BuildOneSimplice_4D()
        {
            Assert.Fail();
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
        public void SameLinePoints3D()
        {
            //interesting to assert the correct neiboorhood
            Assert.Fail();
        }
        [Test]
        public void SameLinePoints4D()
        {
            IVoronoiDelunayGraph gdv = createNewVoronoiDiagram(4);
            IVoronoiRegion reg = gdv.AddNewPoint(new double[] { 10, 0, 45, 2 });
            CheckGeneralDiagramCoherence(gdv);

            IVoronoiRegion regB = gdv.AddNewPoint(new double[] { 10, 0, 45, 50 });
            CheckGeneralDiagramCoherence(gdv);

            IVoronoiRegion regC = gdv.AddNewPoint(new double[] { 10, 0, 45, -1 });
            CheckGeneralDiagramCoherence(gdv);

            Assert.AreEqual(gdv.Simplices.Count(), 2);
            Assert.AreEqual(gdv.VoronoiVertexes.Count(), 4);
            Assert.IsTrue(gdv.VoronoiVertexes.Count(v => v.Infinity) == 2);

            IVoronoiRegion regD = gdv.AddNewPoint(new double[] { 10, 0, 45, -21 });
            CheckGeneralDiagramCoherence(gdv);

            Assert.AreEqual(gdv.VoronoiRegions.Count(), 4);
            Assert.AreEqual(gdv.Simplices.Count(), 3);
            Assert.AreEqual(gdv.VoronoiVertexes.Count(), 5);
            Assert.IsTrue(gdv.VoronoiVertexes.Count(v => v.Infinity) == 2);
            Assert.IsTrue(gdv.Simplices.All(s => s.Rank == 1));

            CheckGeneralDiagramCoherence(gdv);

            /*
            Assert.AreEqual(gdv.Simplices.Count(s => s.Dimensionality == 2), 1);
            Assert.IsTrue(gdv.Simplices.Single().Facets.All(f => f.semiHyperSpaceMatch(gdv.Simplices.Single().VoronoiVertex.Coordinates)));
            Assert.IsTrue(!gdv.Simplices.Any(s => s.Dimensionality > 2));

            Assert.IsTrue(gdv.VoronoiRegions.Contains(reg));
            Assert.IsTrue(gdv.VoronoiRegions.Contains(regB));
            Assert.IsTrue(gdv.VoronoiRegions.Contains(regC));

            Assert.AreEqual(reg.NeighbourgRegions.Count(), 2);
            Assert.IsTrue(reg.IsInfiniteRegion);
            Assert.AreEqual(reg.Vertexes.Count(), 3);
            Assert.AreEqual(reg.Vertexes.Count(v => v.Infinity), 2);
            Assert.AreEqual(reg.Facets.Count(), 2);
            Assert.IsTrue(reg.Facets.All(f => f.semiHyperSpaceMatch(reg.Nuclei.Coordinates)));

            Assert.AreEqual(regB.NeighbourgRegions.Count(), 2);
            Assert.IsTrue(regB.IsInfiniteRegion);
            Assert.AreEqual(regB.Vertexes.Count(), 3);
            Assert.AreEqual(regB.Vertexes.Count(v => v.Infinity), 2);
            Assert.AreEqual(regB.Facets.Count(), 2);
            Assert.IsTrue(regB.Facets.All(f => f.semiHyperSpaceMatch(regB.Nuclei.Coordinates)));

            Assert.AreEqual(regC.NeighbourgRegions.Count(), 2);
            Assert.IsTrue(regC.IsInfiniteRegion);
            Assert.AreEqual(regC.Vertexes.Count(), 3);
            Assert.AreEqual(regC.Vertexes.Count(v => v.Infinity), 2);
            Assert.AreEqual(regC.Facets.Count(), 2);
            Assert.IsTrue(regC.Facets.All(f => f.semiHyperSpaceMatch(regC.Nuclei.Coordinates)));*/
        }

        [Test]
        public void SamePlanePoints3D()
        {
            //interesting to assert the correct neiboorhood
            Assert.Fail();
        }

        [Test]
        public void PlanePoints4D()
        {
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
