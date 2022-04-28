using System.Runtime.InteropServices;
using WindowsProxy.Enums;
using WindowsProxy.Models;
using static WindowsProxy.Enums.INTERNET_OPTION;
using static WindowsProxy.Enums.INTERNET_OPTION_PER_CONN_FLAGS;
using static WindowsProxy.Enums.INTERNET_PER_CONN_OptionEnum;
using static WindowsProxy.NativeMethods;

namespace WindowsProxy;
#if NET6_0_OR_GREATER
[System.Runtime.Versioning.SupportedOSPlatform(@"windows")]
#endif
public sealed class ProxyService : IDisposable
{
	public static readonly string[] LanIp =
	{
		@"<local>",
		@"localhost",
		@"127.*",
		@"10.*",
		@"172.16.*",
		@"172.17.*",
		@"172.18.*",
		@"172.19.*",
		@"172.20.*",
		@"172.21.*",
		@"172.22.*",
		@"172.23.*",
		@"172.24.*",
		@"172.25.*",
		@"172.26.*",
		@"172.27.*",
		@"172.28.*",
		@"172.29.*",
		@"172.30.*",
		@"172.31.*",
		@"192.168.*"
	};

	private readonly Queue<nint> _needToFree = new();

	public string Server { get; set; } = string.Empty;

	public string AutoConfigUrl { get; set; } = string.Empty;

	public string Bypass { get; set; } = @"<local>";

	private static void Initialize(ref INTERNET_PER_CONN_OPTION_LIST options, uint optionCount)
	{
		uint dwBufferSize = (uint)Marshal.SizeOf(typeof(INTERNET_PER_CONN_OPTION_LIST));
		options.Size = dwBufferSize;

		options.OptionCount = optionCount;
		options.OptionError = 0;
	}

	private nint AllocOptions(int length)
	{
		int optSize = Marshal.SizeOf(typeof(INTERNET_PER_CONN_OPTION));
		return Alloc(length * optSize);
	}

	private static void OptionsToPtr(ref INTERNET_PER_CONN_OPTION[] perOption, nint baseAddress)
	{
		int optSize = Marshal.SizeOf(typeof(INTERNET_PER_CONN_OPTION));
		for (int i = 0; i < perOption.Length; ++i)
		{
			nint opt = baseAddress + i * optSize;
			Marshal.StructureToPtr(perOption[i], opt, false);
		}
	}

	private void Initialize(ref INTERNET_PER_CONN_OPTION_LIST options, Operation type)
	{
		uint optionCount = (uint)type;
		INTERNET_PER_CONN_OPTION[] perOption = new INTERNET_PER_CONN_OPTION[optionCount];

		Initialize(ref options, optionCount);
		nint optionsPtr = AllocOptions(perOption.Length);

		switch (type)
		{
			case Operation.Direct:
			{
				perOption[0].dwOption = INTERNET_PER_CONN_FLAGS;
				perOption[0].Value.dwValue = PROXY_TYPE_AUTO_DETECT | PROXY_TYPE_DIRECT;
				break;
			}
			case Operation.Pac:
			{
				perOption[0].dwOption = INTERNET_PER_CONN_FLAGS;
				perOption[0].Value.dwValue = PROXY_TYPE_AUTO_PROXY_URL | PROXY_TYPE_DIRECT;

				perOption[1].dwOption = INTERNET_PER_CONN_AUTOCONFIG_URL;
				perOption[1].Value.pszValue = StringToMem(AutoConfigUrl);
				break;
			}
			case Operation.Global:
			{
				perOption[0].dwOption = INTERNET_PER_CONN_FLAGS;
				perOption[0].Value.dwValue = PROXY_TYPE_PROXY | PROXY_TYPE_DIRECT;

				perOption[1].dwOption = INTERNET_PER_CONN_PROXY_SERVER;
				perOption[1].Value.pszValue = StringToMem(Server);

				perOption[2].dwOption = INTERNET_PER_CONN_PROXY_BYPASS;
				perOption[2].Value.pszValue = StringToMem(Bypass);
				break;
			}
			case Operation.Query:
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

		OptionsToPtr(ref perOption, optionsPtr);

		options.pOptions = optionsPtr;
	}

	public ProxyStatus Query()
	{
		try
		{
			INTERNET_PER_CONN_OPTION_LIST options = new();
			Initialize(ref options, Operation.Query);

			uint dwLen = (uint)Marshal.SizeOf(typeof(INTERNET_PER_CONN_OPTION_LIST));

			nint optionsPtr = Alloc((int)options.Size);
			Marshal.StructureToPtr(options, optionsPtr, false);

			if (!InternetQueryOption(0, INTERNET_OPTION_PER_CONNECTION_OPTION, optionsPtr, ref dwLen))
			{
				throw new SystemException(@"Query Failed");
			}

			INTERNET_PER_CONN_OPTION_LIST result = Marshal.PtrToStructure<INTERNET_PER_CONN_OPTION_LIST>(optionsPtr);

			int size = Marshal.SizeOf(typeof(INTERNET_PER_CONN_OPTION));
			INTERNET_PER_CONN_OPTION[] perOptions = new INTERNET_PER_CONN_OPTION[4];
			for (int i = 0; i < perOptions.Length; ++i)
			{
				perOptions[i] = Marshal.PtrToStructure<INTERNET_PER_CONN_OPTION>(result.pOptions + i * size);
			}

			ProxyStatus res = new();
			res.Parse(perOptions);

			return res;
		}
		finally
		{
			Free();
		}
	}

	public bool Direct()
	{
		try
		{
			INTERNET_PER_CONN_OPTION_LIST options = new();
			Initialize(ref options, Operation.Direct);
			return Apply(ref options);
		}
		finally
		{
			Free();
		}
	}

	public bool Pac()
	{
		try
		{
			INTERNET_PER_CONN_OPTION_LIST options = new();
			Initialize(ref options, Operation.Pac);
			return Apply(ref options);
		}
		finally
		{
			Free();
		}
	}

	public bool Global()
	{
		try
		{
			INTERNET_PER_CONN_OPTION_LIST options = new();
			Initialize(ref options, Operation.Global);
			return Apply(ref options);
		}
		finally
		{
			Free();
		}
	}

	public bool Set(ProxyStatus status)
	{
		try
		{
			INTERNET_PER_CONN_OPTION_LIST options = new();
			INTERNET_PER_CONN_OPTION[] perOption = ToOptions(status);

			Initialize(ref options, (uint)perOption.Length);
			nint optionsPtr = AllocOptions(perOption.Length);

			OptionsToPtr(ref perOption, optionsPtr);

			options.pOptions = optionsPtr;

			return Apply(ref options);
		}
		finally
		{
			Free();
		}
	}

	private bool Apply(ref INTERNET_PER_CONN_OPTION_LIST options)
	{
		bool res = true;
		foreach (string name in GetRasEntryNames())
		{
			if (!ApplyConnect(ref options, name))
			{
				res = false;
			}
		}

		if (!ApplyConnect(ref options, null))
		{
			res = false;
		}

		return res;
	}

	private bool ApplyConnect(ref INTERNET_PER_CONN_OPTION_LIST options, string? conn)
	{
		options.Connection = StringToMem(conn);

		nint optionsPtr = Alloc((int)options.Size);
		Marshal.StructureToPtr(options, optionsPtr, false);

		uint dwBufferSize = (uint)Marshal.SizeOf(typeof(INTERNET_PER_CONN_OPTION_LIST));

		return InternetSetOption(0, INTERNET_OPTION_PER_CONNECTION_OPTION, optionsPtr, dwBufferSize)
			   && InternetSetOption(0, INTERNET_OPTION_SETTINGS_CHANGED, 0, 0)
			   && InternetSetOption(0, INTERNET_OPTION_PROXY_SETTINGS_CHANGED, 0, 0)
			   && InternetSetOption(0, INTERNET_OPTION_REFRESH, 0, 0);
	}

	public static string[] GetRasEntryNames()
	{
		uint sizeOfRasEntryName = (uint)Marshal.SizeOf(typeof(RASENTRYNAME));

		uint cb = sizeOfRasEntryName;
		uint entries = 0u;
		RASENTRYNAME[] lpRasEntryName = new RASENTRYNAME[1];
		lpRasEntryName[0].Size = sizeOfRasEntryName;

		_ = RasEnumEntries(0, 0, lpRasEntryName, ref cb, ref entries);

		if (entries == 0)
		{
			return Array.Empty<string>();
		}

		string[] entryNames = new string[entries];

		lpRasEntryName = new RASENTRYNAME[entries];
		for (int i = 0; i < entries; ++i)
		{
			lpRasEntryName[i].Size = sizeOfRasEntryName;
		}

		_ = RasEnumEntries(0, 0, lpRasEntryName, ref cb, ref entries);

		for (int i = 0; i < entries; ++i)
		{
			entryNames[i] = lpRasEntryName[i].EntryName;
		}

		return entryNames;
	}

	private INTERNET_PER_CONN_OPTION[] ToOptions(ProxyStatus status)
	{
		INTERNET_PER_CONN_OPTION[] res = new INTERNET_PER_CONN_OPTION[4];

		INTERNET_OPTION_PER_CONN_FLAGS flag = 0;
		if (status.IsDirect)
		{
			flag |= PROXY_TYPE_DIRECT;
		}

		if (status.IsProxy)
		{
			flag |= PROXY_TYPE_PROXY;
		}

		if (status.IsAutoProxyUrl)
		{
			flag |= PROXY_TYPE_AUTO_PROXY_URL;
		}

		if (status.IsAutoDetect)
		{
			flag |= PROXY_TYPE_AUTO_DETECT;
		}

		res[0].dwOption = INTERNET_PER_CONN_FLAGS;
		res[0].Value.dwValue = flag;

		res[1].dwOption = INTERNET_PER_CONN_AUTOCONFIG_URL;
		res[1].Value.pszValue = StringToMem(status.AutoConfigUrl);

		res[2].dwOption = INTERNET_PER_CONN_PROXY_SERVER;
		res[2].Value.pszValue = StringToMem(status.ProxyServer);

		res[3].dwOption = INTERNET_PER_CONN_PROXY_BYPASS;
		res[3].Value.pszValue = StringToMem(status.ProxyBypass);

		return res;
	}

	#region Utils

	private static string GetIdnAsciiString(string str)
	{
		try
		{
			Uri uri = new(str);
			string query = uri.Query;
			string host = uri.Host;
			if (!string.IsNullOrEmpty(host))
			{
				string result = uri.GetLeftPart(UriPartial.Path).Replace(host, uri.IdnHost) + query;
				return result;
			}
		}
		catch (UriFormatException)
		{
		}

		return str;
	}

	#endregion

	#region Alloc

	private nint Alloc(int size)
	{
		IntPtr ptr = Marshal.AllocCoTaskMem(size);
		_needToFree.Enqueue(ptr);
		return ptr;
	}

	private nint StringToMem(string? managedString)
	{
		if (managedString is null)
		{
			return 0;
		}

		string str = managedString is @"" ? string.Empty : GetIdnAsciiString(managedString);
		nint ptr = Marshal.StringToCoTaskMemAuto(str);
		_needToFree.Enqueue(ptr);
		return ptr;
	}

	#endregion

	#region Dispose

	private void Free()
	{
		while (_needToFree.Count > 0)
		{
			Marshal.FreeCoTaskMem(_needToFree.Dequeue());
		}
	}

	private volatile bool _disposedValue;

	private void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{

			}
			Free();
			_disposedValue = true;
		}
	}

	~ProxyService()
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
