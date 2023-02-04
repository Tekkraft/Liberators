using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTreeEntryHandler
{
    public static SkillTreeInstance activeTree;
    public static int unitId;

    public static void reset()
    {
        activeTree = null;
    }
}
