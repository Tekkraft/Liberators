using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    Grid mainGrid;
    Dictionary<Vector2, GameObject> unitList = new Dictionary<Vector2, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject getUnitFromCoords(Vector2 coords)
    {
        Debug.Log(coords);
        if (unitList.ContainsKey(coords))
        {
            return unitList[coords];
        }
        return null;
    }

    public Vector2 worldToTilePos(Vector2 worldPos)
    {
        Vector3 localCellCoords = mainGrid.LocalToCell(worldPos);
        Vector3Int localCellInt = new Vector3Int(Mathf.CeilToInt(localCellCoords.x), Mathf.CeilToInt(localCellCoords.y), Mathf.CeilToInt(localCellCoords.z));
        Vector3 localCoords = mainGrid.CellToLocal(localCellInt);
        return new Vector3(localCoords.x, localCoords.y + mainGrid.cellSize.y / 2, localCoords.z);
    }

    public void addUnit(GameObject unit)
    {
        unitList.Add(unit.GetComponent<UnitController>().getUnitPos(), unit);
        Debug.Log(unit.GetComponent<UnitController>().getUnitPos());
    }
}
