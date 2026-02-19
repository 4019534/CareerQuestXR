using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using System.IO;
using System;

public class ScoreboardRenderer : MonoBehaviour
{
    public static ScoreboardRenderer Instance;

    public GameObject contentContainer; 
    public GameObject resultTextPrefab; 

    public TextMeshProUGUI debugText;

    void Awake()
    {
        Instance = this;
    }
    public void DisplayResults(string testArea, Dictionary<string, object> finalResults, float totalTime, string score, Dictionary<string, string> surveyAnswers, List<QuestionResult> questionResults, List<MathQuestionResult> questionResultsMath)
    {


        foreach (Transform child in contentContainer.transform)
        {
            Destroy(child.gameObject);
        }

        CreateResultBlock("Survey Answers:");

        if (surveyAnswers == null)
        {

            CreateResultBlock($"No Survey Results for this test.");
        }
        else
        {
            foreach (var entry in surveyAnswers)
            {
                CreateResultBlock($"• {entry.Key}: {entry.Value}");
            }

        }

        CreateResultBlock("Question Results:");

        if (questionResults == null && questionResultsMath == null)
        {

            if (finalResults == null)
            {
                CreateResultBlock($"No Results found for this test.");

                return;
            }

            if (finalResults.ContainsKey("allMathResults"))
            {
                var allMath = finalResults["allMathResults"] as Dictionary<string, SceneLogicManager.AllMathQuestionData>;
                if (allMath != null)
                {

                    foreach (var kvp in allMath)
                    {
                        var data = kvp.Value;
                        CreateResultBlock(
                            $"--- {data.testArea} ---\n" +
                            $"Time Taken: {data.totalTime:F1}s\n" +
                            $"Correct: {data.totalCorrect}/{data.totalQuestions}\n" +
                            $"Percentage: {data.totalPercentage:F1}%\n"
                        );

                        if (data.questionAndAnswers != null)
                        {
                            var objDict = data.questionAndAnswers as Dictionary<string, object>;
                            if (objDict != null)
                            {
                            
                                if (objDict.ContainsKey("finalQuestionAndAnswers"))
                                {
                                    var qaDict = objDict["finalQuestionAndAnswers"] as Dictionary<string, string>;
                                    if (qaDict != null)
                                    {
                                        foreach (var qa in qaDict)
                                        {
                                            CreateResultBlock($"Q: {qa.Key} | A: {qa.Value}");
                                        }
                                    }
                                }

                                if (objDict.ContainsKey("levelBreakdown"))
                                {
                                    var levelList = objDict["levelBreakdown"] as List<MathQuestionResult>;
                                    if (levelList != null)
                                    {
                                        foreach (var level in levelList)
                                        {
                                            CreateResultBlock(
                                                $"Level {level.levelNumber} ({level.testType})\n" +
                                                $"  Correct: {level.correctAnswers}/{level.totalQuestions}\n" +
                                                $"  Time: {level.levelTime:F1}s"
                                            );

                                            if (level.questionResults != null && level.questionResults.Count > 0)
                                            {
                                                foreach (var q in level.questionResults)
                                                {
                                                    CreateResultBlock(
                                                        $"    SubQ (Level {q.levelNumber}) | " +
                                                        $"Correct: {q.correctAnswers}/{q.totalQuestions} | " +
                                                        $"Time: {q.levelTime:F1}s"
                                                    );
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (finalResults.ContainsKey("finalTotalTime") && finalResults.ContainsKey("finalTotalScore") && finalResults.ContainsKey("finalTotalQuestions") && finalResults.ContainsKey("finalTotalPercentage"))
            {
                foreach (var key in new[] { "finalTotalTime", "finalTotalScore", "finalTotalQuestions", "finalTotalPercentage" })
                {
                    if (finalResults.ContainsKey(key))
                    {
                        object value = finalResults[key];
                        string typeName = value != null ? value.GetType().Name : "null";
                    }
                }
                float overallTime = finalResults.ContainsKey("finalTotalTime") ? Convert.ToSingle(finalResults["finalTotalTime"]) : 0f;
                int overallScore = finalResults.ContainsKey("finalTotalScore") ? Convert.ToInt32(finalResults["finalTotalScore"]) : 0;
                int overallQuestions = finalResults.ContainsKey("finalTotalQuestions") ? Convert.ToInt32(finalResults["finalTotalQuestions"]) : 0;
                float overallPercentage = finalResults.ContainsKey("finalTotalPercentage") ? Convert.ToSingle(finalResults["finalTotalPercentage"]) : 0f;

                CreateResultBlock(
                    $"=== OVERALL TOTALS ===\n" +
                    $"Total Time: {overallTime}s\n" +
                    $"Total Score: {overallScore}" +
                    $"Total Questions: {overallQuestions}\n" +
                    $"Total Percentage: {overallPercentage}%"
                );

                UserAuth.Instance.SaveTestResults(PlayerPrefs.GetString("LoggedInUser"), testArea, finalResults);
                return;

            }

            if (finalResults.ContainsKey("testName") && finalResults["testName"] == "MemoryRecallTest_test2")
            {
                if (finalResults.ContainsKey("totalTime") && finalResults.ContainsKey("totalScore") && finalResults.ContainsKey("totalQuestions") && finalResults.ContainsKey("totalPercentage"))
                {
                    foreach (var key in new[] { "totalTime", "totalScore", "totalQuestions", "totalPercentage" })
                    {
                        if (finalResults.ContainsKey(key))
                        {
                            object value = finalResults[key];
                            string typeName = value != null ? value.GetType().Name : "null";
                        }
                    }

                    float overallTime = finalResults.ContainsKey("totalTime") ? Convert.ToSingle(finalResults["totalTime"]) : 0f;
                    int overallScore = finalResults.ContainsKey("totalScore") ? Convert.ToInt32(finalResults["totalScore"]) : 0;
                    int overallQuestions = finalResults.ContainsKey("totalQuestions") ? Convert.ToInt32(finalResults["totalQuestions"]) : 0;
                    float overallPercentage = finalResults.ContainsKey("totalPercentage") ? Convert.ToSingle(finalResults["totalPercentage"]) : 0f;

                    CreateResultBlock(
                        $"=== OVERALL TOTALS for MEMORY RECALL TEST ===\n" +
                        $"Total Time: {overallTime}s\n" +
                        $"Total Score: {overallScore}" +
                        $"Total Questions: {overallQuestions}\n" +
                        $"Total Percentage: {overallPercentage}%"
                    );

                    UserAuth.Instance.SaveTestResults(PlayerPrefs.GetString("LoggedInUser"), testArea, finalResults);
                    return;
                }
            }

            if (finalResults.ContainsKey("testName") && finalResults["testName"] == "ReactionTimerTest_test2")
            {
             
                if (finalResults.ContainsKey("avgReactionTime") && finalResults.ContainsKey("finalReactionTime") && finalResults.ContainsKey("totalPenalty"))
                {
         
                    foreach (var key in new[] { "avgReactionTime", "finalReactionTime", "totalPenalty" })
                    {
                        if (finalResults.ContainsKey(key))
                        {
                            object value = finalResults[key];
                            string typeName = value != null ? value.GetType().Name : "null";
                        }
                    }

                    float avgReactionTime = finalResults.ContainsKey("avgReactionTime") ? Convert.ToSingle(finalResults["avgReactionTime"]) : 0f;
                    float finalReactionTime = finalResults.ContainsKey("finalReactionTime") ? Convert.ToSingle(finalResults["finalReactionTime"]) : 0f;
                    float totalPenalty = finalResults.ContainsKey("totalPenalty") ? Convert.ToSingle(finalResults["totalPenalty"]) : 0f;

                    CreateResultBlock(
                        $"=== OVERALL TOTALS for REACTION TIME TEST ===\n" +
                        $"Avg Reaction Time: {avgReactionTime}s\n" +
                        $"Final Reaction Time: {finalReactionTime}" +
                        $"Total Penalty: {totalPenalty}\n"

                    );

                    UserAuth.Instance.SaveTestResults(PlayerPrefs.GetString("LoggedInUser"), testArea, finalResults);
                    return;
                }
            }

            if (finalResults.ContainsKey("testName") && finalResults["testName"] == "PsychologicalEvaluationTest_test2")
            {
                if (finalResults.ContainsKey("MotivationAssessment") && finalResults.ContainsKey("Multitasking") && finalResults.ContainsKey("EmotionalResponses") && finalResults.ContainsKey("PersonalityProfiler"))
                {
                    foreach (var taskKey in new[] { "MotivationAssessment", "Multitasking", "EmotionalResponses", "PersonalityProfiler" })
                    {
                        CreateResultBlock($"<b>{taskKey}</b>");

                        var taskResults = finalResults[taskKey] as Dictionary<string, string>;
                        if (taskResults != null)
                        {
                            foreach (var entry in taskResults)
                            {
                                CreateResultBlock($"• {entry.Key}: {entry.Value}");
                            }
                        }
                        else
                        {
                            CreateResultBlock("No results found for this task.");
                        }

                        CreateResultBlock("=======================");
                        CreateResultBlock("");
                    }

                    UserAuth.Instance.SaveTestResults(PlayerPrefs.GetString("LoggedInUser"), testArea, finalResults);
                    return;
                }
            }

            // Display each test area's results
            if (finalResults.ContainsKey("testName") && finalResults["testName"] == "NavigationGrid_test2")
            {
                var allLevels = finalResults["allLevelResults"] as Dictionary<int, MainSceneLogic.LevelResult>;
                if (allLevels != null)
                {
                    foreach (var kvp in allLevels)
                    {
                        MainSceneLogic.LevelResult data = kvp.Value;
                        CreateResultBlock(
                            $"--- Level {data.level} ---\n" +
                            $"Time Taken: {data.timeTaken:F1}s\n" +
                            $"Attempts: {data.attempts}\n" +
                            $"Percentage Completed: {data.percentageCompleted * 100f:F1}%\n"
                        );
                    }

                }

                if (finalResults.ContainsKey("overallTotalTime") && finalResults.ContainsKey("overallTotalPercentage") && finalResults.ContainsKey("avgAttempts") && finalResults.ContainsKey("totalLevels"))
                {
                    foreach (var key in new[] { "overallTotalTime", "overallTotalPercentage", "avgAttempts", "totalLevels" })
                    {
                        if (finalResults.ContainsKey(key))
                        {
                            object value = finalResults[key];
                            string typeName = value != null ? value.GetType().Name : "null";
                        }
                    }

                    float overallTotalTime = finalResults.ContainsKey("overallTotalTime") ? Convert.ToSingle(finalResults["overallTotalTime"]) : 0f;
                    float overallTotalPercentage = finalResults.ContainsKey("overallTotalPercentage") ? Convert.ToSingle(finalResults["overallTotalPercentage"]) : 0f;
                    int avgAttempts = finalResults.ContainsKey("avgAttempts") ? Convert.ToInt32(finalResults["avgAttempts"]) : 0;
                    int totalLevels = finalResults.ContainsKey("totalLevels") ? Convert.ToInt32(finalResults["totalLevels"]) : 0;

                    CreateResultBlock(
                        $"=== OVERALL TOTALS for NAVIGATION GRID TEST ===\n" +
                        $"Overall Total Time: {overallTotalTime}s\n" +
                        $"Avg Attempts: {avgAttempts}\n" +
                        $"Overall Percentage Completed: {overallTotalPercentage}s\n" +
                        $"Total Levels: {totalLevels}\n"

                    );

                    UserAuth.Instance.SaveTestResults(PlayerPrefs.GetString("LoggedInUser"), testArea, finalResults);
                    return;
                }
            }

        }

        if (questionResults == null && questionResultsMath != null)
        {
            foreach (var result in questionResultsMath)
            {
                CreateResultBlock(
                    $"QLevel Number: {result.levelNumber}\n" +
                    $"Correct: {result.correctAnswers} | Time: {result.levelTime:F1}s"
                );
            }

        }

        if (questionResultsMath == null && questionResults != null)
        {
            foreach (var result in questionResults)
            {
                CreateResultBlock(
                    $"Q{result.questionId}: {result.selectedAnswer} (Correct: {result.correctAnswer})\n" +
                    $"Correct No.: {result.wasCorrect} | Confidence: {result.confidence}/5 | Time: {result.timeTaken:F1}s | Hints: {result.hintCount}");
            }
        }
        CreateResultBlock($"Overall Totals:");
        CreateResultBlock($"Total Time: {totalTime}");
        CreateResultBlock($"Final Score: {score}");


        UserAuth.Instance.SaveTestResults(PlayerPrefs.GetString("LoggedInUser"), testArea, finalResults);

    }

    private void CreateResultBlock(string text)
    {
        GameObject newTextObj = Instantiate(resultTextPrefab, contentContainer.transform);
        TMP_Text tmp = newTextObj.GetComponent<TMP_Text>();
        tmp.text = text;
    }
}
