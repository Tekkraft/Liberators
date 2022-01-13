using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    public MapController.actionType linkedAction;
    Grid mainGrid;
    MapController mapController;

    // Start is called before the first frame update
    void Start()
    {
        mainGrid = GameObject.FindObjectOfType<Grid>();
        mapController = mainGrid.GetComponentsInChildren<MapController>()[0];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setButtonAction(MapController.actionType action)
    {
        linkedAction = action;
    }

    public void setAction()
    {
        mapController.setActionState(linkedAction);
    }
}
