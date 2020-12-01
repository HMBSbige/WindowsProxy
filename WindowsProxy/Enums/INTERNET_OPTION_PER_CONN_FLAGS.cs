using System;

namespace WindowsProxy.Enums
{
	/// <summary>
	/// Constants used in INTERNET_PER_CONN_OPTON struct.
	/// </summary>
	[Flags]
	internal enum INTERNET_OPTION_PER_CONN_FLAGS : uint
	{
		PROXY_TYPE_DIRECT = 0x00000001, // direct to net
		PROXY_TYPE_PROXY = 0x00000002, // via named proxy
		PROXY_TYPE_AUTO_PROXY_URL = 0x00000004, // autoproxy URL
		PROXY_TYPE_AUTO_DETECT = 0x00000008 // use autoproxy detection
	}
}
