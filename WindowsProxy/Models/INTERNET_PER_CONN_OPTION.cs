using System.Runtime.InteropServices;
using WindowsProxy.Enums;

namespace WindowsProxy.Models
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	internal struct INTERNET_PER_CONN_OPTION
	{
		// A value in INTERNET_PER_CONN_OptionEnum.
		public INTERNET_PER_CONN_OptionEnum dwOption;
		public INTERNET_PER_CONN_OPTION_OptionUnion Value;
	}
}
