using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using Data;
using Cysharp.Threading.Tasks;
using UnityEditor;

[RequireComponent(typeof(TMP_Text))]
[DisallowMultipleComponent]
public partial class GTMPro : MonoBehaviour
{
    [OnValueChanged("SetEditString")]
    public int stringID = -1;

    public Define.EFONT_TYPE fontType = Define.EFONT_TYPE.ENONE;
    public TMP_Text text;
    public Define.StringFileType strFileType = Define.StringFileType.Normal;

    [NonSerialized]
    public bool isSetText = false;

    private void Awake()
    {
#if UNITY_EDITOR && !TEST_DOWNLOAD
        text = GetComponent<TMP_Text>();
#else
        if (text)
        {
            if (stringID != -1 && isSetText == false)
            {
                text.text = Managers.String.GetString(stringID, strFileType);
            }

            SetString();
        }
#endif
    }

    public async UniTaskVoid SetString()
    {
        text.font = await StringManager.GetFont(fontType);
        text.fontMaterial = await StringManager.GetMaterial(fontType);
    }

#if UNITY_EDITOR

    private void OnValidate()
    {
        if (Application.isPlaying)
            return;
        SetEditString();
    }

    [Button("스트링적용", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
    public void SetEditString()
    {
        text = GetComponent<TMP_Text>();
        text.text = GetEditorString(stringID);
        
        {
            //text.font = await StringManager.GetFont(fontType);
            //text.fontMaterial = await StringManager.GetMaterial(fontType);
            var fontKey = "";
            var matKey = "";
            switch (fontType)
            {
                default:
                    fontKey = "Changa-Medium SDF.asset";
                    matKey = $"{text.fontSharedMaterial.name}.mat";
                    break;
            }
            var font_assetPath = String.Concat("Assets/TDS/ui/font/", fontKey);
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(font_assetPath);
            if (font != null)
            {
                var path2 = AssetDatabase.GetAssetPath(font);
                if (System.IO.Path.GetFileName(font_assetPath).CompareTo(System.IO.Path.GetFileName(path2)) != 0)
                {
                    Debug.LogWarning($"파일명의 대소문자를 확인해주세요! {font_assetPath}");
                }
                else
                {
                    text.font = font;
                }
            }

            var mat_assetPath = String.Concat("Assets/TDS/ui/font/", matKey);
            var material = AssetDatabase.LoadAssetAtPath<Material>(mat_assetPath);
            if (font != null)
            {
                var path2 = AssetDatabase.GetAssetPath(material);
                if (System.IO.Path.GetFileName(mat_assetPath).CompareTo(System.IO.Path.GetFileName(path2)) != 0)
                {
                    Debug.LogWarning($"파일명의 대소문자를 확인해주세요! {mat_assetPath}");
                }
                else
                {
                    text.material = material;
                }
            }

        }
    }

    private static DataManager.StringScriptAll resultScript;
    public static string GetEditorString(int stringID)
    {
        string path = "Assets/TDS/Prefabs/scripts/string/stringEnglish.json";
        TextAsset title = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        if (title != null)
        {
            resultScript = JsonUtility.FromJson<DataManager.StringScriptAll>("{ \"result\" : " + title.text + "}");
        }

        if (resultScript == null)
        {
            Debug.LogWarning("스트링파일을 찾지못했습니다");
            return "";
        }

        foreach (var mit in resultScript.result)
        {
            if (mit.stringID == stringID)
            {
                return mit.stringData;
            }
        }

        return "";
    }
#endif

    public void SetRefresh()
    {
        if (text)
        {
            //text.text = _string;
            text.text = Managers.String.GetString(stringID, strFileType);
            SetString();
        }
    }

    public void SetTextTyping(int _id, float duration, Ease ease = Ease.Linear, float delay = 0)
    {
        stringID = _id;
        if (text) 
        {
            text.DOKill();
            text.text = string.Empty;
            text.DOText(Managers.String.GetString(stringID, strFileType), duration).SetEase(ease).SetDelay(delay);
        }
    }

    public void SetTextTyping(string str, float duration, Ease ease = Ease.Linear)
    {
        if (text)
        {
            text.DOKill();
            text.text = string.Empty;
            text.DOText(str, duration).SetEase(ease);
        }
    }

    public void SetStringID(int _id)
    {
        stringID = _id;
        if (text)
        {
            text.text = Managers.String.GetString(stringID, strFileType);
        }
    }
    
    public void SetStringID(int _id, Define.EFONT_TYPE _font)
    {
        fontType = _font;
        SetString();
        SetStringID(_id);
    }

    public void SetStringID(int _id, Color _color)
    {
        SetColor(_color);
        SetStringID(_id);
    }

    public void SetFontSize(float _size)
    {
        text.autoSizeTextContainer = false;
        text.fontSize = _size;
    }

    public void SetFontType(Define.EFONT_TYPE _font)
    {
        fontType = _font;
        SetString();
    }
    
    public void SetColor(Color _color)
    {
        isSetText = true;
        if (text)
        {
            text.color = _color;
        }
    }

    public void SetFadeAction(float _startAlpha, float _endAlpha, float _duration)
    {
        if (text)
        {
            text.alpha = _startAlpha;
            text.DOFade(_endAlpha, _duration);
        }
    }
    
    public void SetText<T1>(T1 arg1)
    {
        isSetText = true;
        if (text)
        {
            text.text = String.Format(Managers.String.GetString(stringID, strFileType), arg1);
        }
    }

    public void SetText<T1, T2>(T1 arg1, T2 arg2)
    {
        isSetText = true;
        if (text)
        {
            text.text = String.Format(Managers.String.GetString(stringID, strFileType), arg1, arg2);
        }
    }

    public void SetText<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3)
    {
        isSetText = true;
        if (text)
        {
            text.text = String.Format(Managers.String.GetString(stringID, strFileType), arg1, arg2, arg3);
        }
    }

    public void SetText<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        isSetText = true;
        if (text)
        {
            text.text = String.Format(Managers.String.GetString(stringID, strFileType), arg1, arg2, arg3, arg4);
        }
    }

    public void SetText<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        isSetText = true;
        if (text)
        {
            text.text = String.Format(Managers.String.GetString(stringID, strFileType), arg1, arg2, arg3, arg4, arg5);
        }
    }

    public void SetText<T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
    {
        isSetText = true;
        if (text)
        {
            text.text = String.Format(Managers.String.GetString(stringID, strFileType), arg1, arg2, arg3, arg4, arg5, arg6);
        }
    }

    public void SetText<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
    {
        isSetText = true;
        if (text)
        {
            text.text = String.Format(Managers.String.GetString(stringID, strFileType), arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }
    }

    public void SetText<T1, T2, T3, T4, T5, T6, T7, T8>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
    {
        isSetText = true;
        if (text)
        {
            text.text = String.Format(Managers.String.GetString(stringID, strFileType), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }
    }

    public void SetText<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
    {
        isSetText = true;
        if (text)
        {
            text.text = String.Format(Managers.String.GetString(stringID, strFileType), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }
    }

    public void SetText<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9,
        T10 arg10)
    {
        isSetText = true;
        if (text)
        {
            text.text = String.Format(Managers.String.GetString(stringID, strFileType), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }
    }

    public void SetText<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9,
        T10 arg10, T11 arg11)
    {
        isSetText = true;
        if (text)
        {
            text.text = String.Format(Managers.String.GetString(stringID, strFileType), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10,
                arg11);
        }
    }

    public void SetText<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8,
        T9 arg9, T10 arg10, T11 arg11, T12 arg12)
    {
        isSetText = true;
        if (text)
        {
            text.text = String.Format(Managers.String.GetString(stringID, strFileType), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10,
                arg11, arg12);
        }
    }

    public void SetText<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7,
        T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
    {
        isSetText = true;
        if (text)
        {
            text.text = String.Format(Managers.String.GetString(stringID, strFileType), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10,
                arg11, arg12, arg13);
        }
    }

    public void SetText<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7,
        T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
    {
        isSetText = true;
        if (text)
        {
            text.text = String.Format(Managers.String.GetString(stringID, strFileType), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10,
                arg11, arg12, arg13, arg14);
        }
    }

    public void SetText<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6,
        T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
    {
        isSetText = true;
        if (text)
        {
            text.text = String.Format(Managers.String.GetString(stringID, strFileType), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10,
                arg11, arg12, arg13, arg14, arg15);
        }
    }

    public void SetText<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6,
        T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
    {
        isSetText = true;
        if (text)
        {
            text.text = String.Format(Managers.String.GetString(stringID, strFileType), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10,
                arg11, arg12, arg13, arg14, arg15, arg16);
        }
    }
}