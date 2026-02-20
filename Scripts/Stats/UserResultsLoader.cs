using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class UserResultsLoader
{

    public static Dictionary<string, Dictionary<string, object>> LoadUserResults(string username, string rootPath)
    {
        var results = new Dictionary<string, Dictionary<string, object>>();

        string[] testFolders = {
            "VerbalComprehensionTest_test2",
            "MentalMathsTest_test2",
            "MemoryRecallTest_test2",
            "ReactionTimerTest_test2",
            "NavigationGrid_test2",
            "PsychologicalEvaluationTest_test2",
            "OverallEvaluation" 
        };

        foreach (string testFolder in testFolders)
        {
            string testPath = Path.Combine(rootPath, testFolder);
  
            if (!Directory.Exists(testPath))
            {
                continue;

            } 
            if (testFolder == "PsychologicalEvaluationTest_test2")
            {
                var psycheData = new Dictionary<string, object>();
                string[] psycheFiles = { "EmotionalResponses.csv", "MotivationAssessment.csv", "Multitasking.csv", "PersonalityProfiler.csv", "TIPIInventory.csv" };

                foreach (string file in psycheFiles)
                {
                    string path = Path.Combine(testPath, file);
                    if (!File.Exists(path)) 
                    {
                        continue;
                    }

                    var dict = ParsePsychEvalCSV(path, username);
                    string key = Path.GetFileNameWithoutExtension(file);
                    if (dict != null && dict.Count > 0)
                    {
                        psycheData[key] = dict;
                    }
                    
                }
                if (psycheData.Count > 0)
                    results[testFolder] = psycheData;
            }

            else
            {
                string resultsPath = Path.Combine(testPath, "results.csv");
                string surveyPath = Path.Combine(testPath, "survey.csv");
                var combinedData = new Dictionary<string, object>();

                if (File.Exists(resultsPath))
                {
                    var resultsDict = ParseCSV(resultsPath, username);
                    if (resultsDict != null && resultsDict.Count > 0)
                    {
                        combinedData["results"] = resultsDict;
                    }
                    
                       
                }

                if (File.Exists(surveyPath))
                {
                    var surveyDict = ParseSurveyCSV(surveyPath, username);
                    if (surveyDict != null && surveyDict.Count > 0)
                    {
                        combinedData["survey"] = surveyDict;
                    }
                    
                        
                }
                if (combinedData.Count > 0)
                    results[testFolder] = combinedData;
            }
        }

        return results;
    }

    private static List<string> ParseCSVLine(string line)
    {
        var fields = new List<string>();
        bool inQuotes = false;
        var field = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '\"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                {
                    field.Append('\"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(field.ToString());
                field.Clear();
            }
            else
            {
                field.Append(c);
            }
        }

        fields.Add(field.ToString());
        return fields;
    }

    public static Dictionary<string, string> ParsePsychEvalCSV(string path, string username)
    {
        var dict = new Dictionary<string, string>();
        if (!File.Exists(path))
        {
            return dict;
        }

        string[] lines = File.ReadAllLines(path);
        if (lines.Length < 2)
        {
            return dict;
        }

        string[] headers = SplitCsvLine(lines[0]);

        int rowCount = 0;
        foreach (string line in lines.Skip(1))
        {
            rowCount++;
            string[] cols = SplitCsvLine(line);

            if (cols.Length != headers.Length)
            {
                continue;
            }

            string rowUser = cols[0].Trim('"');
            if (!rowUser.Equals(username))
            {
                continue;
            }

            for (int i = 1; i < cols.Length; i++)
            {
                string key = headers[i].Trim('"');
                string val = cols[i].Trim('"');
                string namespacedKey = $"{Path.GetFileName(Path.GetDirectoryName(path))}::{key}";
                dict[namespacedKey] = val;
            }
        }
        return dict;
    }

    private static string[] SplitCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var current = new System.Text.StringBuilder();

        foreach (char c in line)
        {
            if (c == '\"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());
        return result.ToArray();
    }

    private static Dictionary<string, string> ParseSurveyCSV(string path, string username)
    {
        var dict = new Dictionary<string, string>();
        if (!File.Exists(path))
        {
            return dict;
        }

        string[] lines = File.ReadAllLines(path);
        if (lines.Length < 2) return dict;

        var headers = ParseCSVLine(lines[0]);

        foreach (string rawLine in lines.Skip(1))
        {
            var cols = ParseCSVLine(rawLine);

            if (cols.Count != headers.Count)
            {
                continue;
            }

            string rowUser = cols[0].Trim('"');
            if (!rowUser.Equals(username))
            {
                continue;
            }

            for (int i = 1; i < cols.Count; i++)
            {
                string key = headers[i].Trim('"');
                string val = cols[i].Trim('"');
                string namespacedKey = $"{Path.GetFileName(Path.GetDirectoryName(path))}::{key}";
                dict[namespacedKey] = val;
            }
        }
        return dict;
    }
    private static Dictionary<string, object> ParseCSV(string path, string username)
    {
        var dict = new Dictionary<string, object>();
        string[] lines = File.ReadAllLines(path);

        if (lines.Length < 2) return dict;

        string[] headers = lines[0].Split(',');

        foreach (string line in lines.Skip(1))
        {
            string[] cols = line.Split(',');
            if (cols.Length != headers.Length) continue;

            if (!cols[0].Equals(username)) continue; 

            for (int i = 1; i < cols.Length; i++)
            {
                string key = headers[i];
                string val = cols[i];
                string namespacedKey = $"{Path.GetFileName(Path.GetDirectoryName(path))}::{key}";

                if (float.TryParse(val, out float f))
                    dict[namespacedKey] = f;
                else
                    dict[namespacedKey] = val;
            }
        }
        return dict;
    }
}