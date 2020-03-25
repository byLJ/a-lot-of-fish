using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    public Rigidbody2D myRigidbody;

    private float speed = 6f;
    private float rotateSpeed = 800f;

    private float followDistance = 10f;
    private float fleeDistance = 5f;
    private float sharkFleeDistance = 10f;
    private float flockDistance = 6f;

    private float followWeight = 2f;
    private float alignmentWeight = 0.6f;
    private float cohesionWeight = 0.9f;
    private float separationWeight = 1f;
    private float fleeWeight = 20f;

    private Transform avatar;

    void Awake()
    {
        myRigidbody = gameObject.GetComponent<Rigidbody2D>();
        avatar = this.transform.GetChild(0);
    }

    public void UpdateBoid(List<Boid> boids, Rigidbody2D player, List<Predator> predators, Shark shark)
    {
        List<Boid> localBoids = FindLocalBoids(boids);
        UpdateVelocity(localBoids, player, predators, shark);
        TurnAvatar();
    }

    private List<Boid> FindLocalBoids(List<Boid> boids)
    {
        // find boids within flock distance
        List<Boid> localBoids = new List<Boid>();
        for (int i = 0; i < boids.Count; i++)
        {
            if (Vector2.Distance(boids[i].myRigidbody.position, myRigidbody.position) < flockDistance)
            {
                localBoids.Add(boids[i]);
            }
        }
        return localBoids;
    }

    private void UpdateVelocity(List<Boid> localBoids, Rigidbody2D player, List<Predator> predators, Shark shark)
    {
        // compute vectors
        Vector2 follow = ComputeFollow(player);
        Vector2 alignment = ComputeAlignment(localBoids);
        Vector2 cohesion = ComputeCohesion(localBoids);
        Vector2 separation = ComputeSeparation(localBoids);
        Vector2 flee = ComputeFlee(predators);
        Vector2 sharkFlee = new Vector2(0f, 0f);
        if (shark) sharkFlee = ComputeSharkFlee(shark);

        // weight
        float x = myRigidbody.velocity.x + follow.x * followWeight + alignment.x * alignmentWeight + cohesion.x * cohesionWeight + separation.x * separationWeight + flee.x * fleeWeight + sharkFlee.x * fleeWeight;
        float y = myRigidbody.velocity.y + follow.y * followWeight + alignment.y * alignmentWeight + cohesion.y * cohesionWeight + separation.y * separationWeight + flee.y * fleeWeight + sharkFlee.y * fleeWeight;

        // update
        myRigidbody.velocity = new Vector2(x, y);
        myRigidbody.velocity = myRigidbody.velocity.normalized * speed;
    }

    public Vector2 ComputeFollow(Rigidbody2D player)
    {
        // follow behind player position
        Vector2 velocity = new Vector2();

        if (Vector2.Distance(myRigidbody.position, player.position) < followDistance)
        {
            Vector2 invPlayerVelocity = player.velocity * -1;
            Vector2 normVelocity = invPlayerVelocity.normalized * followDistance;
            Vector2 behind = player.position + normVelocity;

            velocity.x += behind.x;
            velocity.y += behind.y;

            velocity = new Vector2(velocity.x - myRigidbody.position.x, velocity.y - myRigidbody.position.y);
        }

        return velocity.normalized;
    }

    public Vector2 ComputeAlignment(List<Boid> boids)
    {
        // align direction with local boids
        Vector2 velocity = new Vector2();
        for (int i = 0; i < boids.Count; i++)
        {
            velocity.x += boids[i].myRigidbody.velocity.x;
            velocity.y += boids[i].myRigidbody.velocity.y;
        }

        velocity.x /= boids.Count;
        velocity.y /= boids.Count;

        return velocity.normalized;
    }

    public Vector2 ComputeCohesion(List<Boid> boids)
    {
        // move towards average local boid position
        Vector2 velocity = new Vector2();
        for (int i = 0; i < boids.Count; i++)
        {
            velocity.x += boids[i].myRigidbody.position.x;
            velocity.y += boids[i].myRigidbody.position.y;
        }

        velocity.x /= boids.Count;
        velocity.y /= boids.Count;

        velocity = new Vector2(velocity.x - myRigidbody.position.x, velocity.y - myRigidbody.position.y);

        return velocity.normalized;
    }

    public Vector2 ComputeSeparation(List<Boid> boids)
    {
        // move away from average local boid position
        Vector2 velocity = new Vector2();
        for (int i = 0; i < boids.Count; i++)
        {
            velocity.x += boids[i].myRigidbody.position.x - myRigidbody.position.x;
            velocity.y += boids[i].myRigidbody.position.y - myRigidbody.position.y;
        }

        velocity.x /= boids.Count;
        velocity.y /= boids.Count;

        velocity.x *= -1;
        velocity.y *= -1;


        return velocity.normalized;
    }

    public Vector2 ComputeFlee(List<Predator> predators)
    {
        // move away from average predator position
        Vector2 velocity = new Vector2();
        int count = 0;

        for (int i = 0; i < predators.Count; i++)
        {
            if (Vector2.Distance(myRigidbody.position, predators[i].myRigidbody.position) < fleeDistance)
            {
                count++;
                velocity.x += predators[i].myRigidbody.position.x - myRigidbody.position.x;
                velocity.y += predators[i].myRigidbody.position.y - myRigidbody.position.y;
            }
        }

        velocity.x /= count;
        velocity.y /= count;

        velocity.x *= -1;
        velocity.y *= -1;

        return velocity.normalized;
    }

    public Vector2 ComputeSharkFlee(Shark shark)
    {
        // move away from shark
        Vector2 velocity = new Vector2();

        if (Vector2.Distance(myRigidbody.position, shark.myRigidbody.position) < sharkFleeDistance)
        {
            velocity.x += shark.myRigidbody.position.x - myRigidbody.position.x;
            velocity.y += shark.myRigidbody.position.y - myRigidbody.position.y;
        }

        velocity.x *= -1;
        velocity.y *= -1;

        return velocity.normalized;
    }

    private void TurnAvatar()
    {
        // turn avatar transform to match rigidbody direction
        Quaternion rotation = Quaternion.LookRotation(new Vector3(myRigidbody.velocity.x, myRigidbody.velocity.y, 0), Vector3.up);
        avatar.rotation = Quaternion.RotateTowards(this.transform.GetChild(0).rotation, rotation, rotateSpeed * Time.deltaTime);
    }
}
