using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class NodeController : MonoBehaviour
{
    public SkillNodeData node;
    public GameObject icon;
    public GameObject abilityCost;
    public GameObject abilityName;
    public GameObject abilityBorder;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Initialize()
    {
        icon.GetComponent<SpriteRenderer>().sprite = node.unlockedAbilties[0].getSprite();
        abilityName.GetComponent<TextMeshPro>().text = node.unlockedAbilties[0].getName();
        abilityCost.GetComponent<TextMeshPro>().text = node.cost + " PTS";
    }

    public SkillNodeData GetLinkedNode()
    {
        return node;
    }

    public void UpdateNodeBorder(int skillPoints)
    {
        if (node.learned)
        {
            abilityBorder.GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 0.7f, 1);
        }
        else if (node.unlocked)
        {
            if (skillPoints >= node.cost)
            {
                abilityBorder.GetComponent<SpriteRenderer>().color = new Color(0, 0.7f, 0, 1);
            }
            else
            {
                abilityBorder.GetComponent<SpriteRenderer>().color = new Color(0.7f, 0, 0, 1);
            }
        } else
        {
            abilityBorder.GetComponent<SpriteRenderer>().color = new Color(0.3f, 0.3f, 0.3f, 1);
        }
    }
}
