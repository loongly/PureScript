using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public interface IWObject
{
    IntPtr Handle { get; }
}

public class WObject : IWObject
{
    private IntPtr _handle;
    public IntPtr Handle { get { return _handle; } }
    internal void SetHandle(IntPtr handle)
    {
        _handle = handle;
    }
}
public static class WObjectExtend
{
    internal static IntPtr __GetHandle(this WObject obj)
    {
        if (object.ReferenceEquals(obj , null) )
            return IntPtr.Zero;

        return obj.Handle;
    }
}


internal static class ObjectStore
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    private static extern object NewObject(Type type);

    [MethodImpl(MethodImplOptions.InternalCall)]
    private static extern object OnException(Exception e);

/*
    public static int Store(object obj)
    {
    }
*/

    public static IntPtr Store(WObject obj,IntPtr handle)
    {
        return handle;
    }

    public static T Get<T>(IntPtr handle)
        where T : WObject
    {
        return null;
    }
}
