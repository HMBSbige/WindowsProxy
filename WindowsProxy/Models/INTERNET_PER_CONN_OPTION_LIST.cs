using System.Runtime.InteropServices;

namespace WindowsProxy.Models;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
internal struct INTERNET_PER_CONN_OPTION_LIST
{
	public uint Size;

	// The connection to be set. NULL means LAN.
	public nint Connection;

	public uint OptionCount;
	public uint OptionError;

	// List of INTERNET_PER_CONN_OPTION.
	public nint pOptions;
}
