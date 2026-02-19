using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class TestTracker : MonoBehaviour
{

    int completedCount = 0;
    int totalTests = 5; 
    private string currentUsername;
    private string filePath;

    public Button loadProgressButton;

    void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "test.csv");
        currentUsername = PlayerPrefs.GetString("LoggedInUser", "");
        loadProgressButton.onClick.AddListener(trackProgress);
    }

    void trackProgress()
    {
        FileInfo fileInfo = new FileInfo(filePath);
    }

    private int GetColumnStartForTestArea(string testArea)
    {
        Dictionary<string, int> testAreaColumns = new Dictionary<string, int>
        {
            { "CognitiveSkillsScene", 3 },
            { "EmotionalResilienceScene", 4 },
            { "CreativityTestScene", 5 },
            { "VocationalSkillsTestScene", 6 },
            { "PsychologicalFactorsScene", 7 },
            { "Background", 8 },
            { "Avatar", 9 }
        };

        return testAreaColumns.ContainsKey(testArea) ? testAreaColumns[testArea] : -1;
    }  
}
