using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsProxy;

namespace UnitTest
{
	[TestClass]
	public class SystemWebProxyTest
	{
		[TestMethod]
		public void GetCurrentProxyTest()
		{
			var proxy = SystemWebProxy.GetCurrentProxy();
			Assert.IsNotNull(proxy);
		}
	}
}
