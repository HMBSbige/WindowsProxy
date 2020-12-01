using System.Runtime.InteropServices;

namespace WindowsProxy.Models
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	internal struct RASENTRYNAME
	{
		public uint Size;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = NativeMethods.RasMaxEntryName + 1)]
		public string EntryName;

		public uint Flags;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = NativeMethods.MAX_PATH + 1)]
		public string PhonebookPath;
	}
}
