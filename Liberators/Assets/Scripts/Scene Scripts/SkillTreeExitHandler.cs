using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTreeExitHandler
{
    public static SkillTreeInstance activeTree;
    public static int unitId = -1;
    public static bool activated = false;

    public static void reset()
    {
        activeTree = null;
        unitId = -1;
        activated = false;
    }
}
