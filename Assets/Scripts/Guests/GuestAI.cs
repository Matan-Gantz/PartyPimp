using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public enum GuestNeed
{
    None,
    Thirst,
    Hunger,
    Toilet
}

public enum GuestState
{
    Wandering,
    Paused,
    HeadingToBathroom,
    AutoCollecting,
    Entering,
    Dancing
    }

[RequireComponent(typeof(NavMeshAgent))]
public class GuestAI : MonoBehaviour
{
    [Header("Needs")]
    public GuestNeed currentActiveNeed = GuestNeed.None;
    public float thirstLevel = 0f;
    public float hungerLevel = 0f;
    public float toiletLevel = 0f;
    public float needIncreaseRate = 0.5f;
    public float satisfactionThreshold = 60f;
    public float satisfactionCooldownMin = 15f;
    public float satisfactionCooldownMax = 30f;

    [Header("Crowd Separation")]
    public float repulsionRadius = 2.5f;
    public float repulsionStrength = 5.0f;

    [Header("Visuals")]
    public MeshRenderer bodyRenderer;
    public Color happyColor = Color.green;
    public Color angryColor = Color.red;

    [Header("Need Indicators")]
    public GameObject thirstIndicator;
    public GameObject hungerIndicator;
    public GameObject toiletIndicator;

    [Header("Movement Loop")]
    public float minWanderTime = 4f;
    public float maxWanderTime = 7f;
    public float minPauseTime = 8f;
    public float maxPauseTime = 14f;

    private NavMeshAgent agent;
    private GuestState currentState = GuestState.Paused;
    private float stateTimer;
    private float cooldownTimer;
    private float originalStoppingDistance;
    private float individualRateMultiplier;
    private Transform autoCollectTarget;
    private float defaultSpeed;

    public MusicStation CurrentMusicStation { get; private set; }

    private void Awake()
{
        agent = GetComponent<NavMeshAgent>();
        defaultSpeed = agent.speed;
        originalStoppingDistance = agent.stoppingDistance;
        individualRateMultiplier = Random.Range(0.8f, 1.2f);
        
        if (thirstIndicator) thirstIndicator.SetActive(false);
        if (hungerIndicator) hungerIndicator.SetActive(false);
        if (toiletIndicator) toiletIndicator.SetActive(false);
    }

    private void Start()
    {
        if (currentState != GuestState.Entering)
        {
            StartSatisfactionCooldown();
            EnterState(GuestState.Paused);
        }
    }

    public void SetEntering(Vector3 targetPos)
    {
        EnterState(GuestState.Entering);
        agent.SetDestination(targetPos);
    }

    private void StartSatisfactionCooldown()
    {
        currentActiveNeed = GuestNeed.None;
        thirstLevel = 0; hungerLevel = 0; toiletLevel = 0;
        cooldownTimer = Random.Range(satisfactionCooldownMin, satisfactionCooldownMax);
    }

    private void RollForNewNeed()
    {
        currentActiveNeed = (GuestNeed)Random.Range(1, 4);
        thirstLevel = 0; hungerLevel = 0; toiletLevel = 0;
        Debug.Log($"[Guest] {gameObject.name} new need: {currentActiveNeed}");
    }

    private void Update()
    {
        HandleCooldown();
        UpdateNeeds();
        UpdateState();
        HandleDancingMovement();
        HandleCrowdSeparation();
        UpdateIndicators();
        CheckStationProximity();
    }

    private void HandleDancingMovement()
    {
        if (currentState != GuestState.Dancing || CurrentMusicStation == null) return;

        // Move toward speaker position with soft attraction
        Vector3 targetPos = CurrentMusicStation.transform.position;
        Vector3 toSpeaker = (targetPos - transform.position).normalized;
        toSpeaker.y = 0;

        float dist = Vector3.Distance(transform.position, targetPos);
        
        // Soft attraction force
        if (dist > 1.5f)
        {
            agent.Move(toSpeaker * CurrentMusicStation.attractionStrength * Time.deltaTime);
        }

        // Add small random movement (dance)
        float danceSpeed = 2f;
        float danceAmplitude = 0.15f;
        Vector3 danceOffset = new Vector3(
            Mathf.Sin(Time.time * danceSpeed + GetHashCode()) * danceAmplitude,
            0,
            Mathf.Cos(Time.time * danceSpeed * 0.7f + GetHashCode()) * danceAmplitude
        );
        agent.Move(danceOffset * Time.deltaTime);
    }

    private void HandleCooldown()
{
        if (currentActiveNeed == GuestNeed.None && currentState != GuestState.Entering)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0) RollForNewNeed();
        }
    }

    private void UpdateNeeds()
    {
        if (currentActiveNeed == GuestNeed.None) return;

        float rate = needIncreaseRate * individualRateMultiplier;
        if (currentState == GuestState.Dancing) rate *= 0.3f; // Slow down needs while dancing

        float currentLevel = 0;

        switch (currentActiveNeed)
        {
            case GuestNeed.Thirst: thirstLevel += rate * Time.deltaTime; currentLevel = thirstLevel; break;
            case GuestNeed.Hunger: hungerLevel += rate * Time.deltaTime; currentLevel = hungerLevel; break;
            case GuestNeed.Toilet: toiletLevel += rate * Time.deltaTime; currentLevel = toiletLevel; break;
        }

        if (currentLevel > satisfactionThreshold)
            bodyRenderer.material.color = Color.Lerp(happyColor, angryColor, (currentLevel - satisfactionThreshold) / (100 - satisfactionThreshold));
        else
            bodyRenderer.material.color = happyColor;
    }

    private void HandleCrowdSeparation()
    {
        PlayerController player = GameObject.FindAnyObjectByType<PlayerController>();
        if (player == null) return;

        Vector3 toPlayer = transform.position - player.transform.position;
        toPlayer.y = 0;
        float dist = toPlayer.magnitude;

        if (dist < repulsionRadius)
        {
            float weight = (currentActiveNeed == GuestNeed.None) ? 1.0f : 0.5f;
            float strength = (1f - (dist / repulsionRadius)) * repulsionStrength * weight;
            Vector3 repulsionDir = toPlayer.normalized;
            agent.Move(repulsionDir * strength * Time.deltaTime);
        }
    }

    private void UpdateState()
    {
        if (!agent.isOnNavMesh) return;

        CheckForAutoCollection();

        stateTimer -= Time.deltaTime;

        bool toiletCritical = currentActiveNeed == GuestNeed.Toilet && toiletLevel > 95f;

        switch (currentState)
        {
            case GuestState.Entering:
                if (!agent.pathPending && agent.remainingDistance < 0.5f) EnterState(GuestState.Wandering);
                break;

            case GuestState.Paused:
                if (toiletCritical) EnterState(GuestState.HeadingToBathroom);
                else if (stateTimer <= 0) EnterState(GuestState.Wandering);
                break;

            case GuestState.Wandering:
                if (toiletCritical) EnterState(GuestState.HeadingToBathroom);
                else if (stateTimer <= 0 || (agent.remainingDistance < 0.5f && !agent.pathPending)) EnterState(GuestState.Paused);
                break;

            case GuestState.HeadingToBathroom:
                if (!agent.pathPending && agent.remainingDistance < 1.0f)
                {
                    BathroomStation bathroom = GameObject.FindAnyObjectByType<BathroomStation>();
                    if (bathroom != null && Vector3.Distance(transform.position, bathroom.transform.position) < 2.5f)
                        FulfillToiletNeed();
                }
                break;

            case GuestState.AutoCollecting:
                if (autoCollectTarget != null)
                {
                    agent.SetDestination(autoCollectTarget.position);
                    if (Vector3.Distance(transform.position, autoCollectTarget.position) < 1.5f)
                    {
                        PlayerController player = autoCollectTarget.GetComponent<PlayerController>();
                        if (player != null && player.HasItem())
                        {
                            Item item = player.GetComponentInChildren<Item>();
                            if (item != null && IsMatchingNeed(item.itemType)) ProcessItem(item);
                        }
                    }
                }
                else EnterState(GuestState.Paused);
                break;

            case GuestState.Dancing:
                if (CurrentMusicStation == null || IsInUrgentNeed())
                {
                    if (CurrentMusicStation != null) CurrentMusicStation.UnregisterGuest(this);
                    AssignToMusicStation(null);
                }
                break;
            }
            }

            private void EnterState(GuestState newState)
            {
                    // If we were dancing and moving to a new state, unregister
        if (currentState == GuestState.Dancing && newState != GuestState.Dancing)
        {
            if (CurrentMusicStation != null)
            {
                CurrentMusicStation.UnregisterGuest(this);
                CurrentMusicStation = null;
            }
        }

        currentState = newState;
        agent.speed = defaultSpeed;
            switch (newState)
            {
            case GuestState.Paused:
                stateTimer = Random.Range(minPauseTime, maxPauseTime);
                agent.ResetPath();
                break;
            case GuestState.Wandering:
                stateTimer = Random.Range(minWanderTime, maxWanderTime);
                SetOrganicDestination();
                break;
            case GuestState.HeadingToBathroom:
                GoToBathroom();
                break;
            case GuestState.AutoCollecting:
                agent.stoppingDistance = 1.0f;
                break;
            case GuestState.Dancing:
                agent.speed = defaultSpeed * 0.7f;
                if (CurrentMusicStation != null)
                {
                    Vector3 danceSpot = CurrentMusicStation.transform.position + Random.insideUnitSphere * 2.5f;
                    danceSpot.y = CurrentMusicStation.transform.position.y;
                    agent.SetDestination(danceSpot);
                }
                break;
            }
            }

    private void SetOrganicDestination()
    {
        float roll = Random.value;
        Vector3 targetPos;

        if (roll < 0.40f) targetPos = FindSocialSpot();
        else if (roll < 0.50f) targetPos = FindStationSpot();
        else targetPos = transform.position + Random.insideUnitSphere * 12f;

        targetPos.x = Mathf.Clamp(targetPos.x, -9f, 9f);
        targetPos.z = Mathf.Clamp(targetPos.z, -9f, 9f);
        targetPos.y = transform.position.y;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 10f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private Vector3 FindSocialSpot()
    {
        GuestAI[] guests = GameObject.FindObjectsByType<GuestAI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        List<Vector3> validClusters = new List<Vector3>();

        foreach (var guest in guests)
        {
            if (guest != this && guest.currentState == GuestState.Paused)
            {
                int clusterCount = 0;
                foreach (var other in guests)
                {
                    if (other != guest && Vector3.Distance(guest.transform.position, other.transform.position) < 3f)
                        clusterCount++;
                }
                if (clusterCount >= 1 && clusterCount < 4) 
                    validClusters.Add(guest.transform.position);
            }
        }

        if (validClusters.Count > 0) 
            return validClusters[Random.Range(0, validClusters.Count)] + (Random.insideUnitSphere * 2f);
        
        return transform.position + Random.insideUnitSphere * 8f;
    }

    private Vector3 FindStationSpot()
    {
        Station[] stations = GameObject.FindObjectsByType<Station>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (stations.Length > 0)
        {
            Station s = stations[Random.Range(0, stations.Length)];
            return s.transform.position + (Random.insideUnitSphere * 3f);
        }
        return transform.position + Random.insideUnitSphere * 8f;
    }

    private void GoToBathroom()
    {
        BathroomStation bathroom = GameObject.FindAnyObjectByType<BathroomStation>();
        if (bathroom != null)
        {
            agent.stoppingDistance = 0.5f;
            agent.SetDestination(bathroom.transform.position);
        }
    }

    private void CheckForAutoCollection()
    {
        if (currentActiveNeed == GuestNeed.None || currentActiveNeed == GuestNeed.Toilet) return;

        PlayerController player = GameObject.FindAnyObjectByType<PlayerController>();
        if (player != null && player.HasItem())
        {
            ItemType playerItemType = player.GetHeldItemType();
            if (IsMatchingNeed(playerItemType) && IsNearestMatching(player, playerItemType))
            {
                if (currentState != GuestState.AutoCollecting || autoCollectTarget != player.transform)
                {
                    autoCollectTarget = player.transform;
                    EnterState(GuestState.AutoCollecting);
                }
            }
            else if (currentState == GuestState.AutoCollecting) EnterState(GuestState.Paused);
        }
        else if (currentState == GuestState.AutoCollecting) EnterState(GuestState.Paused);
    }

    public bool IsMatchingNeed(ItemType type)
    {
        if (type == ItemType.Drink && currentActiveNeed == GuestNeed.Thirst && thirstLevel > 30f) return true;
        if (type == ItemType.Food && currentActiveNeed == GuestNeed.Hunger && hungerLevel > 30f) return true;
        return false;
    }

    private bool IsNearestMatching(PlayerController player, ItemType type)
    {
        GuestAI[] allGuests = GameObject.FindObjectsByType<GuestAI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        float myDist = Vector3.Distance(transform.position, player.transform.position);
        foreach (var guest in allGuests)
        {
            if (guest == this) continue;
            if (guest.IsMatchingNeed(type) && Vector3.Distance(guest.transform.position, player.transform.position) < myDist)
                return false;
        }
        return true;
    }

    private void UpdateIndicators()
    {
        SetActiveIndicator(null);
        if (currentActiveNeed == GuestNeed.None) return;

        float level = (currentActiveNeed == GuestNeed.Thirst) ? thirstLevel : (currentActiveNeed == GuestNeed.Hunger) ? hungerLevel : toiletLevel;
        if (level > satisfactionThreshold)
        {
            if (currentActiveNeed == GuestNeed.Thirst) SetActiveIndicator(thirstIndicator);
            else if (currentActiveNeed == GuestNeed.Hunger) SetActiveIndicator(hungerIndicator);
            else SetActiveIndicator(toiletIndicator);
        }
    }

    private void SetActiveIndicator(GameObject active)
    {
        if (thirstIndicator) thirstIndicator.SetActive(thirstIndicator == active);
        if (hungerIndicator) hungerIndicator.SetActive(hungerIndicator == active);
        if (toiletIndicator) toiletIndicator.SetActive(toiletIndicator == active);
    }

    private void CheckStationProximity()
    {
        if (currentActiveNeed == GuestNeed.None || currentActiveNeed == GuestNeed.Toilet || currentState == GuestState.Dancing) return;

        Station[] stations = GameObject.FindObjectsByType<Station>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
foreach (var s in stations)
        {
            if (Vector3.Distance(transform.position, s.transform.position) < 1.2f) // Tightened from 1.8f
            {
if ((currentActiveNeed == GuestNeed.Thirst && s.stationName.Contains("Drink")) ||
                    (currentActiveNeed == GuestNeed.Hunger && s.stationName.Contains("Pizza")))
                {
                    Debug.Log($"[Guest] {gameObject.name} auto-satisfied at {s.stationName}");
                    ProcessItem(null);
                    return;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Item item = other.GetComponent<Item>();
        if (item == null) item = other.GetComponentInParent<Item>();
        if (item != null && IsMatchingNeed(item.itemType)) ProcessItem(item);
    }

    public void ProcessItem(Item item)
    {
        if (item != null && item.transform.parent != null)
        {
            PlayerController player = item.GetComponentInParent<PlayerController>();
            if (player != null) player.NotifyItemCollected();
        }

        Debug.Log($"[Guest] Satisfied {currentActiveNeed}");
        if (GameManager.Instance != null) GameManager.Instance.OnGuestSatisfied();
        if (item != null) Destroy(item.gameObject);
        StartSatisfactionCooldown();
        EnterState(GuestState.Paused);
    }

    public void FulfillToiletNeed()
    {
        if (currentActiveNeed != GuestNeed.Toilet || toiletLevel < 30f) return;
        Debug.Log("[Guest] Toilet need fulfilled!");
        agent.stoppingDistance = originalStoppingDistance;
        StartSatisfactionCooldown();
        EnterState(GuestState.Paused);
        if (GameManager.Instance != null) GameManager.Instance.OnGuestSatisfied();
    }

    public bool IsInUrgentNeed()
    {
        if (currentActiveNeed == GuestNeed.None) return false;
        float level = (currentActiveNeed == GuestNeed.Thirst) ? thirstLevel : 
                      (currentActiveNeed == GuestNeed.Hunger) ? hungerLevel : toiletLevel;
        return level > 75f; // Lowered from 90f
    }

    public void AssignToMusicStation(MusicStation station)
    {
        CurrentMusicStation = station;
        if (station != null)
        {
            EnterState(GuestState.Dancing);
        }
        else if (currentState == GuestState.Dancing)
        {
            EnterState(GuestState.Paused);
        }
    }
}
