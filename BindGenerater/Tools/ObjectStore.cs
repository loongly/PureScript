//#define WRAPPER_SIDE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    ~WObject()
    {
        ObjectStore.Remove(_handle);
    }

    
}

public class WObjComparer : IEqualityComparer<object>
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

public static class ObjectStore
{
   
    // Lookup handles by object.
    static Dictionary<object, int> objectHandleCache;

    // Stored objects. The first is never used so 0 can be "null".
    static object[] objects;

    // Stack of available handles.
    static int[] handles;

    // Index of the next available handle
    static int nextHandleIndex;

    // The maximum number of objects to store. Must be positive.
    static int maxObjects;

    [MethodImpl(MethodImplOptions.InternalCall)]
    private static extern object NewObject(Type type);

    static ObjectStore()
    {
        Init(1 << 16); 
    }

    public static void Init(int maxObjects)
    {
        ObjectStore.maxObjects = maxObjects;
        objectHandleCache = new Dictionary<object, int>(maxObjects,new WObjComparer());

        // Initialize the objects as all null plus room for the
        // first to always be null.
        objects = new object[maxObjects + 1];

        // Initialize the handles stack as 1, 2, 3, ...
        handles = new int[maxObjects];
        for (
            int i = 0, handle = maxObjects;
            i < maxObjects;
            ++i, --handle)
        {
            handles[i] = handle;
        }
        nextHandleIndex = maxObjects - 1;
    }

    public static int Store(object obj)
    {
        // Null is always zero
        if (object.ReferenceEquals(obj, null))
        {
            return 0;
        }

        lock (objects)
        {
            int handle;

            // Get handle from object cache
            if (objectHandleCache.TryGetValue(obj, out handle))
            {
                return handle;
            }

            // Pop a handle off the stack
            handle = handles[nextHandleIndex];
            nextHandleIndex--;

            // Store the object
            StoreInternal(handle, obj);

            return handle;
        }
    }

    private static void StoreInternal(int handle, object obj)
    {
        objects[handle] = obj;
        objectHandleCache.Add(obj, handle);
    }


    public static T Get<T>(int handle)
#if WRAPPER_SIDE
        where T: WObject
#endif
    {
        var obj = (T)objects[handle];

#if WRAPPER_SIDE
        if (obj == null)
        {
            obj = (T)NewObject(typeof(T));
            obj.SetHandle(handle);
            StoreInternal(handle, obj);
        }
#endif

        return obj;
    }



    public static object Remove(int handle)
    {
        // Null is never stored, so there's nothing to remove
        if (handle == 0)
        {
            return null;
        }

        lock (objects)
        {
            // Forget the object
            object obj = objects[handle];
            objects[handle] = null;

            // Push the handle onto the stack
            nextHandleIndex++;
            handles[nextHandleIndex] = handle;

            // Remove the object from the cache
            objectHandleCache.Remove(obj);

            return obj;
        }
    }
}

