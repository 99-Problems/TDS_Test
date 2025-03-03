using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using TMPro;
using DG.Tweening;
using System.Linq;
using Unity.Linq;
using Sirenix.OdinInspector;
using Data;

[RequireComponent(typeof(Toggle))]
public class UIToggleExtension : MonoBehaviour
{
    public enum EDIRECTION
    {
        UP,
        RIGHT,
        LEFT,
        DOWN,
    }

    [BoxGroup("오브젝트")]
    [HorizontalGroup("오브젝트/A")]
    [LabelText("활성화 시 보일 오브젝트"), ListDrawerSettings(Expanded = true, ShowPaging = false, DraggableItems = false)]
    public GameObject[] visibleObj;

    [HorizontalGroup("오브젝트/A")]
    [LabelText("비활성화 시 보일 오브젝트"), ListDrawerSettings(Expanded = true, ShowPaging = false, DraggableItems = false)]
    public GameObject[] invisibleObj;

    [HorizontalGroup("오브젝트/B")]
    [LabelText("interactable false 시 보일 오브젝트"), ListDrawerSettings(Expanded = true, ShowPaging = false, DraggableItems = false)]
    public GameObject lockImage;

    [HorizontalGroup("오브젝트/C")]
    [LabelText("interactable false 시 꺼질 오브젝트"), ListDrawerSettings(Expanded = true, ShowPaging = false, DraggableItems = false)]
    public GameObject lockOffImage;

    [Title("ON")]
    [BoxGroup("컬러")]
    [HorizontalGroup("컬러/A")]
    [HideLabel]
    public Color onColor = Color.white;

    [Title("OFF")]
    [HorizontalGroup("컬러/A")]
    [HideLabel]
    public Color offColor = Color.white;

    [HorizontalGroup("컬러/B")]
    [LabelText("컬러 적용 오브젝트"), ListDrawerSettings(Expanded = true, ShowPaging = false, DraggableItems = false)]
    public Graphic[] colorObject;


    [Title("ON시 효과")]
    public AudioClip onClickSound;

    public ParticleSystem onParticle;

    public bool bCheckmarkAni = false;
    [ShowIf("@bCheckmarkAni")]
    public EDIRECTION direction = EDIRECTION.UP;
    [ShowIf("@bCheckmarkAni")]
    public float aniDuration = 0.3f;

    List<RectTransform> visibleObjRects = new List<RectTransform>();
    void Start()
    {
        var uiToggle = GetComponent<Toggle>();
        if (uiToggle == null)
            return;
        foreach (var mit in visibleObj)
        {
            if (mit)
            {
                var rect = mit.GetComponent<RectTransform>();
                if (rect)
                    visibleObjRects.Add(rect);
            }
        }

        uiToggle.OnValueChangedAsObservable().DistinctUntilChanged().Subscribe(_ =>
        {
            if (bCheckmarkAni)
            {
                foreach (var mit in visibleObjRects)
                {
                    if (mit)
                    {
                        mit.gameObject.SetActive(_);
                        if (_)
                        {
                            switch (direction)
                            {
                                case EDIRECTION.UP:
                                case EDIRECTION.DOWN:
                                    mit.localScale = new Vector3(1, 0, 1);
                                    mit.DOScaleY(1f, aniDuration).SetEase(Ease.OutCirc);
                                    break;
                                case EDIRECTION.RIGHT:
                                case EDIRECTION.LEFT:
                                    mit.localScale = new Vector3(0, 1, 1);
                                    mit.DOScaleX(1f, aniDuration).SetEase(Ease.OutCirc);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var mit in visibleObj)
                {
                    if (mit) mit.SetActive(_);
                }
            }

            foreach (var mit in invisibleObj)
            {
                if (mit) mit.SetActive(!_);
            }

            foreach (var mit in colorObject)
            {
                if (mit) mit.color = _ ? onColor : offColor;
            }
        });

        if (lockImage)
        {
            uiToggle.UpdateAsObservable().Select(_ => uiToggle.interactable).DistinctUntilChanged().Subscribe(interactable =>
            {
                lockImage.SetActive(!interactable);
            });
        }
        if (lockOffImage)
        {
            uiToggle.UpdateAsObservable().Select(_ => uiToggle.interactable).DistinctUntilChanged().Subscribe(interactable =>
            {
                lockOffImage.SetActive(interactable);
            });
        }
        if (onParticle)
        {
            uiToggle.OnValueChangedAsObservable().DistinctUntilChanged().Where(_ => _).Subscribe(_ => { ClientEffect(); });
        }
        if (onClickSound)
        {
            uiToggle.OnPointerClickAsObservable().Subscribe(_ =>
            {
                Managers.Sound.Play(onClickSound, Define.Sound.Effect);
            });
        }
    }
    public void SetText(int stringID)
    {
        lockImage.GetComponent<GTMPro>().SetStringID(stringID);
        lockOffImage.GetComponent<GTMPro>().SetStringID(stringID);
    }
    public void OffVisibleObj()
    {
        foreach (var mit in visibleObj)
        {
            if (mit) mit.SetActive(false);
        }
    }

    public void ClientEffect()
    {
        if (onParticle)
        {
            onParticle.Simulate(0);
            onParticle.Play();
        }
    }

#if UNITY_EDITOR
    Toggle cacheUiToggle;
    bool cache;
    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        cacheUiToggle = GetComponent<Toggle>();
        if (cache != cacheUiToggle.isOn)
        {
            if (visibleObj != null)
            {
                foreach (var mit in visibleObj)
                {
                    if (mit) mit.SetActive(cacheUiToggle.isOn);
                }
            }

            if (invisibleObj != null)
            {
                foreach (var mit in invisibleObj)
                {
                    if (mit) mit.SetActive(!cacheUiToggle.isOn);
                }
            }

            if (colorObject != null)
            {
                foreach (var mit in colorObject)
                {
                    if (mit) mit.color = cacheUiToggle.isOn ? onColor : offColor;
                }
            }
        }

        cache = cacheUiToggle.isOn;
    }
#endif
}