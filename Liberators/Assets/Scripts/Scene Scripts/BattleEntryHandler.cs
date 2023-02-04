using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEntryHandler
{
    public static Dictionary<UnitEntryData, Vector2Int> deployedUnits = new Dictionary<UnitEntryData, Vector2Int>();

    public static Dictionary<UnitEntryData, Vector2Int> enemyPlacements = new Dictionary<UnitEntryData, Vector2Int>();

    public static void reset()
    {
        deployedUnits = new Dictionary<UnitEntryData, Vector2Int>();
        enemyPlacements = new Dictionary<UnitEntryData, Vector2Int>();
    }
}