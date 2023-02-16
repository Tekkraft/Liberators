using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRoster
{
    static FullUnitData lanaData;
    static FullUnitData ethanData;
    static FullUnitData saeiData;
    static FullUnitData vaueData;
    static FullUnitData mayData;
    static FullUnitData runliData;
    static FullUnitData colinData;
    static FullUnitData hanaeiData;

    public static void LoadData(List<FullUnitData> dataList)
    {
        lanaData = dataList[0];
        ethanData = dataList[1];
        saeiData = dataList[2];
        vaueData = dataList[3];
        mayData = dataList[4];
        runliData = dataList[5];
        colinData = dataList[6];
        hanaeiData = dataList[7];
    }

    public static List<UnitEntryData> GetFullUnitEntryData()
    {
        List<UnitEntryData> dataList = new List<UnitEntryData>
        {
            lanaData.GetUnitEntryData(),
            ethanData.GetUnitEntryData(),
            saeiData.GetUnitEntryData(),
            vaueData.GetUnitEntryData(),
            mayData.GetUnitEntryData(),
            runliData.GetUnitEntryData(),
            colinData.GetUnitEntryData(),
            hanaeiData.GetUnitEntryData()
        };
        return dataList;
    }

    public static void SetFullUnitEntryData(List<UnitEntryData> dataList)
    {
        lanaData.SetUnitEntryData(dataList[0]);
        ethanData.SetUnitEntryData(dataList[1]);
        saeiData.SetUnitEntryData(dataList[2]);
        vaueData.SetUnitEntryData(dataList[3]);
        mayData.SetUnitEntryData(dataList[4]);
        runliData.SetUnitEntryData(dataList[5]);
        colinData.SetUnitEntryData(dataList[6]);
        hanaeiData.SetUnitEntryData(dataList[7]);
    }
}