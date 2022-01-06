using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    //Grid Coordinates are +x to top-right, +y to top-left
    Grid mainGrid;
    Dictionary<Vector2Int, GameObject> unitList = new Dictionary<Vector2Int, GameObject>();

    // Start is called before the first frame update
    void Awake()
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject getUnitFromCoords(Vector2Int coords)
    {
        if (unitList.ContainsKey(coords))
        {
            return unitList[coords];
        }
        return null;
    }

    public void addUnit(GameObject unit)
    {
        unitList.Add(unit.GetComponent<UnitController>().getUnitPos(), unit);
    }

    public bool moveUnit(GameObject unit, Vector2 coords)
    {
        if (getUnitFromCoords(gridTilePos(coords)) && getUnitFromCoords(gridTilePos(coords)) != gameObject)
        {
            return false;
        }
        Vector2Int key = unit.GetComponent<UnitController>().getUnitPos();
        unitList.Remove(key);
        bool moved = unit.GetComponent<UnitController>().moveUnit(coords);
        if (moved)
        {
            unitList.Add(gridTilePos(coords), unit);
        }
        else
        {
            unitList.Add(key, unit);
        }
        return moved;
    }

    public Vector2 tileWorldPos(Vector2 worldPos)
    {
        Vector3 localCellCoords = mainGrid.LocalToCell(worldPos);
        Vector3Int localCellInt = new Vector3Int(Mathf.CeilToInt(localCellCoords.x), Mathf.CeilToInt(localCellCoords.y), Mathf.CeilToInt(localCellCoords.z));
        Vector3 localCoords = mainGrid.CellToLocal(localCellInt);
        return new Vector2(localCoords.x, localCoords.y + mainGrid.cellSize.y / 2);
    }

    public Vector2 tileGridPos(Vector2Int gridPos)
    {
        return new Vector2(((float)(gridPos.x - gridPos.y)/2) * mainGrid.cellSize.x, ((float)(gridPos.x + gridPos.y - 1f) / 2) * mainGrid.cellSize.y);
    }

    public Vector2Int gridTilePos(Vector2 tilePos)
    {
        int gridX = Mathf.CeilToInt(tilePos.y + tilePos.x);
        int gridY = Mathf.CeilToInt(tilePos.y - tilePos.x);
        return new Vector2Int(gridX, gridY);
    }

    public Vector2Int gridWorldPos(Vector2 worldPos)
    {
        return gridTilePos(tileWorldPos(worldPos));
    }

    public int gridDistance(Vector2Int gridPos1, Vector2Int gridPos2)
    {
        int xDist = Mathf.Abs(gridPos1.x - gridPos2.x);
        int yDist = Mathf.Abs(gridPos1.y - gridPos2.y);
        return xDist + yDist;
    }

    public int gridDistanceFromTile(Vector2 tilePos1, Vector2 tilePos2)
    {
        return gridDistance(gridTilePos(tilePos1), gridTilePos(tilePos2));
    }

    public int gridDistanceFromWorld(Vector2 worldPos1, Vector2 worldPos2)
    {
        return gridDistance(gridWorldPos(worldPos1), gridWorldPos(worldPos2));
    }
}
