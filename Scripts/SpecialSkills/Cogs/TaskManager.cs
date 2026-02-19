using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro; 
using UnityEngine.SceneManagement;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance;
    public GameObject[] tasks; 
    public GameObject reactionTimer;
    public GameObject countdownObject;
    private float avgReactionTime;
    private float finalReactionTime;
    private float totalPenalty;
    public GameObject scoreboardPanel, surveyPanel;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI taskNameText;
    private int currentTaskIndex = -1;
    private float taskStartTime;
    private bool isTaskActive = false;
    private List<float> taskTimes = new List<float>();
    private float totalPenaltyTime = 0f;
    private int penaltyT = 0;
    private Dictionary<string, object> personalityProfileResults = new Dictionary<string, object>();
    private string currentUsername;


    void Start()
    {
        Instance = this;
        currentUsername = PlayerPrefs.GetString("LoggedInUser", "");
        StartNextTask();
    }

    void Update()
    {
        if (isTaskActive)
        {
            float elapsedTime = Time.time - taskStartTime;
            timerText.text = $"Time: {FormatTime(elapsedTime)}s";
        }
        else
        {
            if (timerText != null)
                timerText.text = "Time: 0.0s";
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int hours = Mathf.FloorToInt(timeInSeconds / 3600f);
        int minutes = Mathf.FloorToInt((timeInSeconds % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    public Dictionary<string, object> GetAllResults()
    {
        return personalityProfileResults;
    }

    public void SaveTaskResults(string taskName, object results)
    {
        if (personalityProfileResults.ContainsKey(taskName))
            personalityProfileResults[taskName] = results; 
        else
            personalityProfileResults.Add(taskName, results);
    }

    public void StartNextTask()
    {
    
        if (currentTaskIndex >= 0) 
        {
            StopTaskTimerAndSave();
        }

        currentTaskIndex++;


        if (currentTaskIndex >= tasks.Length) 
        {
    
            string currentSceneName = SceneManager.GetActiveScene().name;

            if (currentSceneName == "MemoryRecall")
            {
                EndAllTasks();
            }
            if (currentSceneName == "PschyeTests")
            {
                EndAllPsycheTasks();
            }
            return;
        }

        foreach (GameObject task in tasks)
        {
            task.SetActive(false);
        }

        tasks[currentTaskIndex].SetActive(true);
        taskNameText.text = $"Task {currentTaskIndex + 1}: {tasks[currentTaskIndex].name}";

        if (tasks[currentTaskIndex].name.Contains("ReactionTimeTest"))
        {
            reactionTimer.GetComponent<ReactionTimeTest>().enabled = true;
        }
    }

    public void QuitTest()
    {
        countdownObject.SetActive(false);
        PlayerPrefs.SetString($"{currentUsername}_QuitOrEnd", "Quit");
        PlayerPrefs.Save();
        if (currentTaskIndex == -1)
        {
            taskNameText.text = "Error";
            tasks[currentTaskIndex + 1].SetActive(false);
            return;
        }
        isTaskActive = false;
        if (currentTaskIndex >= 0)
        {
            taskTimes.Add(Time.time - taskStartTime);
        }

        float totalElapsedTime = 0;
        foreach (float time in taskTimes) { totalElapsedTime += time; }

        if (currentTaskIndex > 0)
        {
            taskNameText.text = $"Completed {currentTaskIndex} / {tasks.Length}!\nTotal Time: {totalElapsedTime}s.";
            tasks[currentTaskIndex].SetActive(false);
            surveyPanel.SetActive(true);
        }
        if (currentTaskIndex == 0)
        {
            taskNameText.text = $"Completed {currentTaskIndex} / {tasks.Length}!\nTotal Time: 0.0s.";
            tasks[currentTaskIndex].SetActive(false);
            surveyPanel.SetActive(true);
        }
    }

    public void AddPenalty(float penalty)
    {
        totalPenaltyTime += penalty;
    }
    public bool IsLastTask()
    {
        countdownObject.SetActive(false);
        return currentTaskIndex == tasks.Length - 1;
    }

    public void StartTaskTimer()
    {
        taskStartTime = Time.time;
        isTaskActive = true;        
    }

    public void StopTaskTimerWithoutSave()
    {
        isTaskActive = false;
    }

    public void StopTaskTimerAndSave()
    {
        if (isTaskActive)
        {
            float elapsed = Time.time - taskStartTime;
            taskTimes.Add(elapsed);
        }
        isTaskActive = false;
    }


    public void SaveReactionTimeResults(float avg, float final, float penalty, int penaltyTotal)
    {
        avgReactionTime = avg;
        finalReactionTime = final;
        totalPenalty = penalty;
        penaltyT = penaltyTotal;
    }

    public void EndAllPsycheTasks()
    {
        tasks[currentTaskIndex-1].SetActive(false);
        ShowScoreboard(null);
    }

    public void EndAllTasks()
    {
        PlayerPrefs.SetString($"{currentUsername}_QuitOrEnd", "End");
        PlayerPrefs.Save();
        isTaskActive = false;
        float totalElapsedTime = 0;
        foreach (float time in taskTimes) { totalElapsedTime += time; }

        if (currentTaskIndex > 0)
        {
            taskNameText.text = $"Completed {currentTaskIndex} / {tasks.Length}!\nTotal Time: {totalElapsedTime}s.";
            tasks[currentTaskIndex-1].SetActive(false);
            surveyPanel.SetActive(true);
        }
        if (currentTaskIndex == 0)
        {
            taskNameText.text = $"Completed {currentTaskIndex} / {tasks.Length}!\nTotal Time: 0.0s.";
            tasks[currentTaskIndex].SetActive(false);
            surveyPanel.SetActive(true);
        }
    }

    public void ShowQuitScoreboard(Dictionary<string, string> surveyAnswers)
    {
        
        float totalElapsedTime = 0;
        foreach (float time in taskTimes) { totalElapsedTime += time; }
        int completed = currentTaskIndex;
        float totalPercentage = (float) completed / (float) tasks.Length * 100;
        string score = $"Total Score: {completed} / {tasks.Length}";
        timerText.gameObject.SetActive(false);
        taskNameText.gameObject.SetActive(false);
        scoreboardPanel.SetActive(true);
 
        Dictionary<string, object> resultData = new Dictionary<string, object>
            {
                { "testName", "MemoryRecallTest_test2" },
                { "totalTime", totalElapsedTime },
                { "totalScore", completed },
                { "totalLevels", tasks.Length },
                { "totalPercentage", $"{totalPercentage:F2}" },
                { "surveyAnswers", surveyAnswers }
            };
        
        ScoreboardRenderer scoreboard = FindObjectOfType<ScoreboardRenderer>();
        scoreboard.DisplayResults("MemoryRecallTest_test2", resultData, totalElapsedTime, score, surveyAnswers, null, null);
    }

    public void ShowScoreboard(Dictionary<string, string> surveyAnswers)
    {

        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "MemoryRecall")
        {
            float totalElapsedTime = 0;
            foreach (float time in taskTimes) { totalElapsedTime += time; }

            timerText.gameObject.SetActive(false);
            int completed = currentTaskIndex;
            float totalPercentage = (float) completed / (float) tasks.Length * 100;
            string score = $"Total Score: {completed} / {tasks.Length}";
            taskNameText.gameObject.SetActive(false);
            scoreboardPanel.SetActive(true);

            Dictionary<string, object> resultData = new Dictionary<string, object>
            {
                { "testName", "MemoryRecallTest_test2" },
                { "totalTime", totalElapsedTime },
                { "totalScore", completed },
                { "totalLevels", tasks.Length },
                { "totalPercentage", $"{totalPercentage:F2}" },
                { "surveyAnswers", surveyAnswers }
            };

            ScoreboardRenderer scoreboard = FindObjectOfType<ScoreboardRenderer>();
            scoreboard.DisplayResults("MemoryRecallTest_test2", resultData, totalElapsedTime, score, surveyAnswers, null, null);
        }

        if (currentSceneName == "ReactionTimer")
        {
            timerText.gameObject.SetActive(false);
            string score = $"Average Time: {avgReactionTime}s";
            taskNameText.gameObject.SetActive(false);
            scoreboardPanel.SetActive(true);

            Dictionary<string, object> resultData = new Dictionary<string, object>
            {
                { "testName", "ReactionTimerTest_test2" },
                { "avgReactionTime", avgReactionTime },
                { "finalReactionTime", finalReactionTime },
                { "totalPenalty", penaltyT },
                { "totalPenaltyTime", totalPenalty },
                { "surveyAnswers", surveyAnswers }
            };

            ScoreboardRenderer scoreboard = FindObjectOfType<ScoreboardRenderer>();
            scoreboard.DisplayResults("ReactionTimerTest_test2", resultData, finalReactionTime, score, surveyAnswers, null, null);
        }

        if (currentSceneName == "PschyeTests")
        {
            float totalElapsedTime = 0;
            timerText.gameObject.SetActive(false);      
            string score = $"No scores";
            taskNameText.gameObject.SetActive(false);
            scoreboardPanel.SetActive(true);

            Dictionary<string, object> resultData = new Dictionary<string, object>
            {
                { "testName", "PsychologicalEvaluationTest_test2" },
                { "MotivationAssessment", personalityProfileResults.ContainsKey("MotivationAssessment") ? personalityProfileResults["MotivationAssessment"] : null },
                { "Multitasking", personalityProfileResults.ContainsKey("Multitasking") ? personalityProfileResults["Multitasking"] : null },
                { "EmotionalResponses", personalityProfileResults.ContainsKey("EmotionalResponses") ? personalityProfileResults["EmotionalResponses"] : null },
                { "PersonalityProfiler", personalityProfileResults.ContainsKey("PersonalityProfiler") ? personalityProfileResults["PersonalityProfiler"] : null },
                { "TIPIInventory", personalityProfileResults.ContainsKey("TIPIInventory") ? personalityProfileResults["TIPIInventory"] : null }
            };

            ScoreboardRenderer scoreboard = FindObjectOfType<ScoreboardRenderer>();
            scoreboard.DisplayResults("PsychologicalEvaluationTest_test2", resultData, totalElapsedTime, score, surveyAnswers, null, null);
        }
        
    }
}
