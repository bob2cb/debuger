using System;
using System.Collections.Generic;
using UnityEngine;

public class HiDebug : MonoBehaviour
{
    private enum EDisplay
    {
        Button,
        Panel
    }

    private enum EMouse
    {
        Up,
        Down
    }

    private static float _buttonWidth = 0.2f;

    private static float _buttonHeight = 0.05f;

    private static float _panelHeight = 0.7f;

    private HiDebug.EDisplay _eDisplay;

    private readonly float _mouseClickTime = 0.2f;

    private HiDebug.EMouse _eMouse;

    private float _mouseDownTime;

    private Rect _rect = new Rect(0f, 0f, (float)Screen.width * HiDebug._buttonWidth, (float)Screen.height * HiDebug._buttonHeight);

    private bool _isLogOn = true;

    private bool _isWarnningOn = true;

    private bool _isErrorOn = true;

    private Vector2 _scrollLogPosition;

    private Vector2 _scrollStackPosition;

    private List<LogInfo> logInfos = new List<LogInfo>();

    private LogInfo _stackInfo;

    private void OnGUI()
    {
        this.Button();
        this.Panel();
    }

    private void Button()
    {
        if (this._eDisplay != HiDebug.EDisplay.Button)
        {
            return;
        }
        
        if (this._rect.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.mouseDown)
            {
                this._eMouse = HiDebug.EMouse.Down;
                this._mouseDownTime = Time.realtimeSinceStartup;
            }

            else if (Event.current.type == EventType.mouseUp)
            {
                this._eMouse = HiDebug.EMouse.Up;
                if (Time.realtimeSinceStartup - this._mouseDownTime < this._mouseClickTime)
                {
                    this._eDisplay = HiDebug.EDisplay.Panel;
                }
            }
        }
        if (this._eMouse == HiDebug.EMouse.Down && Event.current.type == EventType.mouseDrag)
        {
            this._rect.x = Event.current.mousePosition.x - this._rect.width / 2f;
            this._rect.y = Event.current.mousePosition.y - this._rect.height / 2f;
        }
        
        GUI.Button(this._rect, "On", this.GetGUISkin(GUI.skin.button, Color.white, TextAnchor.MiddleCenter));
    }

    private void Panel()
    {
        if (this._eDisplay != HiDebug.EDisplay.Panel)
        {
            return;
        }
        GUI.Window(0, new Rect(0f, 0f, (float)Screen.width, (float)Screen.height * HiDebug._panelHeight), new GUI.WindowFunction(this.LogWindow), "", this.GetGUISkin(GUI.skin.window, Color.white, TextAnchor.UpperCenter));
        GUI.Window(1, new Rect(0f, (float)Screen.height * HiDebug._panelHeight, (float)Screen.width, (float)Screen.height * (1f - HiDebug._panelHeight)), new GUI.WindowFunction(this.StackWindow), "Stack", this.GetGUISkin(GUI.skin.window, Color.white, TextAnchor.UpperCenter));
    }

    private void LogWindow(int windowID)
    {
        if (GUI.Button(new Rect(0f, 0f, (float)Screen.width * HiDebug._buttonWidth, (float)Screen.height * HiDebug._buttonHeight), "Clear", this.GetGUISkin(GUI.skin.button, Color.white, TextAnchor.MiddleCenter)))
        {
            this.logInfos.Clear();
            this._stackInfo = null;
        }
        if (GUI.Button(new Rect((float)Screen.width * (1f - HiDebug._buttonWidth), 0f, (float)Screen.width * HiDebug._buttonWidth, (float)Screen.height * HiDebug._buttonHeight), "Close", this.GetGUISkin(GUI.skin.button, Color.white, TextAnchor.MiddleCenter)))
        {
            this._eDisplay = HiDebug.EDisplay.Button;
        }


        int top = GUI.skin.window.padding.top;

        GUIStyle gUISkin = this.GetGUISkin(GUI.skin.toggle, Color.white, TextAnchor.UpperLeft);
        this._isLogOn = GUI.Toggle(new Rect((float)Screen.width * HiDebug._buttonWidth * 1.2f, (float)top, (float)Screen.width * HiDebug._buttonWidth, (float)Screen.height * HiDebug._buttonHeight - (float)top), this._isLogOn, "Log", gUISkin);

        GUIStyle gUISkin2 = this.GetGUISkin(GUI.skin.toggle, Color.yellow, TextAnchor.UpperLeft);
        this._isWarnningOn = GUI.Toggle(new Rect((float)Screen.width * HiDebug._buttonWidth * 2f, (float)top, (float)Screen.width * HiDebug._buttonWidth, (float)Screen.height * HiDebug._buttonHeight - (float)top), this._isWarnningOn, "Warnning", gUISkin2);

        GUIStyle gUISkin3 = this.GetGUISkin(GUI.skin.toggle, Color.red, TextAnchor.UpperLeft);
        this._isErrorOn = GUI.Toggle(new Rect((float)Screen.width * HiDebug._buttonWidth * 3.1f, (float)top, (float)Screen.width * HiDebug._buttonWidth, (float)Screen.height * HiDebug._buttonHeight - (float)top), this._isErrorOn, "Error", gUISkin3);

        GUILayout.Space((float)Screen.height * HiDebug._buttonHeight - (float)top);

        this._scrollLogPosition = GUILayout.BeginScrollView(this._scrollLogPosition, new GUILayoutOption[0]);
        this.LogItem();
        GUILayout.EndScrollView();
    }

    private void LogItem()
    {
        int i = this.logInfos.Count - 1;
        while (i >= 0)
        {
            if (this.logInfos[i].Type == LogType.Log)
            {
                if (this._isLogOn)
                {
                    goto IL_6D;
                }
            }
            else if (this.logInfos[i].Type == LogType.Warning)
            {
                if (this._isWarnningOn)
                {
                    goto IL_6D;
                }
            }
            else if (this.logInfos[i].Type != LogType.Error || this._isErrorOn)
            {
                goto IL_6D;
            }
            IL_C5:
            i--;
            continue;
            IL_6D:
            if (GUILayout.Button(this.logInfos[i].Condition, this.GetGUISkin(GUI.skin.button, this.GetColor(this.logInfos[i].Type), TextAnchor.MiddleLeft), new GUILayoutOption[0]))
            {
                this._stackInfo = this.logInfos[i];
                goto IL_C5;
            }
            goto IL_C5;
        }
    }

    private void StackWindow(int windowID)
    {
        this._scrollStackPosition = GUILayout.BeginScrollView(this._scrollStackPosition, new GUILayoutOption[0]);
        this.StackItem();
        GUILayout.EndScrollView();
    }

    private void StackItem()
    {
        if (this._stackInfo == null)
        {
            return;
        }
        string[] array = this._stackInfo.StackTrace.Split(new char[]
        {
            '\n'
        });
        for (int i = 0; i < array.Length; i++)
        {
            GUILayout.Label(array[i], this.GetGUISkin(GUI.skin.label, this.GetColor(this._stackInfo.Type),(TextAnchor)3), new GUILayoutOption[0]);
        }
    }

    public void UpdateLog(LogInfo logInfo)
    {
        this.logInfos.Add(logInfo);
    }

    private GUIStyle GetGUISkin(GUIStyle guiStyle, Color color, TextAnchor style)
    {
        guiStyle.normal.textColor = color;
        guiStyle.hover.textColor = color;
        guiStyle.active.textColor = color;
        guiStyle.onNormal.textColor = color;
        guiStyle.onHover.textColor = color;
        guiStyle.onActive.textColor = color;
        guiStyle.margin = new RectOffset(0, 0, 0, 0);
        guiStyle.alignment = style;
        guiStyle.fontSize = Debuger.FontSize;
        return guiStyle;
    }

    private Color GetColor(LogType type)
    {
        if ((int)type == 3)
        {
            return Color.white;
        }
        if ((int)type == 2)
        {
            return Color.yellow;
        }
        if ((int)type == 0)
        {
            return Color.red;
        }
        if ((int)type == 4)
        {
            return Color.red;
        }
        return Color.white;
    }
}
