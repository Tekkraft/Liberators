using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEntryHandler
{
    public static List<UnitEntryData> deployedUnits = new List<UnitEntryData>();

    public static void reset()
    {
        deployedUnits = new List<UnitEntryData>();
    }
}