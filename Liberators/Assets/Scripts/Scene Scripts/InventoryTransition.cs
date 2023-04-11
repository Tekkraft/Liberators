using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryTransition
{
    public static UnitData unitData;

    public static int characterIndex;

    public static BattleMenuPage menuPage;

    public static string origin;

    public static bool activated;

    public static void reset()
    {
        unitData = null;
        characterIndex = 0;
        origin = null;
        menuPage = BattleMenuPage.main;
        activated = false;
    }
}
