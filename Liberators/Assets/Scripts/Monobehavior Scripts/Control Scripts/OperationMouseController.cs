using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OperationMouseController : MonoBehaviour
{
    public Sprite positiveCursor;
    public Sprite negativeCursor;
    public Sprite neutralCursor;
    Grid mainGrid;
    MapController mapController;
    OperationController operationController;

    Vector2 cursorPosition = new Vector2(0, 0);
    Vector2 worldPosition = new Vector2(0, 0);
    public Vector2 tilePosition = new Vector2(0, 0);
    Vector2Int gridPosition = new Vector2Int(0, 0);

    // Start is called before the first frame update
    void Start()
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
        operationController = mainGrid.GetComponentsInChildren<OperationController>()[0];
    }

    // Update is called once per frame
    void Update()
    {
        worldPosition = Camera.main.ScreenToWorldPoint(cursorPosition);
        tilePosition = mapController.tileWorldPos(worldPosition);
        Vector3 tileTransform = new Vector3(tilePosition.x, tilePosition.y, -1);
        transform.position = tileTransform;
        gridPosition = mapController.gridTilePos(tilePosition);
        Cursor.visible = mapController.mouseOverCanvas(cursorPosition);
    }

    //Input Handling
    void OnCursorMovement(InputValue value)
    {
        cursorPosition = value.Get<Vector2>();
    }

    void OnCursorPrimary()
    {
        operationController.moveSquadToLocation(GameObject.Find("Player Squad"), gridPosition);
    }

    void OnCursorAlternate()
    {

    }

    public Vector2Int getGridPos()
    {
        return gridPosition;
    }

    public Vector2 getWorldPos()
    {
        return worldPosition;
    }

    public bool mouseOnLeft()
    {
        return (cursorPosition.x / Screen.width) < 0.5;
    }
}