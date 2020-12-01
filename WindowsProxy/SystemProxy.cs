using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WindowsProxy.Models;
using static WindowsProxy.Enums.INTERNET_OPTION;
using static WindowsProxy.Enums.INTERNET_OPTION_PER_CONN_FLAGS;
using static WindowsProxy.Enums.INTERNET_PER_CONN_OptionEnum;

namespace WindowsProxy
{
	public class SystemProxy : IDisposable
	{
		private enum ProxyType : uint
		{
			Direct = 1,
			Pac = 2,
			Global = 3,
			Query = 4
		}

		public static readonly string[] LanIp =
		{
			"<local>",
			"localhost",
			"127.*",
			"10.*",
			"172.16.*",
			"172.17.*",
			"172.18.*",
			"172.19.*",
			"172.20.*",
			"172.21.*",
			"172.22.*",
			"172.23.*",
			"172.24.*",
			"172.25.*",
			"172.26.*",
			"172.27.*",
			"172.28.*",
			"172.29.*",
			"172.30.*",
			"172.31.*",
			"192.168.*"
		};

		private INTERNET_PER_CONN_OPTION_LIST _options;

		private readonly Queue<nint> _needToFree = new();

		public string Url { get; set; } = string.Empty;

		public string Bypass { get; set; } = @"<local>";

		private void Initialize(ref INTERNET_PER_CONN_OPTION_LIST options, ProxyType type)
		{
			var optionCount = (uint)type;

			var dwBufferSize = (uint)Marshal.SizeOf(typeof(INTERNET_PER_CONN_OPTION_LIST));
			options.Size = dwBufferSize;

			options.OptionCount = optionCount;
			options.OptionError = 0;

			var perOption = new INTERNET_PER_CONN_OPTION[optionCount];
			var optSize = Marshal.SizeOf(typeof(INTERNET_PER_CONN_OPTION));
			nint optionsPtr = Marshal.AllocCoTaskMem(perOption.Length * optSize);
			_needToFree.Enqueue(optionsPtr);

			switch (type)
			{
				case ProxyType.Direct:
				{
					perOption[0].dwOption = INTERNET_PER_CONN_FLAGS;
					perOption[0].Value.dwValue = PROXY_TYPE_AUTO_DETECT | PROXY_TYPE_DIRECT;
					break;
				}
				case ProxyType.Pac:
				{
					perOption[0].dwOption = INTERNET_PER_CONN_FLAGS;
					perOption[0].Value.dwValue = PROXY_TYPE_AUTO_PROXY_URL | PROXY_TYPE_DIRECT;

					perOption[1].dwOption = INTERNET_PER_CONN_AUTOCONFIG_URL;

					perOption[1].Value.pszValue = IntPtrFromString(Url);
					break;
				}
				case ProxyType.Global:
				{
					perOption[0].dwOption = INTERNET_PER_CONN_FLAGS;
					perOption[0].Value.dwValue = PROXY_TYPE_PROXY | PROXY_TYPE_DIRECT;

					perOption[1].dwOption = INTERNET_PER_CONN_PROXY_SERVER;

					perOption[1].Value.pszValue = IntPtrFromString(Url);

					perOption[2].dwOption = INTERNET_PER_CONN_PROXY_BYPASS;

					perOption[2].Value.pszValue = IntPtrFromString(Bypass);
					break;
				}
				case ProxyType.Query:
				{
					perOption[0].dwOption = INTERNET_PER_CONN_FLAGS_UI;

					perOption[1].dwOption = INTERNET_PER_CONN_PROXY_SERVER;
					perOption[2].dwOption = INTERNET_PER_CONN_PROXY_BYPASS;
					perOption[3].dwOption = INTERNET_PER_CONN_AUTOCONFIG_URL;

					break;
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}

			// copy the array over into that spot in memory ...
			for (var i = 0; i < perOption.Length; ++i)
			{
				var opt = optionsPtr + i * optSize;
				Marshal.StructureToPtr(perOption[i], opt, false);
			}

			options.pOptions = optionsPtr;
		}

		public void Query()
		{
			Initialize(ref _options, ProxyType.Query);

			var dwLen = (uint)Marshal.SizeOf(typeof(INTERNET_PER_CONN_OPTION_LIST));

			nint optionsPtr = Marshal.AllocCoTaskMem((int)_options.Size);
			_needToFree.Enqueue(optionsPtr);
			Marshal.StructureToPtr(_options, optionsPtr, false);

			if (!NativeMethods.InternetQueryOption(0, INTERNET_OPTION_PER_CONNECTION_OPTION, optionsPtr, ref dwLen))
			{
				throw new SystemException(@"Query Failed");
			}

			var result = Marshal.PtrToStructure<INTERNET_PER_CONN_OPTION_LIST>(optionsPtr);

			var size = Marshal.SizeOf(typeof(INTERNET_PER_CONN_OPTION));
			var perOptions = new INTERNET_PER_CONN_OPTION[4];
			perOptions[0] = Marshal.PtrToStructure<INTERNET_PER_CONN_OPTION>(result.pOptions);
			perOptions[1] = Marshal.PtrToStructure<INTERNET_PER_CONN_OPTION>(result.pOptions + size);
			perOptions[2] = Marshal.PtrToStructure<INTERNET_PER_CONN_OPTION>(result.pOptions + 2 * size);
			perOptions[3] = Marshal.PtrToStructure<INTERNET_PER_CONN_OPTION>(result.pOptions + 3 * size);

			Debug.WriteLine(perOptions[0].Value.dwValue);
			Debug.WriteLine(Marshal.PtrToStringAnsi(perOptions[1].Value.pszValue));
			Debug.WriteLine(Marshal.PtrToStringAnsi(perOptions[2].Value.pszValue));
			Debug.WriteLine(Marshal.PtrToStringAnsi(perOptions[3].Value.pszValue));
		}

		public static string[] GetRasEntryNames()
		{
			var sizeOfRasEntryName = (uint)Marshal.SizeOf(typeof(RASENTRYNAME));

			var cb = sizeOfRasEntryName;
			var entries = 0u;
			var lpRasEntryName = new RASENTRYNAME[1];
			lpRasEntryName[0].Size = sizeOfRasEntryName;

			NativeMethods.RasEnumEntries(0, 0, lpRasEntryName, ref cb, ref entries);

			if (entries == 0)
			{
				return Array.Empty<string>();
			}

			var entryNames = new string[entries];

			lpRasEntryName = new RASENTRYNAME[entries];
			for (var i = 0; i < entries; ++i)
			{
				lpRasEntryName[i].Size = sizeOfRasEntryName;
			}

			NativeMethods.RasEnumEntries(0, 0, lpRasEntryName, ref cb, ref entries);

			for (var i = 0; i < entries; ++i)
			{
				entryNames[i] = lpRasEntryName[i].EntryName;
			}

			return entryNames;
		}

		private nint IntPtrFromString(string? managedString)
		{
			if (managedString is null)
			{
				return 0;
			}
			nint ptr = Marshal.StringToCoTaskMemAuto(managedString);
			_needToFree.Enqueue(ptr);
			return ptr;
		}

		#region Dispose

		private volatile bool _disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					// TODO: 释放托管状态(托管对象)
				}

				// TODO: 释放未托管的资源(未托管的对象)并替代终结器
				// TODO: 将大型字段设置为 null

				while (_needToFree.Count > 0)
				{
					var ptr = _needToFree.Dequeue();
					Marshal.FreeCoTaskMem(ptr);
				}

				_disposedValue = true;
			}
		}

		~SystemProxy()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
