#NoEnv
#NoTrayIcon
#SingleInstance Ignore

Esc::KillPCSX2()

KillPCSX2()
{
    for process in ComObjGet("winmgmts:").ExecQuery("Select * from Win32_Process")
    {
        if process.Name == "pcsx2.exe" OR InStr(process.Name, "pcsx2-r")
            Process, Close, % process.Name
    }
}