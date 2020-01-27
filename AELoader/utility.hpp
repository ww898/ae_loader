#pragma once

#include <windows.h>
#include <wchar.h>
#include <tlhelp32.h>

#include <type_traits>

namespace jbhack {

template<typename Fn>
struct on_exit_scope final
{
    template<typename Fn1>
    on_exit_scope(Fn1 && fn) :
        fn_(std::forward<Fn1>(fn))
    {
    }

    on_exit_scope(on_exit_scope const &) = delete;
    on_exit_scope & operator=(on_exit_scope const &) = delete;

    on_exit_scope(on_exit_scope &&) = delete;
    on_exit_scope & operator=(on_exit_scope &&) = delete;

    ~on_exit_scope()
    {
        try
        {
            std::move(fn_)();
        }
        catch (...)
        {
        }
    }

private:
    Fn fn_;
};

template<typename Fn>
on_exit_scope<std::decay_t<Fn>> make_on_exit_scope(Fn && fn)
{
    return{ std::forward<Fn>(fn) };
}

template <typename Type, DWORD size>
constexpr DWORD elements_of(Type (&)[size]) noexcept { return size; }

inline DWORD get_parent_process_id() noexcept
{
    HANDLE const handle = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (handle != INVALID_HANDLE_VALUE)
    {
        auto && on_exit = make_on_exit_scope([handle] { CloseHandle(handle); });
        PROCESSENTRY32 pe;
        pe.dwSize = sizeof pe;
        if (Process32First(handle, &pe))
            do
                if (pe.th32ProcessID == GetCurrentProcessId())
                    return pe.th32ParentProcessID;
            while (Process32Next(handle, &pe));
    }
    return 0xFFFFFFFFu; // Note: Zero is reserved for system process!!!
}

template<size_t size>
bool get_ini_file(WCHAR (&ini_file)[size])
{
    DWORD pos = GetEnvironmentVariableW(L"ProgramData", ini_file, elements_of(ini_file));
    if (pos == elements_of(ini_file))
        return false;
    if (swprintf_s(ini_file + pos, size - pos, format_ini_file, get_parent_process_id()) < 0)
        return false;
    return true;
}

}
