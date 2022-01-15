using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    public GameObject button;

    public GameObject hpMeter;
    public GameObject apMeter;

    GameObject hoveredUnit;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (hoveredUnit)
        {
            UnitController unitController = hoveredUnit.GetComponent<UnitController>();
            hpMeter.GetComponent<Text>().text = "HP: " + unitController.getStats()[2] + "/" + unitController.getStats()[1];
            apMeter.GetComponent<Text>().text = "AP: " + unitController.getActions()[1] + "/" + unitController.getActions()[0];
        }
    }

    public void hoverUnit(GameObject hover)
    {
        hoveredUnit = hover;
    }

    public void unHoverUnit()
    {
        hoveredUnit = null;
    }

    public bool mouseOverCanvas(Vector2 mousePos)
    {
        //Set up the new Pointer Event
        PointerEventData m_PointerEventData = new PointerEventData(gameObject.GetComponent<EventSystem>());
        //Set the Pointer Event Position to that of the mouse position
        m_PointerEventData.position = mousePos;

        //Create a list of Raycast Results
        List<RaycastResult> results = new List<RaycastResult>();

        //Raycast using the Graphics Raycaster and mouse click position
        gameObject.GetComponent<GraphicRaycaster>().Raycast(m_PointerEventData, results);

        return results.Count > 0;
    }

    public void drawButtons()
    {
        //No implementation yet.
    }
}
