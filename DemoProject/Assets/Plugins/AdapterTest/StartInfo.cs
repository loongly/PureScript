using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PureScript
{
    public class StartInfo
    {
        static string reloadDllName = "Code.dll";
        public static string ReloadDllName { get { return reloadDllName; } }

        static string reloadClassName = "ReloadTest";
        public static string ReloadClassName { get { return reloadClassName; } }

        static string testMethodName = "StartTest";
        public static string TestMethodName { get { return testMethodName; } }
    }
}
