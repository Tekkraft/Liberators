using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePrepHandler
{
    public static List<UnitEntryData> data;

    public static int skillPointsEarned = 0;
    public static bool activated = false;

    public static void reset()
    {
        skillPointsEarned = 0;
        activated = false;
    }
}
