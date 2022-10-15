using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTreeController : MonoBehaviour
{
    public SkillTree activeTree;
    public GameObject skillNodeObject;

    void OnEnable()
    {
            
    }

    void OnDisable()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        drawTree();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void drawTree()
    {
        SkillNode rootSkill = activeTree.getRootSkill();
        drawNode(rootSkill, gameObject, 0, 0);
    }

    void drawNode(SkillNode node, GameObject parent, int layer, float offset)
    {
        GameObject newNode = GameObject.Instantiate(skillNodeObject, parent.transform);
        newNode.SetActive(false);
        newNode.GetComponent<NodeController>().linkedNode = node;
        newNode.transform.Translate(new Vector3(offset * 4.5f, 2.5f, 0));
        newNode.SetActive(true);
        List<float> offsets = getOffsets(activeTree.getRootSkill().getMaxWidth(node), activeTree.getRootSkill().getChildWidths(node));
        for (int i = 0; i < node.getChildSkills().Count; i++)
        {
            SkillNode subNode = node.getChildSkills()[i];
            drawNode(subNode, newNode, layer + 1, offsets[i]);
        }
    }

    List<float> getOffsets(int maxWidth, List<int> descentWidths)
    {
        float midpoint = maxWidth / 2f;
        float index = -midpoint;
        List<float> offsets = new List<float>();
        foreach (int width in descentWidths)
        {
            index += width / 2f;
            offsets.Add(index);
            index += width / 2f;
        }
        return offsets;
    }
}
