using System.Runtime.InteropServices;
using WindowsProxy.Enums;

namespace WindowsProxy.Models
{
	/// <summary>
	/// Used in INTERNET_PER_CONN_OPTION.
	/// When create a instance of OptionUnion, only one filed will be used.
	/// The StructLayout and FieldOffset attributes could help to decrease the struct size.
	/// </summary>
	[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Auto)]
	internal struct INTERNET_PER_CONN_OPTION_OptionUnion
	{
		// A value in INTERNET_OPTION_PER_CONN_FLAGS.
		[FieldOffset(0)]
		public INTERNET_OPTION_PER_CONN_FLAGS dwValue;
		[FieldOffset(0)]
		public nint pszValue;
		[FieldOffset(0)]
		public System.Runtime.InteropServices.ComTypes.FILETIME ftValue;
	}
}
