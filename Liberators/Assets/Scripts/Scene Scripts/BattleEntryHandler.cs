using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEntryHandler
{
    public static List<UnitEntryData> deployedUnits = new List<UnitEntryData>();

    public static Dictionary<UnitEntryData, Vector2Int> enemyPlacements = new Dictionary<UnitEntryData, Vector2Int>();

    public static void reset()
    {
        deployedUnits = new List<UnitEntryData>();
        enemyPlacements = new Dictionary<UnitEntryData, Vector2Int>();
    }
}