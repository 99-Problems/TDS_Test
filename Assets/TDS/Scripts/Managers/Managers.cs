using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class Managers : MonoBehaviour
{
    static Managers S_Instance;

    public static Managers Instance
    {
        get
        {
            Init();
            return S_Instance;
        }
    }

    #region managers
    InputManager input = new InputManager();
    SceneManagerEx scene = new SceneManagerEx();
    TimeManager time = new TimeManager();
    PopupManager popup = new PopupManager();
    ResourceManager resource = new ResourceManager();
    DeviceManager device = new DeviceManager();
    PoolManager pool = new PoolManager();
    DataManager data = new DataManager();
    SoundManager sound = new SoundManager();
    StringManager stringManager = new StringManager();

    // static managers
    public static InputManager Input => Instance.input;
    public static SceneManagerEx Scene => Instance.scene;
    public static TimeManager Time => Instance.time;
    public static PopupManager Popup => Instance.popup;
    public static ResourceManager Resource => Instance.resource;
    public static DeviceManager Device => Instance.device;
    public static PoolManager Pool => Instance.pool;
    public static DataManager Data => Instance.data;
    public static SoundManager Sound => Instance.sound;
    public static StringManager String => Instance.stringManager;

    #endregion

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        input.OnUpdate();
    }

    static void Init()
    {
        if(S_Instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            S_Instance = go.GetComponent<Managers>();

            S_Instance.popup.Init();
            S_Instance.device.Init();
            S_Instance.pool.Init();
            S_Instance.data.Init();
            S_Instance.sound.Init();
            S_Instance.stringManager.Init();
        }
    }

    public static void Clear()
    {
        Input.Clear();
        Scene.Clear();
        Pool.Clear();
    }


#if UNITY_EDITOR
    private const string infinityMode = "Menu/게임 종료 안되게 설정";
    public static bool isinfinityMode => EditorPrefs.GetBool(infinityMode);

    [MenuItem(infinityMode)]
    private static void SetInfinityMode()
    {
        var _isinfinityMode = !isinfinityMode;
        Menu.SetChecked(infinityMode, _isinfinityMode);
        EditorPrefs.SetBool(infinityMode, _isinfinityMode);
        SceneView.RepaintAll();
    }
#endif
}
