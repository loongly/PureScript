using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace PureScriptWrapper
{
    internal static class WrapperUtils
    {
        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern IntPtr GetFuncPtr(object obj, string funcName, int paramCount = 0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern IntPtr Dispose(object obj);

        /* 
        // wrap mono thunk to delegate, invoke with exception
        static IntPtr exceptionPtr;
        public static IntPtr GetExctpitonPtr()
        {
            if(exceptionPtr == IntPtr.Zero)
                exceptionPtr = GetExceptionPtr();
            return exceptionPtr;
        }
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern IntPtr GetFuncThunk(uint handle, string funcName, int paramCount = 0);
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern IntPtr GetExceptionPtr();
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int CheckException();*/
    }
}
