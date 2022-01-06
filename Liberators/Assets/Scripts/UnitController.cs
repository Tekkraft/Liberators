using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public GameObject marker;
    Vector2 unitPosition;
    Vector2Int unitGridPosition;
    Grid mainGrid;
    MapController mapController;
    public int movementRange = 5;
    List<GameObject> markerList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
        unitPosition = mapController.tileWorldPos(transform.position);
        unitPosition.y--;
        unitGridPosition = mapController.gridTilePos(unitPosition);
        mapController.addUnit(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void createMarkers()
    {
        for (int i = -movementRange; i <= movementRange; i++)
        {
            for (int j = -movementRange; j <= movementRange; j++)
            {
                if (Mathf.Abs(i) + Mathf.Abs(j) <= movementRange)
                {
                    GameObject temp = GameObject.Instantiate(marker);
                    Vector2 markerLocation = mapController.tileGridPos(unitGridPosition + new Vector2Int(i,j));
                    temp.GetComponent<MarkerController>().setup(MarkerController.Markers.BLUE, markerLocation);
                }
            }
        }
    }

    public void destroyMarkers()
    {
        for (int i = 0; i < markerList.Count; i = 0)
        {
            GameObject currentMarker = markerList[0];
            currentMarker.GetComponent<MarkerController>().removeMarker();
        }
    }

    public bool moveUnit(Vector2 destination)
    {
        Vector2Int destinationTile = mapController.gridTilePos(destination);
        int distance = mapController.gridDistance(destinationTile, unitGridPosition);
        if (distance <= movementRange)
        {
            setUnitPos(destination);
            return true;
        }
        return false;
    }

    void setUnitPos(Vector2 worldPos)
    {
        transform.position = worldPos;
        transform.Translate(new Vector3(0,0.75f,-2));
        unitGridPosition = mapController.gridWorldPos(transform.position);
        destroyMarkers();
    }

    public Vector2Int getUnitPos()
    {
        return unitGridPosition;
    }
}
