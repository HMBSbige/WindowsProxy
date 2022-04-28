using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using WindowsProxy;

namespace UnitTest;

[TestClass]
public class SystemWebProxyTest
{
	[TestMethod]
	public void GetCurrentProxyTest()
	{
		IWebProxy proxy = SystemWebProxy.GetCurrentProxy();
		Assert.IsNotNull(proxy);
	}
}
