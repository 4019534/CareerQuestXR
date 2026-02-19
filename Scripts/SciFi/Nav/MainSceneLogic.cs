using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainSceneLogic : MonoBehaviour
{
    public static MainSceneLogic Instance;
    public TMP_Text levelInstructionText;
    public List<LevelRule> rules = new List<LevelRule>();
    public LevelRule currentRule;
    public TMP_Text timerText;
    public TMP_Text targetIndicatorText;
    public TMP_Text attemptsText;
    public GameObject targetPrefab;
    public Transform[] spawnPoints;
    public Transform[] targetPoints;
    public Transform player;
    public int totalLevels = 5;
    private int currentLevel = 0;
    public int attempts = 1;
    public float timer = 0f;
    private GameObject currentTarget;
    private bool levelActive = false;
    public bool gameStarted = false; // 🆕
    [Header("Progress Tracking")]
    private int totalMoves = 0;
    private int correctMoves = 0;
    private int levelsCompleted = 0;
    private float finalTime = 0f;
    public int startingLevel = 0;
    public GameObject scoreboardPanel, surveyPanel, startingPanel, gamePanel;
    private Dictionary<int, LevelResult> allLevelResults = new Dictionary<int, LevelResult>();
    private string currentUsername;

    void Awake() => Instance = this;

    void Start()
    {
        currentUsername = PlayerPrefs.GetString("LoggedInUser", "");
        timer = 0f;
        timerText.text = "Time: 0.0s";
    }

    void Update()
    {
        if (gameStarted)
        {
            timer += Time.deltaTime;
            timerText.text = $"Time: {FormatTime(timer)}";
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int hours = Mathf.FloorToInt(timeInSeconds / 3600f);
        int minutes = Mathf.FloorToInt((timeInSeconds % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    public void BeginGame()
    {
        gameStarted = true;
        timer = 0f;
        timerText.text = "Time: 0.0s";
    }

    public void OnLevelComplete()
    {
        RecordLevelComplete();
    }

    public void StartNextLevel()
    {
        if (currentTarget != null)
            Destroy(currentTarget);

        if (currentLevel >= totalLevels)
        {
            ShowSurvey();
            return;
        }


        currentLevel++;
        RecordLevelComplete();
        levelActive = true;

        currentRule = rules[currentLevel]; 
        levelInstructionText.text = currentRule.instructionText;
        attemptsText.text = $"Attempts: {attempts}";
        targetIndicatorText.text = $"Reach Target {currentLevel}/{totalLevels}";

        Transform targetPos = targetPoints[Random.Range(0, targetPoints.Length)];
        currentTarget = Instantiate(targetPrefab, targetPos.position, Quaternion.identity);
        currentTarget.GetComponent<TargetZone>().logic = this;
    }

    public void OnTargetReached()
    {
        levelActive = false;
        StartNextLevel();
    }

    public void AddAttempt()
    {
        attempts++;
        attemptsText.text = $"Attempts: {attempts}";
    }
    public void RecordLevelResults(int currentLevelInt, float timerF, int attemptsInt, int totalSteps, int totalWrongSteps)
    {
        int levelPlusOne = currentLevelInt + 1;
        float totalPercentage = (float)levelPlusOne / (float)totalLevels;
    
        var levelData = new LevelResult
        {
            level = levelPlusOne,
            timeTaken = timerF,
            attempts = attemptsInt,
            percentageCompleted = totalPercentage,
            totatSteps = totalSteps,
            totalWrongSteps = totalWrongSteps,
            totalPathSteps = totalSteps - totalWrongSteps
        };

        allLevelResults[currentLevelInt] = levelData;
        PlayerPrefs.SetInt($"{currentUsername}_LevelCompleted", currentLevelInt);
        PlayerPrefs.Save();
        gamePanel.SetActive(false);
    }

    public void RecordMove(bool wasCorrect)
    {
        totalMoves++;
        if (wasCorrect) correctMoves++;
    }

    public void RecordLevelComplete()
    {
        levelsCompleted++;
    }

    public Dictionary<string, object> GetTestResults()
    {
        return new Dictionary<string, object>
        {
            { "timeTaken", finalTime },
            { "correctMoves", correctMoves },
            { "totalMoves", totalMoves },
            { "levelsCompleted", levelsCompleted },
            { "attempts", attempts }
        };
    }

    public void ShowScoreboard(Dictionary<string, string> surveyAnswers)
    {

        scoreboardPanel.SetActive(true);

        float overallTotalTime = 0f;
        int totalAttempts = 0;
        int completedLevels = allLevelResults.Count;


        foreach (var entry in allLevelResults)
        {
            LevelResult levelData = entry.Value;
            if (levelData != null)
            {
                overallTotalTime += levelData.timeTaken;
                totalAttempts += levelData.attempts;
            }
        }

        float avgAttempts = completedLevels > 0 ? (float)totalAttempts / completedLevels : 0f;
        float overallTotalPercentage = completedLevels > 0 ? (float)completedLevels / (float)totalLevels * 100 : 0f;
        string score = $"Total Score: {completedLevels} / {totalLevels}";

        var allLevelData = new Dictionary<string, object>
        {
            { "testName", "NavigationGrid_test2" },
            { "overallTotalTime", overallTotalTime },
            { "overallTotalPercentage", overallTotalPercentage },
            { "avgAttempts", avgAttempts },
            { "totalLevelsCompleted", completedLevels },
            { "totalLevels", totalLevels },
            { "allLevelResults", allLevelResults },
            { "surveyAnswers", surveyAnswers }
        };

        ScoreboardRenderer scoreboard = FindObjectOfType<ScoreboardRenderer>();
        scoreboard.DisplayResults("NavigationGrid_test2", allLevelData, overallTotalTime, score, surveyAnswers, null, null);
        surveyPanel.SetActive(false);
    }

    public void QuitToSurvey()
    {
        if (GridMovementManager.Instance.currentLevelIndex == 0)
        {
            PlayerPrefs.SetInt($"{currentUsername}_LevelCompleted", -1);
            PlayerPrefs.Save();
            SceneManager.LoadScene("MainMenu");
            return;
        }
        surveyPanel.SetActive(true);
        startingPanel.SetActive(false);
        gamePanel.SetActive(false);
        PlayerPrefs.SetInt($"{currentUsername}_LevelCompleted", GridMovementManager.Instance.currentLevelIndex - 1);
        PlayerPrefs.Save();
    }

    public void ShowSurvey()
    {
        surveyPanel.SetActive(true);
        startingPanel.SetActive(false);
        gamePanel.SetActive(false);
        PlayerPrefs.SetInt($"{currentUsername}_LevelCompleted", -1);
        PlayerPrefs.Save();
    }

    void EndTest()
    {
        finalTime = timer;
        levelActive = false;
        var results = GetTestResults();
        string json = JsonUtility.ToJson(GetTestResults(), true);
        PlayerPrefs.SetString("MentalMathGridResults", json);
    }

    [System.Serializable]
    public class LevelResult
    {
        public int level;
        public float timeTaken;
        public int attempts;
        public float percentageCompleted;
        public int totatSteps;
        public int totalWrongSteps;
        public int totalPathSteps;
    }
 
}

