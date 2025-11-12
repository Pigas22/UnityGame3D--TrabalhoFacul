using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.Collections;
using NUnit.Framework.Interfaces;
using UnityEngine.UIElements;

public class GroundSpawner : MonoBehaviour
{
    [SerializeField] private GameObject groundTilePrefab;
    [SerializeField] private int initialTiles = 10;
    [SerializeField] private float forwardSpeed = 20f;
    [Header("Houses (side scenery)")]
    [SerializeField, Tooltip("X offset from road center to place houses (ex.: 15)")]
    private float houseOffsetX = 30f;
    [SerializeField, Tooltip("How many houses per side per tile (distributed along Z)")]
    private int housesPerSide = 1;
    [SerializeField, Tooltip("Y position for house placement")]
    private float houseY = 0f;
    [SerializeField] private bool enableTimeIncrease = true;
    [SerializeField] private float speedIncreaseInterval = 5f; // segundos entre aumentos
    [SerializeField] private float speedIncreaseMultiplier = 0.03f; // use menor valor (ex: 0.03 = +3%)
    [SerializeField] private bool useAdditiveSpeedIncrease = true;
    [SerializeField, Tooltip("When additive mode is enabled, add this many units to forwardSpeed each step.")]
    private float speedIncreaseAmount = 1f; // +10% a cada intervalo
    [Header("Speed growth (progress)")]
    [SerializeField] private bool enableProgressIncrease = false;
    [SerializeField] private int tilesPerIncrease = 5; // a cada N tiles passados
    [SerializeField] private float progressIncreaseMultiplier = 0.15f;
    [Header("Limits")]
    [SerializeField] private float maxForwardSpeed = 80f;
    private int tilesPassed = 0;
    private float tileLength;
    private Queue<GroundTile> activeTiles = new Queue<GroundTile>();

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
        if (enableTimeIncrease) StartCoroutine(TimeIncreaseRoutine());
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

        // Atualiza a distância percorrida com base na velocidade real (metros por segundo)
        if (GameManager.Instance != null)
            GameManager.Instance.AddDistance(forwardSpeed * Time.fixedDeltaTime);
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
        while (activeTiles.Count > 0 && activeTiles.Peek().transform.position.z < (-tileLength * 1.1f))
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
        // Obstáculos variados usando GameManager list
        int obstacleCount = Random.Range(0, 3);
        for (int i = 0; i < obstacleCount; i++)
        {
            int lane = Random.Range(0, 3);
            float xPosition = (lane - 1) * 5f;
            float zPosition = tile.transform.position.z + Random.Range(1f, tileLength - 1f) - 15f;
            Vector3 spawnPos = new Vector3(xPosition, 0.5f, zPosition);
            SpawnObject(tile, "Obstacle", spawnPos);
        }

        // Moedas (mantém lógica anterior)
        int coinCount = Random.Range(2, 5);
        for (int i = 0; i < coinCount; i++)
        {
            int lane = Random.Range(0, 3);
            float xPosition = (lane - 1) * 5f;
            float zPosition = tile.transform.position.z + (i + 0.5f) * 2f;
            Vector3 spawnPos = new Vector3(xPosition, 0.5f, zPosition);
            SpawnObject(tile, "Coin", spawnPos);
        }

        // Casas (cenário) — opcional: uma cada tile nas laterais
        if (GameManager.Instance != null && housesPerSide > 0)
        {
            float tileStartZ = tile.transform.position.z;
            // loop por lado (-1 = esquerda, +1 = direita)
            for (int side = -1; side <= 1; side += 2)
            {
                // loop para distribuir housesPerSide casas ao longo do tile
                for (int i = 0; i < housesPerSide; i++)
                {
                    float t = (i + 1f) / (housesPerSide + 1f); // normalizado 0..1
                    float houseZ = tileStartZ + t * tileLength;
                    float houseX = side * Mathf.Abs(houseOffsetX); // usa o offset configurável
                    Vector3 housePos = new(houseX, houseY, houseZ);
                    SpawnObject(tile, "House", housePos);
                }
            }
        }
    }

    private void SpawnObject(GameObject parentTile, string objectType, Vector3 position)
    {
        GameObject prefab = null;

        if (objectType == "Obstacle" && GameManager.Instance != null)
        {
            prefab = GameManager.Instance.GetRandomObstaclePrefab();
        }
        else if (objectType == "House" && GameManager.Instance != null)
        {
            prefab = GameManager.Instance.GetRandomHousePrefab();
        }
        else if (objectType == "Coin")
        {
            // keeps using Resources fallback if you still have coin prefab there
            prefab = Resources.Load<GameObject>($"Prefabs/{objectType}");
        }

        if (prefab != null)
        {
            if (objectType == "Coin")
            {
                GameObject newObject = Instantiate(prefab, new(position.x, position.y - 0.5f, position.z), Quaternion.Euler(0, 0, 0), parentTile.transform);
                // optional random scale or slight Y offset to avoid clipping
                float s = Random.Range(0.9f, 1.1f);
                newObject.transform.localScale *= s;
            }
            else if (objectType == "Obstacle")
            {
                Quaternion rot = Quaternion.Euler(0f, 180f, 0f);
                GameObject newObject = Instantiate(prefab, new(position.x, position.y - 0.5f, position.z), rot, parentTile.transform);

                newObject.AddComponent<BoxCollider>();
                newObject.AddComponent<Obstacle>();
            }
            else if (objectType == "House")
            {
                float yAxe = 0f;
                if (position.x > 0) yAxe = -90f;
                else if (position.x < 0) yAxe = 90f; 
                
                Quaternion rot = Quaternion.Euler(0f, yAxe, 0f);
                Instantiate(prefab, position, rot, parentTile.transform);
            }
        }
        else
        {
            Debug.LogWarning($"Prefab for {objectType} not found (GameManager lists or Resources).");
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

        if (useAdditiveSpeedIncrease)
        {
            forwardSpeed = Mathf.Min(forwardSpeed + speedIncreaseAmount, maxForwardSpeed);
        }
        else
        {
            forwardSpeed = Mathf.Min(forwardSpeed * (1f + multiplier), maxForwardSpeed);
        }

        // opcional: Debug
        Debug.Log($"Ground speed increased to {forwardSpeed:F2}");
    }
}