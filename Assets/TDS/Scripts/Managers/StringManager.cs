using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AssetBundles;
using Cysharp.Threading.Tasks;
using TMPro;
using Data;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;


public class StringManager : MonoBehaviour
{
    [Serializable]
    public class StringInfo
    {
        public int id;
        public string str;
    }


    [Serializable]
    public class StringErrorInfo
    {
        public int ErrorID;
        public int StrID;
    }

    [Serializable]
    public class StringInfoTable
    {
        public List<StringInfo> strlist;
        public List<StringErrorInfo> strError;
    }

    const string strFontPath = "default/font";
    static Dictionary<int, string> strTableEng = new Dictionary<int, string>();

    static Dictionary<Define.EFONT_TYPE, TMP_FontAsset> dicLoadedFonts = new Dictionary<Define.EFONT_TYPE, TMP_FontAsset>();
    static Dictionary<Define.EFONT_TYPE, Material> dicLoadedMaterials = new Dictionary<Define.EFONT_TYPE, Material>();

    public List<string> strException = new List<string>();

    public async UniTask LoadStringInfo()
    {
        await LoadStringInfoScript();
    }

    static async UniTask LoadStringInfoScript()
    {
        strTableEng.Clear();
        StringInfoTable stringInfo = null;

        var english = await Managers.Resource.LoadJsonByLabel("stringEnglish");
        {
            var resultScript = JsonUtility.FromJson<DataManager.StringScriptAll>("{ \"result\" : " + english + "}");
            for (int i = 0; i < resultScript.result.Length; i++)
            {
                var stringScript = resultScript.result[i];
                strTableEng[stringScript.stringID] = stringScript.stringData;
#if LOG_ENABLE && (TEST_DOWNLOAD || UNITY_EDITOR)
                if(stringScript.stringData.IsNullOrEmpty())
                {
                    Debug.Log($"stringTableEng added empty string(ID: {stringScript.stringID}) : string is Empty.");
                }
                else
                {
                    Debug.Log($"stringTableEng added string(ID: {stringScript.stringID}) : \"{stringScript.stringData}\"");
                }
                
#endif
            }
        }
    }
    public string GetString(int strID, Define.StringFileType _type)
    {
        return GetString(strID);
    }

    public string GetString(int strID)
    {

        string temp;
        if (strTableEng.TryGetValue(strID, out temp))
            return temp;
              
#if UNITY_EDITOR
        if (temp.IsNullOrEmpty() && strID > 0)
            return $"emptyStringID : {strID}";
#endif

        return "";
    }

    public static async UniTask<TMP_FontAsset> GetFont(Define.EFONT_TYPE _fontType)
    {
        var font = await GetFontEnglish(_fontType);
        return font;
    }

    public static async UniTask<TMP_FontAsset> GetFontEnglish(Define.EFONT_TYPE _fontType)
    {
        TMP_FontAsset font = null;
#if !UNITY_EDITOR
            if (dicLoadedFonts.TryGetValue(_fontType, out font))
            {
                if (font != null)
                {
                    return font;
                }
                else
                {
                    dicLoadedFonts.Remove(_fontType);
                }
            }
#endif

        switch (_fontType)
        {
            case Define.EFONT_TYPE.ENONE:
            case Define.EFONT_TYPE.Default:
                font = await Managers.Resource.LoadAsyncFont("ui/font", "Changa-Medium SDF");

                break;
            default:
                Debug.LogError(_fontType);
                throw new ArgumentOutOfRangeException(nameof(_fontType), _fontType, null);
        }

        if (font == null)
            font = await Managers.Resource.LoadAsyncFont("ui/font", "Changa-Medium SDF");
#if !UNITY_EDITOR
        if(!dicLoadedFonts.ContainsKey(_fontType))
            dicLoadedFonts.Add(_fontType, font);
#endif
        return font;
    }

    public static async UniTask<Material>GetMaterial(Define.EFONT_TYPE _fontType)
    {
        return await GetMaterialEnglish(_fontType);
    }

    public static async UniTask<Material> GetMaterialEnglish(Define.EFONT_TYPE _fontType)
    {
        Material material = null;

#if !UNITY_EDITOR
            if (dicLoadedMaterials.TryGetValue(_fontType, out material))
            {
                if (material != null)
                {
                    return material;
                }
                else
                {
                    dicLoadedMaterials.Remove(_fontType);
                }
            }
#endif

        switch (_fontType)
        {
            case Define.EFONT_TYPE.ENONE:
            case Define.EFONT_TYPE.Default:
                material = await Managers.Resource.LoadAsyncMaterial("ui/material", "Changa-Medium_Default");

                break;
            default:
                Debug.LogError(_fontType);
                throw new ArgumentOutOfRangeException(nameof(_fontType), _fontType, null);
        }

        if (material == null)
            material = await Managers.Resource.LoadAsyncMaterial("ui/material", "Changa-Medium_Default");
#if !UNITY_EDITOR
        if(!dicLoadedMaterials.ContainsKey(_fontType))
            dicLoadedMaterials.Add(_fontType, material);
#endif
        return material;
    }

    public void Init()
    {
        dicLoadedFonts.Clear();
        dicLoadedMaterials.Clear();
    }
}
