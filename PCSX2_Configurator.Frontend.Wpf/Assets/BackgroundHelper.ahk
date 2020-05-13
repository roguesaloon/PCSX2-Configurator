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

    if InStr(data, "OpenGSPlugin->") {
        OpenPlugin("GS", data)
    }

    if InStr(data, "OpenSPU2Plugin->") {
        OpenPlugin("SPU2", data)
    }

    if InStr(data, "OpenPADPlugin->") {
        OpenPlugin("PAD", data)
    }

    return true
}

OpenPlugin(plugin, data)
{
    parts := StrSplit(data, ["->","|"])
    path := parts[2]
    configPath:= parts[3]
    SetWorkingDir, %configPath%

    FileCreateDir, inis
    FileCopy, *.ini, inis

    openPath = %path%\%plugin%configure
    closePath = %path%\%plugin%close

    DllCall(openPath)
    DllCall(closePath)

    FileCopy, inis\*ini, %A_WorkingDir%, 1
    FileRemoveDir, inis, 1
}

KillPCSX2()
{
    for process in ComObjGet("winmgmts:").ExecQuery("Select * from Win32_Process")
    {
        if process.Name == "pcsx2.exe" OR InStr(process.Name, "pcsx2-r")
            Process, Close, % process.Name
    }
}