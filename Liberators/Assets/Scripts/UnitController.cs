using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    Vector2 unitPosition;
    Grid mainGrid;
    MapController mapController;

    // Start is called before the first frame update
    void Start()
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
        unitPosition = mapController.GetComponent<MapController>().worldToTilePos(transform.position);
        unitPosition.y--;
        mapController.addUnit(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void setUnitPos(Vector2 tilePos)
    {
        unitPosition = tilePos;
    }

    public Vector2 getUnitPos()
    {
        return unitPosition;
    }
}
