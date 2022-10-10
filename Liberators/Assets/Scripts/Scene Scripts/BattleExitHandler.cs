using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleExitHandler
{
    public static bool victory = false;
    public static int turn_count = 0;

    public static void reset()
    {
        victory = false;
        turn_count = 0;
    }
}
