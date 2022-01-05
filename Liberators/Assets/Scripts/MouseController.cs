using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseController : MonoBehaviour
{
    public Sprite positiveCursor;
    public Sprite negativeCursor;
    public Sprite neutralCursor;
    Grid mainGrid;
    MapController mapController;

    Vector2 cursorPosition = new Vector2(0, 0);
    Vector2 tilePosition = new Vector2(0, 0);

    // Start is called before the first frame update
    void Start()
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(cursorPosition);
        worldPos = mapController.worldToTilePos(worldPos);
        tilePosition = new Vector3(worldPos.x, worldPos.y, -1);
        transform.position = tilePosition;
    }

    void OnCursorMovement(InputValue value)
    {
        cursorPosition = value.Get<Vector2>();
    }

    void OnCursorPrimary()
    {
        GameObject selectedUnit = mainGrid.GetComponentInChildren<MapController>().getUnitFromCoords(tilePosition);
        if (!selectedUnit)
        {
            Debug.Log("No unit");
            return;
        }
        Debug.Log("Has unit");
    }
}
