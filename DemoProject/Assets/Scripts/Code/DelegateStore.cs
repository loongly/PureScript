// Holds objects and provides handles to them in the form of ints
using System.Collections.Generic;

namespace XMono
{
public static class DelegateStore
    {
        static DelegateStore()
        {
            Init(1024);
        }
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

    public static void Init(int maxObjects)
    {
            DelegateStore.maxObjects = maxObjects;
        objectHandleCache = new Dictionary<object, int>(maxObjects);

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
            // Pop a handle off the stack
            int handle = handles[nextHandleIndex];
            nextHandleIndex--;

            // Store the object
            objects[handle] = obj;
            objectHandleCache.Add(obj, handle);

            return handle;
        }
    }

    public static object Get(int handle)
    {
        if (handle < 0 || handle >= objects.Length)
            return null;

        return objects[handle];
    }

    public static int GetHandle(object obj)
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
        }

        // Object not found
        return Store(obj);
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
    
}