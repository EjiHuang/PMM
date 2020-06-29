#include <iostream>
#include <filesystem>

#include <Windows.h>
#include <WdkTypes.h>
#include <CtlTypes.h>
#include <User-Bridge.h>

using namespace Processes::MemoryManagement;
namespace fs = std::filesystem;

int main()
{
    auto sys_path = fs::current_path().wstring() + L"\\Kernel-Bridge.sys";
    KbLoader::KbLoadAsFilter(sys_path.c_str(), L"260000");

    PVOID Buffer = VirtualAlloc(NULL, 4096, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
    VirtualLock(Buffer, 4096);

    VirtualFree(Buffer, 4096, MEM_FREE);

    //WdkTypes::HANDLE process_handle;
    //Processes::Descriptors::KbOpenProcess(25056, &process_handle);

    unsigned long long module_base = 0x40000;
    unsigned long long temp;

    Processes::MemoryManagement::KbReadProcessMemory(25056, (module_base + 0x11659dc), &temp, sizeof(temp));
    Processes::MemoryManagement::KbReadProcessMemory(25056, (temp + 0x4), &temp, sizeof(temp));
    Processes::MemoryManagement::KbReadProcessMemory(25056, (temp + 0x4), &temp, sizeof(temp));
    Processes::MemoryManagement::KbReadProcessMemory(25056, (temp + 0x98), &temp, sizeof(temp));
    Processes::MemoryManagement::KbReadProcessMemory(25056, (temp + 0x4c0), &temp, sizeof(temp));
    
    // Processes::Descriptors::KbCloseHandle(process_handle);
    KbLoader::KbUnload();
}