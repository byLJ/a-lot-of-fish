using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    public List<Boid> boids;
    public List<Predator> predators;
    public Shark shark;

    public Gate[] gates;

    public GameObject boidPrefab;
    public GameObject predatorPrefab;

    public Transform titleCameraPosition;
    public Transform playCameraPosition;

    public int startFlock = 25;
    public int maxFlock = 100;
    public int flockSize;
    public float overloadTimer = 0.0f;
    public float predatorSpawnInterval = 10.0f;

    public int startPredators = 1;
    public int maxPredators = 10;
    public int predatorNumber;

    public float sharkTimer = 0.0f;
    public float sharkSpawnInterval = 60.0f;

    public float gatesActiveSecs = 5.0f;
    public float gateActiveProbability = 0.001f;
    public float goodGateProbability = 0.8f;

    public Transform flockNeedle;

    public GameObject titleObjects;
    public ParticleSystem bubbleParticle;

    private Transform spawnPoint;
    private Rigidbody2D player;

    // title screen
    private Transform mainCamera;
    private bool titleClicked = false;
    private float startTime;
    public float titleScreenPanSpeed = 1f;
    public static bool gameStarted = false;
    private float journeyLength;

    void Awake()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>();
        spawnPoint = transform.GetChild(0);

        // set up camera lerp
        mainCamera = GameObject.FindWithTag("MainCamera").transform;
        mainCamera.position = titleCameraPosition.position;
        journeyLength = Vector3.Distance(titleCameraPosition.position, playCameraPosition.position);
    }

    private void InitialiseBoids()
    {
        boids = new List<Boid>();
        for (int i = 0; i < startFlock; i++)
        {
            AddBoid();
        }
    }

    public void AddBoid()
    {
        GameObject boidObject = Instantiate(boidPrefab, spawnPoint.position, spawnPoint.rotation);
        boids.Add(boidObject.GetComponent<Boid>());
        flockSize++;
        SetFlockNeedle();
    }

    public void RemoveBoid(Boid boid)
    {
        boids.Remove(boid);
        Destroy(boid.gameObject);
        flockSize--;
        SetFlockNeedle();
    }

    private void InitialisePredators()
    {
        predators = new List<Predator>();
        for (int i = 0; i < startPredators; i++)
        {
            AddPredator();
        }
    }

    public void AddPredator()
    {
        GameObject predatorObject = Instantiate(predatorPrefab, spawnPoint.position, spawnPoint.rotation);
        predators.Add(predatorObject.GetComponent<Predator>());
        predatorNumber++;
    }

    public void RemovePredator(Predator predator)
    {
        predators.Remove(predator);
        Destroy(predator.gameObject);
        predatorNumber--;
    }

    private void InitialiseGates()
    {
        GameObject[] gateObjects = GameObject.FindGameObjectsWithTag("Gate");
        gates = new Gate[gateObjects.Length];
        for (int i = 0; i < gateObjects.Length; i++)
        {
            gates[i] = gateObjects[i].GetComponent<Gate>();
            gates[i].InitialiseGate();
        }
    }

    void Update()
    {
        if (gameStarted)
        {
            UpdateBoids();
            UpdatePredators();
            UpdateGates();
            UpdateShark();

            if (flockSize > (maxFlock * 0.9))
            {
                // spawn predator every x seconds
                overloadTimer += Time.deltaTime;
                if (overloadTimer > predatorSpawnInterval)
                {
                    overloadTimer = 0.0f;
                    AddPredator();
                }
            }

            sharkTimer += Time.deltaTime;
            if (sharkTimer > sharkSpawnInterval && predators.Count > 0 && !shark.isActive)
            {
                sharkTimer = 0.0f;
                ActivateShark();
            }
        }
        else if (titleClicked)
        {
            MoveCamera();

            if (Vector3.Distance(playCameraPosition.position, mainCamera.position) < 0.1f)
            {
                mainCamera.position = playCameraPosition.position;
                StartGame();
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            startTime = Time.time;
            titleClicked = true;
        }
    }

    private void MoveCamera()
    {
        float distCovered = (Time.time - startTime) * titleScreenPanSpeed;
        float fractionOfJourney = distCovered / journeyLength;
        mainCamera.position = Vector3.Lerp(titleCameraPosition.position, playCameraPosition.position, fractionOfJourney);
    }

    private void StartGame()
    {
        Destroy(titleObjects);
        InitialiseBoids();
        InitialisePredators();
        InitialiseGates();
        ActivateShark();
        bubbleParticle.Play();
        gameStarted = true;
    }

    private void UpdateBoids()
    {
        Shark activeShark = null;
        if (shark.isActive)
        {
            activeShark = shark;
        }

        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].UpdateBoid(boids, player, predators, activeShark);
        }
    }

    private void UpdatePredators()
    {
        for (int i = 0; i < predators.Count; i++)
        {
            predators[i].UpdatePredator(boids);
        }
    }

    private void UpdateGates()
    {
        float currentTime = Time.time;
        for (int i = 0; i < gates.Length; i++)
        {
            if (!gates[i].isMoving)
            {
                // check and deactivate open gates
                if (gates[i].isActive)
                {
                    if (currentTime - gates[i].activeSince > gatesActiveSecs)
                    {
                        gates[i].CloseGate();
                    }
                }
                // randomly activate closed gates
                else
                {
                    if (Random.value < gateActiveProbability)
                    {
                        OpenGate(gates[i]);
                    }

                }
            }
        }
    }

    private void ActivateShark()
    {
        shark.ActivateShark(predators);
    }

    private void UpdateShark()
    {
        shark.UpdateShark(predators);
    }

    private void OpenGate(Gate gate)
    {
        int targetCount;
        int rewardCount;
        bool isGood = Random.value < goodGateProbability;
        if (isGood)
        {
            targetCount = (int)Random.Range(5, 30);
            rewardCount = 5;
        }
        else
        {
            targetCount = (int)Random.Range(1f, 10);
            rewardCount = 1;
        }

        gate.OpenGate(isGood, targetCount, rewardCount);
    }

    private void SetFlockNeedle()
    {
        float fractionOfMax = (float)flockSize / (float)maxFlock;
        float angle = 270f * fractionOfMax;
        flockNeedle.localEulerAngles = new Vector3(0f, 0f, angle + 135);
    }
}
