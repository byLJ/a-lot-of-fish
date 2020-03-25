using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private float cameraZ;

    private void Awake()
    {
        cameraZ = Camera.main.transform.position.z;
    }

    void Update()
    {
        // mouse control
        Vector3 mousePos = Input.mousePosition;
        transform.position = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -cameraZ));
    }
}

