using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSequence
{
    List<AnimationBlock> animationBlocks = new List<AnimationBlock>();

    public AnimationSequence(List<CombatData> dataSequence)
    {
        foreach (CombatData data in dataSequence)
        {
            animationBlocks.Add(new AnimationBlock(data));
        }
    }

    public List<AnimationBlock> getSequence()
    {
        return animationBlocks;
    }

}

public class AnimationBlock
{
    CombatData combatData = new CombatData();
    List<AnimationStep> animationSteps = new List<AnimationStep>();
    bool animBreakpoint = false;

    public AnimationBlock(CombatData dataEntry)
    {
        if (dataEntry.getCombatDataType() == combatDataType.BREAK)
        {
            animBreakpoint = true;
            return;
        }
        else
        {
            animBreakpoint = false;
            combatData = dataEntry;
            animationSteps.Add(new AnimationStep(dataEntry.getAttacker(), "attack"));
            if (dataEntry.getCombatDataType() == combatDataType.DAMAGE)
            {
                if (!dataEntry.getAttackHit())
                {
                    animationSteps.Add(new AnimationStep(dataEntry.getDefender(), "evade"));
                }
                else if (dataEntry.getDefenderKilled() && dataEntry.getAttackCrit())
                {
                    animationSteps.Add(new AnimationStep(dataEntry.getDefender(), "dead stagger"));
                }
                else if (dataEntry.getDefenderKilled())
                {
                    animationSteps.Add(new AnimationStep(dataEntry.getDefender(), "dead"));
                }
                else if (dataEntry.getDamageDealt() <= 0)
                {
                    animationSteps.Add(new AnimationStep(dataEntry.getDefender(), "block"));
                }
                else if (dataEntry.getAttackCrit())
                {
                    animationSteps.Add(new AnimationStep(dataEntry.getDefender(), "stagger"));
                }
                else
                {
                    animationSteps.Add(new AnimationStep(dataEntry.getDefender(), "defend"));
                }
            }
            else if (dataEntry.getCombatDataType() == combatDataType.STATUS)
            {
                if (dataEntry.getStatusInflicted())
                {
                    animationSteps.Add(new AnimationStep(dataEntry.getDefender(), "statused"));
                }
            }
            return;
        }
    }

    public CombatData getCombatData()
    {
        return combatData;
    }

    public List<AnimationStep> getAnimationSteps()
    {
        return animationSteps;
    }

    public bool getAnimBreakpoint()
    {
        return animBreakpoint;
    }
}

public class AnimationStep
{
    GameObject actor;
    string action;

    public AnimationStep(GameObject actor, string action)
    {
        this.actor = actor;
        this.action = action;
    }

    public GameObject getActor()
    {
        return actor;
    }

    public string getAction()
    {
        return action;
    }
}