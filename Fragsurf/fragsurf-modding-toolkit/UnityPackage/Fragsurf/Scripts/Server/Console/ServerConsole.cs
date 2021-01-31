using System;
using UnityEngine;
using Fragsurf;

public class ServerConsole : MonoBehaviour
{
    Windows.ConsoleWindow console = new Windows.ConsoleWindow();
    Windows.ConsoleInput input = new Windows.ConsoleInput();

    string strInput;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        console.Initialize();
        console.SetTitle("Fragsurf Server");

        input.OnInputText += OnInputText;

        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
        //Application.logMessageReceived += HandleLog;
        DevConsole.OnMessageLogged += FSConsole_OnMessageLogged;

        Debug.Log("Console Started");
    }

    private void FSConsole_OnMessageLogged(string message)
    {
        var msg = System.Text.RegularExpressions.Regex.Replace(message, "<.*?>", string.Empty);
        HandleLog(msg, string.Empty, LogType.Log);
    }

    public void SetTitle(string title)
    {
        console.SetTitle("Fragsurf Server - " + title);
    }

    void OnInputText(string obj)
    {
        DevConsole.ExecuteLine(obj);
    }

    void HandleLog(string message, string stackTrace, LogType type)
    {
        if (type == LogType.Warning)
            System.Console.ForegroundColor = ConsoleColor.Yellow;
        else if (type == LogType.Error)
            System.Console.ForegroundColor = ConsoleColor.Red;
        else
            System.Console.ForegroundColor = ConsoleColor.White;

        // We're half way through typing something, so clear this line ..
        if (Console.CursorLeft != 0)
            input.ClearLine();

        System.Console.WriteLine(message);

        // If we were typing something re-add it.
        input.RedrawInputLine();
    }

    void Update()
    {
        input.Update();
    }

    void OnDestroy()
    {
        console.Shutdown();
    }
}