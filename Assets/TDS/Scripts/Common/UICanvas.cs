using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class UICanvas : MonoBehaviour
{
    public enum ECANVAS
    {
        NONE,
        MASTER,
        POPUP,
        FLOATING,
        TOUCH,
        VIDIO,
        TUTORIAL,
    }

    private Canvas canvas;
    public ECANVAS canvasType = ECANVAS.NONE;
    public static UICanvas MasterCanvas { get; private set; }
    public static UICanvas PopupCanvas { get; private set; }
    public bool setAspectRatio = false;
    [ShowIf("@canvasType == ECANVAS.MASTER")]
    public Canvas safeArea;

    public void Awake()
    {
        canvas = GetComponent<Canvas>();
        if (canvasType == ECANVAS.MASTER)
            MasterCanvas = this;
        if (canvasType == ECANVAS.POPUP)
            PopupCanvas = this;
    }

    void Start()
    {
        Managers.Device.OnChangeScreenSize.Subscribe(_1 => { SetAspectRatio(); }).AddTo(this);
        SetAspectRatio();
    }

    public void SetAspectRatio()
    {
        CanvasScaler scaler = gameObject.GetOrAddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = Managers.Device.targetResolution;
        scaler.referencePixelsPerUnit = 100;

        if (Managers.Device.canvasMatchWidthOrHeight == 1)
        {
            scaler.referenceResolution = new Vector2(1280, 720 * Managers.Device.screenSafeAreaRatio);
        }
        else
        {
            scaler.referenceResolution = new Vector2(1280 * Managers.Device.screenSafeAreaRatio, 720);
        }

        scaler.matchWidthOrHeight = Managers.Device.canvasMatchWidthOrHeight;
        setAspectRatio = true;
    }
}