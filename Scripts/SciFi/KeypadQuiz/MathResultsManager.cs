using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public class MathResultsManager : MonoBehaviour
{
    public static MathResultsManager Instance;

    public Dictionary<string, AllMathQuestionData> allMathResults = new Dictionary<string, AllMathQuestionData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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
}
