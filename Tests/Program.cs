
using System;


namespace Tests
{


	public class Program
	{

		static void Main (string[] args)
		{
			string name=System.Environment.CurrentDirectory+((System.Environment.OSVersion.Platform==System.PlatformID.Win32Windows)?"\\":"/") +"TestProject.VisualState.xml" ;
			name=System.Reflection.Assembly.GetEntryAssembly().Location;
			Console.WriteLine(name);
			NUnit.ConsoleRunner.Runner.Main(new string[] { name});
		}
	}
}
