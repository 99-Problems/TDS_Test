using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomTimer
{
    private Action onComplete;

    private float time;

    private float currentTime;

    public bool isRepeat = false;
    public bool isEnd = false;

    public CustomTimer(float _time, Action _onComplete)
    {
        time = _time;
        onComplete = _onComplete;
    }

    public void Reset()
    {
        currentTime = 0;
    }

    public void Update(float _delta)
    {
        if (isEnd)
            return;
        currentTime += _delta;

        if (currentTime >= time)
        {
            onComplete?.Invoke();
            currentTime = 0;
            if (!isRepeat)
                isEnd = true;
        }
    }
}