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
    public GameObject combatPreview;
    public GameObject unitDataPreview;
    public GameObject animationPanel;
    [SerializeField]
    GameObject uiBaseBar;
    [SerializeField]
    GameObject marker;
    [SerializeField]
    MapController mapController;

    GameObject activeBanner;
    GameObject activePreview;
    GameObject activeAnimation;

    GameObject hoveredUnit;
    List<GameObject> allButtons = new List<GameObject>();

    List<GameObject> allMarkers = new List<GameObject>();

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
            hpMeter.GetComponent<Text>().text = "HP: " + unitController.GetStat("currentHP") + "/" + unitController.GetStat("maxHP");
            apMeter.GetComponent<Text>().text = "AP: " + unitController.getActions()[1] + "/" + unitController.getActions()[0];
        }
        else
        {
            hpMeter.GetComponent<Text>().text = "HP: ";
            apMeter.GetComponent<Text>().text = "AP: ";
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
        float ratio = button.GetComponent<RectTransform>().sizeDelta.x / 32;
        float spacing = (4f + button.GetComponent<RectTransform>().sizeDelta.x) * ratio;
        if (count % 2 == 0)
        {
            for (int i = -count / 2; i < count / 2; i++)
            {
                GameObject newButton = GameObject.Instantiate(button, uiBaseBar.GetComponent<RectTransform>());
                allButtons.Add(newButton);
                newButton.GetComponent<RectTransform>().localPosition = new Vector2(spacing / 2 + spacing * i, -2 * ratio);
            }
        }
        else
        {
            for (int i = (-count + 1) / 2; i < (count + 1) / 2; i++)
            {
                GameObject newButton = GameObject.Instantiate(button, uiBaseBar.GetComponent<RectTransform>());
                allButtons.Add(newButton);
                newButton.GetComponent<RectTransform>().localPosition = new Vector2(spacing * i, -2 * ratio);
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

    //Marker Handling
    public void drawMarkers(List<Vector2Int> coords, MarkerController.Markers markerColor)
    {
        foreach (Vector2Int gridPos in coords)
        {
            GameObject temp = Instantiate(marker);
            Vector2 markerLocation = mapController.tileGridPos(gridPos);
            temp.GetComponent<MarkerController>().setup(markerColor, markerLocation);
            allMarkers.Add(temp);
        }
    }

    public void clearMarkers()
    {
        for (int i = allMarkers.Count - 1; i >= 0; i--)
        {
            GameObject temp = allMarkers[i];
            allMarkers.Remove(temp);
            GameObject.Destroy(temp);
        }
    }

    //Banner Handling
    //Time is in tenth of seconds
    public void changeBanner(string textMessage, int bannerDuration)
    {
        activeBanner = GameObject.Instantiate(bannerComponent, transform);
        activeBanner.transform.localPosition = new Vector3(0, 0, 0);
        activeBanner.GetComponentInChildren<Text>().text = textMessage;
        StartCoroutine(bannerExit(bannerDuration));
    }

    IEnumerator bannerExit(int time)
    {
        for (int i = 0; i < time; i++)
        {
            yield return new WaitForSeconds(0.1f);
        }
        GameObject.Destroy(activeBanner);
    }

    //Preview Handling
    public GameObject displayAbilityPreview(GameObject defender, Ability activeAbility)
    {
        if (activePreview == null)
        {
            activePreview = GameObject.Instantiate(combatPreview, transform);
        }
        activePreview.GetComponent<PreviewController>().setData(defender, activeAbility);
        return activePreview;
    }

    public GameObject displayUnitDataPreview(GameObject unit)
    {
        GameObject.Destroy(activePreview);
        activePreview = GameObject.Instantiate(unitDataPreview, transform, false);
        activePreview.GetComponent<PreviewController>().setData(unit);
        Canvas.ForceUpdateCanvases();
        return activePreview;
    }

    //TODO: Reimplement Animation
    public GameObject displayBattleAnimation(List<BattleStep> steps)
    {
        activeAnimation = GameObject.Instantiate(animationPanel, transform);
        activeAnimation.GetComponent<AnimController>().createBattleAnimation(steps);
        return activeAnimation;
    }

    public void terminateBattleAnimation()
    {
        if (activeAnimation)
        {
            activeAnimation.GetComponent<AnimController>().TerminateAnimation();
        }
    }

    public void clearPreview()
    {
        GameObject.Destroy(activePreview);
    }

    public bool hasPreview()
    {
        return activePreview;
    }

    public bool hasAnimation()
    {
        return activeAnimation;
    }
}
