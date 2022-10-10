using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayController : MonoBehaviour
{
    Vector3 origin;
    Quaternion originRotation;
    GameObject cursor;
    MouseController cursorController;

    float overlaySize = 1;
    Vector2 overlayDirection = new Vector2(0, 0);
    bool lineMode = true;

    public Sprite beamSprite;
    public Sprite areaSprite;

    void Awake()
    {
        cursor = GameObject.FindGameObjectWithTag("Cursor");
        cursorController = cursor.GetComponent<MouseController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        origin = transform.position;
        originRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (lineMode)
        {
            transform.localPosition = new Vector3(0, 0, 0);
            transform.rotation = originRotation;
            Vector2 originV2 = new Vector2(origin.x, origin.y);
            Vector2 destination = cursorController.getWorldPos();
            Vector2 target = (destination - originV2).normalized * transform.localScale.x;
            overlayDirection = target;
            float angle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
            Vector2 offset = target / 2 + target.normalized / 2;
            transform.Translate(offset);
            transform.Rotate(new Vector3(0, 0, angle));
        }
    }

    public void initalize(float size, bool lineMode)
    {
        overlaySize = size;
        if (lineMode)
        {
            transform.localScale = new Vector3(size, 0.5f, 1);
            gameObject.GetComponent<SpriteRenderer>().sprite = beamSprite;
        }
        else
        {
            transform.localScale = new Vector3(size, size, 1);
            gameObject.GetComponent<SpriteRenderer>().sprite = areaSprite;
        }
    }

    public float getOverlaySize()
    {
        return overlaySize;
    }

    public Vector2 getOverlayDirection()
    {
        return overlayDirection;
    }
}
