using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SceneLogicManager : MonoBehaviour
{
    [System.Serializable]
    public class RiddleHint
    {
        public int questionId;
        [TextArea] public List<string> hints = new List<string>();

    }
    public TMP_Text riddleHeader;
    private int activeQuestionId = -1;   
    private int activeHintIndex = 1;   
    [Header("Hints Setup")]
    public List<RiddleHint> riddleHints = new List<RiddleHint>();
    [Header("UI References")]
    public TMP_Text hintText;
    public GameObject[] triggers;
    public GameObject torcusObject;
    public static SceneLogicManager Instance;
    private Dictionary<string, AllMathQuestionData> allMathResults = new Dictionary<string, AllMathQuestionData>();
    public GameObject[] riddleTriggers;
    public TextMeshProUGUI debugText;
    public int totalQuestions = 5;
    public GameObject startModePanel, riddlePanel, surveyPanel, scoreboardPanel, keypad, quizPanel1, keypadPanel1, menuButton;
    public Button hintButton;
    public TMP_Text hintButtonText;
    public int hintCount = 0;
    public bool usedHint = false;
    private int getZoneIndex = 0;
    [Header("Settings")]
    public float timePerQuestion = 30f;
    public TMP_Text timerText;
    public int totalLevels = 4;
    private float timer;
    private float questionStartTime;
    public float totalTime;
    public int penalty = 0;
    public int totalScore = 0;
    public float finalTotalTime = 0;
    public int finalTotalScore = 0;
    public float finalTotalPercentage = 0;
    public int totalQuestionsAcrossAll = 0;
    [Header("Hints Setup")]
    public Slider timerSlider;
    public GameObject[] quizPanels;
    public GameObject[] keypadPanels;
    public GameObject[] keypads;
    public List<string> manualAnswers;
    private int currentLevel = 1;
    public TMP_Text levelText;
    public float levelTime = 0;
    private float levelStartTime;
    private string currentUsername;
    public int totalSkipped=0;
    public Transform xrRig;

    void Awake()
    {
        Instance = this;
        currentUsername = PlayerPrefs.GetString("LoggedInUser", "");
        getZoneIndex = PlayerPrefs.GetInt($"{currentUsername}_CurrentZoneIndex", 0);
        ShowStartModePanel();
    }

    void Start()
    {
        menuButton.SetActive(false);
        if (getZoneIndex == 1){
            riddleTriggers[0].gameObject.SetActive(false);
            riddleHeader.text = "Riddle 2";
        }
    }

    void Update()
    {
        if (timerSlider.gameObject.activeSelf)
        {
            timer -= Time.deltaTime;
            timerSlider.value = timer / timePerQuestion;

            float elapsedTime = Time.time - levelStartTime;
            timerText.text = $"Time: {FormatTime(timer)}";

            if (timer <= 0f)
            {
                timerText.text = "Time: 0.0s";
                EndLevel(); 
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

    public void IsCorrect()
    {
        totalScore++;
        timer = timePerQuestion;
        EndLevel();
    }

    public void IsNotCorrect()
    {
        penalty++;
    }

    public void SkipToPart1()
    {
        totalSkipped++;

        RiddleZone zone2 = null;
        foreach (GameObject trigger in triggers)
        {
            RiddleZone zone = trigger.GetComponent<RiddleZone>();
            if (zone != null && zone.questionId == 2)
            {
                zone2 = zone;
                break;
            }
        }

        if (zone2 != null && zone2.gameObject.activeSelf)
        {
            zone2.gameObject.SetActive(false);
            OnRiddleZoneReached(2);
        }
        else
        {
            foreach (GameObject trigger in triggers)
            {
                RiddleZone zone = trigger.GetComponent<RiddleZone>();
                if (zone != null && zone.questionId == 1)
                {
                    trigger.SetActive(false);
                    CustomGrabMath.Instance.OnObjectMoved();
                    break;
                }
            }
        }
    }

    void StartLevel(int level)
    {
       
        levelStartTime = Time.time;
        levelText.text = $"Level {level}";
    
        if (level > 1)
        {
            quizPanels[level - 2].SetActive(false);
            quizPanels[level - 1].SetActive(true);
            keypadPanels[level - 2].SetActive(false);
            keypadPanels[level - 1].SetActive(true);
            keypads[level - 2].SetActive(false);
            keypads[level - 1].SetActive(true);

        }

        ShowNextQuestion();
    }
    void ShowNextQuestion()
    {
        timer = timePerQuestion;
        timerSlider.value = 1f;
        questionStartTime = Time.time;
    }


    public void EndQuiz()
    {
        totalTime = totalTime + (penalty * 5);
        Dictionary<string, string> finalQuestionAndAnswers = new Dictionary<string, string>();

        for (int i = 0; i < quizPanels.Length; i++)
        {
            Transform panel = quizPanels[i].transform;
            string equation = panel.Find("EQ").GetComponent<TMPro.TextMeshProUGUI>().text;
            string question = $"{equation}";
            string answer = (i < manualAnswers.Count) ? manualAnswers[i] : "N/A";
            finalQuestionAndAnswers[question] = answer;
        }
        Dictionary<string, object> resultData = new Dictionary<string, object>
        {
            { "totalTime", totalTime },
            { "finalScore", totalScore },
            { "totalQuestions", totalQuestions },
            { "questionAndAnswers", finalQuestionAndAnswers }
        };

        SaveMathTestResults("FullMathTest", resultData, totalTime, totalScore, totalQuestions, (float)totalScore / (float) totalQuestions * 100);
        ShowSurvey();
    }

    void EndLevel()
    {
        timerSlider.gameObject.SetActive(false);
        levelTime = Time.time - questionStartTime;
        totalTime += levelTime;
        currentLevel++;
        if (currentLevel > 4)
        {
            quizPanels[currentLevel - 2].SetActive(false);
            keypadPanels[currentLevel - 2].SetActive(false);
            keypads[currentLevel - 2].SetActive(false);
            EndQuiz();
        }
        else
        {
            timerSlider.gameObject.SetActive(true);
            StartLevel(currentLevel);
        }
    }

    public void ShowStartModePanel()
    {
        if (getZoneIndex != 0)
        {
            ShowHintForQuestion(getZoneIndex);
            startModePanel.SetActive(false);

        }
        else
        {
            startModePanel.SetActive(true);
            riddlePanel.SetActive(false);
            menuButton.SetActive(false);
            scoreboardPanel.SetActive(false);
            surveyPanel.SetActive(false);
            keypad.SetActive(false);
            quizPanel1.SetActive(false);
            keypadPanel1.SetActive(false);
        }
    }
    public void OnHintButtonPressed()
    {
        if (activeQuestionId == -1) return;
        var riddleHint = riddleHints.Find(r => r.questionId == activeQuestionId);
        if (riddleHint == null || riddleHint.hints.Count <= 1) return;

        if (activeHintIndex < riddleHint.hints.Count)
        {
            hintText.text = riddleHint.hints[activeHintIndex];
            activeHintIndex++;  
            
        }

        if (activeHintIndex >= riddleHint.hints.Count)
        {
            hintButton.interactable = false;
            hintButtonText.text = "No More Hints";
        }
    }

    public void ShowTorcus()
    {
        torcusObject.gameObject.SetActive(true);
    }


    public void ShowRiddle()
    {
        activeQuestionId = 1;  
        activeHintIndex = 1;
        var riddleHint = riddleHints.Find(r => r.questionId == 1);
        if (riddleHint != null && riddleHint.hints.Count > 0)
        {
            hintText.text = riddleHint.hints[0]; 
        }
        else
        {
            hintText.text = "No hints available.";
        }
        riddleTriggers[0].SetActive(true);
        riddlePanel.SetActive(true);
    }

    public void ShowHintForQuestion(int questionId)
    {
        activeHintIndex = 1;
        foreach (var riddleHint in riddleHints)
        {
            int riddleId = questionId + 1;
            if (riddleHint.questionId == riddleId)
            {
                activeQuestionId = riddleHint.questionId;

                if (riddleHint.hints.Count > 0)
                {
                    hintText.text = riddleHint.hints[0];  
                }
                else
                {
                    hintText.text = "No hints available.";
                }

                riddleTriggers[questionId].SetActive(true);
                riddlePanel.SetActive(true);
                return;
            }
        }

        hintText.text = "No hint available for this riddle.";
        keypad.SetActive(true);
        riddlePanel.SetActive(false);
        menuButton.SetActive(false);
        quizPanel1.SetActive(true);
        timerSlider.gameObject.SetActive(true);
        keypadPanel1.SetActive(true);
        StartLevel(currentLevel);
    }

    public void ShowSurvey()
    {
        surveyPanel.SetActive(true);
        startModePanel.SetActive(false);
        riddlePanel.SetActive(false);
        menuButton.SetActive(false);
        scoreboardPanel.SetActive(false);
    }

    public void OnRiddleZoneReached(int zoneIndex)
    {
        
        if (zoneIndex == 2)
        {
            riddleHeader.text = "Riddle 2";
            PlayerPrefs.SetInt($"{currentUsername}_CurrentZoneIndex", 0);
            PlayerPrefs.Save();
            getZoneIndex = PlayerPrefs.GetInt($"{currentUsername}_CurrentZoneIndex", 0);
   
            if (xrRig != null)
            {
                xrRig.position = new Vector3(22f, -8.34465027e-07f, 4.25672531f);
                xrRig.rotation = Quaternion.Euler(0f, 450f, 0f);
            }
            else
            {
                return;
            }
            
            ShowHintForQuestion(zoneIndex);
            
        }
    }

    public void AccessShowScoreboard(Dictionary<string, string> surveyAnswers)
    {
        string finalScore = $"{finalTotalScore} / {totalQuestionsAcrossAll}";

        Dictionary<string, object> finalResultData = new Dictionary<string, object>
        {
            { "finalTotalTime", finalTotalTime },
            { "finalTotalScore", finalTotalScore },
            { "finalTotalQuestions", totalQuestionsAcrossAll },
            { "finalTotalPercentage", finalTotalPercentage },
            { "surveyAnswers", surveyAnswers },
            { "allMathResults", allMathResults }
        };
        scoreboardPanel.SetActive(true);

        ShowScoreboard("MentalMathsTest_test2", finalResultData, finalScore, finalTotalTime, surveyAnswers, null, null);
    }

    public void ShowScoreboard(string testArea, Dictionary<string, object> finalResults, string score, float totalTime, Dictionary<string, string> surveyAnswers, List<QuestionResult> questionResults, List<MathQuestionResult> questionResultsMath)
    {
        getZoneIndex = 0;
        PlayerPrefs.SetInt($"{currentUsername}_CurrentZoneIndex", getZoneIndex);
        PlayerPrefs.Save();
        scoreboardPanel.SetActive(true);
        surveyPanel.SetActive(false);
        startModePanel.SetActive(false);
        riddlePanel.SetActive(false);
        menuButton.SetActive(false);

        ScoreboardRenderer scoreboard = FindObjectOfType<ScoreboardRenderer>();
        scoreboard.DisplayResults(testArea, finalResults, totalTime, score, surveyAnswers, questionResults, questionResultsMath);

    }

    public void SaveMathTestResults(string testName, Dictionary<string, object> results, float totalTime, int totalCorrect, int totalQuestions, float totalPercentage)
    {
        AllMathQuestionData data = new AllMathQuestionData
        {
            testArea = testName,
            questionAndAnswers = results,
            totalTime = totalTime,
            totalCorrect = totalCorrect,
            totalQuestions = totalQuestions,
            totalPercentage = totalPercentage
        };

        string userId = PlayerPrefs.GetString("LoggedInUser");
        string userDataKey = $"{userId}_Data";

        if (PlayerPrefs.HasKey(userDataKey))
        {
            string json = PlayerPrefs.GetString(userDataKey);
            SerializationWrapper wrapper = JsonUtility.FromJson<SerializationWrapper>(json);
            Dictionary<string, object> loadedData = wrapper.ToDictionary();

            AllMathQuestionData mathQuizResults = new AllMathQuestionData
            {
                testArea = loadedData["testArea"].ToString(),
                questionAndAnswers = loadedData["levelBreakdown"] as Dictionary<string, object>,
                totalTime = Convert.ToSingle(loadedData["totalTime"]),
                totalCorrect = Convert.ToInt32(loadedData["totalCorrect"]),
                totalQuestions = Convert.ToInt32(loadedData["totalQuestions"]),
                totalPercentage = Convert.ToSingle(loadedData["totalPercentage"])
            };

            if (!string.IsNullOrEmpty(mathQuizResults.testArea))
            {
                allMathResults[mathQuizResults.testArea] = mathQuizResults;
            }
        }

        allMathResults[testName] = data;
        RecalculateFinalTotals();

    }

    private void RecalculateFinalTotals()
    {
        finalTotalTime = 0;
        finalTotalScore = 0;
        totalQuestionsAcrossAll = 0;

        foreach (var entry in allMathResults.Values)
        {
            finalTotalTime += Mathf.RoundToInt(entry.totalTime);
            finalTotalScore += entry.totalCorrect;
            totalQuestionsAcrossAll += entry.totalQuestions;
        }
        finalTotalPercentage = totalQuestionsAcrossAll > 0
            ? (float)finalTotalScore / totalQuestionsAcrossAll * 100f
            : 0f;
    }

    [System.Serializable]
    public class AllMathQuestionData
    {
        public string testArea;
        public Dictionary<string, object> questionAndAnswers;
        public float totalTime;
        public int totalCorrect;
        public int totalQuestions;
        public float totalPercentage;
    }
    
    [System.Serializable]
    public class SerializationWrapper
    {
        public List<string> keys = new List<string>();
        public List<string> values = new List<string>();

        public SerializationWrapper(Dictionary<string, object> dict)
        {
            foreach (var kvp in dict)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value.ToString()); 
            }
        }

        public Dictionary<string, object> ToDictionary()
        {
            var dict = new Dictionary<string, object>();
            for (int i = 0; i < keys.Count; i++)
            {
                dict[keys[i]] = values[i]; 
            }
            return dict;
        }
    }
}
