using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

[HideMonoScript]
public abstract class BaseScene : MonoBehaviour
{
    [Title("πË∞Ê¿Ωæ«")]
    [PropertySpace(0,15)]
    public AudioClip bgm;

    void Awake()
    {
        Managers.Scene.SetCurrentScene(this);
        Init();
    }

    protected virtual void Init()
    {
        if (EventSystem.current != null) return;
        if (FindObjectOfType<EventSystem>() != null) return;
        new GameObject("@EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    public abstract void Clear();

#if UNITY_EDITOR
    public void DebugWindow(int _id)
    {
        if (GUI.Button(new Rect(5, 20, 50, 30), "UP"))
        {
            var speed = Managers.Time.GetGameSpeed();
            speed += 0.1f;
            Debug.Log($"GameSpeed {speed}");
            Managers.Time.SetGameSpeed(speed);
        }

        if (GUI.Button(new Rect(5, 30 + 20, 50, 30), "Down"))
        {
            var speed = Managers.Time.GetGameSpeed();
            speed -= 0.1f;
            Debug.Log($"GameSpeed {speed}");
            Managers.Time.SetGameSpeed(speed);
        }

        if (GUI.Button(new Rect(5, 60 + 20, 50, 30), "Reset"))
        {
            var speed = 1;
            Debug.Log($"GameSpeed {speed}");
            Managers.Time.SetGameSpeed(speed);
        }
        if (GUI.Button(new Rect(5, 90 + 20, 50, 30), "UP*10"))
        {
            var speed = Managers.Time.GetGameSpeed();
            speed += 1f;
            Debug.Log($"GameSpeed {speed}");
            Managers.Time.SetGameSpeed(speed);
        }

        GUI.DragWindow();
    }

#endif
}
