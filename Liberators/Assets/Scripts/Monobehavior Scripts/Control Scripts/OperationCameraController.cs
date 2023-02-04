using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OperationCameraController : MonoBehaviour
{
    bool cameraLock = false;
    public float cameraSpeed = 5;
    Vector2 cameraMove;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!cameraLock)
        {
            transform.Translate(new Vector3(cameraMove.x, cameraMove.y, 0) * cameraSpeed * Time.deltaTime);
        }
    }

    public void lockCamera()
    {
        cameraLock = true;
    }

    public void unlockCamera()
    {
        cameraLock = false;
    }

    void OnCameraMovement(InputValue value)
    {
        cameraMove = value.Get<Vector2>();
    }

    public void ForceCameraMove(Vector2 value)
    {
        cameraMove = value;
    }
}
