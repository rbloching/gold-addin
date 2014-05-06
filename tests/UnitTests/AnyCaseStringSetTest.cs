using System;
using GoldAddin;
using NUnit.Framework;

namespace UnitTests
{
	public class AnyCaseStringSetTest
	{
		[Test]
		public void ContainsTest()
		{
			var target = new AnyCaseStringSet ();
			target.Add ("{abcdef}");
			Assert.IsTrue (target.Contains ("{AbcDef}"));
		}
	}
}

