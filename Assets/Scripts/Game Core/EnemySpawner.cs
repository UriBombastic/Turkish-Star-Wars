using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyPack
{
    public GameObject prefab;
    public int weight;
    public GameObject spawnParticles;
}

public enum SpawnMode
{
    ENDLESS, // Spawns enemies indefinitely. (Clever idea but I don't think I ever used it.)
    FINITE,  // Spawns a set amount of enemies.
    DEPENDENT // Spawns endlessly, but only when called upon by an outside source.
};

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    public EnemyPack[] enemies;

    public EnemyPack boss;

    public int maxEnemies;
    public int initialEnemies;
    public float timeBetweenSpawns = 1.0f;
    public float spawnRandomRadius = 0f;
    private float timeElapsedBetweenSpawns;
    public int enemiesToDefeat;
    public int enemiesSpawned = 0;

    public Transform[] spawnLocations;

    public SpawnMode spawnMode = SpawnMode.FINITE;
    public Transform enemyParentTransform;

   // private int lastKillCount = 0;
  //  private int enemiesAlive;

    public void Start()
    {
        for (int i = 0; i < initialEnemies; i++)
            SpawnEnemy();
    }

    public void Update()
    {
        if (spawnMode != SpawnMode.DEPENDENT)
        {
            timeElapsedBetweenSpawns += Time.deltaTime;
            if (timeElapsedBetweenSpawns >= timeBetweenSpawns)
            {
                int enemiesAlive = CountEnemiesAlive();
                if (enemiesAlive < maxEnemies)
                    SpawnEnemy();
                timeElapsedBetweenSpawns = 0; //reset timer
            }
        }

    }

    private int CountEnemiesAlive()
    {
        return FindObjectsOfType<Enemy>().Length; 
    }

    public void SpawnEnemy()
    {
        enemiesSpawned++;
        Transform selectedTransform = SelectTransform();
        EnemyPack selectedEnemy = (BossConditions())? boss : SelectEnemy();
        GameObject spawnedEnemy = Instantiate(selectedEnemy.prefab, selectedTransform.position, selectedTransform.rotation); //spawn enemies
        spawnedEnemy.transform.Translate(RandomDistance(), RandomDistance(), RandomDistance()); //so that it's not right on spawn transform
        if (enemyParentTransform != null)
            spawnedEnemy.transform.SetParent(enemyParentTransform);
        if(selectedEnemy.spawnParticles!=null)
            Instantiate(selectedEnemy.spawnParticles, selectedTransform.position, selectedTransform.rotation); //spawn particles

        CheckSpawnEnd();

    }

    private float RandomDistance()
    {
        return Random.Range(-spawnRandomRadius, spawnRandomRadius);
    }

    private Transform SelectTransform()
    {
        return spawnLocations[Random.Range(0, spawnLocations.Length)];
    }

    private bool BossConditions()
    {
        return (spawnMode == SpawnMode.FINITE && boss.prefab != null && enemiesSpawned == enemiesToDefeat);
    }

    private EnemyPack SelectEnemy()
    {
        int totalWeight = 0;
        for(int i  = 0; i < enemies.Length; i++)
        {
            totalWeight += enemies[i].weight;
        }

        int selectedWeight = Random.Range(0, totalWeight);
        for(int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i].weight >= selectedWeight) //enemy is within range of random weighted selection
                return enemies[i];
            else
                selectedWeight -= enemies[i].weight;
        }
        Debug.LogError("My math was off; enemy is null"); //hopefully unreachable
        return null;
    }

    void CheckSpawnEnd()
    {
        if (spawnMode == SpawnMode.FINITE && enemiesSpawned >= enemiesToDefeat)
            gameObject.SetActive(false);
    }

}
