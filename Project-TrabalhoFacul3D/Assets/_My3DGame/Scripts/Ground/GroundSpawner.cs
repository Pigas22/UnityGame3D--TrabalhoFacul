using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.Collections;

public class GroundSpawner : MonoBehaviour
{
    [SerializeField] private GameObject groundTilePrefab;
    [SerializeField] private int initialTiles = 10;
    [SerializeField] private float forwardSpeed = 20f;
    [SerializeField] private bool enableTimeIncrease = true;
    [SerializeField] private float speedIncreaseInterval = 5f; // segundos entre aumentos
    [SerializeField] private float speedIncreaseMultiplier = 0.10f; // +10% a cada intervalo
    [Header("Speed growth (progress)")]
    [SerializeField] private bool enableProgressIncrease = false;
    [SerializeField] private int tilesPerIncrease = 5; // a cada N tiles passados
    [SerializeField] private float progressIncreaseMultiplier = 0.15f;

    [Header("Limits")]
    [SerializeField] private float maxForwardSpeed = 80f;

    private int tilesPassed = 0;
    private float tileLength;

    private Queue<GroundTile> activeTiles = new Queue<GroundTile>();
    private Coroutine timeIncreaseCoroutine;

    private void Awake()
    {
        tileLength = groundTilePrefab.GetComponent<GroundTile>().GetTileLength();
    }

    private void Start()
    {
        if (groundTilePrefab == null)
        {
            Debug.LogError("Ground Tile Prefab not assigned in GroundSpawner!");
            return;
        }

        // Spawn inicial de tiles
        for (int i = 0; i < initialTiles; i++)
        {
            if (i == 0) SpawnTile(i * tileLength, true);
            else SpawnTile(i * tileLength);
        }        
        // inicia o "clock" de tempo apenas uma vez
        if (enableTimeIncrease)
            timeIncreaseCoroutine = StartCoroutine(TimeIncreaseRoutine());
    }

    private void Update()
    {
        // Remove tiles que saíram da câmera
        RemoveOffscreenTileAndSpawnNextOne();
    }

    private void FixedUpdate()
    {
        // Move todos os tiles para trás (ilusão de movimento)
        MoveAllTiles();

        // Atualiza a distância percorrida sem inteferência da taxa de quadros
        GameManager.Instance.IncrementDistanceText();
    }

    private void MoveAllTiles()
    {
        foreach (GroundTile tile in activeTiles)
        {
            Vector3 newPos = tile.transform.position;
            newPos.z -= forwardSpeed * Time.deltaTime;
            tile.transform.position = newPos;
        }
    }

    private void RemoveOffscreenTileAndSpawnNextOne()
    {
        // Remove tiles que ficaram muito atrás
        while (activeTiles.Count > 0 && activeTiles.Peek().transform.position.z < -tileLength)
        {
            GroundTile tile = activeTiles.Dequeue();
            Destroy(tile.gameObject);
            SpawnTile((initialTiles - 1) * tileLength);

            // Contabiliza tile passado e aplica aumento por progresso
            tilesPassed++;
            if (enableProgressIncrease && tilesPassed % tilesPerIncrease == 0)
            {
                IncreaseSpeedBy(progressIncreaseMultiplier);
            }
        }
    }

    private void SpawnTile(float zPosition, bool isStartingTile = false)
    {
        GameObject newTileObject = Instantiate(
            groundTilePrefab,
            new Vector3(0, 0, zPosition),
            Quaternion.identity
        );

        GroundTile newTile = newTileObject.GetComponent<GroundTile>();

        if (newTile == null)
        {
            Debug.LogError("Ground Tile Prefab doesn't have GroundTile script!");
            Destroy(newTileObject);
            return;
        }

        activeTiles.Enqueue(newTile);

        // Spawn obstáculos e moedas
        if (!isStartingTile) SpawnObstaclesAndCoins(newTileObject);
    }

    private void SpawnObstaclesAndCoins(GameObject tile)
    {
        // Spawn obstáculos aleatoriamente
        int obstacleCount = Random.Range(0, 3);
        for (int i = 0; i < obstacleCount; i++)
        {
            int lane = Random.Range(0, 3);
            float xPosition = (lane - 1) * 5f;
            float zPosition = tile.transform.position.z + (i + 1) * 2.5f;

            SpawnObject(tile, "Obstacle", new Vector3(xPosition, 1f, zPosition));
        }

        // Spawn moedas aleatoriamente
        int coinCount = Random.Range(2, 5);
        for (int i = 0; i < coinCount; i++)
        {
            int lane = Random.Range(0, 3);
            float xPosition = (lane - 1) * 5f;
            float zPosition = tile.transform.position.z + (i + 0.5f) * 2f;

            SpawnObject(tile, "Coin", new Vector3(xPosition, 0.5f, zPosition));
        }
    }

    private void SpawnObject(GameObject parentTile, string objectType, Vector3 position)
    {
        GameObject prefab = Resources.Load<GameObject>($"Prefabs/{objectType}");

        if (prefab != null)
        {
            GameObject newObject = Instantiate(prefab, position, Quaternion.identity);
            newObject.transform.parent = parentTile.transform;
        }
        else
        {
            Debug.LogWarning($"Prefab for {objectType} not found in Resources/Prefabs/");
        }
    }

    public float GetForwardSpeed() => forwardSpeed;

    private IEnumerator TimeIncreaseRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(speedIncreaseInterval);
            IncreaseSpeedBy(speedIncreaseMultiplier);
        }
    }

    private void IncreaseSpeedBy(float multiplier)
    {
        forwardSpeed += forwardSpeed * multiplier;
        forwardSpeed = Mathf.Min(forwardSpeed, maxForwardSpeed);
        // opcional: Debug
        Debug.Log($"Ground speed increased to {forwardSpeed:F2}");
    }
}