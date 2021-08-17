//#define WRAPPER_SIDE
using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

internal static class ObjectStore
{
    // Lookup handles by object.
    static Dictionary<object, int> objectHandleCache;
    // Stored objects. The first is never used so 0 can be "null".
    static object[] objects;
    private static Handler handler;

    static ObjectStore()
    {
        Init(1 << 16);
    }


    public static void Init(int maxObjects)
    {
        ref Handler h = ref Custom.GetHandler((int)HandlerType.Object);
        h.InitHandle(maxObjects);
        handler = h;

        objectHandleCache = new Dictionary<object, int>(maxObjects, new RObjComparer());

        // Initialize the objects as all null plus room for the
        // first to always be null.
        objects = new object[maxObjects + 1];
    }

    public static int Store(object obj, int target = 0)
    {
        // Null is always zero
        if (object.ReferenceEquals(obj, null))
        {
            return 0;
        }

        lock (objects)
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
        objects[handle] = obj;
        objectHandleCache.Add(obj, handle);
    }

    public static int GetHandle(object obj)
    {
        if (objectHandleCache.TryGetValue(obj, out var handle))
            return handle;

        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object Get(int handle)
    {
        return objects[handle];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Get<T>(int handle)
    {
        return (T)Get(handle);
    }

    [MonoPInvokeCallback(typeof(Custom.RemoveHandleType))]
    internal static int OnRemove(int handle)
    {
        lock (objects)
        {
            // Forget the object
            object obj = objects[handle];
            objects[handle] = null;

            // Remove the object from the cache
            objectHandleCache.Remove(obj);
        }
        return 0;
    }

    internal class RObjComparer : IEqualityComparer<object>
    {
        public bool Equals(object x, object y)
        {
            return RuntimeHelpers.ReferenceEquals(x, y);
        }

        public int GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}