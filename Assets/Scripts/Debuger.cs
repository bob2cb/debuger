using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Debuger
{
    private static int _fontSize = (int)((float)Screen.height * 0.02f);

    private static bool _isOnConsole;

    private static bool _isOnText;

    private static bool _isOnScreen;

    private static HiDebug _instance;

    private static bool _isCallBackSet;

    private static List<LogInfo> loginfosCache = new List<LogInfo>();

    public static int FontSize
    {
        get
        {
            return Debuger._fontSize;
        }
        set
        {
            Debuger._fontSize = value;
        }
    }

    public static void SetFontSize(int size)
    {
        Debuger._fontSize = size;
    }

    public static void EnableHiDebugLogs(bool isOn)
    {
        Debuger._isOnConsole = isOn;
        if (Debuger._isOnConsole && !Debuger._isCallBackSet)
        {
            Debuger._isCallBackSet = true;
            Application.logMessageReceivedThreaded += new Application.LogCallback(Debuger.LogCallBack);
            Debuger.Log("HiDebug start up success, you can download newest version from https://github.com/hiramtan/HiDebug_unity");
        }
    }

    public static void EnableOnText(bool isOn)
    {
        Debuger._isOnText = isOn;
        if (Debuger._isOnText)
        {
            Debuger.EnableHiDebugLogs(true);
        }
    }

    public static void EnableOnScreen(bool isOn)
    {
        Debuger._isOnScreen = isOn;
        if (Debuger._isOnScreen)
        {
            Debuger.EnableHiDebugLogs(true);
            if (Debuger._instance == null)
            {
                GameObject expr_2A = new GameObject("HiDebug");
                UnityEngine.Object.DontDestroyOnLoad(expr_2A);
                Debuger._instance = expr_2A.AddComponent<HiDebug>();
            }
        }
    }

    public static void Log(object obj)
    {
        if (Debuger._isOnConsole)
        {
            string text = string.Format(Debuger.GetTime(), obj);
            text = "<color=white>" + text + "</color>";
            Debug.Log(text);
        }
    }

    public static void LogWarning(object obj)
    {
        if (Debuger._isOnConsole)
        {
            string text = string.Format(Debuger.GetTime(), obj);
            text = "<color=yellow>" + text + "</color>";
            Debug.LogWarning(text);
        }
    }

    public static void LogError(object obj)
    {
        if (Debuger._isOnConsole)
        {
            string text = string.Format(Debuger.GetTime(), obj);
            text = "<color=red>" + text + "</color>";
            Debug.LogError(text);
        }
    }

    private static void LogCallBack(string condition, string stackTrace, LogType type)
    {
        LogInfo expr_08 = new LogInfo(condition, stackTrace, type);
        Debuger.OnText(expr_08);
        Debuger.OnScreen(expr_08);
    }

    private static string GetTime()
    {
        return DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss") + ": {0}";
    }

    private static void OnText(LogInfo logInfo)
    {
        if (Debuger._isOnText)
        {
            StreamWriter expr_1B = File.AppendText(Application.persistentDataPath + "/HiDebug.txt");
            expr_1B.WriteLine(logInfo.Condition + logInfo.StackTrace);
            expr_1B.Close();
        }
    }

    private static void ProcessloginfosCache()
    {
        for (int i = 0; i < Debuger.loginfosCache.Count; i++)
        {
            Debuger._instance.UpdateLog(Debuger.loginfosCache[i]);
        }
        Debuger.loginfosCache.Clear();
    }

    private static void OnScreen(LogInfo logInfo)
    {
        if (Debuger._isOnScreen)
        {
            if (Debuger._instance == null)
            {
                Debuger.loginfosCache.Add(logInfo);
                return;
            }
            Debuger.ProcessloginfosCache();
            Debuger._instance.UpdateLog(logInfo);
        }
    }
}
