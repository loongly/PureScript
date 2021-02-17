using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public class WObject
{
    private int _handle;
    internal int Handle { get { return _handle; } }
    internal void SetHandle(int handle)
    {
        _handle = handle;
    }
}
public static class WObjectExtend
{
    internal static int __GetHandle(this WObject obj)
    {
        if (object.ReferenceEquals(obj , null) )
            return 0;

        return obj.Handle;
    }
}


internal static class ObjectStore
{
    // Lookup handles by object.
    static ConditionalWeakTable<object, HandleRef> objectHandleCache = new ConditionalWeakTable<object, HandleRef>();

    // Stored objects. The first is never used so 0 can be "null".
    static GCHandle[] Handles;

    [MethodImpl(MethodImplOptions.InternalCall)]
    private static extern object NewObject(Type type);

    [MethodImpl(MethodImplOptions.InternalCall)]
    private static extern object OnException(Exception e);

    private static Handler handler;
    private static List<int> dropList = new List<int>();

    static ObjectStore()
    {
        Init(1 << 16);
    }

    public static void Init(int maxObjects)
    {
        handler = Custom.GetHandler((int)HandlerType.Object);

        // Initialize the objects as all null plus room for the
        // first to always be null.
        Handles = new GCHandle[maxObjects + 1];
    }

    public static int Store(object obj, int target = 0)
    {
        // Null is always zero
        if (object.ReferenceEquals(obj, null))
        {
            return 0;
        }

        lock (Handles)
        {
            if (target == 0)
            {
                // Get handle from object cache

                var h = GetHandle(obj);
                if (h > 0)
                    return h;

                target = handler.GrabHandle();
            }

            // Store the object
            StoreInternal(target, obj);

            return target;
        }
    }

    private static void StoreInternal(int handle, object obj)
    {
        Handles[handle] = GCHandle.Alloc(obj, GCHandleType.Weak);
        objectHandleCache.Add(obj, handle);
        CheckDropList();
    }

    public static int GetHandle(object obj)
    {
        if (objectHandleCache.TryGetValue(obj, out var targetRef))
            return targetRef.Handle;

        return 0;
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object Get(int handle)
    {
        var gcHandle = Handles[handle];
        if (gcHandle.IsAllocated)
            return gcHandle.Target;

        return null;
    }

    public static T Get<T>(int handle)
        where T : WObject
    {
        if (handle == 0)
            return null;

        var obj = Get(handle);
        if(obj != null)
            return (T) obj;

        // create wrapper
        var nObj = (T)NewObject(typeof(T));
        nObj.SetHandle(handle);
        StoreInternal(handle, nObj);
        return nObj;
    }

    /// <summary>
    /// when ~HandleRef()
    /// </summary>
    internal static void OnRemove(int handle)
    {
        // Null is never stored, so there's nothing to remove
        if (handle == 0)
            return;

        lock (Handles)
        {
            dropList.Add(handle);
        }
    }

    private static void CheckDropList()
    {
        lock (Handles)
        {
            if (dropList.Count > 0)
            {
                foreach (var handle in dropList)
                {
                    // Forget the object
                    ref var gcHandle = ref Handles[handle];
                    if (gcHandle.IsAllocated)
                    {
                        var obj = gcHandle.Target;
                        gcHandle.Free();

                        //disable handle
                        int cur = handler.DropHandle(handle);
                        if (cur == handle)
                            Custom.RemoveHandle(handle);

                        // Remove the object from the cache
                        if (obj != null)
                            objectHandleCache.Remove(obj);
                    }
                }

                dropList.Clear();
            }
        }
    }


    /// <summary>
    /// wrap int Handle to class
    /// </summary>
    internal class HandleRef
    {
        public int Handle;
        public HandleRef(int handle)
        {
            Handle = handle;
        }

        public static implicit operator HandleRef(int handle)
        {
            return new HandleRef(handle);
        }

        public static implicit operator Int32(HandleRef r)
        {
            return r.Handle;
        }

        ~HandleRef()
        {
            ObjectStore.OnRemove(Handle);
        }
    }
}
