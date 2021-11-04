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
    public void SetHandle(IntPtr handle)
    {
        _handle = handle;
    }

    protected virtual Type GetWType() { return typeof(WObject); }
}
public static class WObjectExtend
{
    public static IntPtr __GetHandle(this WObject obj)
    {
        if (object.ReferenceEquals(obj , null) )
            return IntPtr.Zero;

        return obj.Handle;
    }
}


public static class ObjectStore
{
    [MethodImpl(MethodImplOptions.InternalCall)]
    private static extern object GetObject(IntPtr ptr);
    [MethodImpl(MethodImplOptions.InternalCall)]
    private static extern IntPtr StoreObject(object obj, IntPtr ptr);

    [MethodImpl(MethodImplOptions.InternalCall)]
    private static extern object OnException(Exception e);

/*
    public static int Store(object obj)
    {
    }
*/

    public static IntPtr Store(WObject obj,IntPtr handle)
    {
        if (obj == null)
            return IntPtr.Zero;

        return StoreObject(obj,handle);
    }

    public static T Get<T>(IntPtr handle)
        where T : WObject
    {
        if (handle == IntPtr.Zero)
            return null;

        var obj = GetObject(handle);
        return obj as T;
    }
}
