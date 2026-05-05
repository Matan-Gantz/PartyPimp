using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public float gameDuration = 300f; // 5 minutes
    public GameObject guestPrefab;
    public Transform doorEntrance;
    public float initialSpawnCount = 5;
    public float spawnInterval = 20f;

    [Header("Game State")]
    private float timer;
    private bool isGameActive = false;
    private int satisfiedGuests = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        timer = gameDuration;
        isGameActive = true;
        satisfiedGuests = 0;
        Debug.Log("Game Started! 5:00 on the clock.");
        
        for (int i = 0; i < initialSpawnCount; i++)
        {
            SpawnGuest();
        }
        
        StartCoroutine(SpawnGuestRoutine());
    }

    private void Update()
    {
        if (!isGameActive) return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            EndGame();
        }
    }

    private IEnumerator SpawnGuestRoutine()
    {
        yield return new WaitForSeconds(spawnInterval);

        while (isGameActive)
        {
            SpawnGuest();
            yield return new WaitForSeconds(Random.Range(spawnInterval * 0.8f, spawnInterval * 1.2f));
        }
    }

    private void SpawnGuest()
    {
        if (guestPrefab == null) return;

        Vector3 spawnPos = Vector3.zero;
        Vector3 walkToPos = Vector3.zero;

        if (doorEntrance != null)
        {
            spawnPos = doorEntrance.position;
            walkToPos = doorEntrance.position + doorEntrance.forward * 3f;
        }
        else
        {
            float x = Random.Range(-9f, 9f);
            float z = Random.Range(-9f, 9f);
            spawnPos = new Vector3(x, 1f, z);
            walkToPos = spawnPos;
        }

        GameObject guestObj = Instantiate(guestPrefab, spawnPos, Quaternion.identity);
        GuestAI ai = guestObj.GetComponent<GuestAI>();
        if (ai != null && doorEntrance != null)
        {
            ai.SetEntering(walkToPos);
        }

        Debug.Log("A new guest has arrived through the door!");
    }

    public void OnGuestSatisfied()
    {
        satisfiedGuests++;
        Debug.Log($"Guest satisfied! Total: {satisfiedGuests}");
    }

    private void EndGame()
    {
        isGameActive = false;
        Debug.Log($"Game Over! You kept {satisfiedGuests} guests happy.");
    }

    public float GetRemainingTime() => Mathf.Max(0, timer);
}