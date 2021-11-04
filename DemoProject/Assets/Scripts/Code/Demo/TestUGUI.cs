using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

class TestUGUI : MonoBehaviour
{
    Canvas canvas;
    private void Start()
    {
        Debug.LogError("start UGUI...");
        canvas = GameObject.FindObjectOfType<Canvas>();
        var input = canvas.gameObject.GetComponentInChildren<InputField>();
        var btn = canvas.gameObject.GetComponentInChildren<Button>();

        btn.onClick.AddListener(() => {
            input.text = "AABBCC";
            btn.transform.position += Vector3.one * 5;
        });

        btn.onClick.AddListener(() =>
        {
            Debug.LogError("Click !!");
        });

        Debug.LogError("UGUI !!");


    }


}
