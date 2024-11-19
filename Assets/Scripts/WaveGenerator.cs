using UnityEngine;
using System.Collections.Generic;

public class WaveGenerator : MonoBehaviour
{
    public WaveManager waveManager;
    public float enemyMultiplier;
    public int startSkeletonCount;
    public int startGoblinCount;
    public int waveCount;
    private Queue<Queue<EnemyRequest>> waveQueue = new Queue<Queue<EnemyRequest>>();
    [Header("Enemy Prefabs")]
    public EnemyPrefab skeleton;
    public EnemyPrefab goblin;

    private void Start()
    {
        GenerateWaves();
        waveManager.StartWaveQueue(waveQueue);
    }

    public void GenerateWaves()
    {
        Queue<EnemyRequest> wave = new Queue<EnemyRequest>();
        for (int i = 0; i <= waveCount; i++)
        {
            wave.Enqueue(new EnemyRequest { enemyPrefab = goblin, count = startGoblinCount * (int)(enemyMultiplier * i) });
            wave.Enqueue(new EnemyRequest { enemyPrefab = skeleton, count = startSkeletonCount * (int)(enemyMultiplier * i) });
            waveQueue.Enqueue(wave);
        }
    }
}

[System.Serializable]
public class EnemyRequest
{
    public EnemyPrefab enemyPrefab;  
    public int count;                
}
