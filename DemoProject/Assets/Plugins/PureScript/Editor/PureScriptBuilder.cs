using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEditor.Callbacks;
using UnityEngine;
using System.Reflection;
using System.Runtime.CompilerServices;
using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using System.IO;
using System.Text;
using Process = System.Diagnostics.Process;

public static class PureScriptBuilder 
{
    public static bool Enable = true;
    static string ScriptEngineDir = "../ScriptEngine";
    static bool Inbuild = false;

    static MethodHooker hooker;


    [UnityEditor.Callbacks.PostProcessScene]
    public static void AutoInjectAssemblys()
    {
        if (EditorApplication.isCompiling || Application.isPlaying)
            return;

        if (Enable && !Inbuild)
        {
            Inbuild = true;

            // wait IL2CPPBuilder,maybe you can use SBP in the future.
            InsertBuildTask();
        }
    }
    [MenuItem("PureScript/TestBuild", false, 1)]
    public static void TestBuild()
    {
       // RunBinder(null, null, null, Application.dataPath.Replace("Assets", Path.Combine("Library", "PlayerDataCache", "Managed")));
    }

    //called by UnityEditor when IL2CPPBuilder.RunIl2CppWithArguments
    //public static void RunBinder(object obj, List<string> arguments, Action<System.Diagnostics.ProcessStartInfo> setupStartInfo, string workingDirectory)
    public static void RunBinder(string managedAssemblyFolderPath, object platformProvider, object rcr, ManagedStrippingLevel managedStrippingLevel)
    {
        var workingDirectory = managedAssemblyFolderPath;
        if (hooker != null)
        {
            hooker.Dispose();
            hooker = null;
        }
        Inbuild = false;

        ScriptEngineDir = NiceWinPath(ScriptEngineDir);

        //copy all striped assemblys
        var managedPath = Path.Combine(ScriptEngineDir , "Managed");
        CreateOrCleanDirectory(managedPath);

        foreach (string fi in Directory.GetFiles(workingDirectory))
        {
            string fname = Path.GetFileName(fi);
            string targetfname = Path.Combine(managedPath, fname);
            File.Copy(fi, targetfname);
        }

        // call binder,bind icall and adapter
        var binderPath = Path.Combine(ScriptEngineDir, "Tools", "Binder.exe");
        var configPath = Path.GetFullPath(Path.Combine(ScriptEngineDir, "Tools", "config.json"));
        var toolsetPath = GetEnginePath(Path.Combine("Tools", "Roslyn"));
        CallBinder(binderPath, new List<string>() { configPath, toolsetPath });

        // replace adapter by generated assembly
        var generatedAdapter = Path.Combine(managedPath, "Adapter.gen.dll");
        var adapterGenPath = Path.Combine(workingDirectory, "Adapter.gen.dll");
        if (File.Exists(generatedAdapter))
        {
            File.Copy(generatedAdapter, adapterGenPath, true);
            File.Delete(generatedAdapter);
        }

        // call the realy method
        /*if (obj != null)
            RunIl2CppWithArguments(obj, arguments, setupStartInfo, workingDirectory);*/

        StripAssemblies(managedAssemblyFolderPath, platformProvider, rcr, managedStrippingLevel);
    }

    public static void CallBinder(string binderPath,List<string> args)
    {
        var monoPath = GetEnginePath(Path.Combine("MonoBleedingEdge", "bin", Application.platform == RuntimePlatform.OSXEditor ? "mono" : "mono.exe")); 
        if (!File.Exists(monoPath))
        {
            UnityEngine.Debug.LogError("can not find mono!");
        }

        if (!File.Exists(binderPath))
        {
            UnityEngine.Debug.LogError("please install the Binder");
            return;
        }

        Process binder = new Process();
        binder.StartInfo.FileName = monoPath;

        string debugger = "--debug";
        //debugger += " --debugger-agent=transport=dt_socket,address=127.0.0.1:8089,server=y,suspend=y ";

        binder.StartInfo.Arguments = debugger + " --runtime=v4.0.30319 \"" + binderPath + "\" \"" + string.Join("\" \"", args.ToArray()) + "\"";
        binder.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        binder.StartInfo.RedirectStandardOutput = true;
        binder.StartInfo.RedirectStandardError = true;
        binder.StartInfo.UseShellExecute = false;
        binder.StartInfo.CreateNoWindow = true;
        binder.Start();

        while (!binder.StandardOutput.EndOfStream)
        {
            string line = binder.StandardOutput.ReadLine();
            UnityEngine.Debug.LogWarning(line);
        }


        binder.WaitForExit();

        if (binder.ExitCode != 0)
        {
            var errorInfo = "Binder.exe run error. \n" + binder.StandardError.ReadToEnd();
            UnityEngine.Debug.LogError(errorInfo);
            throw new Exception(errorInfo);
        }
    }

    public static string GetEnginePath(string path)
    {
        string dataPath;
        var editorAppPath = NiceWinPath(EditorApplication.applicationPath);

        if (Application.platform == RuntimePlatform.OSXEditor)
            dataPath = Path.Combine(editorAppPath, "Contents");
        else
            dataPath = Path.Combine(Path.GetDirectoryName(editorAppPath), "Data");

        return Path.Combine(dataPath, path);
    }

    static string NiceWinPath(string unityPath)
    {
        // IO functions do not like mixing of \ and / slashes, esp. for windows network paths (\\path)
        return Application.platform == RuntimePlatform.WindowsEditor ? unityPath.Replace("/", @"\") : unityPath;
    }

    //redirect to IL2CPPBuilder.RunIl2CppWithArguments
    /*public static void RunIl2CppWithArguments(object obj, List<string> arguments, Action<System.Diagnostics.ProcessStartInfo> setupStartInfo, string workingDirectory)
    {
        throw new NotImplementedException();
    }*/

    public static void StripAssemblies(string managedAssemblyFolderPath, object platformProvider, object rcr, ManagedStrippingLevel managedStrippingLevel)
    {
        throw new NotImplementedException();
    }


    private static void InsertBuildTask()
    {
        if(hooker == null)
        {
            /* var builderType = typeof(Editor).Assembly.GetType("UnityEditorInternal.IL2CPPBuilder");
             MethodBase orign = builderType.GetMethod("RunIl2CppWithArguments", BindingFlags.Instance | BindingFlags.NonPublic);
             MethodBase custom = typeof(PureScriptBuilder).GetMethod("RunBinder");
             MethodBase wrap2Orign = typeof(PureScriptBuilder).GetMethod("RunIl2CppWithArguments");*/

             var builderType = typeof(Editor).Assembly.GetType("UnityEditorInternal.AssemblyStripper");
            MethodBase orign = builderType.GetMethod("StripAssemblies", BindingFlags.Static | BindingFlags.NonPublic);
            MethodBase custom = typeof(PureScriptBuilder).GetMethod("RunBinder");
            MethodBase wrap2Orign = typeof(PureScriptBuilder).GetMethod("StripAssemblies");

            hooker = new MethodHooker(orign, custom, wrap2Orign);
        }
    }


    static void CreateOrCleanDirectory(string dir)
    {
        if (Directory.Exists(dir))
            Directory.Delete(dir, true);
        Directory.CreateDirectory(dir);
    }


    #region internal

    private class MethodHooker : IDisposable
    {
        IntPtr orignPtr;
        IntPtr resumeData;
        int dataLength;

        public MethodHooker(MethodBase orign, MethodBase custom, MethodBase wrap2Orign)
        {
            RuntimeHelpers.PrepareMethod(orign.MethodHandle);
            RuntimeHelpers.PrepareMethod(custom.MethodHandle);
            RuntimeHelpers.PrepareMethod(wrap2Orign.MethodHandle);

            Debug.Log($"replace method:{orign.ToString()} -->{custom.ToString()}");
            RedirectMethod(wrap2Orign.MethodHandle.GetFunctionPointer(), orign.MethodHandle.GetFunctionPointer(),false);
            RedirectMethod(orign.MethodHandle.GetFunctionPointer(), custom.MethodHandle.GetFunctionPointer(),true);
        }

        public void Dispose()
        {
            Resume();
        }

        public unsafe void Resume()
        {
            if (resumeData == IntPtr.Zero || orignPtr == IntPtr.Zero || dataLength <= 0)
                return;

            Debug.Log($"resume method length:{dataLength.ToString()}");
            UnsafeUtility.MemCpy(orignPtr.ToPointer(), resumeData.ToPointer(), dataLength);
            Marshal.FreeHGlobal(resumeData);
            resumeData = IntPtr.Zero;
        }

        private unsafe void SaveResumeData(void* ptr,int length)
        {
            orignPtr = new IntPtr(ptr);
            dataLength = length;
            resumeData = Marshal.AllocHGlobal(dataLength);
            UnsafeUtility.MemCpy(resumeData.ToPointer(), ptr, dataLength);
        }

        private void RedirectMethod(IntPtr pBody, IntPtr pBorrowed,bool saveResume)
        {
            unsafe
            {
                var ptr = (byte*)pBody.ToPointer();
                var ptr2 = (byte*)pBorrowed.ToPointer();
                var ptrDiff = ptr2 - ptr - 5;
                if (ptrDiff < (long)0xFFFFFFFF && ptrDiff > (long)-0xFFFFFFFF)
                {
                    if(saveResume)
                        SaveResumeData(ptr,5);
                    // 32-bit relative jump, available on both 32 and 64 bit arch.
                    *ptr = 0xe9; // JMP
                    *((uint*)(ptr + 1)) = (uint)ptrDiff;
                }
                else
                {
                    if (Environment.Is64BitProcess)
                    {
                        if (saveResume)
                            SaveResumeData(ptr, 15);
                        // For 64bit arch and likely 64bit pointers, do:
                        // PUSH bits 0 - 32 of addr
                        // MOV [RSP+4] bits 32 - 64 of addr
                        // RET
                        var cursor = ptr;
                        *(cursor++) = 0x68; // PUSH
                        *((uint*)cursor) = (uint)ptr2;
                        cursor += 4;
                        *(cursor++) = 0xC7; // MOV [RSP+4]
                        *(cursor++) = 0x44;
                        *(cursor++) = 0x24;
                        *(cursor++) = 0x04;
                        *((uint*)cursor) = (uint)((ulong)ptr2 >> 32);
                        cursor += 4;
                        *(cursor++) = 0xc3; // RET
                    }
                    else
                    {
                        if (saveResume)
                            SaveResumeData(ptr, 6);
                        // For 32bit arch and 32bit pointers, do: PUSH addr, RET.
                        *ptr = 0x68;
                        *((uint*)(ptr + 1)) = (uint)ptr2;
                        *(ptr + 5) = 0xC3;
                    }
                }
            }
        }
    }
    #endregion
}
