#NoEnv
#NoTrayIcon
#SingleInstance Force
Gui +LastFound -AlwaysOnTop -Caption +ToolWindow
Gui, Show

OnMessage(0x4a, "Receive_WM_COPYDATA")
Esc::KillPCSX2()

Receive_WM_COPYDATA(wParam, lParam)
{
    address := NumGet(lParam + 2*A_PtrSize)  ; Retrieves the CopyDataStruct's lpData member.
    data := StrGet(address)  ; Copy the string out of the structure.

    if InStr(data, "OpenGSPlugin->")
    {
        parts := StrSplit(data, "->")
        path := parts[2]
        configPath = %path%\GSconfigure
        closePath = %path%\GSclose
        DllCall(configPath)
        DllCall(closePath)
    }

    if InStr(data, "OpenSPU2Plugin->")
    {
        parts := StrSplit(data, "->")
        path := parts[2]
        configPath = %path%\SPU2configure
        closePath = %path%\SPU2close
        DllCall(configPath)
        DllCall(closePath)
    }

    if InStr(data, "OpenPADPlugin->")
    {
        parts := StrSplit(data, "->")
        path := parts[2]
        configPath = %path%\PADconfigure
        closePath = %path%\PADclose
        DllCall(configPath)
        DllCall(closePath)
    }
    
    return true
}

KillPCSX2()
{
    for process in ComObjGet("winmgmts:").ExecQuery("Select * from Win32_Process")
    {
        if process.Name == "pcsx2.exe" OR InStr(process.Name, "pcsx2-r")
            Process, Close, % process.Name
    }
}

