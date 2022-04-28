using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using WindowsProxy;

namespace UnitTest;

[TestClass]
#if NET6_0_OR_GREATER
[System.Runtime.Versioning.SupportedOSPlatform(@"windows")]
#endif
public class WindowsProxyServiceTest
{
	[TestMethod]
	public void GetRasEntryNamesTest()
	{
		string[] names = ProxyService.GetRasEntryNames();
		foreach (string name in names)
		{
			Console.WriteLine(name);
		}
	}

	[TestMethod]
	public void QueryTest()
	{
		using ProxyService service = new();
		ProxyStatus status = service.Query();
		Console.WriteLine(status);
	}

	[TestMethod]
	public void DirectTest()
	{
		using ProxyService service = new();
		ProxyStatus old = service.Query();
		try
		{
			Assert.IsTrue(service.Direct());
			ProxyStatus status = service.Query();
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
	[DataRow(@"http://中文.cn/?12345678901234?567890123456_4184184&78901234567456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890")]
	public void PacTest(string url)
	{
		using ProxyService service = new()
		{
			AutoConfigUrl = url
		};
		ProxyStatus old = service.Query();
		IdnMapping idn = new();

		try
		{
			Assert.IsTrue(service.Pac());
			ProxyStatus status = service.Query();
			Assert.IsTrue(status.IsDirect);
			Assert.IsFalse(status.IsProxy);
			Assert.IsTrue(status.IsAutoProxyUrl);
			Assert.IsFalse(status.IsAutoDetect);
			Assert.AreEqual(service.AutoConfigUrl.Replace(@"中文.cn", idn.GetAscii(@"中文.cn")), status.AutoConfigUrl);
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
		using ProxyService service = new()
		{
			Server = @"中文测试1919810",
			Bypass = string.Join(@";", ProxyService.LanIp)
		};
		IdnMapping idn = new();
		ProxyStatus old = service.Query();

		try
		{
			Assert.IsTrue(service.Global());
			ProxyStatus status = service.Query();
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
