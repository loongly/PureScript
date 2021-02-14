using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TestStaticClass 
{
    static TestClass testClass;
    public static void StartTest(int s)
    {
        testClass = new TestClass();
        testClass.StartTest(s + 1);
    }

}
