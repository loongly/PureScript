using UnityEngine;
using UnityEngine.Internal;
using System;
using System.IO;
using System.Text;

[ExecuteInEditMode]
public  class DebugHelper
{
    public const string DEBUG_TAG_KEY = "debug_tag";
    private static string sCurTag;
    static StreamWriter LogWriter;

    //  [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("Debug")]
    public static void InitTag(string tag = null)
    {
        if(tag == null)
            tag = PlayerPrefs.GetString(DEBUG_TAG_KEY, "");
        sCurTag = tag;
    }
    private static bool CheckTag(string tag)
    {
        return string.IsNullOrEmpty(sCurTag) || sCurTag.Equals(tag);
    }

    public static void InitLog(bool debug)
    {
        Debug.LogError("init log: " + (LogWriter == null && debug));
        if (LogWriter == null && debug)
        {//debug模式下开启一个实时文件写入
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string filePath = GetLogPath() + "../unity." + date + ".log";
            Debug.LogError("log path: " + filePath);
            LogWriter = new StreamWriter(filePath, true);
            LogWriter.WriteLine(date + " - " + DateTime.Now.TimeOfDay);
        }

        Application.logMessageReceivedThreaded += OnLog;
        System.AppDomain.CurrentDomain.UnhandledException += new System.UnhandledExceptionEventHandler(OnDebugLogCallbackHandler);
    }

    static private void OnDebugLogCallbackHandler(object sender, UnhandledExceptionEventArgs e)
    {
        Debug.LogError("## Unhandled Exception: " + e.ExceptionObject);
    }


    static void OnLog(string condition, string stackTrace, LogType type)
    {
        if (LogWriter != null)
        {//debug模式下开启一个实时文件写入
            LogToFile(condition);
        }
    }
   
    // [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("Debug")]
    public static void LogMsg(string tag, object msg)
    {
        if (CheckTag(tag))
            UnityEngine.Debug.Log(tag +" | "+ msg); //Time.frameCount +"  "+
        //if (SaveToFile && !string.IsNullOrEmpty(sCurTag))
        //    LogToFile(sCurTag, msg.ToString());
    }

    /// <summary>
    /// 这个用作记录流程/步骤,单独一个写文件，流程中断时可以用来查看中断位置
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="msg"></param>
   // [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("Debug")]
    public static void LogStep(string tag, object msg)
    {
        tag = "<STEP-" + tag + "> " ;
        LogWarning(tag, msg);
    //    LogToFile("STEP.log", msgStr);

    }

   // [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("Debug")]
    public static void LogWarning(string tag, object message)
    {
        if (CheckTag(tag))
            UnityEngine.Debug.LogWarning(tag + " | " + message);
    }

    //[Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("Debug")]
    public static void LogError(string tag,object message)
    {
     //   if (CheckTag(tag))
            UnityEngine.Debug.LogError(Time.frameCount + "  " + tag + " | " + message);
    }


    #region old interface
    // [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("Debug")]
    public static void LogError(object message)
    {
      //  if (CheckTag(""))
            UnityEngine.Debug.LogError(message);
    }

    // [Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("Debug")]
    public static void LogWarning(object message)
    {
        if (CheckTag(""))
            UnityEngine.Debug.LogWarning(message.ToString());
    }

    //[Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("Debug")]
    public static void Log(object message)
    {
        if (CheckTag(""))
            UnityEngine.Debug.Log(message.ToString());
    }
    #endregion

    public static void Break(string tag = null)
    {
        if (CheckTag(tag))
            UnityEngine.Debug.Break();
    }
    public static void ClearDeveloperConsole()
    {
        UnityEngine.Debug.ClearDeveloperConsole();
    }
    public static void DebugBreak(string tag = null)
    {
        if (CheckTag(tag))
            UnityEngine.Debug.DebugBreak();
    }
    //
    // Summary:
    //     Draws a line between specified start and end points.
    public static void DrawLine(Vector3 start, Vector3 end)
    {
        UnityEngine.Debug.DrawLine(start, end);
    }
    //
    // Summary:
    //     Draws a line between specified start and end points.
    public static void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        UnityEngine.Debug.DrawLine(start, end, color);
    }
    //
    // Summary:
    //     Draws a line between specified start and end points.
    public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration)
    {
        UnityEngine.Debug.DrawLine(start, end, color, duration);
    }
    //
    // Summary:
    //     Draws a line between specified start and end points.
    public static void DrawLine(Vector3 start, Vector3 end, [DefaultValue("Color.white")] Color color, [DefaultValue("0.0f")] float duration, [DefaultValue("true")] bool depthTest)
    {
        UnityEngine.Debug.DrawLine(start, end, color, duration, depthTest);
    }
    //
    // Summary:
    //     Draws a line from start to start + dir in world coordinates.
    public static void DrawRay(Vector3 start, Vector3 dir)
    {
        UnityEngine.Debug.DrawRay(start, dir);
    }
    //
    // Summary:
    //     Draws a line from start to start + dir in world coordinates.
    public static void DrawRay(Vector3 start, Vector3 dir, Color color)
    {
        UnityEngine.Debug.DrawRay(start, dir, color);
    }
    //
    // Summary:
    //     Draws a line from start to start + dir in world coordinates.
    public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration)
    {
        UnityEngine.Debug.DrawRay(start, dir, color, duration);
    }
    //
    // Summary:
    //     Draws a line from start to start + dir in world coordinates.
    public static void DrawRay(Vector3 start, Vector3 dir, [DefaultValue("Color.white")] Color color, [DefaultValue("0.0f")] float duration, [DefaultValue("true")] bool depthTest)
    {
        UnityEngine.Debug.DrawRay(start, dir, color, duration, depthTest);
    }

    static string CachePath = null;
    public static string GetLogPath()
    {
        if (CachePath == null)
        {

#if UNITY_EDITOR
            CachePath = "D:/log/unity/";
#else
            CachePath = Application.persistentDataPath +"/log/";
#endif
            if (!Directory.Exists(CachePath))
                Directory.CreateDirectory(CachePath);
        }


        return CachePath;
    }

    /*static FileStream sLogSteam;
    static void initSteam()
    {
        string path = GetCachePath() + "log.txt";
        sLogSteam = File.Open(path);
    }
    public static void toFile(string str)
    {
        if (sLogSteam == null)
            initSteam();
    }*/


    static void LogToFile(string str)
    {
        lock (LogWriter)
        {
            LogWriter.WriteLine(DateTime.Now.TimeOfDay + " | " + str);
            LogWriter.Flush();
        }
    }

    //[Conditional("UNITY_EDITOR"), Conditional("DEBUG"), Conditional("Debug")]
    public static void SaveFile(string fileName, string str)
    {
        if (str == null)
            str = "[null]";
        string path = GetLogPath();
        
        string curDate = System.Environment.NewLine + str;
        FileStream fs = File.Open(path + fileName, FileMode.Append, FileAccess.Write);
        byte[] bytes = Encoding.UTF8.GetBytes(curDate);
        fs.Write(bytes, 0, bytes.Length);
        fs.Close();
    }

}