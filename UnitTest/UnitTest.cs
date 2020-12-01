using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using WindowsProxy;

namespace UnitTest
{
	[TestClass]
	public class UnitTest
	{
		[TestMethod]
		public void GetRasEntryNamesTest()
		{
			var names = ProxyService.GetRasEntryNames();
			foreach (var name in names)
			{
				Console.WriteLine(name);
			}
		}

		[TestMethod]
		public void QueryTest()
		{
			using var service = new ProxyService();
			var status = service.Query();
			Console.WriteLine(status);
		}

		[TestMethod]
		public void DirectTest()
		{
			using var service = new ProxyService();
			var old = service.Query();
			try
			{
				Assert.IsTrue(service.Direct());
				var status = service.Query();
				Assert.IsTrue(status.IsDirect);
				Assert.IsFalse(status.IsProxy);
				Assert.IsFalse(status.IsAutoProxyUrl);
				Assert.IsTrue(status.IsAutoDetect);
			}
			finally
			{
				Assert.IsTrue(service.Set(old));
				Assert.AreEqual(old, service.Query());
			}
		}

		[TestMethod]
		public void PacTest()
		{
			using var service = new ProxyService
			{
				AutoConfigUrl = @"中文测试114514"
			};
			var idn = new IdnMapping();
			var old = service.Query();

			try
			{
				Assert.IsTrue(service.Pac());
				var status = service.Query();
				Assert.IsTrue(status.IsDirect);
				Assert.IsFalse(status.IsProxy);
				Assert.IsTrue(status.IsAutoProxyUrl);
				Assert.IsFalse(status.IsAutoDetect);
				Assert.AreEqual(idn.GetAscii(service.AutoConfigUrl), status.AutoConfigUrl);
			}
			finally
			{
				Assert.IsTrue(service.Set(old));
				Assert.AreEqual(old, service.Query());
			}
		}

		[TestMethod]
		public void GlobalTest()
		{
			using var service = new ProxyService
			{
				Server = @"中文测试1919810",
				Bypass = string.Join(@";", ProxyService.LanIp)
			};
			var idn = new IdnMapping();
			var old = service.Query();

			try
			{
				Assert.IsTrue(service.Global());
				var status = service.Query();
				Assert.IsTrue(status.IsDirect);
				Assert.IsTrue(status.IsProxy);
				Assert.IsFalse(status.IsAutoProxyUrl);
				Assert.IsFalse(status.IsAutoDetect);
				Assert.AreEqual(idn.GetAscii(service.Server), status.ProxyServer);
				Assert.AreEqual(idn.GetAscii(service.Bypass), status.ProxyBypass);
			}
			finally
			{
				Assert.IsTrue(service.Set(old));
				Assert.AreEqual(old, service.Query());
			}
		}
	}
}
