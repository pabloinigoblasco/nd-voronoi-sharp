using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;

using ndvoronoisharp;

namespace Voronoi2DTestingWindow
{
	class MainClass: Form
	{
		public static void Main (string[] args)
		{
			Application.Run(new MainClass());
		}

        VoronoiDelunayDiagram voronoi;
		public MainClass()
		{
			this.WindowState=System.Windows.Forms.FormWindowState.Maximized;
			voronoi=new VoronoiDelunayDiagram(2);

            int sample_count = 0;
            Random r = new Random();

            for (int i = 0; i < sample_count; i++)
            {
                double[] newPoint = new double[] { r.NextDouble()*this.Width ,r.NextDouble()*this.Height};
                voronoi.AddNewPoint(newPoint);
            }
			this.Paint+=paint_event;
            this.MouseDown += new MouseEventHandler(MainClass_MouseDown);
			
		}

        void MainClass_MouseDown(object sender, MouseEventArgs e)
        {
            double[] newPoint = new double[] { (double)e.X ,(double) e.Y};
            voronoi.AddNewPoint(newPoint);
            this.Invalidate();
        }

		System.Drawing.Font f=new System.Drawing.Font("arial",8);
		void paint_event(object sender, PaintEventArgs e)
		{
			e.Graphics.DrawString("Hello",f,Brushes.Red,new Point(0,0));

            foreach (Nuclei n in voronoi.Nucleis)
            {
                PointF npos = new PointF((float)n.Coordinates[0], (float)n.Coordinates[1]); ;
                e.Graphics.DrawEllipse(Pens.Black,new RectangleF((float)n.Coordinates[0]-2,(float)n.Coordinates[1]-2,3,3));
                e.Graphics.DrawString(n.NucleiNeigbourgs.Count().ToString(), f, Brushes.Black,npos);

                foreach (Nuclei neighbour in n.NucleiNeigbourgs)
                {
                    PointF pos=new PointF((float)neighbour.Coordinates[0], (float)neighbour.Coordinates[1]);
                    e.Graphics.DrawLine(Pens.Red, pos,npos);
                }
            }

            foreach (SimpliceCentroid s in voronoi.VoronoiVertexes)
            {
                PointF npos = new PointF((float)s.Coordinates[0], (float)s.Coordinates[1]); ;
                e.Graphics.FillEllipse(Brushes.Blue, new RectangleF((float)s.Coordinates[0] - 2, (float)s.Coordinates[1] - 2, 3, 3));

            }

			
		}
	}
}
