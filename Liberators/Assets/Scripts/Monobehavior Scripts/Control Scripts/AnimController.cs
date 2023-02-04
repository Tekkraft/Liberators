using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class AnimController : MonoBehaviour
{
    public GameObject damageText;
    public GameObject unitBase;
    List<GameObject> activeUnits = new List<GameObject>();
    Dictionary<GameObject, GameObject> activeUnitsUI = new Dictionary<GameObject, GameObject>();
    List<GameObject> deadUnits = new List<GameObject>();
    List<GameObject> damageTextList = new List<GameObject>();

    public void createBattleAnimation(List<CombatData> combatSequence)
    {
        AnimationSequence sequence = new AnimationSequence(combatSequence);
        StartCoroutine(playAnimation(sequence));
    }

    IEnumerator playAnimation(AnimationSequence sequence)
    {
        if (sequence.getSequence().Count <= 0)
        {
            terminateAnimation();
            yield break;
        }
        if (sequence.getSequence()[0].getAnimationSteps().Count <= 0)
        {
            terminateAnimation();
            yield break;
        }
        battleTeam activeTeam = sequence.getSequence()[0].getAnimationSteps()[0].getActor().GetComponent<UnitController>().getTeam();
        List<GameObject> leftUnits = new List<GameObject>();
        List<GameObject> rightUnits = new List<GameObject>();
        foreach (AnimationBlock block in sequence.getSequence())
        {
            if (block.getAnimationSteps().Count <= 0)
            {
                continue;
            }
            foreach (AnimationStep step in block.getAnimationSteps())
            {
                if (step.getActor().GetComponent<UnitController>().getTeam() == activeTeam)
                {
                    if (!leftUnits.Contains(step.getActor()))
                    {
                        leftUnits.Add(step.getActor());
                    }
                }
                else
                {
                    if (!rightUnits.Contains(step.getActor()))
                    {
                        rightUnits.Add(step.getActor());
                    }
                }
            }
        }
        for (int i = 0; i < leftUnits.Count; i++)
        {
            if (!activeUnits.Contains(leftUnits[i]))
            {
                GameObject temp = GameObject.Instantiate(unitBase, gameObject.transform);
                temp.GetComponent<Image>().sprite = leftUnits[i].GetComponent<UnitController>().getUnitInstance().getBattleSprite("idle");
                activeUnitsUI.Add(leftUnits[i], temp);
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
                temp.GetComponent<Image>().sprite = rightUnits[i].GetComponent<UnitController>().getUnitInstance().getBattleSprite("idle");
                temp.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 180, 0);
                activeUnitsUI.Add(rightUnits[i], temp);
                activeUnits.Add(rightUnits[i]);
                //Will have problems down the line, but can fix later
                temp.GetComponent<RectTransform>().anchoredPosition = new Vector2(105 * (i + 1), 100);
            }
        }
        foreach (AnimationBlock block in sequence.getSequence())
        {
            foreach (AnimationStep step in block.getAnimationSteps())
            {
                if (deadUnits.Contains(step.getActor()))
                {
                    continue;
                }
                activeUnitsUI[step.getActor()].GetComponent<Image>().sprite = step.getActor().GetComponent<UnitController>().getUnitInstance().getBattleSprite(step.getAction());
                createDamageText(step);
                if ((step.getAction() == "dead" || step.getAction() == "dead stagger") && !deadUnits.Contains(step.getActor()))
                {
                    deadUnits.Add(step.getActor());
                }
            }
            yield return new WaitForSeconds(1f);
            for (int i = damageTextList.Count - 1; i >= 0; i--)
            {
                GameObject.Destroy(damageTextList[i]);
            }
            //TEMPORARY CODE
            foreach (AnimationStep step in block.getAnimationSteps())
            {
                if (deadUnits.Contains(step.getActor()))
                {
                    continue;
                }
                activeUnitsUI[step.getActor()].GetComponent<Image>().sprite = step.getActor().GetComponent<UnitController>().getUnitInstance().getBattleSprite("idle");
            }
            yield return new WaitForSeconds(0.5f);
        }
        terminateAnimation();
    }

    public void terminateAnimation()
    {
        for (int i = damageTextList.Count - 1; i >= 0; i--)
        {
            GameObject.Destroy(damageTextList[i]);
        }
        StopCoroutine("playAnimation");
        GameObject.Destroy(gameObject);
    }

    void createDamageText(AnimationStep step)
    {
        Vector3 translation = new Vector3(0, 60, 0);
        switch (step.getAction())
        {
            case "evade":
                setupDamageText("MISS", translation, step);
                break;

            case "block":
                setupDamageText("BLOCK", translation, step);
                break;

            case "defend":
                setupDamageText("-" + step.getEffect(), translation, step);
                break;

            case "dead":
                setupDamageText("-" + step.getEffect(), translation, step);
                break;

            case "stagger":
                setupDamageText("-" + step.getEffect() + "!", translation, step);
                break;

            case "dead stagger":
                setupDamageText("-" + step.getEffect() + "!", translation, step);
                break;
        }
    }

    void setupDamageText(string message, Vector3 translation, AnimationStep step)
    {
        GameObject temp = GameObject.Instantiate(damageText, activeUnitsUI[step.getActor()].transform);
        temp.transform.Translate(translation);
        temp.GetComponent<TMPro.TextMeshProUGUI>().text = message;
        damageTextList.Add(temp);
        if (Mathf.Abs(activeUnitsUI[step.getActor()].GetComponent<RectTransform>().rotation.eulerAngles.y) == 180)
        {
            temp.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, Mathf.PI, 0);
        }
    }
}