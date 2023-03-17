using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSequence
{
    List<AnimationBlock> animationBlocks = new List<AnimationBlock>();

    public AnimationSequence()
    {
        
    }

    public List<AnimationBlock> getSequence()
    {
        return animationBlocks;
    }

}

public class AnimationBlock
{
    List<AnimationStep> animationSteps = new List<AnimationStep>();

    public AnimationBlock()
    {

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