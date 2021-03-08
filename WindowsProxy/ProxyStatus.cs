using System.Linq;
using System.Runtime.InteropServices;
using WindowsProxy.Models;
using static WindowsProxy.Enums.INTERNET_OPTION_PER_CONN_FLAGS;
using static WindowsProxy.Enums.INTERNET_PER_CONN_OptionEnum;

namespace WindowsProxy
{
	public sealed record ProxyStatus
	{
		public bool IsDirect { get; private set; }
		public bool IsProxy { get; private set; }
		public bool IsAutoProxyUrl { get; private set; }
		public bool IsAutoDetect { get; private set; }
		public string? ProxyServer { get; private set; }
		public string? ProxyBypass { get; private set; }
		public string? AutoConfigUrl { get; private set; }

		internal void Parse(INTERNET_PER_CONN_OPTION[] list)
		{
			var flagList = list.First(l => l.dwOption == INTERNET_PER_CONN_FLAGS_UI);
			var flag = flagList.Value.dwValue;

			IsDirect = flag.HasFlag(PROXY_TYPE_DIRECT);
			IsProxy = flag.HasFlag(PROXY_TYPE_PROXY);
			IsAutoProxyUrl = flag.HasFlag(PROXY_TYPE_AUTO_PROXY_URL);
			IsAutoDetect = flag.HasFlag(PROXY_TYPE_AUTO_DETECT);

			var proxyServerList = list.First(l => l.dwOption == INTERNET_PER_CONN_PROXY_SERVER);
			ProxyServer = Marshal.PtrToStringAnsi(proxyServerList.Value.pszValue);

			var proxyBypassList = list.First(l => l.dwOption == INTERNET_PER_CONN_PROXY_BYPASS);
			ProxyBypass = Marshal.PtrToStringAnsi(proxyBypassList.Value.pszValue);

			var autoConfigUrlList = list.First(l => l.dwOption == INTERNET_PER_CONN_AUTOCONFIG_URL);
			AutoConfigUrl = Marshal.PtrToStringAnsi(autoConfigUrlList.Value.pszValue);
		}
	}
}
