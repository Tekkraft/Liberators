using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SkillTreeUISync : MonoBehaviour
{
    public GameObject currentSkillPointTracker;
    public GameObject maxSkillPointTracker;

    public void setCurrentSkillPoints(int value)
    {
        currentSkillPointTracker.GetComponent<TextMeshProUGUI>().text = value.ToString();
    }

    public void setMaxSkillPoints(int value)
    {
        maxSkillPointTracker.GetComponent<TextMeshProUGUI>().text = value.ToString();
    }
}
