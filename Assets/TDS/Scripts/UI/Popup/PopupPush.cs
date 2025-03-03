using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PBPush : PopupArg
{
    public float pushTime = 2f;
    public string strDesc;
    public Color colorDesc = Color.white;
    public PopupPush.STATE state = PopupPush.STATE.UP;
    public PopupPush.POSITION_STATE position = PopupPush.POSITION_STATE.MIDDLE;
    public Action onClose;
    public bool isTextAni;
}

public class PopupPush : PopupBase
{
    public enum STATE
    {
        NORMAL,
        UP,
        DOWN,
    }

    public enum POSITION_STATE
    {
        UP,
        MIDDLE,
        DOWN,
    }
    public UIOpenAni textAni;

    public GTMPro textDesc;
    public GameObject touchLock;
    public GameObject back;

    public GameObject effectUp;
    public GameObject effectNormal;
    public GameObject effectDown;

    public RectTransform upPostion;
    public RectTransform downPotion;
    RectTransform backRect;
    CanvasGroup backCg;
    protected PBPush arg;

    public float aniOutTime = 0.3f;
    public virtual void Start()
    {
        backRect = back.GetComponent<RectTransform>();
        backCg = back.GetComponent<CanvasGroup>();
        if(effectUp)
            effectUp.gameObject.SetActive(arg.state == STATE.UP);
        if(effectNormal)
            effectNormal.gameObject.SetActive(arg.state == STATE.NORMAL);
        if(effectDown)
            effectDown.gameObject.SetActive(arg.state == STATE.DOWN);
        if (arg.position == POSITION_STATE.UP)
            backRect.position = upPostion.position;
        else if (arg.position == POSITION_STATE.DOWN)
            backRect.position = downPotion.position; ;
        StartCoroutine(WakeUP());
        if (arg.state == STATE.NORMAL)
            StartCoroutine(SleepNormal());
        else
            StartCoroutine(Sleep());

        if (arg.isTextAni)
            textAni.StartPlay();
    }
    IEnumerator WakeUP()
    {
        backRect.localScale = new Vector3(0, backRect.localScale.y, backRect.localScale.z);
        backRect.DOScaleX(1f, 0.3f);
        textDesc.SetText(arg.strDesc);
        textDesc.SetColor(arg.colorDesc);
        textDesc.text.alpha = 0f;
        yield return new WaitForSeconds(0.3f);
        textDesc.text.DOFade(1f, 0.3f).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(0.3f);
        touchLock.gameObject.SetActive(false);
        yield return null;
    }

    IEnumerator Sleep()
    {
        yield return new WaitForSeconds(arg.pushTime - aniOutTime);
        textDesc.text.DOFade(0, aniOutTime);
        yield return new WaitForSeconds(aniOutTime);
        backRect.DOScaleX(0, aniOutTime);
        backCg.DOFade(0, aniOutTime).SetEase(Ease.OutExpo);
        yield return new WaitForSeconds(aniOutTime);
        ClosePopupPush();
        yield return null;
    }
    IEnumerator SleepNormal()
    {
        yield return new WaitForSeconds(arg.pushTime);
        textDesc.text.DOFade(0, aniOutTime);
        backRect.DOAnchorPosY(backRect.rect.height, aniOutTime);
        backCg.DOFade(0, aniOutTime).SetEase(Ease.OutExpo);
        yield return new WaitForSeconds(aniOutTime);
        ClosePopupPush();
        yield return null;
    }

    private void ClosePopupPush()
    {
        arg.onClose?.Invoke();
        Managers.Popup.ClosePopupBox(gameObject);
    }

    public override void InitPopupbox(PopupArg popupData)
    {
        arg = (PBPush)popupData;
    }

    public override void PressBackButton()
    {

    }
}