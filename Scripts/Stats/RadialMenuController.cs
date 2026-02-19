using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;

public class RadialMenuController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject contentContainer;
    public GameObject resultTextPrefab; 
    
    [Header("Config")]
    public string testAreaColumnName;

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

    void Start()
    {
        rootPath = Application.persistentDataPath;
        username = PlayerPrefs.GetString("LoggedInUser", "");
        LoadAllTests();
    }

    void LoadAllTests()
    {
        CreateResultBlock($"Username: {username}");
        CreateResultBlock("");

        foreach (string testFolder in testFolders)
        {

            string displayFolder = System.Text.RegularExpressions.Regex.Replace(
                testFolder.Split('_')[0],
                "(?<!^)([A-Z])",
                " $1"
            );

            if (testFolder != testAreaColumnName)
            {
                continue;
            }

            string testPath = Path.Combine(rootPath, testFolder);
            if (!Directory.Exists(testPath))
            {
                continue;
            }

            CreateResultBlock($"{displayFolder}");
            CreateResultBlock("--------------------");

            string resultsPath = Path.Combine(testPath, "results.csv");
            string surveyPath = Path.Combine(testPath, "survey.csv");

            bool foundData = false;

            if (File.Exists(resultsPath))
            {
                foundData = true;
                LoadResultsForTest(testFolder, resultsPath);
            }

            if (!foundData)
            {

                if (testFolder == "PsychologicalEvaluationTest_test2")
                {
                    HandlePsychEvaluationTestDetailed(testPath, displayFolder, username);
                }
                else
                {
                    CreateResultBlock("No CSV data found.");
                }
            }

            CreateResultBlock("");
        }

    }

    private void HandlePsychEvaluationTestDetailed(string testPath, string displayFolder, string username)
    {
        string[] psycheFiles = {
            "EmotionalResponses.csv",
            "MotivationAssessment.csv",
            "Multitasking.csv",
            "PersonalityProfiler.csv",
            "TIPIInventory.csv"
        };

        int completedModules = 0;

        foreach (string file in psycheFiles)
        {
            string path = Path.Combine(testPath, file);
            string moduleName = Path.GetFileNameWithoutExtension(file);

            if (!File.Exists(path))
            {
                CreateResultBlock($"✘ {moduleName}");
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
               
            }

            if (hasData)
            {
                completedModules++;
                CreateResultBlock($"✔ {moduleName}");
            }
            else
            {
                CreateResultBlock($"✘ {moduleName}");
            }
        }

        CreateResultBlock($"Completed: {completedModules}/{psycheFiles.Length}");
    }

    // private void HandlePsychEvaluationTestDetailed(string testPath, string displayFolder, string username)
    // {
    //     string[] psycheFiles = {
    //         "EmotionalResponses.csv",
    //         "MotivationAssessment.csv",
    //         "Multitasking.csv",
    //         "PersonalityProfiler.csv",
    //         "TIPIInventory.csv"
    //     };

    //     int completedModules = 0;

    //     foreach (string file in psycheFiles)
    //     {
    //         string path = Path.Combine(testPath, file);
    //         string moduleName = Path.GetFileNameWithoutExtension(file);

    //         if (!File.Exists(path))
    //         {
    //             CreateResultBlock($"✘ {moduleName}");
    //             continue;
    //         }

    //         bool hasData = false;
    //         try
    //         {
    //             string[] lines = File.ReadAllLines(path);
    //             foreach (string line in lines.Skip(1))
    //             {
    //                 if (line.Contains(username))
    //                 {
    //                     hasData = true;
    //                     break;
    //                 }
    //             }
    //         }
    //         catch { }

    //         if (hasData)
    //         {
    //             completedModules++;
    //             CreateResultBlock($"• {moduleName}");
    //         }
    //         else
    //         {
    //             CreateResultBlock($"• {moduleName}");
    //         }
    //     }

    //     CreateResultBlock($"Completed: {completedModules}/{psycheFiles.Length}");
    // }

    void LoadResultsForTest(string testFolder, string resultsPath)
    {
        var lines = File.ReadAllLines(resultsPath);
        string displayFolder = System.Text.RegularExpressions.Regex.Replace(
            testFolder.Split('_')[0], 
            "(?<!^)([A-Z])", 
            " $1"
        );
        if (lines.Length < 2)
        {
            CreateResultBlock($"Results file for {displayFolder} is empty.");
            return;
        }

        string[] headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();
        bool userFound = false;

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',').Select(v => v.Trim()).ToArray();
            if (values.Length < headers.Length)
                continue;

            string csvUsername = values[0];
            if (csvUsername != username)
                continue;

            userFound = true;

            CreateResultBlock("Test Results:");

            switch (testFolder)
            {
                case "VerbalComprehensionTest_test2":
                    CreateResultBlock($"Total Time: {values[1]}");
                    CreateResultBlock($"Total Correct: {values[2]}");
                    CreateResultBlock($"Total Questions: {values[3]}");
                    CreateResultBlock($"Total Percentage: {values[4]}");
                    CreateResultBlock($"Total Skipped: {values[5]}");
                    CreateResultBlock($"Avg Confidence: {values[6]}");
                    break;

                case "MentalMathsTest_test2":
                    CreateResultBlock($"Final Total Time: {values[1]}");
                    CreateResultBlock($"Final Total Score: {values[2]}");
                    CreateResultBlock($"Final Total Questions: {values[3]}");
                    CreateResultBlock($"Final Total Percentage: {values[4]}");
                    CreateResultBlock($"MentalMathsTest1 Time: {values[5]}");
                    CreateResultBlock($"MentalMathsTest1 %: {values[6]}");
                    CreateResultBlock($"FullMathTest Time: {values[7]}");
                    CreateResultBlock($"FullMathTest %: {values[8]}");
                    break;

                case "MemoryRecallTest_test2":
                    CreateResultBlock($"Total Time: {values[1]}");
                    CreateResultBlock($"Total Score: {values[2]}");
                    CreateResultBlock($"Total Questions: {values[3]}");
                    CreateResultBlock($"Total Percentage: {values[4]}");
                    break;

                case "ReactionTimerTest_test2":
                    CreateResultBlock($"Average Reaction Time: {values[1]}");
                    CreateResultBlock($"Final Reaction Time: {values[2]}");
                    CreateResultBlock($"Total Penalty: {values[3]}");
                    break;
                case "NavigationGrid_test2":
                    CreateResultBlock($"Total Time: {values[1]}");
                    CreateResultBlock($"Total Score: {values[5]}");
                    CreateResultBlock($"Total Questions: {values[4]}");
                    CreateResultBlock($"Total Percentage: {values[2]}");
                    break;
            }

            CreateResultBlock("");
            break;
        }

        if (!userFound)
            CreateResultBlock($"No results found for user \"{username}\" in {displayFolder}.");
    }

    void LoadSurveyData(string testFolder, string surveyPath)
    {
        string displayFolder = System.Text.RegularExpressions.Regex.Replace(
            testFolder.Split('_')[0], 
            "(?<!^)([A-Z])", 
            " $1"
        );

        var lines = File.ReadAllLines(surveyPath);
        if (lines.Length < 2)
        {
            CreateResultBlock($"Survey file for {displayFolder} is empty.");
            return;
        }

        string[] headers = lines[0].Split(',').Select(h => h.Trim()).ToArray();
        bool userFound = false;

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',').Select(v => v.Trim()).ToArray();
            if (values.Length < headers.Length)
                continue;

            string csvUsername = values[0];
            if (csvUsername != username)
                continue;

            userFound = true;

            CreateResultBlock("Survey Responses:");
            for (int j = 1; j < headers.Length; j++)
            {
                string question = headers[j];
                string answer = values[j];
                CreateResultBlock($"{question}: {answer}");
            }
            CreateResultBlock("");
            break;
        }

        if (!userFound)
            CreateResultBlock($"No survey found for user \"{username}\" in {displayFolder}.");
    }

    private void CreateResultBlock(string text)
    {
        GameObject newTextObj = Instantiate(resultTextPrefab, contentContainer.transform);
        TMP_Text tmp = newTextObj.GetComponent<TMP_Text>();
        tmp.text = text;
    }
}

