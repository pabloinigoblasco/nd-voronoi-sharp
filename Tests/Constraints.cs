
using System;
using NUnit.Framework;
using ndvoronoisharp;
using ndvoronoisharp.implementations;

namespace Tests
{

	[TestFixture]
	public class Constraints
	{

		[Test()]
		public void ConstraintAndSinglePoint ()
		{

			Constraint c=new DefaultConstraint(new double[]{0,0,0},new double[]{1,1,1});
			
			Assert.IsTrue(c.ContainsSubspace(new double[]{0.5,0.5,0.5}));
			Assert.IsTrue(c.ContainsSubspace(new double[]{-0.5,-0.5,-0.5}));
			Assert.IsFalse(c.ContainsSubspace(new double[]{1,1,2}));
			
	
			
			
		}
	}
}
