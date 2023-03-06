using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleExitHandler
{
    public static BattleOutcome outcome = BattleOutcome.UNSAVED;
    public static int turn_count = 0;
    public static List<UnitEntryData> unitData = new List<UnitEntryData>();
    public static int playerUnitsRemaining;
    public static int enemyUnitsRemaining;

    public static void reset()
    {
        outcome = BattleOutcome.UNSAVED;
        turn_count = 0;
        unitData = new List<UnitEntryData>();
        playerUnitsRemaining = 0;
        enemyUnitsRemaining = 0;
    }
}
