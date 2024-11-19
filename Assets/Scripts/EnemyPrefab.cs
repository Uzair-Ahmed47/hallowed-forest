using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "EnemyPrefab", menuName = "ScriptableObjects/EnemyPrefab", order = 1)]
public class EnemyPrefab : ScriptableObject
{
    public GameObject enemyPrefab;
    public string type;
}