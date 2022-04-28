using System.Runtime.InteropServices;
using WindowsProxy.Enums;
using WindowsProxy.Models;

namespace WindowsProxy;

internal static class NativeMethods
{
	/// <summary>
	/// Querying current Internet option.
	/// </summary>
	[DllImport(@"wininet.dll", SetLastError = true)]
	internal static extern bool InternetQueryOption(
		nint hInternet,
		INTERNET_OPTION dwOption,
		nint lpBuffer,
		ref uint lpdwBufferLength);

	/// <summary>
	/// Sets an Internet option.
	/// </summary>
	[DllImport(@"wininet.dll", SetLastError = true, CharSet = CharSet.Auto)]
	internal static extern bool InternetSetOption(
		nint hInternet,
		INTERNET_OPTION dwOption,
		nint lpBuffer,
		uint dwBufferLength);

	/// <summary>
	/// Lists all entry names in a remote access phone book.
	/// </summary>
	[DllImport(@"rasapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
	internal static extern uint RasEnumEntries(
		nint reserved,
		nint lpszPhoneBook,
		[In, Out] RASENTRYNAME[] lpRasEntryName,
		ref uint lpcb,
		ref uint lpcEntries);

	public const int MAX_PATH = 260;

	public const int RasMaxEntryName = 256;
}
