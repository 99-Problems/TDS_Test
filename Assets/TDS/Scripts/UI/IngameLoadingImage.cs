using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Data;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;


public class IngameLoadingImage : MonoBehaviour
{
    public static IngameLoadingImage instance;
    public static Subject<int> LoadingEvent = new Subject<int>();
    public Slider slider;
    private float size;
    private float targetProgress;
    private float currentProgress;
    private bool isLoading;
    public bool isloadingComplete;
    private void OnDestroy()
    {
        instance = null;
    }

    public void Show()
    {
        gameObject.SetActive(true);
       


        isloadingComplete = false;
        var cg = GetComponent<CanvasGroup>();
        cg.alpha = 1;
        currentProgress = 0;
        targetProgress = 0;

        isLoading = true;
    }

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        isloadingComplete = true;
        LoadingEvent.Subscribe(_1 =>
        {
            targetProgress = _1;
            if (_1 == 100)
            {
                currentProgress = 100;
                if (gameObject.activeInHierarchy)
                    StartCoroutine(DestroyAnimation());
            }
        }).AddTo(this);
    }

    public async UniTask WaitLoading()
    {
        await UniTask.WaitUntil(() => isLoading);
    }

    private void Update()
    {
        currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime);
        slider.value = Mathf.Min(currentProgress, 100);
    }

    private IEnumerator DestroyAnimation()
    {
        yield return new WaitForSeconds(0.3f);
        var cg = GetComponent<CanvasGroup>();
        cg.DOFade(0, 0.3f).OnComplete(() =>
        {
            gameObject.SetActive(false);
            isloadingComplete = true;
        });
    }
}