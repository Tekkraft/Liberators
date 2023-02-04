using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class NodeController : MonoBehaviour
{
    public SkillNode linkedNode;
    public GameObject icon;
    public GameObject abilityCost;
    public GameObject abilityName;
    public GameObject abilityBorder;

    // Start is called before the first frame update
    void Start()
    {
        generate();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void generate()
    {
        icon.GetComponent<SpriteRenderer>().sprite = linkedNode.getAbility().getSprite();
        abilityName.GetComponent<TextMeshPro>().text = linkedNode.getAbility().getName();
        abilityCost.GetComponent<TextMeshPro>().text = linkedNode.getNodeCost() + " PTS";
    }

    public SkillNode getLinkedNode()
    {
        return linkedNode;
    }
}
