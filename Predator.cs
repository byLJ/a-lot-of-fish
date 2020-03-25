using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Predator : MonoBehaviour
{
    public Rigidbody2D myRigidbody;

    private float chaseDistance = 15f;
    private float chaseWeight = 10f;
    private float speed = 4f;
    private float rotateSpeed = 800f;

    private bool isEating = false;

    private Rigidbody2D target;
    private GameController gameController;
    private Transform avatar;

    void Awake()
    {
        myRigidbody = gameObject.GetComponent<Rigidbody2D>();
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        avatar = this.transform.GetChild(0);
    }

    public void UpdatePredator(List<Boid> boids)
    {
        UpdateTarget(boids);
        UpdateVelocity();
        TurnAvatar();
    }

    private void UpdateTarget(List<Boid> boids)
    {
        // if there isn't a target, or target has got away
        if (!target || Vector2.Distance(myRigidbody.position, target.position) > chaseDistance)
        {
            if (!isEating)
            {
                target = FindTarget(boids);
            }
            else
            {
                target = null;
            }
        }
    }

    private Rigidbody2D FindTarget(List<Boid> boids)
    {
        Rigidbody2D newTarget = FindClosestBoid(boids);

        // check target in range
        if (!TargetInRange(newTarget))
        {
            newTarget = null;
        }
        return newTarget;
    }

    private Rigidbody2D FindClosestBoid(List<Boid> boids)
    {
        // find distance to first boid
        Rigidbody2D closest = boids[0].myRigidbody;
        float distance = Vector2.Distance(closest.position, myRigidbody.position);

        // compare other boids
        for (int i = 1; i < boids.Count; i++)
        {
            float newDistance = Vector2.Distance(boids[i].myRigidbody.position, myRigidbody.position);
            if (newDistance < distance)
            {
                closest = boids[i].myRigidbody;
                distance = newDistance;
            }
        }

        return closest;
    }

    private bool TargetInRange(Rigidbody2D newTarget)
    {
        if (Vector2.Distance(newTarget.position, myRigidbody.position) > chaseDistance)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private Vector2 ComputeChase(Rigidbody2D target)
    {
        Vector2 velocity = new Vector2();

        if (target && Vector2.Distance(myRigidbody.position, target.position) < chaseDistance)
        {
            velocity.x += target.position.x;
            velocity.y += target.position.y;

            velocity = new Vector2(velocity.x - myRigidbody.position.x, velocity.y - myRigidbody.position.y);
        }

        return velocity.normalized;
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

    void OnTriggerEnter2D(Collider2D col)
    {
        // if predator hits boid
        if (col.gameObject.tag == "Boid" && !isEating)
        {
            // turn around
            TurnAround();
            TurnAvatar();
            // eat boid
            Boid boid = col.gameObject.GetComponent<Boid>();
            StartCoroutine(EatBoid(boid));

        }
    }

    private void TurnAround()
    {
        myRigidbody.velocity = -myRigidbody.velocity;
    }

    private void TurnAvatar()
    {
        Quaternion rotation;

        if (!target)
        {
            // face front
            rotation = Quaternion.LookRotation(Vector3.back);
            avatar.rotation = Quaternion.RotateTowards(this.transform.GetChild(0).rotation, rotation, rotateSpeed * Time.deltaTime);
        }
        else
        {
            // turn avatar transform to match rigidbody direction
            rotation = Quaternion.LookRotation(new Vector3(myRigidbody.velocity.x, myRigidbody.velocity.y, 0), Vector3.up);
            avatar.rotation = Quaternion.RotateTowards(this.transform.GetChild(0).rotation, rotation, rotateSpeed * Time.deltaTime);
        }
    }

    IEnumerator EatBoid(Boid boid)
    {
        isEating = true;
        if (boid)
        {
            gameController.RemoveBoid(boid);
        }
        yield return new WaitForSeconds(1);
        isEating = false;
    }
}