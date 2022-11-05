using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class MapController : MonoBehaviour
{
    //Main Variabless
    Grid mainGrid;
    Canvas uiCanvas;
    Tilemap mainTilemap;
    Pathfinder pathfinder;

    //Public Objects
    public LayerMask lineOfSightLayer;

    void Awake()
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        uiCanvas = GameObject.FindObjectOfType<Canvas>();
        mainTilemap = mainGrid.GetComponentInChildren<Tilemap>();
        pathfinder = new Pathfinder(new Vector2Int(0, 0), 0, 0, mainTilemap);
    }

    //Canvas Management
    public bool mouseOverCanvas(Vector2 mousePos)
    {
        if (uiCanvas.GetComponent<UIController>())
        {
            return uiCanvas.GetComponent<UIController>().mouseOverCanvas(mousePos);
        } else if (uiCanvas.GetComponent<OperationUIController>())
        {
            return uiCanvas.GetComponent<OperationUIController>().mouseOverCanvas(mousePos);
        }
        else
        {
            int zero = 0;
            int breaker = 10 / zero;
            return false;
        }
    }

    //Other Helpers
    public Pathfinder getPathfinder()
    {
        return pathfinder;
    }

    //Grid Management
    public Vector2 tileWorldPos(Vector2 worldPos)
    {
        Vector3 localCellCoords = mainGrid.LocalToCell(worldPos);
        Vector3Int localCellInt = new Vector3Int(Mathf.CeilToInt(localCellCoords.x), Mathf.CeilToInt(localCellCoords.y), Mathf.CeilToInt(localCellCoords.z));
        Vector3 localCoords = mainGrid.CellToLocal(localCellInt);
        return new Vector2(localCoords.x + mainGrid.cellSize.x / 2, localCoords.y + mainGrid.cellSize.y / 2);
    }

    public Vector2 tileGridPos(Vector2Int gridPos)
    {
        return new Vector2((float)gridPos.x - mainGrid.cellSize.x / 2, (float)gridPos.y - mainGrid.cellSize.y / 2);
    }

    public Vector2 tileGridPos(Vector2 gridPos)
    {
        return new Vector2((float)gridPos.x - mainGrid.cellSize.x / 2, (float)gridPos.y - mainGrid.cellSize.y / 2);
    }

    public Vector2Int gridTilePos(Vector2 tilePos)
    {
        float tileX = tilePos.x + mainGrid.cellSize.x / 2;
        float tileY = tilePos.y + mainGrid.cellSize.y / 2;
        return new Vector2Int(Mathf.CeilToInt(tileX), Mathf.CeilToInt(tileY));
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