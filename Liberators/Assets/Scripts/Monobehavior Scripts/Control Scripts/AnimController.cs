using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;

public class AnimController : MonoBehaviour
{
    public GameObject damageText;
    public GameObject unitBase;
    List<GameObject> activeUnits = new List<GameObject>();
    Dictionary<GameObject, GameObject> activeUnitsUI = new Dictionary<GameObject, GameObject>();
    List<GameObject> deadUnits = new List<GameObject>();
    List<GameObject> damageTextList = new List<GameObject>();
    Coroutine currentAnimation;

    //TODO: Reimplement Animations
    public void createBattleAnimation(List<BattleStep> steps)
    {
        currentAnimation = StartCoroutine(PlayAnimation(steps));
    }

    IEnumerator PlayAnimation(List<BattleStep> steps)
    {
        if (steps.Count <= 0)
        {
            TerminateAnimation();
            yield break;
        }
        List<GameObject> relevantUnits = new List<GameObject>();
        foreach (BattleStep step in steps)
        {
            relevantUnits.AddRange(step.GetBattleDetails().Keys);
        }
        relevantUnits.Distinct().ToList();
        List<GameObject> leftUnits = new List<GameObject>();
        List<GameObject> rightUnits = new List<GameObject>();
        foreach (GameObject unit in relevantUnits)
        {
            if (unit.GetComponent<UnitController>().GetTeam() == BattleTeam.ENEMY)
            {
                rightUnits.Add(unit);
            }
            else
            {
                leftUnits.Add(unit);
            }
        }
        for (int i = 0; i < leftUnits.Count; i++)
        {
            if (!activeUnits.Contains(leftUnits[i]))
            {
                GameObject temp = GameObject.Instantiate(unitBase, gameObject.transform);
                temp.GetComponent<Image>().sprite = leftUnits[i].GetComponent<UnitController>().GetUnitData().GetBattleSprite("idle");
                activeUnitsUI.Add(leftUnits[i], temp);
                activeUnits.Add(leftUnits[i]);
                //TODO: Will have problems down the line
                temp.GetComponent<RectTransform>().anchoredPosition = new Vector2(-105 * (i + 1), 100);
            }
        }
        for (int i = 0; i < rightUnits.Count; i++)
        {
            if (!activeUnits.Contains(rightUnits[i]))
            {
                GameObject temp = GameObject.Instantiate(unitBase, gameObject.transform);
                temp.GetComponent<Image>().sprite = rightUnits[i].GetComponent<UnitController>().GetUnitData().GetBattleSprite("idle");
                temp.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 180, 0);
                activeUnitsUI.Add(rightUnits[i], temp);
                activeUnits.Add(rightUnits[i]);
                //TODO: Will have problems down the line
                temp.GetComponent<RectTransform>().anchoredPosition = new Vector2(105 * (i + 1), 100);
            }
        }
        foreach (BattleStep step in steps)
        {
            Dictionary<GameObject, List<BattleDetail>> details = step.GetBattleDetails();
            foreach (GameObject unit in details.Keys)
            {
                int depth = 1;
                foreach (BattleDetail detail in details[unit])
                {
                    //TODO: Setup animation priority
                    switch (detail.GetEffect())
                    {
                        case "attack":
                            activeUnitsUI[unit].GetComponent<Image>().sprite = unit.GetComponent<UnitController>().GetUnitData().GetBattleSprite("attack");
                            break;

                        case "miss":
                            activeUnitsUI[unit].GetComponent<Image>().sprite = unit.GetComponent<UnitController>().GetUnitData().GetBattleSprite("evade");
                            CreateDamageText("evade", 0, depth, unit);
                            depth++;
                            break;

                        case "damage":
                            if (detail.GetCritical())
                            {
                                activeUnitsUI[unit].GetComponent<Image>().sprite = unit.GetComponent<UnitController>().GetUnitData().GetBattleSprite("defend");
                                CreateDamageText("critical", detail.GetValue(), depth, unit);
                                depth++;
                            }
                            else
                            {
                                activeUnitsUI[unit].GetComponent<Image>().sprite = unit.GetComponent<UnitController>().GetUnitData().GetBattleSprite("stagger");
                                CreateDamageText("damage", detail.GetValue(), depth, unit);
                                depth++;
                            }
                            if (detail.GetDead() && !deadUnits.Contains(unit))
                            {
                                activeUnitsUI[unit].GetComponent<Image>().sprite = unit.GetComponent<UnitController>().GetUnitData().GetBattleSprite("dead");
                                deadUnits.Add(unit);
                            }
                            break;

                        case "status":
                            activeUnitsUI[unit].GetComponent<Image>().sprite = unit.GetComponent<UnitController>().GetUnitData().GetBattleSprite("defend");
                            break;

                        case "heal":
                            activeUnitsUI[unit].GetComponent<Image>().sprite = unit.GetComponent<UnitController>().GetUnitData().GetBattleSprite("idle");
                            CreateDamageText("heal", detail.GetValue(), depth, unit);
                            depth++;
                            break;

                        default:
                            Debug.LogError("Unrecognized effect: " + detail.GetEffect());
                            break;
                    }
                }
            }
            yield return new WaitForSeconds(1f);
            for (int i = damageTextList.Count - 1; i >= 0; i--)
            {
                GameObject.Destroy(damageTextList[i]);
            }
            foreach (GameObject tempUnit in relevantUnits)
            {
                if (deadUnits.Contains(tempUnit))
                {
                    activeUnitsUI[tempUnit].GetComponent<Image>().sprite = tempUnit.GetComponent<UnitController>().GetUnitData().GetBattleSprite("dead");
                    continue;
                }
                activeUnitsUI[tempUnit].GetComponent<Image>().sprite = tempUnit.GetComponent<UnitController>().GetUnitData().GetBattleSprite("idle");
            }
            yield return new WaitForSeconds(0.5f);
        }
        TerminateAnimation();
    }

    public void TerminateAnimation()
    {
        for (int i = damageTextList.Count - 1; i >= 0; i--)
        {
            GameObject.Destroy(damageTextList[i]);
        }
        StopCoroutine(currentAnimation);
        GameObject.Destroy(gameObject);
    }

    void CreateDamageText(string action, int value, int height, GameObject unit)
    {
        Vector3 translation = new Vector3(0, 60 * height, 0);
        switch (action)
        {
            case "evade":
                SetupDamageText("MISS", translation, unit);
                break;

            case "damage":
                SetupDamageText("-" + value, translation, unit);
                break;

            case "critical":
                SetupDamageText("-" + value + "!", translation, unit);
                break;

            case "heal":
                SetupDamageText("+" + value, translation, unit);
                break;

            default:
                Debug.LogError("Unrecognized action: " + action);
                break;
        }
    }

    void SetupDamageText(string message, Vector3 translation, GameObject target)
    {
        GameObject temp = GameObject.Instantiate(damageText, activeUnitsUI[target].transform);
        float ratio = temp.GetComponent<RectTransform>().sizeDelta.y / 25;
        temp.GetComponent<RectTransform>().localPosition = translation * ratio;
        temp.GetComponent<TMPro.TextMeshProUGUI>().text = message;
        damageTextList.Add(temp);
        if (Mathf.Abs(activeUnitsUI[target].GetComponent<RectTransform>().rotation.eulerAngles.y) == 180)
        {
            temp.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, Mathf.PI, 0);
        }
    }
}