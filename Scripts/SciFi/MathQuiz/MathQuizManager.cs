using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MathQuizManager : MonoBehaviour
{
    public static MathQuizManager Instance;

    [Header("UI References")]
    public TMP_Text levelText;
    public TMP_Text questionText;
    public TMP_Text scoreText;
    public Slider timerSlider;
    public List<Button> answerButtons;
    [Header("Fixed Questions")]
    public List<MathQuestionData> allQuestions = new List<MathQuestionData>();
    private MathQuestionData currentQuestionData;
    [Header("Settings")]
    public float timePerQuestion = 30f;
    public TMP_Text timerText;
    public int questionsPerLevel = 5;
    [Header("Popups")]
    public GameObject levelCompletePopup;
    public GameObject scoreboardPanel, questionPanel;
    public TMP_Text popupScoreText;
    public Button nextLevelButton;
    public Image[] starIcons;
    public Sprite fullStar, emptyStar;
    private int currentLevel = 1;
    private int currentQuestion = 0;
    private int globalQuestionIndex = 0;
    private int correctAnswers = 0;
    private float timer;
    private float taskStartTime;
    private float levelStartTime;
    private float questionStartTime;
    private List<MathQuestionResult> currentLevelQuestions;
    private List<MathQuestionResult> allLevelResults = new List<MathQuestionResult>();
    public int totalSkipped=0;

    void Awake() => Instance = this;

    void Start()
    {
        LoadQuestions();
        StartLevel(currentLevel);
    }

    void Update()
    {
        if (timerSlider.gameObject.activeSelf)
        {
            timer -= Time.deltaTime;
            timerSlider.value = timer / timePerQuestion;

            float elapsedTime = Time.time - levelStartTime;
            timerText.text = $"Time: {FormatTime(timer)}s";

            if (timer <= 0f)
            {
                timerText.text = "Time: 0.0s";
                SubmitAnswer(-999); 
            }
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int hours = Mathf.FloorToInt(timeInSeconds / 3600f);
        int minutes = Mathf.FloorToInt((timeInSeconds % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    void ShowStars(int count)
    {
        for (int i = 0; i < starIcons.Length; i++)
        {
            starIcons[i].sprite = i < count ? fullStar : emptyStar;
        }
    }

    void StartLevel(int level)
    {
        currentQuestion = 0;
        correctAnswers = 0;
        currentLevelQuestions = new List<MathQuestionResult>();
        levelStartTime = Time.time;
        levelText.text = $"Level {level}";
        ShowNextQuestion();
    }

    void LoadQuestions()
    {
        allQuestions = new List<MathQuestionData>
        {
            new MathQuestionData { id = 1, question = "1. 60-36 =", answers = new List<string>{"22", "24", "26", "34", "None"}, correctIndex = 1 },
            new MathQuestionData { id = 2, question = "2. 23-16=", answers = new List<string>{"5", "6", "7", "8", "None"}, correctIndex = 2 },
            new MathQuestionData { id = 3, question = "3. 84÷7 =", answers = new List<string>{"11r5", "11r6", "12r1", "12r2", "None"}, correctIndex = 4 },
            new MathQuestionData { id = 4, question = "4. 90÷6 =", answers = new List<string>{"14", "15", "16", "17", "None"}, correctIndex = 1 },
            new MathQuestionData { id = 5, question = "5. 81÷3=", answers = new List<string>{"26", "27", "28", "29", "None"}, correctIndex = 1 },
            new MathQuestionData { id = 6, question = "6. 42÷3 =", answers = new List<string>{"12", "13 ", "14", "15", "None"}, correctIndex = 2 },
            new MathQuestionData { id = 7, question = "7. 40÷6 =", answers = new List<string>{"5r4", "6r2", "6r3", "6r4", "None"}, correctIndex = 3 },
            new MathQuestionData { id = 8, question = "8. 61-19=", answers = new List<string>{"40", "41", "42", "43", "None"}, correctIndex = 2 },
            new MathQuestionData { id = 9, question = "9. 91÷8 =", answers = new List<string>{"11r2", "11r3", "11r4", "11r5", "None"}, correctIndex = 1 },
            new MathQuestionData { id = 10, question = "10. 29x3 =", answers = new List<string>{"87", "77", "97", "67", "None"}, correctIndex = 0 },
            new MathQuestionData { id = 11, question = "11. 28x6 =", answers = new List<string>{"128", "148", "168", "188", "None"}, correctIndex = 2 },
            new MathQuestionData { id = 12, question = "12. 94x7 =", answers = new List<string>{"648", "652", "658", "668", "None"}, correctIndex = 2 },
            new MathQuestionData { id = 13, question = "13. 9x34 =", answers = new List<string>{"306", "296", "206 ", "316", "None"}, correctIndex = 0 },
            new MathQuestionData { id = 14, question = "14. 13+24+19+8 =", answers = new List<string>{"54", "58", "62", "64", "None"}, correctIndex = 3 },
            new MathQuestionData { id = 15, question = "15. 78÷9 =", answers = new List<string>{"7r6", "8r4", "8r5", "8r7", "None"}, correctIndex = 4 },
            new MathQuestionData { id = 16, question = "16. 26x8 =", answers = new List<string>{"108", "198", "204", "208", "None"}, correctIndex = 3 },
            };
    }

    void ShowNextQuestion()
    {
        timer = timePerQuestion;
        timerSlider.value = 1f;
        questionStartTime = Time.time;
        if (globalQuestionIndex >= allQuestions.Count)
        {
            return;
        }

        currentQuestionData = allQuestions[globalQuestionIndex];
        questionStartTime = Time.time;
        ShowQuestion(currentQuestionData);
    }

    public void ShowQuestion(MathQuestionData data)
    {
        questionText.text = data.question;
        for (int i = 0; i < answerButtons.Count; i++)
        {
            if (i < data.answers.Count)
            {
                int index = i;
                answerButtons[i].gameObject.SetActive(true);
                answerButtons[i].GetComponentInChildren<TMP_Text>().text = data.answers[i];
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => SubmitAnswer(index));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
    }


    void SubmitAnswer(int selectedIndex)
    {
        bool isCorrect = selectedIndex == currentQuestionData.correctIndex;

        if (isCorrect)
        {
            correctAnswers++;
        }
        timer = timePerQuestion;

        currentLevelQuestions.Add(new MathQuestionResult
        {
            levelNumber = currentLevel,
            correctAnswers = correctAnswers,
            totalQuestions = questionsPerLevel,
            levelTime = Time.time - questionStartTime,
            testType = "MentalMath",
            questionResults = new List<MathQuestionResult>(currentLevelQuestions)
        });

        globalQuestionIndex++;

        currentQuestion++;

        if (currentQuestion < questionsPerLevel && globalQuestionIndex < allQuestions.Count)
        {
            ShowNextQuestion();
        }
        else
        {
            EndLevel();
        }
    }

    List<MathQuestionResult> FlattenQuestionResults(List<MathQuestionResult> levels)
    {
        List<MathQuestionResult> all = new List<MathQuestionResult>();
        foreach (var level in levels)
        {
            all.AddRange(level.questionResults);
        }
        return all;
    }

    void ShowFinalResults()
    {
        float totalTime = 0f;
        int totalCorrect = 0;
        int totalQuestions = 0;

        foreach (var level in allLevelResults)
        {
            totalTime += level.levelTime;
            totalCorrect += level.correctAnswers;
            totalQuestions += level.totalQuestions;
        }

        string finalScore = $"{totalCorrect} / {totalQuestions}";

        Dictionary<string, object> resultData = new Dictionary<string, object>
        {
            { "testArea", "MentalMathsTest1" },
            { "totalTime", totalTime },
            { "totalCorrect", totalCorrect },
            { "totalQuestions", totalQuestions },
            { "totalPercentage", (float)totalCorrect / (float) totalQuestions * 100 },
            { "levelBreakdown", allLevelResults }
        };
        
        UserAuth.Instance.SaveTestResults(PlayerPrefs.GetString("LoggedInUser"), "MentalMathsTest1", resultData);        
        ShowScoreboard("MentalMathScene", resultData, finalScore, totalTime, null, null, allLevelResults);
        }
        
    public SceneLogicManager.AllMathQuestionData GetTestResults()
    {
        float totalTime = 0f;
        int totalCorrect = 0;
        int totalQuestions = 0;

        foreach (var level in allLevelResults)
        {
            totalTime += level.levelTime;
            totalCorrect += level.correctAnswers;
            totalQuestions += level.totalQuestions;
        }

        Dictionary<string, object> resultData = new Dictionary<string, object>
        {
            { "levelBreakdown", allLevelResults }
        };

        SceneLogicManager.AllMathQuestionData mathQuizResults = new SceneLogicManager.AllMathQuestionData
        {
            testArea = "MentalMathScene",
            questionAndAnswers = resultData,
            totalTime = totalTime,
            totalCorrect = totalCorrect,
            totalQuestions = totalQuestions,
            totalPercentage = (float)totalCorrect / totalQuestions
        };
        return mathQuizResults;
    }

    public void ShowScoreboard(string testArea, Dictionary<string, object> finalResults, string score, float totalTime, Dictionary<string, string> surveyAnswers, List<QuestionResult> questionResults, List<MathQuestionResult> questionResultsMath)
    {
        scoreboardPanel.SetActive(true);
        string currentUsername = PlayerPrefs.GetString("LoggedInUser", "");
        PlayerPrefs.SetInt($"{currentUsername}_CurrentZoneIndex", 1);
        PlayerPrefs.Save();
        string userDataKey = $"{currentUsername}_Data";
        string json = JsonUtility.ToJson(new SceneLogicManager.SerializationWrapper(finalResults));
        PlayerPrefs.SetString(userDataKey, json);
        PlayerPrefs.Save();
        ScoreboardRenderer scoreboard = FindObjectOfType<ScoreboardRenderer>();
        scoreboard.DisplayResults(testArea, finalResults, totalTime, score, surveyAnswers, questionResults, questionResultsMath);
        levelCompletePopup.SetActive(false);
        questionPanel.SetActive(false);
    }

    public void BackToMainMathScene()
    {
        SceneManager.LoadScene("SciFi_Warehouse");
    }

    void EndLevel()
    {
        timerSlider.gameObject.SetActive(false);
        timerText.text = "Time: 0.0s";
        allLevelResults.Add(new MathQuestionResult
        {
            levelNumber = currentLevel,
            correctAnswers = correctAnswers,
            totalQuestions = questionsPerLevel,
            levelTime = Time.time - questionStartTime,
            testType = "MentalMath",
            questionResults = new List<MathQuestionResult>(currentLevelQuestions)
        });

        levelCompletePopup.SetActive(true);
        questionPanel.SetActive(false);
        popupScoreText.text = $"You got {correctAnswers} / {questionsPerLevel} right!";
        int stars = (int)Mathf.Ceil(3f * correctAnswers / (float)questionsPerLevel);
        Debug.Log($"🌟 Awarded Stars: {stars}");
        ShowStars(stars);
        nextLevelButton.onClick.RemoveAllListeners();
        nextLevelButton.onClick.AddListener(() => {
        levelCompletePopup.SetActive(false);
        questionPanel.SetActive(true);
        currentLevel++;
        if (currentLevel > 4)
            {
                ShowFinalResults();
            }
            else
            {
                timerSlider.gameObject.SetActive(true);
                StartLevel(currentLevel);
            }
        });
    }
}

[System.Serializable]
public class MathQuestionData
{
    public int id;
    public string question;
    public List<string> answers;
    public int correctIndex;
}

