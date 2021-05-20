using System.Net;
#if NETCOREAPP
using System;
using System.Linq.Expressions;
using System.Net.Http;
#endif

namespace WindowsProxy
{
	public static class SystemWebProxy
	{
#if NETCOREAPP
		private static readonly Lazy<Func<IWebProxy>> GetProxyLazy = new(() =>
		{
			const string className = @"System.Net.Http.SystemProxyInfo";
			const string methodName = @"ConstructSystemProxy";
			var t = typeof(HttpClient).Assembly.GetType(className);
			var m = t?.GetMethod(methodName);
			if (m is null)
			{
				throw new MissingMethodException(className, methodName);
			}

			var a = Expression.Call(null, m);
			var e = Expression.Lambda<Func<IWebProxy>>(a).Compile();
			return e;
		});
#endif

		public static IWebProxy GetCurrentProxy()
		{
#if NETCOREAPP

			return GetProxyLazy.Value();
#else
			return WebRequest.GetSystemWebProxy();
#endif
		}
	}
}
