using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayController : MonoBehaviour
{
    Vector3 origin;
    Quaternion originRotation;

    float overlaySize = 1;
    Vector2 overlayDirection = new Vector2(0, 0);
    bool lineMode = true;

    public Sprite beamSprite;
    public Sprite areaSprite;

    GameObject target;

    void Awake()
    {

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
            if (target.GetComponent<MouseController>() != null)
            {
                transform.localPosition = new Vector3(0, 0, 0);
                transform.rotation = originRotation;
                Vector2 originV2 = new Vector2(origin.x, origin.y);
                Vector2 destination = target.GetComponent<MouseController>().GetWorldPos();
                Vector2 targetCoord = (destination - originV2).normalized * transform.localScale.x;
                overlayDirection = targetCoord;
                float angle = Mathf.Atan2(targetCoord.y, targetCoord.x) * Mathf.Rad2Deg;
                Vector2 offset = targetCoord / 2 + targetCoord.normalized / 2;
                transform.Translate(offset);
                transform.Rotate(new Vector3(0, 0, angle));
            }
        }
    }

    public void initalize(float size, bool lineMode, GameObject target)
    {
        overlaySize = size;
        if (lineMode)
        {
            transform.localScale = new Vector3(size, 0.5f, 1);
            gameObject.GetComponent<SpriteRenderer>().sprite = beamSprite;
            this.target = target;
        }
        else
        {
            transform.localScale = new Vector3(size, size, 1);
            gameObject.GetComponent<SpriteRenderer>().sprite = areaSprite;
            this.target = target;
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
