using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;

public class UserAuth : MonoBehaviour
{
    public static UserAuth Instance;
    public TextMeshProUGUI debugText;
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button submitButton;
    private string filePath;
    private string loggedInUser;
    private string currentUsername;
    private bool userExists = false;


    void Start()
    {
        Instance = this;
        filePath = Path.Combine(Application.persistentDataPath, "userDeets.csv");
        currentUsername = PlayerPrefs.GetString("LoggedInUser", "");
        // ClearPersistentData();
        CreateDefaultConfigFiles();
    }

    private void ClearPersistentData()
    {
        string persistentPath = Application.persistentDataPath;

        foreach (string file in Directory.GetFiles(persistentPath, "*", SearchOption.AllDirectories))
        {
            File.Delete(file);
        }

        foreach (string dir in Directory.GetDirectories(persistentPath))
        {
            Directory.Delete(dir, true);
        }

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    public void OpenWithDelay(GameObject target)
    {
        StartCoroutine(ActivateAfterDelay(target, 0.1f));
    }

    private IEnumerator ActivateAfterDelay(GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);
        target.SetActive(true);
    }

    private void CreateDefaultConfigFiles()
    {
        string configDir = Path.Combine(Application.persistentDataPath, "ConfigFiles");

        if (!Directory.Exists(configDir))
        {
            Directory.CreateDirectory(configDir);
        }

        string[] configFiles = { "WeightsConfig.json", "CareerFieldDB.json", "BigFiveConfig.json", "generate_career_report.py" };

        foreach (string fileName in configFiles)
        {
            string filePath = Path.Combine(configDir, fileName);

            if (!File.Exists(filePath))
            {
                string sourcePath = Path.Combine(Application.streamingAssetsPath, "Configs", fileName);
                string jsonContent = "";

    #if UNITY_ANDROID && !UNITY_EDITOR
                UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(sourcePath);
                www.SendWebRequest();
                while (!www.isDone) { } 
                if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    jsonContent = www.downloadHandler.text;
                
    #else
                jsonContent = File.ReadAllText(sourcePath);
    #endif

                File.WriteAllText(filePath, jsonContent);
            }
        }
    }
    public void HandleLogin()
    {
        filePath = Application.persistentDataPath + "/userDeets.csv";

        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {        
            return;
        }

        if (!File.Exists(filePath))
        {

            CreateDB_File();
            SceneManager.LoadScene("StartingScene");
            return;
        }

        var lines = File.ReadAllLines(filePath).ToList();
        if (lines.Count < 2)
        {
            return;
        }

        for (int i = 1; i < lines.Count; i++)
        {
            string[] fields = ParseCSVLine(lines[i]);

            if (fields.Length >= 2 && fields[0] == username)
            {
                if (fields[1] == password)
                {
     
                    PlayerPrefs.SetString("LoggedInUser", username);
                    PlayerPrefs.Save();

                    string backgroundChoice = (fields.Length >= 4) ? fields[3] : "";

                    if (string.IsNullOrEmpty(backgroundChoice))
                    {
                        SceneManager.LoadScene("StartingScene");
                    }
                    else
                    {
                        PlayerPrefs.SetString($"{username}_SelectedBackground", backgroundChoice);
                        PlayerPrefs.Save();
                        SceneManager.LoadScene("MainMenu");
                    }

                    return;
                }
                else
                {
                    return;
                }
            }
        }

        string[] data = new string[4];
        data[0] = username;
        data[1] = password;
        data[2] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        data[3] = "";

        AddOrUpdateLine(username, data);
        PlayerPrefs.SetString("LoggedInUser", username);
        PlayerPrefs.Save();
        SceneManager.LoadScene("StartingScene");
    }

    public void CreateDB_File()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;
        filePath = Path.Combine(Application.persistentDataPath, "userDeets.csv");

        if (!File.Exists(filePath))
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string csvLine = $"{username},{password},{date}";

            using (StreamWriter writer = new StreamWriter(filePath, append: true))
            {
                writer.WriteLine("Username,Password,Date Created, Background");
                writer.WriteLine(csvLine);
            }
            PlayerPrefs.SetString("LoggedInUser", username);
        }
    }

    public void AddOrUpdateLine(string username, string[] newData)
    {
        if (string.IsNullOrEmpty(username))
        {
            return;
        }

        if (!File.Exists(filePath))
        {
            CreateDB_File();
        }

        List<string> lines = File.ReadAllLines(filePath).ToList();
        string header = lines[0];

        bool userFound = false;
        for (int i = 1; i < lines.Count; i++)
        {
            string[] fields = ParseCSVLine(lines[i]);

            if (fields.Length > 0 && fields[0] == username)
            {
                for (int j = 0; j < newData.Length; j++)
                {
                    if (!string.IsNullOrEmpty(newData[j]))
                    {
                        if (fields.Length <= j)
                        {
                            Array.Resize(ref fields, j + 1);
                        }

                        fields[j] = newData[j];
                    }
                }
         
                lines[i] = string.Join(",", fields.Select(f => $"\"{f.Replace("\"", "\"\"")}\""));
           
                userFound = true;
                break;
            }
        }

        if (!userFound)
        {
      
            string[] fullData = new string[4]; 
            fullData[0] = username;

            for (int i = 1; i < newData.Length; i++)
            {
                fullData[i] = newData[i];
            }

            lines.Add(string.Join(",", fullData.Select(f => $"\"{(f ?? "").Replace("\"", "\"\"")}\"")));
        }

        File.WriteAllLines(filePath, lines);
    }


    public void SaveBackgroundChoice(string username, string backgroundChoice)
    {  
        FileInfo fileInfo = new FileInfo(filePath);
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(backgroundChoice))
        {
            return;
        }

        if (!fileInfo.Exists)
        {
            return;
        }
        string[] data = new string[4]; 
        data[3] = backgroundChoice;
        AddOrUpdateLine(username, data);
    }
    private Dictionary<string, string> FlattenResults(Dictionary<string, object> data)
    {
        var flat = new Dictionary<string, string>();

        foreach (var kvp in data)
        {
            if (kvp.Value == null)
            {
                flat[kvp.Key] = "";
            }
            else if (kvp.Value is Dictionary<string, string> dictStr)
            {
                foreach (var inner in dictStr)
                {
                    flat[$"{kvp.Key}_{inner.Key}"] = inner.Value ?? "";
                }
            }
            else if (kvp.Value is Dictionary<string, object> dictObj)
            {
                foreach (var inner in dictObj)
                {
                    flat[$"{kvp.Key}_{inner.Key}"] = inner.Value?.ToString() ?? "";
                }
            }
            else if (kvp.Value is List<QuestionResult> qList)
            {
                for (int i = 0; i < qList.Count; i++)
                {
                    var q = qList[i];
                    flat[$"question{i+1}_questionId"] = q.questionId.ToString();
                    flat[$"question{i+1}_selectedAnswer"] = q.selectedAnswer ?? "";
                    flat[$"question{i+1}_correctAnswer"] = q.correctAnswer ?? "";
                    flat[$"question{i+1}_timeTaken"] = q.timeTaken.ToString();
                    flat[$"question{i+1}_confidence"] = q.confidence.ToString();
                    flat[$"question{i+1}_wasCorrect"] = q.wasCorrect.ToString();
                    flat[$"question{i+1}_usedHint"] = q.usedHint.ToString();
                    flat[$"question{i+1}_hintCount"] = q.hintCount.ToString();
                }
            }
            else if (kvp.Value is IEnumerable<object> objList)
            {
                int idx = 1;
                foreach (var item in objList)
                {
                    flat[$"{kvp.Key}_{idx}"] = item?.ToString() ?? "";
                    idx++;
                }
            }
            else
            {
                flat[kvp.Key] = kvp.Value.ToString();
            }
        }

        return flat;
    }

    private void SaveOrUpdateCSV(string username, Dictionary<string, string> flatResults, string csvPath)
    {
        List<string> headers = new List<string> { "Username" };
        headers.AddRange(flatResults.Keys);

        if (File.Exists(csvPath))
        {
            var lines = File.ReadAllLines(csvPath).Select(l => ParseCSVLine(l)).ToList();

            var existingHeaders = lines[0].ToList();
            foreach (var h in headers)
            {
                if (!existingHeaders.Contains(h))
                    existingHeaders.Add(h);
            }
            headers = existingHeaders;

            bool userFound = false;
            for (int i = 1; i < lines.Count; i++)
            {
                if (lines[i][0] == username)
                {
                    var rowDict = headers.ToDictionary(h => h, h => "");
                    for (int j = 0; j < lines[i].Length; j++)
                        rowDict[headers[j]] = lines[i][j];

                    foreach (var kvp in flatResults)
                        rowDict[kvp.Key] = kvp.Value ?? "";

                    lines[i] = headers.Select(h => rowDict[h]).ToArray();
                    userFound = true;
                    break;
                }
            }

            if (!userFound)
            {
                var newRow = headers.Select(h =>
                    h == "Username" ? username :
                    flatResults.ContainsKey(h) ? flatResults[h] : ""
                ).ToArray();
                lines.Add(newRow);
            }

            var allLines = new List<string[]> { headers.ToArray() };
            allLines.AddRange(lines.Skip(1));
            File.WriteAllLines(csvPath, allLines.Select(r => string.Join(",", r.Select(EscapeCSVField))));
        }
        else
        {
            var rows = new List<string[]>();
            rows.Add(headers.ToArray());
            var row = headers.Select(h =>
                h == "Username" ? username :
                flatResults.ContainsKey(h) ? flatResults[h] : ""
            ).ToArray();
            rows.Add(row);
            File.WriteAllLines(csvPath, rows.Select(r => string.Join(",", r.Select(EscapeCSVField))));
        }
    }

    public void SaveTestResults(string username, string testName, Dictionary<string, object> results)
    {
        if (string.IsNullOrEmpty(username))
        {
            return;
        }

        if (string.IsNullOrEmpty(testName))
        {
            return;
        }

        string folderPath = Path.Combine(Application.persistentDataPath, testName);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        switch (testName)
        {
            case "VerbalComprehensionTest_test2":
                SaveVerbalComprehension(username, results, folderPath, testName);
                break;

            case "MentalMathsTest_test2":
                SaveMentalMaths(username, results, folderPath, testName);
                break;

            case "PsychologicalEvaluationTest_test2": 
                SavePsychologicalEvaluation(username, results, folderPath, testName);
                break;

            case "MemoryRecallTest_test2": 
                SaveMemoryRecall(username, results, folderPath, testName);
                break;

            case "ReactionTimerTest_test2":
                SaveReactionTimer(username, results, folderPath, testName);
                break;
            
            case "NavigationGrid_test2": 
                SaveNavigationGrid(username, results, folderPath, testName);
                break;
            
            case "OverallEvaluation2": 
                SaveOverallEval(username, results, folderPath, testName);
                break;

            default:
                var flat = FlattenResults(results);
                SaveOrUpdateCSV(username, flat, Path.Combine(folderPath, "defaultResults.csv"));
                break;
        }
    }


    private Dictionary<string, string> RemapSurveyHeaders(string testName, Dictionary<string, string> surveyAnswers)
    {
        string[] possibleConfigPaths = {
            Path.Combine(Application.persistentDataPath, "WeightsConfig.json"),
            Path.Combine(Application.persistentDataPath, "ConfigFiles", "WeightsConfig.json"),
            Path.Combine(Application.persistentDataPath, "WeightsConfig.json")
        };

        JObject weightsConfig = null;
        foreach (string path in possibleConfigPaths)
        {
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                weightsConfig = JObject.Parse(json);
                break;
            }
        }

        if (weightsConfig == null)
        {
            return surveyAnswers; 
        }

        var remapped = new Dictionary<string, string>();

        var configSurvey = weightsConfig[testName]?["survey"] as JObject;
        if (configSurvey == null)
        {
            return surveyAnswers;
        }

        int i = 0;
        foreach (var configKey in configSurvey.Properties())
        {
            if (i < surveyAnswers.Count)
            {
                string answer = surveyAnswers.ElementAt(i).Value;
                remapped[configKey.Name] = answer;
            }
            else
            {
                remapped[configKey.Name] = "";
            }
            i++;
        }

        return remapped;
    }


    private void SaveVerbalComprehension(string username, Dictionary<string, object> results, string folderPath, string testName)
    {
        var questionResults = results["questionResults"] as List<QuestionResult>;
        var surveyAnswers = results["surveyAnswers"] as Dictionary<string, string>;

        int totalQuestions = questionResults.Count;
        int totalCorrect = questionResults.Count(q => q.wasCorrect);
        int totalSkipped = questionResults.Count(q => q.isSkipped);
        int totalHints = Convert.ToInt32(results["totalHints"]);
        double avgConfidence = questionResults.Any() ? questionResults.Average(q => q.confidence) : 0;
        int totalTime = Convert.ToInt32(results["totalTime"]);
        double totalPercentage = (totalCorrect / (double)totalQuestions) * 100;

        var flat = new Dictionary<string, string>
        {
            { "totalTime", totalTime.ToString() },
            { "totalCorrect", totalCorrect.ToString() },
            { "totalQuestions", totalQuestions.ToString() },
            { "totalPercentage", $"{totalPercentage:F2}" },
            { "totalSkipped", totalSkipped.ToString() },
            { "totalHints", totalHints.ToString() },
            { "avgConfidence", $"{avgConfidence:F2}" }
        };

        SaveOrUpdateCSV(username, flat, Path.Combine(folderPath, "results.csv"));

        if (surveyAnswers != null)
        {
            var remappedSurvey = RemapSurveyHeaders(testName, surveyAnswers);
            SaveOrUpdateCSV(username, remappedSurvey, Path.Combine(folderPath, "survey.csv"));
        }
    }

    private void SaveMentalMaths(string username, Dictionary<string, object> results, string folderPath, string testName)
    {
        var surveyAnswers = results["surveyAnswers"] as Dictionary<string, string>;
        var allMathResults = results["allMathResults"] as Dictionary<string, SceneLogicManager.AllMathQuestionData>;

        var flat = new Dictionary<string, string>
        {
            { "finalTotalTime", results["finalTotalTime"].ToString() },
            { "finalTotalScore", results["finalTotalScore"].ToString() },
            { "finalTotalQuestions", results["finalTotalQuestions"].ToString() },
            { "finalTotalPercentage", results["finalTotalPercentage"].ToString() }
        };

        if (allMathResults != null)
        {
            foreach (var kvp in allMathResults)
            {
                var task = kvp.Key;
                var data = kvp.Value;

                flat[$"{task}_totalTime"] = data.totalTime.ToString();
                flat[$"{task}_totalPercentage"] = $"{data.totalPercentage:F1}";
            }
        }

        SaveOrUpdateCSV(username, flat, Path.Combine(folderPath, "results.csv"));

        if (surveyAnswers != null)
        {
            var remappedSurvey = RemapSurveyHeaders(testName, surveyAnswers);
            SaveOrUpdateCSV(username, remappedSurvey, Path.Combine(folderPath, "survey.csv"));
        }
    }

    private void SavePsychologicalEvaluation(string username, Dictionary<string, object> results, string folderPath, string testName)
    {
        foreach (var kvp in results)
        {
            if (kvp.Key == "testName") continue;

            if (kvp.Key == "surveyAnswers")
            {
                var survey = kvp.Value as Dictionary<string, string>;
                if (survey != null)
                    SaveOrUpdateCSV(username, survey, Path.Combine(folderPath, "survey.csv"));
                continue;
            }

            var taskResults = kvp.Value as Dictionary<string, string>;
            if (taskResults == null) continue;

            string csvPath = Path.Combine(folderPath, $"{kvp.Key}.csv");
            SaveOrUpdateCSV(username, taskResults, csvPath);
        }
    }

    private void SaveMemoryRecall(string username, Dictionary<string, object> results, string folderPath, string testName)
    {
      
        var surveyAnswers = results["surveyAnswers"] as Dictionary<string, string>;
        foreach (var kvp in results)
        {
            string key = kvp.Key;
            string value = kvp.Value != null ? kvp.Value.ToString() : "NULL";
        }
        var flat = new Dictionary<string, string>
        { 
            { "totalTime", results["totalTime"].ToString() },
            { "totalLevelsCompleted", results["totalScore"].ToString() },
            { "totalLevels", results["totalLevels"].ToString() },
            { "totalPercentage", results["totalPercentage"].ToString() }
        };
        SaveOrUpdateCSV(username, flat, Path.Combine(folderPath, "results.csv"));

        if (surveyAnswers != null)
        {
            var remappedSurvey = RemapSurveyHeaders(testName, surveyAnswers);
            SaveOrUpdateCSV(username, remappedSurvey, Path.Combine(folderPath, "survey.csv"));
        }
    }

    private void SaveReactionTimer(string username, Dictionary<string, object> results, string folderPath, string testName)
    {
        var surveyAnswers = results["surveyAnswers"] as Dictionary<string, string>;
        var flat = new Dictionary<string, string>
        {
            { "avgReactionTime", results["avgReactionTime"].ToString() },
            { "finalReactionTime", results["finalReactionTime"].ToString() },
            { "totalPenalty", results["totalPenalty"].ToString() }
        };
        SaveOrUpdateCSV(username, flat, Path.Combine(folderPath, "results.csv"));

        if (surveyAnswers != null)
        {
            var remappedSurvey = RemapSurveyHeaders(testName, surveyAnswers);
            SaveOrUpdateCSV(username, remappedSurvey, Path.Combine(folderPath, "survey.csv"));
        }
    }
    
    private void SaveNavigationGrid(string username, Dictionary<string, object> results, string folderPath, string testName)
    {
        var surveyAnswers = results["surveyAnswers"] as Dictionary<string, string>;

        var flat = new Dictionary<string, string>
        {
            { "overallTotalTime", results["overallTotalTime"].ToString() },
            { "overallTotalPercentage", results["overallTotalPercentage"].ToString() },
            { "avgAttempts", results["avgAttempts"].ToString() },
            { "totalLevels", results["totalLevels"].ToString() },
            { "totalLevelsCompleted", results["totalLevelsCompleted"].ToString() },
        };

        SaveOrUpdateCSV(username, flat, Path.Combine(folderPath, "results.csv"));

        if (surveyAnswers != null)
        {
            var remappedSurvey = RemapSurveyHeaders(testName, surveyAnswers);
            SaveOrUpdateCSV(username, remappedSurvey, Path.Combine(folderPath, "survey.csv"));
        }
    }

    private void SaveOverallEval(string username, Dictionary<string, object> results, string folderPath, string testName)
    {
        var surveyAnswers = results["surveyAnswers"] as Dictionary<string, string>;

        if (surveyAnswers != null)
        {
            var remappedSurvey = RemapSurveyHeaders(testName, surveyAnswers);
            SaveOrUpdateCSV(username, remappedSurvey, Path.Combine(folderPath, "survey.csv"));
        }
    }
    private string[] ParseCSVLine(string line)
    {
        List<string> fields = new List<string>();
        bool inQuotes = false;
        string current = "";
        foreach (char c in line)
        {
            if (c == '"' && !inQuotes)
                inQuotes = true;
            else if (c == '"' && inQuotes)
                inQuotes = false;
            else if (c == ',' && !inQuotes)
            {
                fields.Add(current);
                current = "";
            }
            else
                current += c;
        }
        fields.Add(current);
        return fields.ToArray();
    }

    private string EscapeCSVField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }

}
