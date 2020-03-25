using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shark : MonoBehaviour
{
    public Rigidbody2D myRigidbody;
    public bool isActive = false;

    private float chaseWeight = 10f;
    private float speed = 6f;
    private float rotateSpeed = 800f;

    private bool hasEaten = false;

    public Rigidbody2D target;
    private GameController gameController;
    private Camera mainCamera;
    private Transform avatar;

    private Animator anim;

    void Awake()
    {
        myRigidbody = gameObject.GetComponent<Rigidbody2D>();
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        mainCamera = Camera.main;
        avatar = this.transform.GetChild(0);
        anim = avatar.GetComponent<Animator>();
    }

    public void ActivateShark(List<Predator> predators)
    {
        hasEaten = false;
        isActive = true;
        anim.ResetTrigger("triggerEat");
        // choose target at random
        target = predators[(int)Random.Range(0, predators.Count)].transform.GetComponent<Rigidbody2D>();
        // spawn opposite where it last left screen
        WrapPosition();
    }

    private void OnBecameInvisible()
    {
        StartCoroutine(DeactivateShark());
    }

    private IEnumerator DeactivateShark()
    {
        yield return new WaitForSeconds(4);
        isActive = false;
    }

    public void UpdateShark(List<Predator> predators)
    {
        if (isActive)
        {
            if (!hasEaten)
            {
                UpdateVelocity();
                TurnAvatar();

            }
            else
            {
                LeaveScreen();
            }

        }
        else
        {
            target = null;
            UpdateVelocity();
        }
    }

    private void UpdateVelocity()
    {
        float x = 0;
        float y = 0;

        if (target)
        {
            Vector2 chase = ComputeChase(target);
            x = myRigidbody.velocity.x + chase.x * chaseWeight;
            y = myRigidbody.velocity.y + chase.y * chaseWeight;
        }

        myRigidbody.velocity = new Vector2(x, y);
        myRigidbody.velocity = myRigidbody.velocity.normalized * speed;

    }

    private void LeaveScreen()
    {
        Vector3 targetPosition = avatar.position + avatar.forward;

        Vector2 velocity = new Vector2();
        velocity.x += targetPosition.x;
        velocity.y += targetPosition.y;
        velocity = new Vector2(velocity.x - myRigidbody.position.x, velocity.y - myRigidbody.position.y);

        myRigidbody.velocity = velocity * chaseWeight;
        myRigidbody.velocity = myRigidbody.velocity.normalized * speed;

    }

    private Vector2 ComputeChase(Rigidbody2D target)
    {
        Vector2 velocity = new Vector2();
        velocity.x += target.position.x;
        velocity.y += target.position.y;
        velocity = new Vector2(velocity.x - myRigidbody.position.x, velocity.y - myRigidbody.position.y);

        return velocity.normalized;
    }

    private void TurnAvatar()
    {
        // turn avatar transform to match rigidbody direction
        Quaternion rotation = Quaternion.LookRotation(new Vector3(myRigidbody.velocity.x, myRigidbody.velocity.y, 0), Vector3.up);
        avatar.rotation = Quaternion.RotateTowards(avatar.rotation, rotation, rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // if predator hits predator
        if (col.transform.GetComponent<Rigidbody2D>() == target)
        {
            StartCoroutine(EatPredator(col.gameObject.GetComponent<Predator>()));
        }
    }

    public IEnumerator EatPredator(Predator predator)
    {
        hasEaten = true;

        anim.SetTrigger("triggerEat");
        yield return new WaitForSeconds(0.2f);

        if (predator)
        {
            gameController.RemovePredator(predator);
        }

        yield return null;
    }

    private void WrapPosition()
    {
        Vector3 position = myRigidbody.position;
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(position);
        if (viewportPosition.x > 1 || viewportPosition.x < 0)
        {
            position.x = -position.x;
        }
        if (viewportPosition.y > 1 || viewportPosition.y < 0)
        {
            position.y = -position.y;
        }
        myRigidbody.position = position;
    }
}