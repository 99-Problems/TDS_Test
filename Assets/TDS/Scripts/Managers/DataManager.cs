using Cysharp.Threading.Tasks;
using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class DataManager
{
    public void Init()
    {

    }

    public async UniTask LoadScript()
    {
        await LoadAllParser();
    }

    public UnitInfoScript GetUnitInfo(int _unitID)
    {
        return GetUnitInfoScript(_ => _.unitID == _unitID);
    }
}
