using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTreeExitHandler
{
    public static SkillTreeInstance activeTree;
    public static int unitId = -1;

    public static void reset()
    {
        activeTree = null;
    }
}
