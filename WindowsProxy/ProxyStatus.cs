using System.Runtime.InteropServices;
using WindowsProxy.Enums;
using WindowsProxy.Models;
using static WindowsProxy.Enums.INTERNET_OPTION_PER_CONN_FLAGS;
using static WindowsProxy.Enums.INTERNET_PER_CONN_OptionEnum;

namespace WindowsProxy;

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
		INTERNET_PER_CONN_OPTION flagList = list.First(l => l.dwOption is INTERNET_PER_CONN_FLAGS_UI);
		INTERNET_OPTION_PER_CONN_FLAGS flag = flagList.Value.dwValue;

		IsDirect = flag.HasFlag(PROXY_TYPE_DIRECT);
		IsProxy = flag.HasFlag(PROXY_TYPE_PROXY);
		IsAutoProxyUrl = flag.HasFlag(PROXY_TYPE_AUTO_PROXY_URL);
		IsAutoDetect = flag.HasFlag(PROXY_TYPE_AUTO_DETECT);

		INTERNET_PER_CONN_OPTION proxyServerList = list.First(l => l.dwOption is INTERNET_PER_CONN_PROXY_SERVER);
		ProxyServer = Marshal.PtrToStringAnsi(proxyServerList.Value.pszValue);

		INTERNET_PER_CONN_OPTION proxyBypassList = list.First(l => l.dwOption is INTERNET_PER_CONN_PROXY_BYPASS);
		ProxyBypass = Marshal.PtrToStringAnsi(proxyBypassList.Value.pszValue);

		INTERNET_PER_CONN_OPTION autoConfigUrlList = list.First(l => l.dwOption is INTERNET_PER_CONN_AUTOCONFIG_URL);
		AutoConfigUrl = Marshal.PtrToStringAnsi(autoConfigUrlList.Value.pszValue);
	}
}
