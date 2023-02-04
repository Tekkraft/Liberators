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
    List<AnimationStep> animationSteps = new List<AnimationStep>();

    public AnimationBlock(CombatData dataEntry)
    {
        if (dataEntry.getCombatDataType() == combatDataType.DAMAGE)
        {
            animationSteps.Add(new AnimationStep(dataEntry.getAttacker(), "attack", 0));
            if (!dataEntry.getAttackHit())
            {
                animationSteps.Add(new AnimationStep(dataEntry.getDefender(), "evade", dataEntry.getDamageDealt()));
            }
            else if (dataEntry.getDefenderKilled() && dataEntry.getAttackCrit())
            {
                animationSteps.Add(new AnimationStep(dataEntry.getDefender(), "dead stagger", dataEntry.getDamageDealt()));
            }
            else if (dataEntry.getDefenderKilled())
            {
                animationSteps.Add(new AnimationStep(dataEntry.getDefender(), "dead", dataEntry.getDamageDealt()));
            }
            else if (dataEntry.getDamageDealt() <= 0)
            {
                animationSteps.Add(new AnimationStep(dataEntry.getDefender(), "block", dataEntry.getDamageDealt()));
            }
            else if (dataEntry.getAttackCrit())
            {
                animationSteps.Add(new AnimationStep(dataEntry.getDefender(), "stagger", dataEntry.getDamageDealt()));
            }
            else
            {
                animationSteps.Add(new AnimationStep(dataEntry.getDefender(), "defend", dataEntry.getDamageDealt()));
            }
        }
        else if (dataEntry.getCombatDataType() == combatDataType.STATUS)
        {
            if (dataEntry.getStatusInflicted())
            {
                animationSteps.Add(new AnimationStep(dataEntry.getDefender(), "statused", 0));
            }
        }
    }

    public List<AnimationStep> getAnimationSteps()
    {
        return animationSteps;
    }
}

public class AnimationStep
{
    GameObject actor;
    string action;
    int effect;

    public AnimationStep(GameObject actor, string action, int effect)
    {
        this.actor = actor;
        this.action = action;
        this.effect = effect;
    }

    public GameObject getActor()
    {
        return actor;
    }

    public string getAction()
    {
        return action;
    }

    public int getEffect()
    {
        return effect;
    }
}