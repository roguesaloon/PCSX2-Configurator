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

    if(InStr(data, "OpenGSPlugin_"))
    {
        parts :=  StrSplit(data, "_")
        path := parts[2]
        ToolTip, %path%
    }
    
    return true  ; Returning 1 (true) is the traditional way to acknowledge this message.
}

KillPCSX2()
{
    for process in ComObjGet("winmgmts:").ExecQuery("Select * from Win32_Process")
    {
        if process.Name == "pcsx2.exe" OR InStr(process.Name, "pcsx2-r")
            Process, Close, % process.Name
    }
}

