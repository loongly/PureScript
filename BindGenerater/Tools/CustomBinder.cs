using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#if !WRAPPER_SIDE
using AOT;
#endif


public enum HandlerType
{
    Object,

    Count,
}

[StructLayout(LayoutKind.Sequential)]
public struct Handler
{
    // Index of the next available handle
    IntPtr nextHandleIndex;

    // Stack of available handles.
    IntPtr handles;

    public void InitHandle(int count)
    {
        var maxObjects = count;
        // Initialize the handles stack as 3, 2, 1, ...
        //handles = new int[maxObjects];
        handles = Marshal.AllocHGlobal(maxObjects * sizeof(int));
        nextHandleIndex = Marshal.AllocHGlobal(sizeof(int));
        for (
            int i = 0, handle = maxObjects;
            i < maxObjects;
            ++i, --handle)
        {
            //handles[i] = handle;
            Marshal.WriteInt32(handles, i * sizeof(int), handle);
        }

        //nextHandleIndex = maxObjects - 1;
        Marshal.WriteInt32(nextHandleIndex, maxObjects - 1);
    }

    public int GrabHandle()
    {
        // Pop a handle off the stack
        //var h = handles[nextHandleIndex];
        var next = Marshal.ReadInt32(nextHandleIndex);
        var h = Marshal.ReadInt32(handles, next * sizeof(int));
        //nextHandleIndex--;
        Marshal.WriteInt32(nextHandleIndex, next - 1);
        return h;
    }

    public int DropHandle(int handle)
    {
        // Push the handle onto the stack
        //nextHandleIndex++;
        var next = Marshal.ReadInt32(nextHandleIndex) + 1;
        Marshal.WriteInt32(nextHandleIndex, next);

        //handles[nextHandleIndex] = handle;
        Marshal.WriteInt32(handles, next * sizeof(int), handle);

        return handle;
    }
}

public static unsafe class Custom
{
    enum DelegateType
    {
        StoreString,
    }
    public delegate int StoreDelegateType(int type, IntPtr func);
    public delegate ref Handler GetHandlerType(int type);
    public delegate int StoreStringType(char* str);
    public delegate int RemoveHandleType(int handle);

    public static GetHandlerType GetHandler;
    public static StoreStringType StoreString;
    public static RemoveHandleType RemoveHandle;
    public static StoreDelegateType StoreDelegate;

#if WRAPPER_SIDE
        public static void DeSer(IntPtr ptr)
        {
            int offset = 0;

            StoreDelegate = Marshal.GetDelegateForFunctionPointer<StoreDelegateType>(Marshal.ReadIntPtr(ptr, offset));
            offset += IntPtr.Size;

            GetHandler = Marshal.GetDelegateForFunctionPointer<GetHandlerType>(Marshal.ReadIntPtr(ptr, offset));
            offset += IntPtr.Size;

            StoreString = Marshal.GetDelegateForFunctionPointer<StoreStringType>(Marshal.ReadIntPtr(ptr, offset));
            offset += IntPtr.Size;

            RemoveHandle = Marshal.GetDelegateForFunctionPointer<RemoveHandleType>(Marshal.ReadIntPtr(ptr, offset));
            offset += IntPtr.Size;

            StoreStringType storeString = StringStore.OnReceiveStr;
            StoreDelegate((int)DelegateType.StoreString, Marshal.GetFunctionPointerForDelegate(storeString));
        }
#else
    public static void Ser(IntPtr ptr)
    {
        int offset = 0;

        StoreDelegateType storeDelegate = StoreDelegateAction;
        Marshal.WriteIntPtr(ptr, offset, Marshal.GetFunctionPointerForDelegate(storeDelegate));
        offset += IntPtr.Size;

        GetHandler = GetHandle;
        Marshal.WriteIntPtr(ptr, offset, Marshal.GetFunctionPointerForDelegate(GetHandler));
        offset += IntPtr.Size;

        StoreStringType storeString = StringStore.OnReceiveStr;
        Marshal.WriteIntPtr(ptr, offset, Marshal.GetFunctionPointerForDelegate(storeString));
        offset += IntPtr.Size;

        RemoveHandle = ObjectStore.OnRemove;
        Marshal.WriteIntPtr(ptr, offset, Marshal.GetFunctionPointerForDelegate(RemoveHandle));
        offset += IntPtr.Size;

        //init ObjectStore
        ObjectStore.Get(0);
    }

    static Handler[] HandleStore = new Handler[(int)HandlerType.Count];

    [MonoPInvokeCallback(typeof(GetHandlerType))]
    static ref Handler GetHandle(int type)
    {
        return ref HandleStore[type];
    }


    [MonoPInvokeCallback(typeof(StoreDelegateType))]
    static int StoreDelegateAction(int type, IntPtr ptr)
    {
        switch ((DelegateType)type)
        {
            case DelegateType.StoreString:
                StoreString = Marshal.GetDelegateForFunctionPointer<StoreStringType>(Marshal.ReadIntPtr(ptr));
                break;

        }
        return 0;
    }

#endif


}

