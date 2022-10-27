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
        newNode.transform.Translate(new Vector3(offset * 4.5f, 2.5f, 0));
        newNode.SetActive(true);
        List<float> offsets = getOffsets(node.widthAboveNode(), node.widthOfChildren());
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
