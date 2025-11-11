using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private PlayerMovement playerMovement;

    private int score = 0;
    private static int highestScore = 0;
    private int distance = 0;
    private static int highestDistance = 0;
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        UpdateScoreUI();
    }

    public void IncrementScoreText() { score++; }
    public void IncrementDistanceText() { distance++; }

    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = "Score : " + score;

        if (distanceText != null) distanceText.text = "Distance : " + distance + " m";
    }

    public static void PlayerDied()
    {
        Time.timeScale = 0f;
        UpdateResults();

        //ShowGameOver();
        Restart();
    }

    private static void UpdateResults()
    {
        if (Instance.score > highestScore) { highestScore = Instance.score; }

        if (Instance.distance > highestDistance) {  highestDistance = Instance.distance; }
    }

    public static void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public int GetScore() => score;
    public int GetDistance() => distance;
}