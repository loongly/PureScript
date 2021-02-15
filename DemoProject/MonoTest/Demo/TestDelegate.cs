using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDelegate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.LogError("######### TestDelegate: " + gameObject.name);
        Application.focusChanged += OnFocus;
    }

    void OnFocus(bool focus)
    {
        Debug.LogError("==focus: " + focus);
        var tempObj = new TestClass();
        tempObj.StartTest(2);

        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
    }

    private void OnDestroy()
    {
        Application.focusChanged -= OnFocus;
        Debug.LogError("== TestDelegate OnDestroy");
    }

    ~TestDelegate()
    {
        Debug.LogError("~TestDelegate");
    }


    // Update is called once per frame
    void Update()
    {
    }
}
