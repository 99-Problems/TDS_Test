using System;
using System.Collections;
using System.Collections.Generic;
//using Sirenix.OdinInspector;
using UnityEngine;


[ExecuteInEditMode]
public class ContentWidthResolution : MonoBehaviour
{
    //[Title("1280*720기준 사이즈")]
    public float width = 100;

    private void Awake()
    {
        var rect = GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width * Screen.width / 1280, rect.sizeDelta.y);
    }
#if UNITY_EDITOR
    public void Update()
    {
        OnValidate();
    }
    
    private void OnValidate()
    {
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
        var screenSize = (Vector2) Res;

        var rect = GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width * screenSize.x / 1280, rect.sizeDelta.y);
    }

#endif
}