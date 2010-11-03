using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;

using ndvoronoisharp;
using System.Text;

namespace Voronoi2DTestingWindow
{
    class MainClass : Form
    {
        public static void Main(string[] args)
        {
            Application.Run(new MainClass());
        }

        private CheckBox viewCircumpheres;
        private CheckBox viewDelunay;
        private CheckBox viewVoronoi;
        private CheckBox cb_showNucleiInfo;

        IVoronoiDelunayGraph voronoi;
        public MainClass()
        {
            InitializeComponent();

            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            voronoi = new ndvoronoisharp.Bowyer.BowyerVoronoiDelunayGraph(2);

            int sample_count = 0;
            Random r = new Random();

            for (int i = 0; i < sample_count; i++)
            {
                double[] newPoint = new double[] { r.NextDouble() * this.Width, r.NextDouble() * this.Height };
                voronoi.AddNewPoint(newPoint);
            }
            this.Paint += paint_event;
            this.MouseDown += new MouseEventHandler(MainClass_MouseDown);

        }

        int step = 0;
        int stages = 1;
        void MainClass_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                double[] newPoint = new double[] { (double)e.X, (double)e.Y };
                voronoi.AddNewPoint(newPoint);

                stages = voronoi.Simplices.Count();
            }
            else
            {
                step = (step + 1) % stages;
            }
            this.Invalidate();
        }

        System.Drawing.Font f = new System.Drawing.Font("arial", 8);
        void paint_event(object sender, PaintEventArgs e)
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Simplices Count: " + voronoi.Simplices.Count());
            sb.AppendLine("Nuclei Count: " + voronoi.Nucleis.Count());
            sb.AppendLine("Paint Stage: " + step);
            e.Graphics.DrawString(sb.ToString(), f, Brushes.Red, new Point(0, 0));

            foreach (INuclei n in voronoi.Nucleis)
            {
                PointF npos = new PointF((float)n.Coordinates[0], (float)n.Coordinates[1]); ;
                e.Graphics.DrawEllipse(Pens.Black, new RectangleF((float)n.Coordinates[0] - 1, (float)n.Coordinates[1] - 1, 3, 3));
                if (cb_showNucleiInfo.Checked)
                {
                    e.Graphics.DrawString("neigh:" + n.Neighbourgs.Count().ToString()
                                          + "\nsimps:" + n.Simplices.Count(), f, Brushes.Black, npos);
                }

                if (viewDelunay.Checked)
                    foreach (INuclei neighbour in n.Neighbourgs)
                    {
                        PointF pos = new PointF((float)neighbour.Coordinates[0], (float)neighbour.Coordinates[1]);
                        e.Graphics.DrawLine(Pens.Red, pos, npos);
                    }
            }

            foreach (ISimplice s in voronoi.Simplices)
            {
                PointF npos = new PointF((float)s.VoronoiVertex.Coordinates[0], (float)s.VoronoiVertex.Coordinates[1]); ;
                float radious = (float)s.Radious;

                if (viewCircumpheres.Checked)
                    e.Graphics.DrawEllipse(Pens.DarkGray, new RectangleF(new PointF(npos.X - radious, npos.Y - radious), new SizeF(radious * 2, radious * 2)));

                if (viewVoronoi.Checked)
                    foreach (ISimplice s2 in s.NeighbourSimplices)
                    {
                        e.Graphics.DrawLine(Pens.Gray, s2.VoronoiVertex.ToPoint(), s.VoronoiVertex.ToPoint());
                    }

                if (s.Facets.Any())
                {
                    /*e.Graphics.FillEllipse(Brushes.HotPink, new RectangleF(npos.X - 2, npos.Y - 2, 5, 5));

                    //pseudoInfiniteRadiousForPainting
                    float paintRadious = (float)Math.Sqrt(this.Width * this.Width + this.Height * this.Height);

                    Nuclei[] points = s.InfiniteNeighbourVoronoiVertexes;


                    PointF middlePoint = new PointF
                        {
                            X = (float)(points[0].Coordinates[0] + points[1].Coordinates[0]) / 2.0f,
                            Y = (float)(points[0].Coordinates[1] + points[1].Coordinates[1]) / 2.0f
                        };

                    PointF directionVector = new PointF
                    {
                        X = (float)(middlePoint.X - s.VoronoiVertex.Coordinates[0]),
                        Y = (float)(middlePoint.Y - s.VoronoiVertex.Coordinates[1])

                    };

                    PointF infinitePoint = new PointF
                                          {
                                              X = npos.X + paintRadious * directionVector.X,
                                              Y = npos.Y + paintRadious * directionVector.Y
                                          };
                    */
                    foreach(ISimpliceFacet facet in s.Facets)
                        if(facet.IsConvexHullFacet && facet.Nucleis.Count()>1)
                            e.Graphics.DrawLine(Pens.Lime, facet.Nucleis[0].Coordinates.ToPoint2F(), facet.Nucleis[1].Coordinates.ToPoint2F());

                }

            }

            if (viewVoronoi.Checked)
            {
                if (voronoi.Simplices.Any())
                {
                    ISimplice s = voronoi.Simplices.ElementAt(step);
                    PointF npos = new PointF((float)s.VoronoiVertex.Coordinates[0], (float)s.VoronoiVertex.Coordinates[1]); ;
                    float radious = (float)s.Radious;

                    e.Graphics.FillEllipse(Brushes.Blue, new RectangleF((float)s.VoronoiVertex.Coordinates[0] - 2, (float)s.VoronoiVertex.Coordinates[1] - 2, 5, 5));

                    e.Graphics.FillEllipse(Brushes.Lime, (float)s.VoronoiVertex.Coordinates[0] - 2, (float)s.VoronoiVertex.Coordinates[1] - 2, 5, 5);
                    e.Graphics.DrawString(s.GetHashCode().ToString(), f, Brushes.Black, s.VoronoiVertex.ToPoint());

                    foreach (ISimplice s2 in s.NeighbourSimplices)
                    {
                        e.Graphics.DrawLine(Pens.Blue, s2.VoronoiVertex.ToPoint(), s.VoronoiVertex.ToPoint());
                        e.Graphics.FillEllipse(Brushes.Blue, (float)s2.VoronoiVertex.Coordinates[0] - 1, (float)s2.VoronoiVertex.Coordinates[1] - 1, 3, 3);
                        foreach (INuclei n in s2.Nucleis)
                            e.Graphics.FillEllipse(Brushes.Yellow, new RectangleF((float)n.Coordinates[0] - 2, (float)n.Coordinates[1] - 2, 5, 5));
                    }


                }
            }




        }

        private void InitializeComponent()
        {
            this.viewCircumpheres = new System.Windows.Forms.CheckBox();
            this.viewDelunay = new System.Windows.Forms.CheckBox();
            this.viewVoronoi = new System.Windows.Forms.CheckBox();
            this.cb_showNucleiInfo = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // viewCircumpheres
            // 
            this.viewCircumpheres.AutoSize = true;
            this.viewCircumpheres.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.viewCircumpheres.Location = new System.Drawing.Point(0, 245);
            this.viewCircumpheres.Name = "viewCircumpheres";
            this.viewCircumpheres.Size = new System.Drawing.Size(284, 17);
            this.viewCircumpheres.TabIndex = 0;
            this.viewCircumpheres.Text = "view circumspheres";
            this.viewCircumpheres.UseVisualStyleBackColor = true;
            this.viewCircumpheres.CheckedChanged += new System.EventHandler(this.viewCircumpheres_CheckedChanged);
            // 
            // viewDelunay
            // 
            this.viewDelunay.AutoSize = true;
            this.viewDelunay.Checked = true;
            this.viewDelunay.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewDelunay.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.viewDelunay.Location = new System.Drawing.Point(0, 228);
            this.viewDelunay.Name = "viewDelunay";
            this.viewDelunay.Size = new System.Drawing.Size(284, 17);
            this.viewDelunay.TabIndex = 1;
            this.viewDelunay.Text = "view delunay";
            this.viewDelunay.UseVisualStyleBackColor = true;
            this.viewDelunay.CheckedChanged += new System.EventHandler(this.viewDelunay_CheckedChanged);
            // 
            // viewVoronoi
            // 
            this.viewVoronoi.AutoSize = true;
            this.viewVoronoi.Checked = true;
            this.viewVoronoi.CheckState = System.Windows.Forms.CheckState.Checked;
            this.viewVoronoi.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.viewVoronoi.Location = new System.Drawing.Point(0, 211);
            this.viewVoronoi.Name = "viewVoronoi";
            this.viewVoronoi.Size = new System.Drawing.Size(284, 17);
            this.viewVoronoi.TabIndex = 2;
            this.viewVoronoi.Text = "view voronoi";
            this.viewVoronoi.UseVisualStyleBackColor = true;
            this.viewVoronoi.CheckedChanged += new System.EventHandler(this.viewVoronoi_CheckedChanged);
            // 
            // cb_showNucleiInfo
            // 
            this.cb_showNucleiInfo.AutoSize = true;
            this.cb_showNucleiInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.cb_showNucleiInfo.Location = new System.Drawing.Point(0, 194);
            this.cb_showNucleiInfo.Name = "cb_showNucleiInfo";
            this.cb_showNucleiInfo.Size = new System.Drawing.Size(284, 17);
            this.cb_showNucleiInfo.TabIndex = 3;
            this.cb_showNucleiInfo.Text = "view nucleiInfo";
            this.cb_showNucleiInfo.UseVisualStyleBackColor = true;
            // 
            // MainClass
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.cb_showNucleiInfo);
            this.Controls.Add(this.viewVoronoi);
            this.Controls.Add(this.viewDelunay);
            this.Controls.Add(this.viewCircumpheres);
            this.Name = "MainClass";
            this.Load += new System.EventHandler(this.MainClass_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void MainClass_Load(object sender, EventArgs e)
        {

        }

        private void viewCircumpheres_CheckedChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void viewDelunay_CheckedChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void viewVoronoi_CheckedChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }
    }

    public static class extensions
    {
        public static PointF ToPoint(this IVoronoiVertex a)
        {
            return new PointF((float)a.Coordinates.ElementAt(0), (float)a.Coordinates.ElementAt(1));
        }
        public static PointF ToPoint2F(this double[] coordinatees)
        {
            return new PointF((float)coordinatees[0], (float)coordinatees[1]);
        }
    }
}
