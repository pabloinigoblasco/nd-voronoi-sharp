using System;
using System.Windows.Forms;
using System.Drawing;

using ndvoronoisharp;

namespace Voronoi2DTestingWindow
{
	class MainClass: Form
	{
		public static void Main (string[] args)
		{
			Application.Run(new MainClass());
		}
		
		
		public MainClass()
		{
			this.WindowState=System.Windows.Forms.FormWindowState.Maximized;
			VoronoiDiagram voronoi=new VoronoiDiagram(3);
			this.Paint+=paint_event;
			
		}
		System.Drawing.Font f=new System.Drawing.Font("arial",8);
		void paint_event(object sender, PaintEventArgs e)
		{
			e.Graphics.DrawString("Hello",f,Brushes.Red,new Point(0,0));
			
		}
	}
}
