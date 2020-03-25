using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenWrap : MonoBehaviour
{
    private Camera mainCamera;
    private Rigidbody2D rb;
    private Renderer rend;

    private float failsafeTimer = 0.0f;
    private float failsafeInterval = 5.0f;

    private Transform spawnPoint;

    void Awake()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        rend = GetComponentInChildren<Renderer>();
        spawnPoint = GameObject.FindWithTag("GameController").transform.GetChild(0);
    }

    void Update()
    {
        // sometimes fish escape, check occasionally
        failsafeTimer += Time.deltaTime;
        if(failsafeTimer > failsafeInterval)
	{
            failsafeTimer = 0f;
            if(!rend.isVisible) WrapPosition();
        }
    }

    void OnBecameInvisible()
    {
    	if (GameController.gameStarted)
	{
            WrapPosition();
        }
    }

    private void WrapPosition()
    {
        Vector3 position = rb.position;
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(position);
        if (viewportPosition.x > 1 || viewportPosition.x < 0)
        {
            position.x = -position.x;
        }
        if (viewportPosition.y > 1 || viewportPosition.y < 0)
        {
            position.y = -position.y;
        }
        rb.position = position;
    }


    private void Failsafe()
    {
        if (GameController.gameStarted)
        {
            Vector3 viewportPosition = mainCamera.WorldToViewportPoint(rb.position);
            if (viewportPosition.x > 1 || viewportPosition.x < 0 || viewportPosition.y > 1 || viewportPosition.y < 0)
            {
                rb.position = new Vector2(spawnPoint.position.x, spawnPoint.position.y);
            }
        }
    }
}
