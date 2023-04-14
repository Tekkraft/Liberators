using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleTransition
{
    //Entry
    public static List<Vector2Int> playerSpawnLocations = new List<Vector2Int>();
    public static Dictionary<UnitData, Vector2Int> enemyPlacements = new Dictionary<UnitData, Vector2Int>();
    //Exit
    public static BattleOutcome outcome = BattleOutcome.UNSAVED;
    public static int turn_count = 0;

    public static void reset()
    {
        playerSpawnLocations = new List<Vector2Int>();
        enemyPlacements = new Dictionary<UnitData, Vector2Int>();
        outcome = BattleOutcome.UNSAVED;
        turn_count = 0;
    }
}
