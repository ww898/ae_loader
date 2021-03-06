#include <windows.h>

#include "../AELoader/declarations.hpp"
#include "../AELoader/utility.hpp"

#include <wchar.h>

int main()
{
    WCHAR ini_file[1024];
    if (!jbhack::get_ini_file(ini_file))
        return 2;
    if (!WritePrivateProfileStringW(jbhack::app_name, jbhack::key_enable_event_logs, L"1", ini_file))
        return 2;
    if (!WritePrivateProfileStringW(jbhack::app_name, jbhack::key_command_line, L"C:\\Windows\\System32\\cmd.exe",
        ini_file))
        return 2;
    HMODULE const handle = LoadLibraryW(jbhack::dll_name);
    auto const message = handle ? L"Unexpected behavior." : L"Failed to load injection DLL.";
    MessageBoxW(nullptr, message, L"Injection failed", MB_OK);
    return 1;
}
