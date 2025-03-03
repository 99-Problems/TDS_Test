using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PBCountDown : PopupArg
{
    public float limitTime;
    public Action onClose;
}

public class PopupCountDown : PopupBase
{

    public GTMPro countText;

    protected PBCountDown arg;
    private float curTime;
    private double time;

    private void Start()
    {
        gameObject.FixedUpdateAsObservable().Subscribe(_ =>
        {
            curTime += Time.deltaTime;
            if(arg.limitTime - curTime < 0)
                Managers.Popup.ClosePopupBox(this);

        }).AddTo(this);

        Observable.Interval(TimeSpan.FromSeconds(1f)).StartWith(0).Subscribe(_ =>
        {
            UpdateUI();
        }).AddTo(this);
    }

    public void UpdateUI()
    {
        var remainTime = ((double)arg.limitTime - curTime).DecimalRound(Define.DECIMALROUND.RoundUp, 1);
        if(remainTime < time)
        {
            time = Mathf.Max((int)remainTime, 1);
        }    
        countText.SetText(time);
    }

    public override void InitPopupbox(PopupArg _popupData)
    {
        base.InitPopupbox(_popupData);
        arg = (PBCountDown)_popupData;
        time = arg.limitTime;
        UpdateUI();
    }

    public override void OnClosePopup()
    {
        base.OnClosePopup();
        arg.onClose?.Invoke();
    }

    public override void PressBackButton()
    {
        Managers.Popup.ClosePopupBox(this);
    }
}
