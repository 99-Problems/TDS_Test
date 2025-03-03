using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;
using Data;

[Serializable]
public class StringScript
{
    public int stringID;
    public Defines.SystemLanguage languegeType;
    public string stringData;
}

public partial class DataManager
{
    private List<StringScript> listStringScript = new List<StringScript>();

    private List<StringScript> GetStringScript
    {
        get { return listStringScript; }
    }

    [Serializable]
    public class StringScriptAll
    {
        public StringScript[] result;
    }

    void ClearString()
    {
        listStringScript.Clear();
    }
}