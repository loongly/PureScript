using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class TestGC : MonoBehaviour
{
    public string Info;
    private void Start()
    {
        Debug.LogError("== TestGC Start");
    }

    private void OnDestroy()
    {
        Debug.LogError("== TestGC OnDestroy "+ Info);
    }

    ~TestGC()
    {
        Debug.LogError("~TestGC " + Info);
    }
}
