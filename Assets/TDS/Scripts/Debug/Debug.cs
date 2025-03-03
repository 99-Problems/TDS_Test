#if UNITY_EDITOR
#define LOG_ENABLE
#endif

using System;
using System.Text;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using Object = UnityEngine.Object;

/// 
/// It overrides UnityEngine.Debug to mute debug messages completely on a platform-specific basis.
/// 
/// Putting this inside of 'Plugins' foloder is ok.
/// 
/// Important:
///     Other preprocessor directives than 'UNITY_EDITOR' does not correctly work.
/// 
/// Note:
///     [Conditional] attribute indicates to compilers that a method call or attribute should be 
///     ignored unless a specified conditional compilation symbol is defined.
/// 
/// See Also: 
///     http://msdn.microsoft.com/en-us/library/system.diagnostics.conditionalattribute.aspx
/// 
/// 2012.11. @kimsama
/// 
public static class Debug
{
    [System.Diagnostics.Conditional("LOG_ENABLE")]
    public static void Log(object message)
    {
        UnityEngine.Debug.Log(message);
    }

    [System.Diagnostics.Conditional("LOG_ENABLE")]
    public static void Log(object message, Color color)
    {
        ColorLog(message, color);
    }

    [System.Diagnostics.Conditional("LOG_ENABLE")]
    public static void ColorLog(object message)
    {
        //default = cyan
        var _message = $"<color=cyan>{message}</color>";
        UnityEngine.Debug.Log(_message);
    }

    [System.Diagnostics.Conditional("LOG_ENABLE")]
    public static void ColorLog(object message, Color color)
    {
        // Color를 Hex 코드 문자열로 변환
        string hexColor = ColorUtility.ToHtmlStringRGB(color);
        var _message = $"<color=#{hexColor}>{message}</color>";
        UnityEngine.Debug.Log(_message);
    }


    [System.Diagnostics.Conditional("LOG_ENABLE")]
    public static void Log(object message, UnityEngine.Object context)
    {
        UnityEngine.Debug.Log(message, context);
    }

    [System.Diagnostics.Conditional("LOG_ENABLE")]
    public static void LogFormat(string format, params object[] args)
    {
        UnityEngine.Debug.LogFormat(format, args);
    }

    [System.Diagnostics.Conditional("LOG_ENABLE")]
    public static void LogError(object message)
    {
        UnityEngine.Debug.LogError(message);
    }

    [System.Diagnostics.Conditional("LOG_ENABLE")]
    public static void LogError(object message, UnityEngine.Object context)
    {
        UnityEngine.Debug.LogError(message, context);
    }

    [System.Diagnostics.Conditional("LOG_ENABLE")]
    public static void LogErrorFormat(string format, params object[] args)
    {
        UnityEngine.Debug.LogErrorFormat(format, args);
    }


    [System.Diagnostics.Conditional("LOG_ENABLE")]
    public static void LogWarning(object message)
    {
        UnityEngine.Debug.LogWarning(message.ToString());
    }

    [System.Diagnostics.Conditional("LOG_ENABLE")]
    public static void LogWarning(object message, UnityEngine.Object context)
    {
        UnityEngine.Debug.LogWarning(message.ToString(), context);
    }

    [System.Diagnostics.Conditional("LOG_ENABLE")]
    public static void LogWarningFormat(string format, params object[] args)
    {
        UnityEngine.Debug.LogWarningFormat(format, args);
    }

    [System.Diagnostics.Conditional("LOG_ENABLE")]
    public static void LogWarningFormat(Object context, string format, params object[] args)
    {
        UnityEngine.Debug.LogWarningFormat(context, format, args);
    }

    [System.Diagnostics.Conditional("LOG_ENABLE")]
    public static void DrawLine(Vector3 start, Vector3 end, Color color = default(Color), float duration = 0.0f, bool depthTest = true)
    {
        UnityEngine.Debug.DrawLine(start, end, color, duration, depthTest);
    }

    [System.Diagnostics.Conditional("LOG_ENABLE")]
    public static void DrawRay(Vector3 start, Vector3 dir, Color color = default(Color), float duration = 0.0f, bool depthTest = true)
    {
        UnityEngine.Debug.DrawRay(start, dir, color, duration, depthTest);
    }

    [System.Diagnostics.Conditional("LOG_ENABLE")]
    public static void Assert(bool condition, string msg = "")
    {
        UnityEngine.Debug.Assert(condition, msg);
    }

    [System.Diagnostics.Conditional("LOG_ENABLE")]
    public static void LogException(Exception exception, Object context)
    {
        UnityEngine.Debug.LogException(exception, context);
    }

    public static void Break()
    {
        UnityEngine.Debug.Break();
    }

    [System.Diagnostics.Conditional("LOG_ENABLE")]
    public static void LogFormat(GameObject _gameObject, string _bBHasBeenUpgradedIVersionI, string _name, string _s, int _mVersion,
        int _expectedVersion)
    {
        UnityEngine.Debug.LogFormat(_gameObject, _bBHasBeenUpgradedIVersionI, _name, _s, _mVersion, _expectedVersion);
    }

    [System.Diagnostics.Conditional("LOG_ENABLE")]
    public static void LogException(Exception _exception)
    {
        UnityEngine.Debug.LogException(_exception);
    }

    [System.Diagnostics.Conditional("LOG_ENABLE")]
    public static void LogToast(string _toast)
    {
        var toast = Resources.Load<GameObject>("DebugToast");
        if (toast == null)
            return;

        var clone = GameObject.Instantiate(toast);
        var debug = clone.GetComponent<DebugToast>();
        debug.test.text = _toast;
    }
}