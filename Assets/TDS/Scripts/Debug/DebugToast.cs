using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DebugToast : MonoBehaviour
{
    public Text test;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        StartCoroutine(DelayDestroy());
    }

    public IEnumerator DelayDestroy()
    {
        yield return new WaitForSeconds(1);
        gameObject.Destroy();
    }
}