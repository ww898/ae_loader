#include <windows.h>
#include <wchar.h>

#include "declarations.hpp"
#include "utility.hpp"

namespace jbhack {

struct event_log_reporter final
{
    event_log_reporter() noexcept :
        has_handle_(false),
        handle_(nullptr)
    {
    }

    ~event_log_reporter() noexcept
    {
        if (handle_)
            DeregisterEventSource(handle_);
    }

    void report(WORD const type, DWORD const eventId) noexcept
    {
        ensure_handle();
        if (handle_)
            ReportEventW(handle_, type, 0, eventId, nullptr, 0, 0, nullptr, nullptr);
    }

    void report(WORD const type, DWORD const eventId, WCHAR const * const text) noexcept
    {
        ensure_handle();
        if (handle_)
        {
            LPCWSTR strings[] = {text};
            ReportEventW(handle_, type, 0, eventId, nullptr, 1, 0, strings, nullptr);
        }
    }

private:
    bool has_handle_;
    HANDLE handle_;

    void ensure_handle() noexcept
    {
        if (has_handle_)
            return;
        handle_ = RegisterEventSourceW(nullptr, app_name);
        has_handle_ = true;
    }
};

bool do_hack() noexcept
{
    event_log_reporter reporter;

    WCHAR ini_file[1024];
    if (!get_ini_file(ini_file))
    {
        reporter.report(EVENTLOG_ERROR_TYPE, 301);
        return false;
    }
    auto && on_exit = make_on_exit_scope([ini_file] { DeleteFileW(ini_file); });

    bool const full_reporting = GetPrivateProfileIntW(app_name, key_enable_event_logs, 0, ini_file) != 0;

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
            if (full_reporting)
                reporter.report(EVENTLOG_ERROR_TYPE, 102, executable);
            return false;
        }
        if (full_reporting)
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
        if (full_reporting)
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
