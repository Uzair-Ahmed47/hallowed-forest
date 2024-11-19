using UnityEngine;
using TMPro;

public class WaveUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text enemyText;
    [SerializeField] private WaveManager waveManager;

    private void Start()
    {
        if (waveManager != null)
        {
            waveManager.OnWaveStarted += UpdateWaveText;
            waveManager.OnEnemyCountChanged += UpdateEnemyCountText;
        }
    }

    private void OnDestroy()
    {
        if (waveManager != null)
        {
            waveManager.OnWaveStarted -= UpdateWaveText;
            waveManager.OnEnemyCountChanged -= UpdateEnemyCountText;
        }
    }

    private void UpdateWaveText(int waveNumber)
    {
        waveText.text = $"Wave: {waveNumber}";
    }

    private void UpdateEnemyCountText(int enemyCount)
    {
        enemyText.text = $"Enemies Left: {enemyCount}";
    }
}
