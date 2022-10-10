using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTreeEntryHandler
{
    public static SkillTree activeTree;

    public static void reset()
    {
        activeTree = null;
    }
}
