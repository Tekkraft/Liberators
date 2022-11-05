using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadController : MonoBehaviour
{
    public int baseSpeed;
    List<UnitEntryData> unitList;
    bool movementActive = false;
    public int team;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void moveSquad(List<Vector2Int> directions)
    {
        if (!movementActive)
        {
            StartCoroutine(stepSquadMove(directions));
        }
    }

    IEnumerator stepSquadMove(List<Vector2Int> directions)
    {
        movementActive = true;
        foreach (Vector2Int move in directions)
        {
            Vector3 moveData = new Vector3(move.x, move.y, 0);
            gameObject.transform.Translate(moveData);
            yield return new WaitForSeconds(.5f);
        }
        movementActive = false;
    }
}