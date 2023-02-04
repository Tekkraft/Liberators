 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SkillTreeController : MonoBehaviour
{
    public SkillTreeInstance activeTree;
    public GameObject skillNodeObject;

    public GameObject skillPointTracker;

    Vector2 cursorPosition;

    void OnEnable()
    {
        activeTree = SkillTreeEntryHandler.activeTree;
        SkillTreeExitHandler.reset();
    }

    void OnDisable()
    {
        SkillTreeExitHandler.unitId = SkillTreeEntryHandler.unitId;
        SkillTreeExitHandler.activeTree = activeTree;
        SkillTreeExitHandler.activated = true;
        SkillTreeEntryHandler.reset();
    }

    // Start is called before the first frame update
    void Start()
    {
        drawTree();
    }

    // Update is called once per frame
    void Update()
    {
        skillPointTracker.GetComponent<SkillTreeUISync>().setCurrentSkillPoints(activeTree.getAvailableSkillPoints());
        skillPointTracker.GetComponent<SkillTreeUISync>().setMaxSkillPoints(activeTree.getMaxSkillPoints());
        //Find root gameobject
        List<GameObject> children = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.CompareTag("SkillNode"))
            {
                children.Add(child.gameObject);
            }
        }
        foreach (GameObject child in children)
        {
            updateNodeBorder(child, activeTree.getUnlockedSkills(), activeTree.getObtainedSkills());
        }
    }

    void drawTree()
    {
        SkillNode rootSkill = activeTree.getRootNode();
        drawNode(rootSkill, gameObject, 0, 0);
    }

    void drawNode(SkillNode node, GameObject parent, int layer, float offset)
    {
        GameObject newNode = GameObject.Instantiate(skillNodeObject, parent.transform);
        newNode.SetActive(false);
        newNode.GetComponent<NodeController>().linkedNode = node;
        newNode.transform.Translate(new Vector3(offset * 4.5f, 3.5f, 0));
        newNode.SetActive(true);
        List<float> offsets = getOffsets(node.widthAboveNode(), node.widthOfChildren());
        for (int i = 0; i < node.getChildSkills().Count; i++)
        {
            SkillNode subNode = node.getChildSkills()[i];
            drawNode(subNode, newNode, layer + 1, offsets[i]);
        }
    }

    void updateNodeBorder(GameObject root, List<SkillNode> unlocked, List<SkillNode> obtained)
    {
        if (unlocked.Contains(root.GetComponent<NodeController>().getLinkedNode()))
        {
            if (activeTree.getAvailableSkillPoints() >= root.GetComponent<NodeController>().getLinkedNode().getNodeCost())
            {
                root.GetComponent<NodeController>().abilityBorder.GetComponent<SpriteRenderer>().color = new Color(0, 0.7f, 0, 1);
            }
            else
            {
                root.GetComponent<NodeController>().abilityBorder.GetComponent<SpriteRenderer>().color = new Color(0.7f, 0, 0, 1);
            }
        } else if (obtained.Contains(root.GetComponent<NodeController>().getLinkedNode()))
        {
            root.GetComponent<NodeController>().abilityBorder.GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 0.7f, 1);
        }
        else
        {
            root.GetComponent<NodeController>().abilityBorder.GetComponent<SpriteRenderer>().color = new Color(0.3f, 0.3f, 0.3f, 1);
        }

        List<GameObject> children = new List<GameObject>();
        for (int i = 0; i < root.transform.childCount; i++)
        {
            Transform child = root.transform.GetChild(i);
            if (child.gameObject.CompareTag("SkillNode"))
            {
                children.Add(child.gameObject);
            }
        }
        foreach (GameObject child in children)
        {
            updateNodeBorder(child, unlocked, obtained);
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

    public void learnSkill(SkillNode node)
    {
        activeTree.learnAbility(node);
    }

    //Input Handling
    void OnCursorMovement(InputValue value)
    {
        cursorPosition = value.Get<Vector2>();
    }

    void OnCursorPrimary()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(cursorPosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            GameObject node = hit.collider.gameObject;
            learnSkill(node.GetComponent<NodeController>().getLinkedNode());
        }
    }

    //Scene Transitions
    public void toBattlePreps()
    {
        SceneManager.LoadSceneAsync("BattlePrep");
    }
}
