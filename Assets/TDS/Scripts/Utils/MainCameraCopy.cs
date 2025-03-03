using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraCopy : MonoBehaviour
{
    private Camera camera;

    void Start()
    {
        camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Camera.main == null)
            return;
        camera.fieldOfView = Camera.main.fieldOfView;
    }
}