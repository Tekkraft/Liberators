using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadController : MonoBehaviour
{
    public int baseSpeed;
    public List<UnitEntryData> unitList;
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

    public SquadData getSquadData()
    {
        return new SquadData(baseSpeed, unitList, team, transform.position, gameObject.name);
    }

    public void loadSquadData(SquadData data)
    {
        baseSpeed = data.baseSpeed;
        unitList = data.unitList;
        team = data.team;
        transform.position = new Vector3(data.position.x, data.position.y, -1);
        gameObject.name = data.name;
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

    public List<UnitEntryData> getUnitList()
    {
        return unitList;
    }

    public int getTeam()
    {
        return team;
    }
}

public class SquadData
{
    public int baseSpeed;
    public List<UnitEntryData> unitList;
    public int team;
    public Vector2 position;
    public string name;

    public SquadData(int baseSpeed, List<UnitEntryData> unitList, int team, Vector2 position, string name)
    {
        this.baseSpeed = baseSpeed;
        this.unitList = unitList;
        this.team = team;
        this.position = position;
        this.name = name;
    }
}