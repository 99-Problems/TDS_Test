using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using Data;
using UniRx;
using UniRx.Triggers;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.Universal;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class PopupArg
{
    public static readonly PopupArg empty = new PopupArg();
}

public class PopupWaitData
{
    public int uid;
    public Define.EPOPUP_TYPE PopupBoxType;
    public PopupArg PopupArg;
    public bool BackgroundOverlayOn;
    public bool MenuVisible = false;
}

public class PopupManager
{
        private const int offset = 30;
private GameObject PopupBackgroundOverlay;

public List<PopupBase> activePopup = new List<PopupBase>();

private List<PopupWaitData> waitActivePopup = new List<PopupWaitData>();
private List<PopupWaitData> reservationPopup = new List<PopupWaitData>();

private List<PopupBase> waitClosePopup = new List<PopupBase>();

private int uid = 1;

private Canvas canvas;
private Canvas tutorialCanvas;
private GameObject waitObj;

private GameObject blockUIObj;
private GameObject blackUIObj;

public List<int> focusStack = new List<int>();
public List<int> fullPopupStack = new List<int>();

protected Subject<int> closePopupSubject = new Subject<int>();
protected Subject<int> closeFullPopupSubject = new Subject<int>();
protected Subject<int> showPopupSubject = new Subject<int>();

protected BoolReactiveProperty systemMenuVisible = new BoolReactiveProperty(false);

public IObservable<int> OnClosePopupSubject
{
    get { return closePopupSubject.AsObservable(); }
}

public IObservable<int> OncloseFullPopupSubject
{
    get { return closeFullPopupSubject.AsObservable(); }
}

public IObservable<int> OnshowPopupSubject
{
    get { return showPopupSubject.AsObservable(); }
}

public IObservable<bool> OnSystemMenuVisible
{
    get { return systemMenuVisible.AsObservable(); }
}

public void Init()
{
#if UNITY_EDITOR
    Resources.UnloadUnusedAssets();
#endif
    var touchObj = Resources.Load<GameObject>("TouchCanvas");
    var touchclone = GameObject.Instantiate(touchObj);
    GameObject.DontDestroyOnLoad(touchclone);

    var tutorialObj = Resources.Load<GameObject>("TutorialCanvas");
    var tutorialClone = GameObject.Instantiate(tutorialObj);
    tutorialCanvas = tutorialClone.GetComponent<Canvas>();
    GameObject.DontDestroyOnLoad(tutorialClone);

    var cameraObj = Resources.Load<GameObject>("PopupCanvas");
    var clone = GameObject.Instantiate(cameraObj);
    canvas = clone.GetComponent<Canvas>();
    //waitObj = canvas.transform.Find("Wait").gameObject;
    blockUIObj = canvas.transform.Find("BlockInputUI").gameObject;
    blackUIObj = canvas.transform.Find("DarkCover").gameObject;
    GameObject.DontDestroyOnLoad(clone);
    PopupBackgroundOverlay = canvas.gameObject.GetComponentInChildren<Image>().gameObject;
    var btn = PopupBackgroundOverlay.GetComponent<Button>();
    btn.OnClickAsObservable().Subscribe(_ => { CloseFrontPopupBox(); });
    ActiveBackground(false);

    Managers.Popup.OnClosePopupSubject.Subscribe(async _ =>
    {
        //풀 팝업 꺼졌을때
        if (_ == 1)
        {
            await UniTask.DelayFrame(1);
        }
    });

    MainThreadDispatcher.UpdateAsObservable()
        .Subscribe(_ =>
        {
            if (waitActivePopup.Count != 0)
            {
                foreach (var waitData in waitActivePopup)
                {
                    ShowPopupBox(waitData);
                }

                waitActivePopup.Clear();
            }

            if (waitClosePopup.Count != 0)
            {
                bool closeFullPopup = false;
                foreach (var waitData in waitClosePopup)
                {
                    if (IsWindowPopUp(waitData) == false)
                        closeFullPopup = true;
                    CloseWaitPopupBox(waitData);
                }

                SetAbleFocusPopup();
                waitClosePopup.Clear();
                if (closeFullPopup)
                    closePopupSubject.OnNext(1);
                else
                    closePopupSubject.OnNext(0);
            }
        });
}

public void SetDisableAllPopup()
{
    foreach (var popup in activePopup)
    {
        popup.DisablePopup();
    }
}

public void SetAbleFocusPopup()
{
    foreach (var popup in activePopup)
    {
        if (popup.IsFullFocus())
            popup.AblePopup();
    }
}

internal bool IsShowPopup()
{
    return activePopup.Count != 0;
}

internal bool IsGameStopPopup()
{
    if (activePopup.Count == 0)
        return false;
    for (int i = 0; i < activePopup.Count; i++)
    {
        if (IsGameStopPopup(activePopup[i].popupboxType))
            return true;
    }

    return false;
}

    internal bool IsShowPopupWithoutBlock()
    {
        return IsShowPopup() && !activePopup.Where(_ => _.isBackground == true).ToList().IsNullOrEmpty();
    }

public bool IsGameStopPopup(Define.EPOPUP_TYPE type)
{
    switch (type)
    {
            case Define.EPOPUP_TYPE.PopupOption:
            return true;
    }
    return false;
}

internal bool IsGameMustStopPopup()
{
    if (activePopup.Count == 0)
        return false;
    for (int i = 0; i < activePopup.Count; i++)
    {
        if (IsMustStopPopup(activePopup[i].popupboxType))
            return true;
    }

    return false;
}

public bool IsMustStopPopup(Define.EPOPUP_TYPE type)
{
    switch (type)
    {
            case Define.EPOPUP_TYPE.PopupOption:
            return true;
    }

    return false;

}
public bool IsShowPopup(Define.EPOPUP_TYPE boxType)
{
    foreach (var popup in activePopup)
    {
        if (popup.popupboxType == boxType)
        {
            return true;
        }
    }

    return false;
}

public bool IsWaitPopup(Define.EPOPUP_TYPE boxType)
{
    foreach (var popup in waitActivePopup)
    {
        if (popup.PopupBoxType == boxType)
        {
            return true;
        }
    }

    return false;

}
public bool IsWaitPopup()
{
    return waitActivePopup.Count != 0;
}
public int GetCountActivePopupbox()
{
    return activePopup.Count;
}

public PopupBase GetActivePopupBox(Define.EPOPUP_TYPE boxType)
{
    foreach (var popup in activePopup)
    {
        if (popup.popupboxType == boxType)
        {
            return popup;
        }
    }
    return null;
}

public void ClearPopupBox()
{
    waitActivePopup.RemoveAll(_ => _.PopupBoxType != Define.EPOPUP_TYPE.None);
    waitClosePopup.Clear();
    foreach (var mit in activePopup)
    {
        if (mit == null)
            continue;
        mit.gameObject.Destroy();
    }

    activePopup.Clear();
    ActiveBackground(false);
}

public void CloseFrontPopupBox()
{
    if (activePopup.Count != 0)
    {
        var popup = activePopup[activePopup.Count - 1];
        if (popup.popupboxType != Define.EPOPUP_TYPE.PopupNetWait
            && popup.isFullPopup == false)
        {
            ClosePopupBox(activePopup[activePopup.Count - 1].gameObject);
        }
    }
}



public async void ShowPopupBox(PopupWaitData boxData)
{
    if (CheckMaintainNewObjectPopup(boxData.PopupBoxType))
    {
        var popup = activePopup.Find(_1 => _1.popupboxType == boxData.PopupBoxType);
        if (popup != null)
        {
            popup.OnClosePopup();
            activePopup.Remove(popup);
            GameObject clone = popup.gameObject;
            clone.gameObject.SetActive(false);
            ActiveBackground(activePopup.Count != 0);
            clone.transform.SetAsLastSibling();

            popup.popupboxType = boxData.PopupBoxType;
            popup.popupUID = boxData.uid;
            popup.isBackground = boxData.BackgroundOverlayOn;
            popup.isRecycle = true;
            if (boxData.MenuVisible)
            {
                var dungeonState = Managers.Scene.CurrentScene as IGameState;
                dungeonState.SetMenuVisible(true);
            }

            clone.transform.localPosition += new Vector3(0, 0, activePopup.Count);
            clone.SetActive(true);
            popup.InitPopupbox(boxData.PopupArg);
            //popup.PlayAlarm(); //팝업 알람음 출력

            //풀 팝업 이면 뒤에 있는 모든 팝업을 오프
            if (IsWindowPopUp(popup) == false)
            {
                SetDisableAllPopup();
                popup.GetCanvas();
            }

            activePopup.Add(popup);
            //메뉴 UI 캔버스에서 사용
            showPopupSubject.OnNext(0);
            //어두운 백그라운드 설정
            if (popup.isBackground)
                ActiveBackground(true);

            SetAbleFocusPopup();
            return;
        }
    }

    var origin = await GetPoupBoxPrefab(boxData.PopupBoxType);
    if (origin != null)
    {
        var clone = canvas.gameObject.Add(origin);
        clone.gameObject.SetActive(false);
        await UniTask.Delay(10);
        var rect = clone.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var script = clone.GetComponent<PopupBase>();
        script.popupboxType = boxData.PopupBoxType;
        script.popupUID = boxData.uid;
        script.isBackground = boxData.BackgroundOverlayOn;
        script.InitPopupbox(boxData.PopupArg);
        if (boxData.MenuVisible)
        {
                var dungeonState = Managers.Scene.CurrentScene as IGameState;
                dungeonState.SetMenuVisible(true);
        }

        clone.transform.localPosition += new Vector3(0, 0, activePopup.Count);
        clone.SetActive(true);
        //script.PlayAlarm(); //팝업 알람음 출력

        //풀 팝업 이면 뒤에 있는 모든 팝업을 오프
        if (IsWindowPopUp(script) == false)
        {
            SetDisableAllPopup();
            script.GetCanvas();
        }


        activePopup.Add(script);
        //메뉴 UI 캔버스에서 사용
        showPopupSubject.OnNext(0);
        //어두운 백그라운드 설정
        if (script.isBackground)
            ActiveBackground(true);
    }
    else
    {
        CloseWaitPopupBox(null);
    }
}
    
public bool CheckDuplicatedNotAllowed(Define.EPOPUP_TYPE boxType)
{
        if (boxType == Define.EPOPUP_TYPE.PopupPause)
            return true;

        return false;
}

public bool CheckMaintainNewObjectPopup(Define.EPOPUP_TYPE boxTyoe)
{
    if (boxTyoe == Define.EPOPUP_TYPE.None
        )
        return true;

    return false;
}

public void OpenBlockInputUI()
{
    blockUIObj.transform.SetAsLastSibling();
    blockUIObj.SetActive(true);
}

public void CloseBlockInputUI()
{
    blockUIObj.transform.SetAsFirstSibling();
    blockUIObj.SetActive(false);
}

public void OpenBlackUI()
{
    blackUIObj.transform.SetAsLastSibling();
    blackUIObj.SetActive(true);
}

public void CloseBlackUI()
{
    blackUIObj.transform.SetAsFirstSibling();
    blackUIObj.SetActive(false);
}

public bool IsOpenBlockInputPopup()
{
    return blockUIObj.activeSelf;
}


public int ShowPopupBox(Define.EPOPUP_TYPE boxType, PopupArg arg, bool _BackgroundOverlayOn = true)
{
    // 중복 허용하지않는 팝업 검사
    if (CheckDuplicatedNotAllowed(boxType))
    {
        foreach (var popup in activePopup)
        {
            if (popup.popupboxType == boxType)
                return -1;
        }
    }

    var waitPopup = new PopupWaitData
    {
        uid = uid,
        PopupBoxType = boxType,
        PopupArg = arg,
        BackgroundOverlayOn = _BackgroundOverlayOn
    };

    waitActivePopup.Add(waitPopup);
    return uid++;
}

    public bool IsReservationPopup()
{
    return reservationPopup.Count > 0;
}
public void ReservationPopupClear()
{
    reservationPopup.Clear();
}

public void ShowReservationPopup()
{
    if (reservationPopup.Count != 0)
    {
        foreach (var waitData in reservationPopup)
        {
            ShowPopupBox(waitData);
        }

        reservationPopup.Clear();
    }
}

public int ReservationPopup(Define.EPOPUP_TYPE boxType, PopupArg arg, bool _BackgroundOverlayOn = true, bool _MenuVisible = false)
{
    var waitPopup = new PopupWaitData
    {
        uid = uid,
        PopupBoxType = boxType,
        PopupArg = arg,
        BackgroundOverlayOn = _BackgroundOverlayOn,
        MenuVisible = _MenuVisible
    };
    if (reservationPopup.Any(_ => _.PopupBoxType == boxType))
        return uid;
    reservationPopup.Add(waitPopup);
    return uid++;
}


public void ClosePopupBox(GameObject _popupObject)
{
    if (_popupObject)
    {
        ClosePopupBox(_popupObject.GetComponent<PopupBase>());
    }
}

public void ClosePopupBox(List<Define.EPOPUP_TYPE> ePOPUP_TYPEs)
{
    if (ePOPUP_TYPEs != null && ePOPUP_TYPEs.Count > 0)
    {
        foreach (var type in ePOPUP_TYPEs)
        {
            ClosePopupBox(type);
        }
    }
}

internal void ClosePopupBox(PopupBase _popup)
{
    if (_popup)
    {
        _popup.OnClosePopup();
        waitClosePopup.Add(_popup);
        if (IsWindowPopUp(_popup) == false)
        {
            closeFullPopupSubject.OnNext(0);
        }
    }

}
public void CloseAllPopupBox()
{
    foreach (var mit in activePopup)
    {
        if (mit.popupboxType == Define.EPOPUP_TYPE.PopupLock)
            continue;
        mit.OnClosePopup();
        waitClosePopup.Add(mit);
    }
}

public void ClosePopupBox(Define.EPOPUP_TYPE type)
{
    foreach (var mit in activePopup)
    {
        if (mit.popupboxType == type)
        {
            mit.OnClosePopup();
            waitClosePopup.Add(mit);
            if (IsWindowPopUp(type) == false)
                closeFullPopupSubject.OnNext(0);
        }
    }
}

public void ClosePopupBox(int uid)
{
    if (-1 == uid)
        return;
    for (int i = 0; i < waitActivePopup.Count; i++)
    {
        if (waitActivePopup[i].uid == uid)
        {
            waitActivePopup.Remove(waitActivePopup[i]);
            return;
        }
    }

    foreach (var mit in activePopup)
    {
        if (mit.popupUID == uid)
        {
            mit.OnClosePopup();
            waitClosePopup.Add(mit);
            break;
        }
    }
}

private void CloseWaitPopupBox(PopupBase popupObject)
{
    if (popupObject)
    {
        activePopup.Remove(popupObject);
        popupObject.gameObject.SetActive(false);
#if TEST_DOWNLOAD || !UNITY_EDITOR
            Managers.Resource.ReleasePopupAsset(popupObject.popupboxType);
#endif
            GameObject.Destroy(popupObject.gameObject);
            popupObject = null;
    }

    ActiveBackground(activePopup.Count != 0);
}

//해당팝업이 이미 존재하는지 확인
public bool CheckContainPopupbox(Define.EPOPUP_TYPE _type)
{
    foreach (var mit in activePopup)
    {
        if (mit == null)
            continue;
        if (mit.popupboxType == _type)
            return true;
    }

    return false;
}

public UniTask<GameObject> GetPoupBoxPrefab(Define.EPOPUP_TYPE _eType)
{
    return GetPoupBoxPrefab(_eType.ToString());
}

public UniTask<GameObject> GetPoupBoxPrefab(string _popup)
{
#if TEST_DOWNLOAD || !UNITY_EDITOR
        return Managers.Resource.LoadPopup(_popup);
#elif UNITY_EDITOR
        return Managers.Resource.LoadAsyncGameObject($"popup/{_popup}", _popup + ".prefab");
#endif

    }

    private void ActiveBackground(bool active)
{
    if (PopupBackgroundOverlay)
    {
        PopupBackgroundOverlay.SetActive(active);
        PopupBackgroundOverlay.transform.SetSiblingIndex(activePopup.Count);
    }
}

public void AddFocus(int id)
{
    if (focusStack.Contains(id))
        return;

    focusStack.Add(id);
}

public void RemoveFocus(int id)
{
    focusStack.RemoveAll(_ => _ == id);
}

public bool IsFocus(int id)
{
    //esc와 캔버스 끄고 켜는데 사용
    if (focusStack.Count == 0)
        return false;
    return focusStack[focusStack.Count - 1] == id;
}

public void AddFullpopup(int id)
{
    if (fullPopupStack.Contains(id))
        return;

    fullPopupStack.Add(id);
}

public void RemoveFullpopup(int id)
{
    fullPopupStack.RemoveAll(_ => _ == id);
}

public bool IsFocusFullpopup(int id)
{
    //캔버스 끄고 켜는데 사용
    if (fullPopupStack.Count == 0)
        return false;
    return fullPopupStack[fullPopupStack.Count - 1] == id;

}
public bool IsWindowPopUp(PopupBase _popup)
{
    if (_popup != null)
    {
        return IsWindowPopUp(_popup.popupboxType) || _popup.CheckFullPopup() == false;
    }

    return false;
}

public bool IsWindowPopUp(Define.EPOPUP_TYPE type)
{
    if (type == Define.EPOPUP_TYPE.PopupConfirm
        || type == Define.EPOPUP_TYPE.PopupYesNo
        || type == Define.EPOPUP_TYPE.PopupOption)
        return true;
    else
        return false;
}

public bool CheckExistFullPopup()
{
    if (activePopup.Count == 0)
        return false;
    foreach (var mit in activePopup)
    {
        if (IsWindowPopUp(mit) == false)
            return true;
    }

    return false;
}

public void ShowWait()
{
    if (this.waitObj.activeSelf) return;

    this.waitObj.transform.SetAsLastSibling();
    this.waitObj.SetActive(true);
}

public void CloseWait()
{
    if (this.waitObj.activeSelf == false) return;

    this.waitObj.transform.SetAsFirstSibling();
    this.waitObj.SetActive(false);
}

public bool CheckContainReservationPopupbox(Define.EPOPUP_TYPE type)
{
    return reservationPopup.Find(x => x.PopupBoxType == type) != null;
}

public bool CheckContainPopup(Define.EPOPUP_TYPE type)
{
    if (CheckContainPopupbox(type))
        return true;
    else if (CheckContainReservationPopupbox(type))
        return true;

    return false;
}


public bool IsPopupActive(Define.EPOPUP_TYPE type)
{
    foreach (var pop in activePopup)
    {
        if (pop.popupboxType == type)
            return true;
    }
    return false;
}

public Vector2 GetCanvasScale()
{
    return canvas.GetComponent<RectTransform>().localScale;
}

public void ShowSystenMenu(bool _show)
{
    systemMenuVisible.SetValueAndForceNotify(_show);
}

public bool IsShowSystemMenu()
{
    return systemMenuVisible.Value;
}
    }