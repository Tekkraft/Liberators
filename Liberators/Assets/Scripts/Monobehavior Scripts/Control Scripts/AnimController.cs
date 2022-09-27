using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class AnimController : MonoBehaviour
{
    public GameObject unitBase;
    List<GameObject> activeUnits = new List<GameObject>();
    Dictionary<GameObject, GameObject> activeUnitsUI = new Dictionary<GameObject, GameObject>();

    public void createBattleAnimation(List<CombatData> combatSequence)
    {
        AnimationSequence sequence = new AnimationSequence(combatSequence);
        StartCoroutine(playAnimation(sequence));
    }

    IEnumerator playAnimation(AnimationSequence sequence)
    {
        foreach (AnimationBlock block in sequence.getSequence())
        {
            if (block.getAnimBreakpoint())
            {
                for (int i = activeUnits.Count - 1; i >= 0; i--)
                {
                    GameObject.Destroy(activeUnitsUI[activeUnits[i]]);
                }
                continue;
            }
            int activeTeam = block.getAnimationSteps()[0].getActor().GetComponent<UnitController>().getTeam();
            List<GameObject> leftUnits = new List<GameObject>();
            List<GameObject> rightUnits = new List<GameObject>();
            foreach (AnimationStep step in block.getAnimationSteps())
            {
                if (step.getActor().GetComponent<UnitController>().getTeam() == activeTeam)
                {
                    if (!leftUnits.Contains(step.getActor()))
                    {
                        leftUnits.Add(step.getActor());
                    }
                } else
                {
                    if (!rightUnits.Contains(step.getActor()))
                    {
                        rightUnits.Add(step.getActor());
                    }
                }
            }
            for (int i = 0; i < leftUnits.Count; i++)
            {
                if (!activeUnits.Contains(leftUnits[i]))
                {
                    GameObject temp = GameObject.Instantiate(unitBase, gameObject.transform);
                    temp.GetComponent<Image>().sprite = leftUnits[i].GetComponent<UnitController>().getUnit().getBattleSprite();
                    activeUnitsUI.Add(leftUnits[i],temp);
                    activeUnits.Add(leftUnits[i]);
                    //Will have problems down the line, but can fix later
                    temp.GetComponent<RectTransform>().anchoredPosition = new Vector2(-105 * (i + 1), 100);
                }
            }
            for (int i = 0; i < rightUnits.Count; i++)
            {
                if (!activeUnits.Contains(rightUnits[i]))
                {
                    GameObject temp = GameObject.Instantiate(unitBase, gameObject.transform);
                    temp.GetComponent<Image>().sprite = rightUnits[i].GetComponent<UnitController>().getUnit().getBattleSprite();
                    activeUnitsUI.Add(rightUnits[i], temp);
                    activeUnits.Add(rightUnits[i]);
                    //Will have problems down the line, but can fix later
                    temp.GetComponent<RectTransform>().anchoredPosition = new Vector2(105 * (i + 1), 100);
                }
            }
            foreach (AnimationStep step in block.getAnimationSteps())
            {
                Debug.Log(step.getActor() + " does " + step.getAction());
            }
            yield return new WaitForSeconds(2f);
        }
        terminateAnimation();
    }

    public void terminateAnimation()
    {
        StopCoroutine("playAnimation");
        GameObject.Destroy(gameObject);
    }
}
