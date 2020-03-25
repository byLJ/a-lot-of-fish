using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    public Material[] materials;
    public float activeSince;

    public int targetCount;
    public int rewardCount;
    public bool isGood;
    public bool isActive;
    public bool isMoving;
    public bool rewardTriggered = false;

    private float speed = 1.5f;

    private Vector3 inactivePosition;
    private Vector3 activePosition;
    private float journeyLength;

    private Renderer circleRend;
    private Material[] circleMats;
    private Renderer cylinderRend;
    private Material[] cylinderMats;

    private Collider2D[] colliders;

    private int collisionCount;
    public float secondsBetweenCounts;
    public float timeSinceLastCollision;

    public GameController gameController;
    public TextMesh counter;
    public Color[] textColor;

    public void InitialiseGate()
    {
        inactivePosition = transform.position;
        activePosition = new Vector3(transform.position.x, transform.position.y, 0);
        journeyLength = Vector3.Distance(activePosition, inactivePosition);

        // get renderers and starting materials
        circleRend = transform.GetChild(1).GetComponent<Renderer>();
        circleMats = circleRend.materials;
        cylinderRend = transform.GetChild(3).GetComponent<Renderer>();
        cylinderMats = cylinderRend.materials;

        // set starting text color
        ChangeTextColor(textColor[0]);

        // get colliders
        colliders = transform.GetComponents<Collider2D>();
    }

    public void OpenGate(bool good, int target, int reward)
    {
        isGood = good;
        targetCount = target;
        rewardCount = reward;
        StartCoroutine(MoveGateForward());
    }

    public void CloseGate()
    {
        rewardTriggered = true;
        StartCoroutine(MoveGateBack());
    }

    private IEnumerator MoveGateForward()
    {
        float startTime = Time.time;

        isMoving = true;

        while (isMoving)
        {
            float distCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(inactivePosition, activePosition, fractionOfJourney);

            if (Vector3.Distance(transform.position, activePosition) < 0.01)
            {
                transform.position = activePosition;
                ActivateGate();
                isMoving = false;
            }

            yield return null;
        }
    }

    private void ActivateGate()
    {
        // activate and enable colliders
        isActive = true;
        activeSince = Time.time;

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = true;
        }

        // change materials and text color
        Material newMaterial;
        Color col;
        if (isGood)
        {
            newMaterial = materials[0];
            col = textColor[1];
        }
        else
        {
            newMaterial = materials[1];
            col = textColor[2];
        }

        ChangeMaterials(newMaterial);

        // set tubes
        SetTubes();
        ChangeTextColor(col);

        // reset flag
        rewardTriggered = false;
    }

    private IEnumerator MoveGateBack()
    {
        DeactivateGate();

        float startTime = Time.time;
        isMoving = true;

        while (isMoving)
        {
            float distCovered = (Time.time - startTime) * speed;
            float fractionOfJourney = distCovered / journeyLength;

            transform.position = Vector3.Lerp(activePosition, inactivePosition, fractionOfJourney);
            if (Vector3.Distance(transform.position, inactivePosition) < 0.01)
            {
                transform.position = inactivePosition;
                isMoving = false;
            }

            yield return null;
        }
    }

    private void DeactivateGate()
    {
        // deactivate and disable colliders
        isActive = false;

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        // change materials
        Material newMaterial = materials[2];
        ChangeMaterials(newMaterial);

        // set tubes
        targetCount = 0;
        collisionCount = 0;
        SetTubes();
        ChangeTextColor(textColor[0]);
    }

    private void ChangeMaterials(Material mat)
    {
        // gate materials
        Material[] newCircleMats = circleMats;
        newCircleMats[0] = mat;
        circleRend.materials = newCircleMats;

        Material[] newCylinderMats = cylinderMats;
        newCylinderMats[1] = mat;
        cylinderRend.materials = newCylinderMats;
    }

    private void ChangeTextColor(Color newColor)
    {
        counter.GetComponent<Renderer>().material.color = newColor;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.tag == "Boid" && isActive && !rewardTriggered)
        {
            timeSinceLastCollision = 0f;
            collisionCount++;
            // set tubes
            SetTubes();

            if (collisionCount >= targetCount)
            {
                rewardTriggered = true;
                StartCoroutine(GateCompleteSequence());

            }
        }
    }

    private IEnumerator GateCompleteSequence()
    {
        yield return new WaitForSeconds(0.8f);
        CloseGate();
        StartCoroutine(TriggerReward());
    }

    private void Update()
    {
        if (timeSinceLastCollision < secondsBetweenCounts && isActive)
        {
            timeSinceLastCollision += Time.deltaTime;
            if (timeSinceLastCollision > secondsBetweenCounts)
            {
                collisionCount = 0;
                // set tubes
                SetTubes();
            }
        }
    }

    private IEnumerator TriggerReward()
    {
        gameController.bubbleParticle.Play();

        if (isGood)
        {
            for (int i = 0; i < rewardCount; i++)
            {
                if (gameController.flockSize < gameController.maxFlock)
                {
                    gameController.AddBoid();
                    yield return new WaitForSeconds(0.2f);
                }
            }

        }
        else
        {
            for (int i = 0; i < rewardCount; i++)
            {
                if (gameController.predatorNumber < gameController.maxPredators)
                {
                    gameController.AddPredator();
                    yield return new WaitForSeconds(0.2f);
                }
            }

        }

        yield return new WaitForSeconds(0.5f);

        gameController.bubbleParticle.Stop();
    }

    private void SetTubes()
    {
        int remaining = targetCount - collisionCount;
        if (remaining == 0 || !isActive)
        {
            counter.text = "0 0";
        }
        else if (remaining < 10)
        {
            counter.text = "0 " + remaining.ToString();
        }
        else
        {
            string digits = remaining.ToString();
            counter.text = digits[0] + " " + digits[1];
        }
    }
}
