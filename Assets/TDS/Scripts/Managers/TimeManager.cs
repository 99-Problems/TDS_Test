using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;

public class TimeManager : MonoBehaviour
{
    public static float timeScale { get; private set; } = 1f;
    public static float playTime;
    public static float inGameTime;
    public bool IsPause => GetPaused();

    public void SetGameSpeed(float _speed)
    {
        Debug.Log($"게임속도변경{timeScale} -> {_speed}", Color.green);
        timeScale = _speed;
        Time.timeScale = timeScale;
    }

    public float GetGameSpeed()
    {
        return Time.timeScale;
    }

    public void Pause()
    {
        Time.timeScale = 0f;
    }

    public bool GetPaused()
    {
        return GetGameSpeed() == 0f;
    }


    public void Resume()
    {
        Time.timeScale = timeScale;
    }
}
