using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Spawn Points")]
    public List<Transform> spawnPoints;

    [Header("Spawning Settings")]
    public float spawnDelay = 2f;
    public float timeBetweenWaves = 3f;

    private Queue<Queue<EnemyRequest>> waveQueue = new Queue<Queue<EnemyRequest>>();
    private Camera mainCamera;
    private bool isWaveInProgress = false;
    private int currentWaveNumber = 0;
    private int activeEnemies = 0;

    public event Action<int> OnWaveStarted;      
    public event Action<int> OnEnemyCountChanged; 
    public event Action OnAllWavesCompleted;    

    private void Start()
    {
        mainCamera = Camera.main;
        OnAllWavesCompleted += HandleAllWavesCompleted;
    }

    private void HandleAllWavesCompleted()
    {
        Debug.Log("You Won! All waves are complete!");
    }

    public void StartWaveQueue(Queue<Queue<EnemyRequest>> waves)
    {
        waveQueue = waves;
        if (!isWaveInProgress)
        {
            StartCoroutine(ProcessWaves());
        }
    }

    private IEnumerator ProcessWaves()
    {
        isWaveInProgress = true;

        while (waveQueue.Count > 0)
        {
            currentWaveNumber++;
            OnWaveStarted?.Invoke(currentWaveNumber);

            Queue<EnemyRequest> currentWave = waveQueue.Dequeue();
            yield return StartCoroutine(SpawnEnemiesInWave(currentWave));

            while (activeEnemies > 0)
            {
                yield return null;
            }

            yield return new WaitForSeconds(timeBetweenWaves);
        }

        isWaveInProgress = false;
        OnAllWavesCompleted?.Invoke(); 
    }

    private IEnumerator SpawnEnemiesInWave(Queue<EnemyRequest> wave)
    {
        while (wave.Count > 0)
        {
            EnemyRequest enemyRequest = wave.Dequeue();

            for (int i = 0; i < enemyRequest.count; i++)
            {
                Transform spawnPoint = GetAvailableSpawnPoint();
                SpawnEnemy(enemyRequest.enemyPrefab, spawnPoint);
                yield return new WaitForSeconds(spawnDelay);
            }
        }
    }

    private void SpawnEnemy(EnemyPrefab enemyRequest, Transform spawnPoint)
    {
        if (enemyRequest != null && enemyRequest.enemyPrefab != null)
        {
            BaseEnemy newEnemy = Instantiate(enemyRequest.enemyPrefab, spawnPoint.position, Quaternion.identity).GetComponent<BaseEnemy>();
            if (newEnemy != null)
            {
                newEnemy.OnEnemyDied += OnEnemyDestroyed; 
            }
            activeEnemies++;
            OnEnemyCountChanged?.Invoke(activeEnemies);
        }
    }

    public void OnEnemyDestroyed(BaseEnemy enemy)
    {
        if (enemy != null)
        {
            enemy.OnEnemyDied -= OnEnemyDestroyed; 
        }
        activeEnemies = Mathf.Max(0, activeEnemies - 1);
        OnEnemyCountChanged?.Invoke(activeEnemies);
    }

    private Transform GetAvailableSpawnPoint()
    {
        foreach (var spawnPoint in spawnPoints)
        {
            if (!IsPointVisible(spawnPoint))
            {
                return spawnPoint;
            }
        }
        return spawnPoints[0]; 
    }

    private bool IsPointVisible(Transform spawnPoint)
    {
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(spawnPoint.position);
        return viewportPoint.x >= 0f && viewportPoint.x <= 1f && viewportPoint.y >= 0f && viewportPoint.y <= 1f;
    }
}
