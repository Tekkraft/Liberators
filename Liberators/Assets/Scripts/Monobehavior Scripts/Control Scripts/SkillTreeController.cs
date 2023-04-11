 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SkillTreeController : MonoBehaviour
{
    UnitData data;
    List<GameObject> nodes = new List<GameObject>();

    public GameObject skillNodeObject;
    public GameObject skillPointTracker;

    Vector2 cursorPosition;

    void OnEnable()
    {
        data = SkillTreeTransition.unitData;
        data.skillTree.CheckAllUnlocked();
        data.skillTree.LoadAllLearnedAbilities();
    }

    void OnDisable()
    {
        SkillTreeTransition.unitData = data;
        SkillTreeTransition.activated = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        drawTree();
    }

    // Update is called once per frame
    void Update()
    {
        skillPointTracker.GetComponent<SkillTreeUISync>().setCurrentSkillPoints(data.availableSkillPoints);
        skillPointTracker.GetComponent<SkillTreeUISync>().setMaxSkillPoints(data.maxSkillPoints);
        foreach (GameObject node in nodes)
        {
            node.GetComponent<NodeController>().UpdateNodeBorder(data.availableSkillPoints);
        }
    }

    void drawTree()
    {
        List<List<SkillNodeData>> treeNodesDepth = data.skillTree.NodesByDepth();
        for (int i = 0; i < treeNodesDepth.Count; i++)
        {
            List<SkillNodeData> nodesDepth = treeNodesDepth[i];
            for (int j = 0; j < nodesDepth.Count; j++)
            {
                float xOffset;
                if (nodesDepth.Count % 2 == 1)
                {
                    xOffset = (j - (nodesDepth.Count - 1) / 2) * 5.5f;
                }
                else
                {
                    xOffset = (j - nodesDepth.Count / 2 + .5f) * 5.5f;
                }
                Vector3 offset = new Vector3(xOffset, i * 4.5f, 0f);
                GameObject newNode = GameObject.Instantiate(skillNodeObject);
                newNode.SetActive(false);
                newNode.GetComponent<NodeController>().node = nodesDepth[j];
                newNode.transform.Translate(offset);
                newNode.SetActive(true);
                nodes.Add(newNode);
            }
        }
    }

    public void learnSkill(SkillNodeData node)
    {
        if (data.availableSkillPoints < node.cost)
        {
            return;
        }
        if (!node.unlocked)
        {
            return;
        }
        node.learned = true;
        data.availableSkillPoints -= node.cost;
        data.skillTree.CheckAllUnlocked();
        data.skillTree.LoadAllLearnedAbilities();
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
            learnSkill(node.GetComponent<NodeController>().GetLinkedNode());
        }
    }

    //Scene Transitions
    public void toBattlePreps()
    {
        SceneManager.LoadSceneAsync("BattlePrep");
    }
}
