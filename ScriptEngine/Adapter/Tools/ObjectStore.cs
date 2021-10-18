//#define WRAPPER_SIDE
using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

internal static class ObjectStore
{

   
    public static IntPtr Store(object obj)
    {
        return IntPtr.Zero;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Get<T>(IntPtr handle) where T : class
    {
        return null;
    }

}