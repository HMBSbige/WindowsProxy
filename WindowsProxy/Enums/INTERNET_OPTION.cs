namespace WindowsProxy.Enums;

/// <summary>
/// https://docs.microsoft.com/en-us/windows/win32/wininet/option-flags
/// </summary>
internal enum INTERNET_OPTION : uint
{
	INTERNET_OPTION_PER_CONNECTION_OPTION = 75,

	INTERNET_OPTION_SETTINGS_CHANGED = 39,

	INTERNET_OPTION_REFRESH = 37,

	INTERNET_OPTION_PROXY_SETTINGS_CHANGED = 95
}
