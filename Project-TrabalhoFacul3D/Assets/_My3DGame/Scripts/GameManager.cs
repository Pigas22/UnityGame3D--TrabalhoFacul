using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI distanceText;

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI highestScoreText;
    [SerializeField] private TextMeshProUGUI highestDistanceText;

    [Header("Display / tuning")]
    [SerializeField, Tooltip("Multiplier to scale world units -> displayed meters. Use <1 to show smaller distance.")] 
    private float distanceScale = 0.5f;

    private int score = 0;
    private static int highestScore = 0;
    private float distanceTravelled = 0f;
    private static float highestDistance = 0f;
    public static GameManager Instance { get; private set; }


    [Header("Prefabs (drag into inspector)")]
    [SerializeField] private List<GameObject> obstaclePrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> housePrefabs = new List<GameObject>();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // primeira execução do jogo? — se sim, zera os high scores e marca a flag
        if (!PlayerPrefs.HasKey("HasRunBefore"))
        {
            PlayerPrefs.SetInt("HighScore", 0);
            PlayerPrefs.SetFloat("HighDistance", 0f);
            PlayerPrefs.SetInt("HasRunBefore", 1);
            PlayerPrefs.Save();
        }

        // carregar highs (persistem entre reinícios de fase)
        highestScore = PlayerPrefs.GetInt("HighScore", 0);
        highestDistance = PlayerPrefs.GetFloat("HighDistance", 0f);

        // zerar score/distância atuais ao iniciar a cena
        score = 0;
        distanceTravelled = 0f;
        
        ShowGameOver(false);
    }

    void Update()
    {
        UpdateScoreUI();
    }


    public void IncrementScoreText() { score++; }
    // adicionar distância em metros (chamar com speed * deltaTime)
    public void AddDistance(float meters) 
    { 
        // aplica escala para reduzir sensação de “muito rápido”
        distanceTravelled += meters * distanceScale; 
    }


    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Score : " + score;
        if (distanceText != null) distanceText.text = "Distance : \n" + Mathf.FloorToInt(distanceTravelled) + " m";
    }

    private static void UpdateResults()
    {
        if (Instance.score > highestScore)
        {
            highestScore = Instance.score;
            PlayerPrefs.SetInt("HighScore", highestScore);
        }

        if (Instance.distanceTravelled > highestDistance)
        {
            highestDistance = Instance.distanceTravelled;
            PlayerPrefs.SetFloat("HighDistance", highestDistance);
        }
        PlayerPrefs.Save();
    }

    public static void PlayerDied()
    {
        Time.timeScale = 0f;
        UpdateResults();

        ShowGameOver();
    }

    private static void ShowGameOver(bool show = true)
    {
        if (Instance.gameOverPanel != null)
        {
            if (show)
            {
                if (Instance.highestScoreText != null)
                    Instance.highestScoreText.text = "" + highestScore;

                if (Instance.highestDistanceText != null)
                    Instance.highestDistanceText.text = "" + Mathf.FloorToInt(highestDistance);
            }
            Instance.gameOverPanel.SetActive(show);
        }
    }

    public static void Restart()
    {
        Time.timeScale = 1f;
        ShowGameOver(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public static void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        ShowGameOver(false);
        SceneManager.LoadScene("MenuPrincipalScene");
    }

    public int GetScore() => score;
    public int GetDistance() => Mathf.FloorToInt(distanceTravelled);

    public GameObject GetRandomObstaclePrefab()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Count == 0) return null;
        return obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];
    }

    public GameObject GetRandomHousePrefab()
    {
        if (housePrefabs == null || housePrefabs.Count == 0) return null;
        return housePrefabs[Random.Range(0, housePrefabs.Count)];
    }
}