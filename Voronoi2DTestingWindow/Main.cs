using System;
using System.Windows.Forms;
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
			
			VoronoiDiagram voronoi=new VoronoiDiagram(2);
			voronoi.
		}
	}
}
