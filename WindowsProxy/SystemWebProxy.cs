using System.Net;
#if NETCOREAPP
using System.Reflection;
using System.Linq.Expressions;
#endif

namespace WindowsProxy;

public static class SystemWebProxy
{
#if NETCOREAPP
	private static readonly Lazy<Func<IWebProxy>> GetProxyLazy = new(() =>
	{
		const string className = @"System.Net.Http.SystemProxyInfo";
		const string methodName = @"ConstructSystemProxy";
		Type? t = typeof(HttpClient).Assembly.GetType(className);
		MethodInfo? m = t?.GetMethod(methodName);
		if (m is null)
		{
			throw new MissingMethodException(className, methodName);
		}

		MethodCallExpression a = Expression.Call(null, m);
		Func<IWebProxy> e = Expression.Lambda<Func<IWebProxy>>(a).Compile();
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
