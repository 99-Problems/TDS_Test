using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine.EventSystems;
using Data;

public abstract class PopupBase : MonoBehaviour
{
    [ReadOnly] public Define.EPOPUP_TYPE popupboxType;

    [ReadOnly] public int popupUID = 0;

    [ReadOnly] public bool isBackground = false;

    [ReadOnly] public Canvas canvas;

    [ReadOnly] public GraphicRaycaster graphicRaycaster;

    [ReadOnly] public bool isRecycle = false;

    public AudioClip alarmAudioSource;

    public bool isFullPopup = true;

    public bool isAfeectedBlockUI = true;

    //public Defines.ContentsType contentsType;
#if UNITY_EDITOR
    public void Awake()
    {
        Selection.objects = new UnityEngine.Object[] { this.gameObject };
    }
#endif
    public void GetCanvas()
    {
        canvas = gameObject.GetComponent<Canvas>();
        graphicRaycaster = gameObject.GetComponent<GraphicRaycaster>();
    }

    // 닫기 버튼에 대한 정의는 PressBackButton 을 통해서 구현하자.
    // PopupKeyPressToClose.cs 뒤로가기와 동일한 기능 구현을 위함.
    public abstract void PressBackButton();

    virtual public void InitPopupbox(PopupArg _popupData)
    {
    }

    virtual public void OnClosePopup()
    {
        Managers.Popup.RemoveFocus(gameObject.GetInstanceID());
        if (isFullPopup)
        {
            Managers.Popup.RemoveFullpopup(gameObject.GetInstanceID());
        }
        Input.ResetInputAxes();
        //var bgmController = GetComponent<PopupBgmController>();
        //if (bgmController)
        //{
        //    bgmController.StopBgm();
        //}
    }

    private void OnEnable()
    {
        Managers.Popup.AddFocus(gameObject.GetInstanceID());
        if (isFullPopup)
        {
            Managers.Popup.AddFullpopup(gameObject.GetInstanceID());
        }
    }

    public bool CheckFullPopup()
    {
        return isFullPopup;
    }

    public void AblePopup()
    {
        //if (canvas)
        //    canvas.enabled = true;
        //if (graphicRaycaster)
        //    graphicRaycaster.enabled = true;
        //var bgmController = GetComponent<PopupBgmController>();
        //if (bgmController)
        //{
        //    bgmController.Play();
        //}
    }

    public void DisablePopup()
    {
        if (canvas)
            canvas.enabled = false;
        if (graphicRaycaster)
            graphicRaycaster.enabled = false;
    }

    public void OnDestroy()
    {
    }

    public bool IsFocus()
    {
        return Managers.Popup.IsFocus(gameObject.GetInstanceID());
    }

    public bool IsFullFocus()
    {
        return Managers.Popup.IsFocusFullpopup(gameObject.GetInstanceID());
    }

    //public void PlayAlarm()
    //{
    //    if (alarmAudioSource) Managers.Sound.Play(alarmAudioSource, Define.Sound.Effect);
    //}
}
