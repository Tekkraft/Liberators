using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerController : MonoBehaviour
{
    public enum Markers {BLUE, RED, GREEN, NEUTRAL};
    public Sprite[] sprites;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setup(Markers markColor, Vector2 tileCoords)
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = sprites[(int) markColor];
        transform.position = new Vector3(tileCoords.x, tileCoords.y, -1);
    }

    public void removeMarker()
    {
        GameObject.Destroy(gameObject);
    }
}
