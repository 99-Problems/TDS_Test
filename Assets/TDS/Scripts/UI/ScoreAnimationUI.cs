using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using System.Linq;
using Unity.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using System;

public class ScoreAnimationUI : MonoBehaviour
{
    [ReadOnly]
    public long currentScore;
    public long targetScore;
    public float gap = 0.1f;
    public int minValue = 1;
    public float delay = 0.1f;
    [ReadOnly]
    public bool stop = false;
    public bool isVariableMinValue = false;
    TMP_Text text;
    void Start()
    {
        text = GetComponent<TMP_Text>();
        if (targetScore == 0)
            text.text = targetScore.ValueToString();
        gameObject.UpdateAsObservable().ThrottleFirst(TimeSpan.FromSeconds(delay)).Subscribe(_1 =>
        {
            if (stop)
                return;
            if (currentScore != targetScore)
            {
                currentScore += Mathf.Max(minValue, (int)((targetScore - currentScore) * gap));
                if (currentScore > targetScore)
                    currentScore = targetScore;
                text.text = currentScore.ValueToString();
            }
        });
    }

    public void SetMinValue()
    {
        minValue = Mathf.Max((int)(targetScore * 0.01f), minValue);
    }

    public void SetCurrentScore(long curScore)
    {
        currentScore = curScore;
        text.text = currentScore.ValueToString();
    }
    public void SetStop(bool _stop)
    {
        stop = _stop;
    }
    public void SetColor(Color _color)
    {
        text.color = _color;
    }

    public void SetTargetScore(int score)
    {
        targetScore = score;
        if (isVariableMinValue)
            SetMinValue();
    }
    public void SetTargetScore(long score)
    {
        targetScore = score;
        if (isVariableMinValue)
            SetMinValue();

    }
}
