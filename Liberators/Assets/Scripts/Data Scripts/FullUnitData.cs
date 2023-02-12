using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FullUnitData
{
    [SerializeField]
    UnitEntryData battleData;

    public UnitEntryData GetUnitEntryData()
    {
        return battleData;
    }

    public void SetUnitEntryData(UnitEntryData battleData)
    {
        this.battleData = battleData;
    }
}
