using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchEffectUI : MonoBehaviour
{
    public ParticleSystem particleStart;
    public ParticleSystem particleEnd;
    public RectTransform startRect;
    public RectTransform endRect;
    private RectTransform canvasRect;

    void Start()
    {
        Canvas mainCanvas = GetComponentInParent<Canvas>();
        canvasRect = mainCanvas.GetComponent<RectTransform>();

        this.UpdateAsObservable()
        .Select(_ => Input.GetMouseButtonDown(0))
        .Where(x => x)
        .Subscribe(x =>
        {
            startRect.position = Input.mousePosition;
            particleStart.Play();
        });

        this.UpdateAsObservable()
            .Select(_ => Input.GetMouseButtonUp(0))
            .Where(x => x)
            .Subscribe(x =>
            {
                endRect.position = Input.mousePosition;
                particleEnd.Play();
            });
    }
}