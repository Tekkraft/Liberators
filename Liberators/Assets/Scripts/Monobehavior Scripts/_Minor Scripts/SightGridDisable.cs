using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script simply hides an object by moving the transform backwards when loaded. Use for sight grid primarily
public class SightGridDisable : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.position = transform.position + new Vector3(0, 0, 100);
    }
}
