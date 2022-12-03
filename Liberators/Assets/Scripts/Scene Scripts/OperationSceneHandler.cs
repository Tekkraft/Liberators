using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OperationSceneHandler
{
    public static List<SquadData> squadDataList = new List<SquadData>();
    public static battleOutcome battleOutcome = battleOutcome.UNSAVED;
    public static SquadData attackerData;
    public static int attackerId;
    public static SquadData defenderData;
    public static int defenderId;

    public static void reset()
    {
        attackerData = null;
        defenderData = null;
        squadDataList.Clear();
        battleOutcome = battleOutcome.UNSAVED;
    }
}