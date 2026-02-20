using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;
using System;

public class ResultsOverviewController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject contentContainer;
    public GameObject resultTextPrefab;
    
    [Header("Scroll Settings")]
    public ScrollRect scrollRect;

    [Header("Settings")]
    public string username;
    public string rootPath;

    private readonly string[] testFolders = {
        "VerbalComprehensionTest_test2",
        "MentalMathsTest_test2",
        "MemoryRecallTest_test2",
        "ReactionTimerTest_test2",
        "NavigationGrid_test2",
        "PsychologicalEvaluationTest_test2",
        "OverallEvaluation"
    };

    private class TestSummary
    {
        public string TestName;
        public bool Completed;
        public float TotalTime;
        public float TotalCorrect;
        public float TotalQuestions;
        public float AvgReactionTime;
        public float TotalPenalties;
        public float FinalAttemptReactionTime;
        public float Percentage;
        
    }

    private List<TestSummary> summaries = new();
    private bool useAlternateBackground = false;

    void Start()
    {
        rootPath = Application.persistentDataPath;
        username = PlayerPrefs.GetString("LoggedInUser", "");

        CanvasGroup cg = contentContainer.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = contentContainer.AddComponent<CanvasGroup>();
            cg.alpha = 0;
        }

        LoadAllResults();
        DisplayOverallSummary();

        LeanTween.alphaCanvas(cg, 1f, 0.8f).setEaseOutCubic();
        LeanTween.moveLocalY(contentContainer, contentContainer.transform.localPosition.y + 30f, 0.8f)
                 .setEaseOutCubic()
                 .setFrom(contentContainer.transform.localPosition.y - 60f);
      
    }

    void OnEnable()
    {
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void HandlePsychEvaluationTest(string testPath, string displayFolder, string username)
    {
        string[] psycheFiles = {
            "EmotionalResponses.csv",
            "MotivationAssessment.csv",
            "Multitasking.csv",
            "PersonalityProfiler.csv",
            "TIPIInventory.csv"
        };

        int totalModules = psycheFiles.Length;
        int completedModules = 0;

        foreach (string file in psycheFiles)
        {
            string path = Path.Combine(testPath, file);

            if (!File.Exists(path))
            {
                continue;
            }

    
            bool hasData = false;
            try
            {
                string[] lines = File.ReadAllLines(path);
                foreach (string line in lines.Skip(1)) 
                {
                    if (line.Contains(username))
                    {
                        hasData = true;
                        break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                continue;
            }

            if (hasData)
            {
                completedModules++;
            }
            else
            {
                continue;
            }
        }

        float percentage = totalModules > 0
            ? (completedModules / (float)totalModules) * 100f
            : 0f;

        TestSummary summary = new TestSummary
        {
            TestName = displayFolder,
            Completed = completedModules > 0,
            TotalQuestions = totalModules,
            TotalCorrect = completedModules,
            Percentage = percentage
        };

        CreateTestSummary(summary);
    }



    void LoadAllResults()
    {
        CreateHeader($"<b>{username}</b>");
        CreateDivider();

        foreach (string folder in testFolders)
        {
            string testPath = Path.Combine(rootPath, folder);
            
            string displayFolder = System.Text.RegularExpressions.Regex.Replace(
                folder.Split('_')[0], 
                "(?<!^)([A-Z])", 
                " $1"
            );

            if (folder == "PsychologicalEvaluationTest_test2")
            {
                HandlePsychEvaluationTest(testPath, displayFolder, username);
                continue;
            }

            string resultsPath = Path.Combine(testPath, "results.csv");

            if (!File.Exists(resultsPath))
            {
                CreateSubHeader($"{displayFolder} <color=#888888>(No data found)</color>");
                continue;
            }

            var lines = File.ReadAllLines(resultsPath);
            if (lines.Length < 2)
                continue;

            string[] headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();
            bool userFound = false;

            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',').Select(v => v.Trim()).ToArray();
                if (values.Length < headers.Length)
                    continue;

                if (values[0] != username)
                    continue;

                userFound = true;

                TestSummary summary = new TestSummary
                {
                    TestName = displayFolder,
                    Completed = true
                };

                switch (folder)
                {
                    case "VerbalComprehensionTest_test2":
                        summary.TotalTime = TryParse(values[1]);
                        summary.TotalCorrect = TryParse(values[2]);
                        summary.TotalQuestions = TryParse(values[3]);
                        summary.Percentage = TryParse(values[4]);
                        break;

                    case "MentalMathsTest_test2":
                        summary.TotalTime = TryParse(values[1]);
                        summary.TotalCorrect = TryParse(values[2]);
                        summary.TotalQuestions = TryParse(values[3]);
                        summary.Percentage = TryParse(values[4]);
                        break;

                    case "MemoryRecallTest_test2":
                        summary.TotalTime = TryParse(values[1]);
                        summary.TotalCorrect = TryParse(values[2]);
                        summary.TotalQuestions = TryParse(values[3]);
                        summary.Percentage = TryParse(values[4]);
                        break;

                    case "ReactionTimerTest_test2":
                        summary.AvgReactionTime = TryParse(values[1]);
                        summary.FinalAttemptReactionTime = TryParse(values[2]);
                        summary.TotalPenalties = TryParse(values[3]);
                        break;
                    
                    case "NavigationGrid_test2":
                        summary.TotalTime = TryParse(values[1]);
                        summary.TotalCorrect = TryParse(values[5]);
                        summary.TotalQuestions = TryParse(values[4]);
                        summary.Percentage = TryParse(values[2]);
                        break;
                

                }

                summaries.Add(summary);
                CreateTestSummary(summary);
                break;
            }

            if (!userFound)
            {
                summaries.Add(new TestSummary { TestName = folder, Completed = false });
                CreateSubHeader($"{displayFolder} <color=#FFA000>In Progress</color>");
                CreateSpacer();
            }
        }
    }

    void DisplayOverallSummary()
    {
        Dictionary<string, Dictionary<string, object>> userResults = UserResultsLoader.LoadUserResults(username, rootPath);
        int totalTests = testFolders.Length;
        int completedTests = userResults.Count;
        float totalTime = summaries.Sum(s => s.TotalTime);
        TimeSpan time = TimeSpan.FromSeconds(totalTime);
        string formatted = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", (int)time.TotalHours, time.Minutes, time.Seconds);
        float totalCorrect = summaries.Sum(s => s.TotalCorrect);
        float totalQuestions = summaries.Sum(s => s.TotalQuestions);
        float overallPercentage = totalQuestions > 0 ? (totalCorrect / totalQuestions) * 100f : 0f;
        float progressPercentage = ((float)completedTests / totalTests) * 100f;

        CreateDivider();
        CreateAnimatedHeader("<b>OVERALL SUMMARY</b>", new Color(0.16f, 0.47f, 0.85f));
        CreateDivider();

        string progressColor = progressPercentage >= 75 ? "#4CAF50" :
                               progressPercentage >= 50 ? "#FFC107" : "#F44336";

        CreateRichBlock($"<b>Tests Completed:</b> {completedTests}/{totalTests} " +
                        $"(<color={progressColor}>{progressPercentage:F1}%</color>)");
        CreateRichBlock($"<b>Total Time:</b> {formatted}");
        CreateRichBlock($"<b>Total Correct:</b> {totalCorrect}/{totalQuestions}");

        string scoreColor = overallPercentage >= 80 ? "#4CAF50" :
                            overallPercentage >= 60 ? "#FFC107" : "#F44336";

        CreateRichBlock($"<b>Overall Score:</b> <color={scoreColor}>{overallPercentage:F1}%</color>");
        CreateSpacer();

        string performanceNote = GeneratePerformanceNote(overallPercentage, progressPercentage);
        CreateRichBlock($"<i>{performanceNote}</i>");
    }

    void CreateTestSummary(TestSummary s)
    {
        TimeSpan time = TimeSpan.FromSeconds(s.TotalTime);
        string formatted = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", (int)time.TotalHours, time.Minutes, time.Seconds);

        string color = s.Percentage >= 80 ? "#4CAF50" :
                       s.Percentage >= 50 ? "#FFC107" : "#F44336";

        CreateAnimatedHeader($"<b>{s.TestName}</b>", new Color(0.25f, 0.25f, 0.25f));
        if (s.TestName.Contains("Psychological Evaluation"))
        {
            CreateRichBlock($"<b>Subtasks Completed:</b> {s.TotalCorrect}/{s.TotalQuestions}");
            CreateRichBlock($"<b>Completion:</b> <color={color}>{s.Percentage:F1}%</color>");
            CreateRichBlock($"<i>Time was not recorded for this test</i>");
        }

        if (s.TestName.Contains("Reaction Timer"))
        {
            CreateRichBlock($"<b>Average Time:</b> {s.AvgReactionTime} ms");
            CreateRichBlock($"<b>Final Attempt Time:</b> {s.FinalAttemptReactionTime} ms");
            CreateRichBlock($"<b>Total Penalties:</b> {s.TotalPenalties}");
        }
        if (!s.TestName.Contains("Reaction Timer") && !s.TestName.Contains("Psychological Evaluation"))
        {
            CreateRichBlock($"<b>Time:</b> {formatted}");
            CreateRichBlock($"<b>Correct:</b> {s.TotalCorrect}/{s.TotalQuestions}");
            CreateRichBlock($"<b>Completion:</b> <color={color}>{s.Percentage:F1}%</color>");
        }
        CreateSpacer();
    }

    string GeneratePerformanceNote(float scorePercent, float progressPercent)
    {
        if (progressPercent > 10 && progressPercent < 30)
            return "You’ve made a solid start — keep going to unlock your full potential!";
        if (scorePercent >= 85)
            return "Outstanding performance! You’re excelling across all areas.";
        if (scorePercent >= 70)
            return "Strong performance — maintain your focus and consistency.";
        if (scorePercent >= 50)
            return "Average results so far — identify weak spots and practice more.";
        return "Don’t give up! Improvement comes with effort and persistence.";
    }

    float TryParse(string val)
    {
        if (string.IsNullOrWhiteSpace(val)) return 0f;
        val = val.Replace("%", "").Replace(",", ".").Trim();

        if (float.TryParse(val, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float f))
            return f;
        return 0f;
    }

    void CreateHeader(string text)
    {
        CreateResultBlock($"<size=135%><b>{text}</b></size>", "#FFFFFF");
    }

    void CreateSubHeader(string text)
    {
        CreateResultBlock($"<size=120%>{text}</size>", "#DDDDDD");
    }

    void CreateRichBlock(string text)
    {
        CreateResultBlock($"<size=100%>{text}</size>", "#CCCCCC");
    }

    void CreateDivider()
    {
        CreateResultBlock("<color=#888888>────────────────────</color>", "#888888");
    }

    void CreateSpacer()
    {
        CreateResultBlock(" ", "#00000000");
    }

    void CreateResultBlock(string text, string color = "#FFFFFF")
    {
        GameObject newTextObj = Instantiate(resultTextPrefab, contentContainer.transform);

        TextMeshProUGUI tmp = newTextObj.GetComponent<TextMeshProUGUI>();
        if (tmp == null)
        {
            return;
        }

        tmp.text = $"<color={color}>{text}</color>";
        tmp.richText = true;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 14;
        tmp.fontSizeMax = 24;

        Image bg = newTextObj.GetComponent<Image>();
        if (bg != null)
        {
            bg.color = useAlternateBackground
                ? new Color(0.2f, 0.2f, 0.2f, 0.15f)
                : new Color(0f, 0f, 0f, 0f);
        }

        useAlternateBackground = !useAlternateBackground;

        CanvasGroup cg = newTextObj.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = newTextObj.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
        }

        LeanTween.alphaCanvas(cg, 1f, 0.4f).setDelay(UnityEngine.Random.Range(0f, 0.3f)).setEaseOutCubic();
    }

    void CreateAnimatedHeader(string text, Color accentColor)
    {
        GameObject headerObj = Instantiate(resultTextPrefab, contentContainer.transform);

        TextMeshProUGUI tmp = headerObj.GetComponent<TextMeshProUGUI>();
        if (tmp == null)
        {
            return;
        }

        string hexColor = ColorUtility.ToHtmlStringRGB(accentColor);
        tmp.text = $"<mark=#{hexColor}33><b><size=125%>{text}</size></b></mark>"; 
        tmp.richText = true;
        tmp.enableAutoSizing = true;
        tmp.enableWordWrapping = true;

        CanvasGroup cg = headerObj.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = headerObj.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        LeanTween.alphaCanvas(cg, 1f, 0.6f).setEaseOutCubic();
    }

}
