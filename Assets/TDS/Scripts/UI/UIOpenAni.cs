using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIOpenAni : MonoBehaviour
{
    public enum EDIRECTION
    {
        UP,
        RIGHT,
        LEFT,
        DOWN,
    }
    [Title("텍스트 설정")]
    public bool textOn = false;
    [ShowIf("textOn")]
    [LabelText("원본 스트링 변경")]
    public bool setString = false;
    [ShowIf("textOn")]
    [LabelText("스트링 설정")]
    public bool useCustomString = false;
    [ShowIf("@useCustomString || setString")]
    [LabelText("스트링 ID 받기")]
    public bool getStringID = false;
    [ShowIf("getStringID")]
    public int stringID;
    [ShowIf("@useCustomString || setString")]
    [HideIf("getStringID")]
    [LabelText("스트링 직접 입력")]
    public string customString;


    [ShowIf("textOn")]
    public bool doText = false;
    [ShowIf("doText")]
    public ScrambleMode scrambleMode = ScrambleMode.All;

    [ShowIf("@textOn && doBlendedColor == false")]
    [LabelText("색상 변경")]
    public bool doColor = false;
    [ShowIf("@textOn && doColor == false")]
    [LabelText("색상 변경(Blended)")]
    public bool doBlendedColor = false;
    [ShowIf("@doColor || doBlendedColor")]
    public Color textColor = Color.red;

    [ShowIf("textOn")]
    [LabelText("시간")]
    public float textDuration = 2f;
    [ShowIf("textOn")]
    public Ease textEase = Ease.Unset;
    [ShowIf("textOn")]

    [LabelText("텍스트 루프")]
    public bool textLoopOn = false;
    [ShowIf("textLoopOn")]
    [LabelText("루프 횟수 (-1: 무한)")]
    public int textLoopTime = -1;
    [ShowIf("@textLoopOn && doText && textLoopTime != -1")]
    [BoxGroup("루프 후 원복")]
    public bool setOriginText = false;
    [ShowIf("@textLoopOn && (doColor || doBlendedColor) && textLoopTime != -1")]
    [BoxGroup("루프 후 원복")]
    public bool setOriginColor = false;
    [ShowIf("textLoopOn")]
    [LabelText("루프 타입")]
    public LoopType textLoopType = LoopType.Yoyo;

    [Title("UI 설정")]
    public bool fadeOn = false;
    [ShowIf("fadeOn")]
    [LabelText("페이드 루프")]
    public bool fadeLoop = false;
    [ShowIf("fadeOn")]
    [LabelText("루프 횟수 (-1: 무한)")]
    public int fadeLoopTime = -1;
    [ShowIf("fadeLoop")]
    [LabelText("루프 타입")]
    public LoopType fadeLoopType = LoopType.Yoyo;
    [ShowIf("fadeLoop")]
    public Ease fadeEase = Ease.Linear;

    public bool moveOn = false;
    [ShowIf("moveOn")]
    [LabelText("무브 루프")]
    public bool loopOn = false;
    [ShowIf("loopOn")]
    [LabelText("루프 횟수 (-1: 무한)")]
    public int loopTime = -1;
    [ShowIf("loopOn")]
    [LabelText("루프 타입")]
    public LoopType loopType = LoopType.Yoyo;

    [Title("이동방향")]
    public EDIRECTION directionType = EDIRECTION.UP;

    public Ease directionEase = Ease.Linear;

    [Title("이동거리")]
    public float offset = 30;

    [Title("페이드 기간")]
    public float fadeDuration = 0.5f;

    [Title("페이드 아웃 기간")]
    public float fadeOutDuration = 0.5f;

    [Title("이동 기간")]
    public float moveDuration = 0.5f;

    [Title("애니 딜레이")]
    public float delay = 0.1f;

    [Title("페이드인 시작 알파값")]
    public float firstAlpha = 0f;

    [Title("페이드인 END 알파값")]
    public float endAlpha = 1f;

    [Title("스케일")]
    public bool scaleOn = false;

    [ShowIf("@scaleOn && scaleX == false")]
    [LabelText("Y만 변경")]
    public bool scaleY = false;
    [ShowIf("@scaleOn && scaleY == false")]
    [LabelText("X만 변경")]
    public bool scaleX = false;
    [ShowIf("scaleOn")]
    public float scaleOffset = 0f;
    [ShowIf("scaleOn")]
    public float scaleDuration = 0.5f;
    [ShowIf("scaleOn")]
    public Ease scaleEase = Ease.Linear;
    [ShowIf("scaleOn")]
    public bool scaleLoop = false;
    [ShowIf("scaleLoop")]
    [LabelText("루프 횟수 (-1: 무한)")]
    public int scaleLoopTime = -1;
    [ShowIf("scaleLoop")]
    public LoopType scaleLoopType = LoopType.Yoyo;
    [ShowIf("scaleLoop")]
    public bool setOriginScale = false;

    [Title("점프")]
    public bool jumpOn = false;

    public int jumpOffsety = 0;
    public float jumpPower = 10f;
    public int jumpNum = 1;
    public int jumpLoop = 0;
    public float jumpDuration = 0.5f;
    Vector2 jumpOffset;

    [Title("코드에서 시작 컨트롤")]
    public bool startNotPlay = false;

    public bool onEnablePlay = false;
    public bool awakeInit = true;

    public Subject<int> OnPlay = new Subject<int>();

    bool bFirst = true;
    Vector3 firstScaleVec;
    float firstPosValue = 0f;
    [Title("코드에서 세팅")]
    public CanvasGroup canvasGroup;
    public RectTransform rect;
    public TMP_Text TMPtext;
    public Text text;
    private string originText;
    private Color originColor;
    private Sequence tween;

    public void Awake()
    {
        InitComponent();
    }

    public void OnEnable()
    {
        if (onEnablePlay)
            StartPlay();
    }

    public void OnDisable()
    {
        tween?.Kill();
    }

    public void Start()
    {
        if (startNotPlay || onEnablePlay)
        {
            if (awakeInit)
                Init();
            return;
        }

        StartPlay();
    }

    [Button]
    public void StartPlay()
    {
        Init();
        if (delay > 0)
        {
            EntryAni();
        }
        else
        {
            Play();
        }
    }

    public void InitComponent()
    {
        if (fadeOn)
        {
            if (canvasGroup == null)
                canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (textOn)
        {
            if (TMPtext == null)
                TMPtext = gameObject.GetComponent<TMP_Text>();
            if (TMPtext == null)
            {
                text = gameObject.GetComponent<Text>();
            }

            if (TMPtext == null && text == null)
                textOn = false;
            else if (TMPtext)
            {
                originText = TMPtext.text;
                originColor = TMPtext.color;
            }
            else if (text)
            {
                originText = text.text;
                originColor = text.color;
            }

        }
        if (moveOn || scaleOn || jumpOn)
        {
            if (rect == null)
                rect = gameObject.GetComponent<RectTransform>();
        }
    }

    public void InitValue()
    {
        if (bFirst == false)
            return;

        if (moveOn || scaleOn || jumpOn)
        {
            if (rect == null)
                InitComponent();
        }

        if (rect == null)
            return;
        if (moveOn)
        {
            switch (directionType)
            {
                case EDIRECTION.UP:
                case EDIRECTION.DOWN:
                    firstPosValue = rect.anchoredPosition.y;
                    break;
                case EDIRECTION.RIGHT:
                case EDIRECTION.LEFT:
                    firstPosValue = rect.anchoredPosition.x;
                    break;
                default:
                    break;
            }
        }

        if (scaleOn)
        {
            firstScaleVec = rect.localScale;
        }

        if (jumpOn) firstPosValue = rect.anchoredPosition.y;
        bFirst = false;
    }

    public void Init()
    {
        //if(awakeInit == false)
        InitValue();

        if (fadeOn)
        {
            canvasGroup.DOKill();
            canvasGroup.alpha = firstAlpha;
        }

        if (moveOn)
        {
            rect.DOKill();
            switch (directionType)
            {
                case EDIRECTION.UP:
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, firstPosValue - offset);
                    break;
                case EDIRECTION.RIGHT:
                    rect.anchoredPosition = new Vector2(firstPosValue - offset, rect.anchoredPosition.y);
                    break;
                case EDIRECTION.LEFT:
                    rect.anchoredPosition = new Vector2(firstPosValue + offset, rect.anchoredPosition.y);
                    break;
                case EDIRECTION.DOWN:
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, firstPosValue + offset);
                    break;
                default:
                    break;
            }
        }

        if (scaleOn)
        {
            rect.DOKill();
            rect.localScale = firstScaleVec;
        }

        if (jumpOn)
        {
            rect.DOKill();
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, firstPosValue);
            jumpOffset = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y + jumpOffsety);
        }
    }

    public void Play()
    {
        #region Text Ani
        if (textOn)
        {
            if (TMPtext)
            {
                TMPtext.DOKill();
                var _text = TMPtext.text;
                if (setString)
                {
                    var GTMPro = GetComponent<GTMPro>();
                    if (GTMPro)
                    {
                        //if (getStringID)
                        //{
                        //    GTMPro.SetStringID(stringID);
                        //}
                        //else
                        //{
                        //    GTMPro.SetText(customString);
                        //}
                    }
                }

                if (useCustomString)
                {
                    //if (getStringID)
                    //{
                    //    _text = Managers.String.GetString(stringID);
                    //}
                    //else
                    //{
                    //    _text = customString.IsNullOrEmpty() ? TMPtext.text : customString;
                    //}
                }

                switch (textLoopOn)
                {
                    case true:
                        if (doText)
                        {
                            TMPtext.DOText(_text, textDuration, true, scrambleMode).SetEase(textEase).SetLoops(textLoopTime, textLoopType).OnComplete(() =>
                            {
                                if (setOriginText)
                                {
                                    TMPtext.DOText(originText, textDuration, true, scrambleMode).SetEase(textEase);
                                }
                            });
                        }

                        if (doColor)
                        {
                            TMPtext.DOColor(textColor, textDuration).SetEase(textEase).SetLoops(textLoopTime, textLoopType).OnComplete(() =>
                            {
                                if (setOriginColor)
                                {
                                    TMPtext.DOColor(originColor, textDuration).SetEase(textEase).SetLoops(textLoopTime, textLoopType);
                                }
                            }); ;
                        }
                        else if (doBlendedColor)
                        {
                            TMPtext.DOBlendableColor(textColor, textDuration).SetEase(textEase).SetLoops(textLoopTime, textLoopType).OnComplete(() =>
                            {
                                if (setOriginColor)
                                {
                                    TMPtext.DOBlendableColor(originColor, textDuration).SetEase(textEase).SetLoops(textLoopTime, textLoopType);
                                }
                            });
                        }

                        break;
                    case false:
                        if (doText)
                        {
                            TMPtext.DOText(_text, textDuration, true, scrambleMode).SetEase(textEase);
                        }

                        if (doColor)
                        {
                            TMPtext.DOColor(textColor, textDuration).SetEase(textEase);
                        }
                        else if (doBlendedColor)
                        {
                            TMPtext.DOBlendableColor(textColor, textDuration).SetEase(textEase);
                        }

                        break;
                }

            }
            else if (text)
            {
                text.DOKill();
                var _text = text.text;

                if (setString)
                {
                    if (getStringID)
                    {
                        //text.text = Managers.String.GetString(stringID);
                    }
                    else
                    {
                        text.text = customString;
                    }
                }

                if (useCustomString)
                {
                    if (getStringID)
                    {
                        //_text = Managers.String.GetString(stringID);
                    }
                    else
                    {
                        _text = customString.IsNullOrEmpty() ? text.text : customString;
                    }
                }

                switch (textLoopOn)
                {
                    case true:
                        if (doText)
                        {
                            text.DOText(_text, textDuration, true, scrambleMode).SetEase(textEase).SetLoops(textLoopTime, textLoopType).OnComplete(() =>
                            {
                                if (setOriginText)
                                {
                                    text.DOText(originText, textDuration, true, scrambleMode).SetEase(textEase);
                                }
                            });
                        }

                        if (doColor)
                        {
                            text.DOColor(textColor, textDuration).SetEase(textEase).SetLoops(textLoopTime, textLoopType).OnComplete(() =>
                            {
                                if (setOriginColor)
                                {
                                    text.DOColor(originColor, textDuration).SetEase(textEase).SetLoops(textLoopTime, textLoopType);
                                }
                            });
                        }
                        else if (doBlendedColor)
                        {
                            text.DOBlendableColor(textColor, textDuration).SetEase(textEase).SetLoops(textLoopTime, textLoopType).OnComplete(() =>
                            {
                                if (setOriginColor)
                                {
                                    text.DOBlendableColor(originColor, textDuration).SetEase(textEase).SetLoops(textLoopTime, textLoopType);
                                }
                            });
                        }

                        break;
                    case false:
                        if (doText)
                        {
                            text.DOText(_text, textDuration, true, scrambleMode).SetEase(textEase);
                        }

                        if (doColor)
                        {
                            text.DOColor(textColor, textDuration).SetEase(textEase);
                        }
                        else if (doBlendedColor)
                        {
                            text.DOBlendableColor(textColor, textDuration).SetEase(textEase);
                        }

                        break;
                }

            }
        }
        #endregion
        if (fadeOn)
        {
            switch (fadeLoop)
            {
                case true:
                    canvasGroup.DOFade(endAlpha, fadeDuration).SetLoops(fadeLoopTime, fadeLoopType).SetEase(fadeEase);
                    break;
                case false:
                    canvasGroup.DOFade(endAlpha, fadeDuration).SetEase(fadeEase);
                    break;
            }
        }
        if (moveOn)
        {
            if (rect == null)
                return;
            switch (directionType)
            {
                case EDIRECTION.UP:
                    if (loopOn)
                    {
                        rect.DOAnchorPosY(rect.anchoredPosition.y + offset, moveDuration).SetEase(directionEase).SetLoops(loopTime, loopType);
                    }
                    else
                    {
                        rect.DOAnchorPosY(rect.anchoredPosition.y + offset, moveDuration).SetEase(directionEase);
                    }
                    break;
                case EDIRECTION.RIGHT:
                    if (loopOn)
                    {
                        rect.DOAnchorPosX(rect.anchoredPosition.x + offset, moveDuration).SetEase(directionEase).SetLoops(loopTime, loopType);
                    }
                    else
                    {
                        rect.DOAnchorPosX(rect.anchoredPosition.x + offset, moveDuration).SetEase(directionEase);
                    }
                    break;
                case EDIRECTION.LEFT:
                    if (loopOn)
                    {
                        rect.DOAnchorPosX(rect.anchoredPosition.x - offset, moveDuration).SetEase(directionEase).SetLoops(loopTime, loopType);
                    }
                    else
                    {
                        rect.DOAnchorPosX(rect.anchoredPosition.x - offset, moveDuration).SetEase(directionEase);
                    }
                    break;
                case EDIRECTION.DOWN:
                    if (loopOn)
                    {
                        rect.DOAnchorPosY(rect.anchoredPosition.y - offset, moveDuration).SetEase(directionEase).SetLoops(loopTime, loopType);
                    }
                    else
                    {
                        rect.DOAnchorPosY(rect.anchoredPosition.y - offset, moveDuration).SetEase(directionEase);
                    }
                    break;
                default:
                    break;
            }
        }

        if (scaleOn)
        {
            if (rect == null)
                return;

            switch (scaleLoop)
            {
                case true:
                    if (scaleY)
                    {
                        rect.DOScaleY(firstScaleVec.y * scaleOffset, scaleDuration).SetLoops(scaleLoopTime, scaleLoopType).SetEase(scaleEase).OnComplete(() =>
                        {
                            if (setOriginScale)
                            {
                                rect.DOScaleY(firstScaleVec.y, scaleDuration).SetEase(scaleEase);
                            }
                        });
                    }
                    else if (scaleX)
                    {
                        rect.DOScaleX(firstScaleVec.x * scaleOffset, scaleDuration).SetLoops(scaleLoopTime, scaleLoopType).SetEase(scaleEase).OnComplete(() =>
                        {
                            if (setOriginScale)
                            {
                                rect.DOScaleX(firstScaleVec.x, scaleDuration).SetEase(scaleEase);
                            }
                        });
                    }
                    else
                    {
                        rect.DOScale(firstScaleVec * scaleOffset, scaleDuration).SetLoops(scaleLoopTime, scaleLoopType).SetEase(scaleEase).OnComplete(() =>
                        {
                            if (setOriginScale)
                            {
                                rect.DOScale(firstScaleVec * scaleOffset, scaleDuration).SetEase(scaleEase);
                            }
                        });
                    }
                    break;

                case false:
                    if (scaleY)
                    {
                        rect.DOScaleY(firstScaleVec.y * scaleOffset, scaleDuration).SetEase(scaleEase).OnComplete(() =>
                        {
                            if (setOriginScale)
                            {
                                rect.DOScaleY(firstScaleVec.y, scaleDuration).SetEase(scaleEase);
                            }
                        });
                    }
                    else if (scaleX)
                    {
                        rect.DOScaleX(firstScaleVec.x * scaleOffset, scaleDuration).SetEase(scaleEase).OnComplete(() =>
                        {
                            if (setOriginScale)
                            {
                                rect.DOScaleX(firstScaleVec.x, scaleDuration).SetEase(scaleEase);
                            }
                        });
                    }
                    else
                    {
                        rect.DOScale(firstScaleVec * scaleOffset, scaleDuration).SetEase(scaleEase).OnComplete(() =>
                        {
                            if (setOriginScale)
                            {
                                rect.DOScale(firstScaleVec * scaleOffset, scaleDuration).SetEase(scaleEase);
                            }
                        });
                    }
                    break;
            }
        }

        if (jumpOn)
        {
            tween?.Kill();
            tween = rect.DOJumpAnchorPos(jumpOffset, jumpPower, jumpNum, jumpDuration).SetLoops(jumpLoop, LoopType.Restart).SetEase(directionEase);
        }

        OnPlay.OnNext(0);
    }


    async void EntryAni()
    {
        await UniTask.Delay((int)(delay * 1000), true);
        Play();
    }

    public async void SetActiveFalseWithScale(float delaySec = 0)
    {
        if (delaySec > 0)
            await UniTask.Delay((int)(delaySec * 1000), true);
        rect.localScale = Vector3.one;
        rect.DOScale(rect.localScale * scaleOffset, scaleDuration).SetEase(scaleEase);
        await UniTask.Delay((int)(scaleDuration * 1000), true);
        canvasGroup.DOKill();
        canvasGroup.DOFade(0, fadeOutDuration);
        await UniTask.Delay((int)(fadeOutDuration * 1000), true);
        if (gameObject) gameObject.SetActive(false);
    }

    public async void SetActiveFalse(float delaySec)
    {
        await UniTask.Delay((int)(delaySec * 1000), true);
        canvasGroup.DOKill();
        canvasGroup.DOFade(0, fadeOutDuration);
        await UniTask.Delay((int)(fadeOutDuration * 1000), true);
        if (gameObject) gameObject.SetActive(false);
    }

    public async void SetFadeOut()
    {
        if (canvasGroup)
        {
            canvasGroup.DOKill();
            if (fadeOn) canvasGroup.DOFade(firstAlpha, fadeDuration * 0.5f);
        }
        if (moveOn)
        {
            if (rect == null)
                return;
            switch (directionType)
            {
                case EDIRECTION.UP:
                    rect.DOAnchorPosY(rect.anchoredPosition.y - offset, moveDuration).SetEase(directionEase);
                    break;
                case EDIRECTION.RIGHT:
                    rect.DOAnchorPosX(rect.anchoredPosition.x - offset, moveDuration).SetEase(directionEase);
                    break;
                case EDIRECTION.LEFT:
                    rect.DOAnchorPosX(rect.anchoredPosition.x + offset, moveDuration).SetEase(directionEase);
                    break;
                case EDIRECTION.DOWN:
                    rect.DOAnchorPosY(rect.anchoredPosition.y + offset, moveDuration).SetEase(directionEase);
                    break;
                default:
                    break;
            }
        }
        await UniTask.Delay((int)(fadeOutDuration * 1000), true);
        if (gameObject) gameObject.SetActive(false);
    }

    public void SetActive(bool _OnOff)
    {
        if (_OnOff)
            gameObject.SetActive(true);
        else
            SetFadeOut();
    }
#if UNITY_EDITOR
    [Button]
    public void BackSetting()
    {
        directionType = EDIRECTION.DOWN;
        offset = 10;
        fadeDuration = 0.5f;
        moveDuration = 0.5f;
        firstAlpha = 0.5f;
    }

    [Button]
    public void ButtonSetting()
    {
        directionType = EDIRECTION.UP;
        offset = 15;
        fadeDuration = 0.3f;
        moveDuration = 0.3f;
        delay = 0.3f;
    }

    [Button]
    public void BarSetting()
    {
        directionType = EDIRECTION.LEFT;
        offset = 30;
        fadeDuration = 0.1f;
        moveDuration = 0.5f;
        delay = 0.3f;
        firstAlpha = 0.5f;
    }
#endif
}