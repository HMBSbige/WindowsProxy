using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WindowsProxy;

namespace UnitTest
{
	[TestClass]
	public class UnitTest
	{
		[TestMethod]
		public void GetRasEntryNamesTest()
		{
			var names = SystemProxy.GetRasEntryNames();
			foreach (var name in names)
			{
				Console.WriteLine(name);
			}
		}

		[TestMethod]
		public void QueryTest()
		{
			using var service = new SystemProxy();
			service.Query();
		}
	}
}
