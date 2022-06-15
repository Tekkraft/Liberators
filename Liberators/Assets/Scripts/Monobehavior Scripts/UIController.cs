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
    public GameObject bannerComponent;

    GameObject hoveredUnit;
    List<GameObject> allButtons = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        bannerComponent.transform.Translate(new Vector3(0, 300, 0));
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

    //Button Handling
    public void drawButtons(List<Ability> allAbilities, GameObject linkedUnit)
    {
        clearButtons();
        int count = allAbilities.Count;
        float spacing = 4f + button.GetComponent<RectTransform>().rect.height;
        if (count % 2 == 0)
        {
            for (int i = -count / 2; i < count / 2; i++)
            {
                GameObject newButton = GameObject.Instantiate(button, transform);
                allButtons.Add(newButton);
                newButton.transform.Translate(new Vector2((spacing / 2 + spacing * i), 0));
            }
        }
        else
        {
            for (int i = (-count + 1) / 2; i < (count + 1) / 2; i++)
            {
                GameObject newButton = GameObject.Instantiate(button, transform);
                allButtons.Add(newButton);
                newButton.transform.Translate(new Vector2((spacing * i), 0));
            }
        }
        for (int i = 0; i < allButtons.Count; i++)
        {
            allButtons[i].GetComponent<ButtonController>().setupButton(allAbilities[i], linkedUnit);
        }
    }

    public void clearButtons()
    {
        for (int i = allButtons.Count - 1; i >= 0; i--)
        {
            GameObject temp = allButtons[i];
            allButtons.Remove(temp);
            GameObject.Destroy(temp);
        }
    }

    public void validateButtons(int availableActions)
    {
        resetButtons();
        foreach (GameObject button in allButtons)
        {
            if (availableActions < button.GetComponent<ButtonController>().getAbility().getAPCost())
            {
                button.GetComponent<Button>().interactable = false;
            }
        }
    }

    public void resetButtons()
    {
        foreach (GameObject button in allButtons)
        {
            button.GetComponent<Button>().interactable = true;
        }
    }

    //Banner Handling
    //Time is in tenth of seconds
    public void changeBanner(string textMessage, int bannerDuration)
    {
        bannerComponent.transform.Translate(new Vector3(0, -300, 0));
        bannerComponent.GetComponentInChildren<Text>().text = textMessage;
        StartCoroutine(bannerExit(bannerDuration));
    }

    IEnumerator bannerExit(int time)
    {
        for (int i = 0; i < time; i++)
        {
            yield return new WaitForSeconds(0.1f);
        }
        bannerComponent.transform.Translate(new Vector3(0, 300, 0));
    }
}
