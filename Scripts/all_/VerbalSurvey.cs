using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.SceneManagement;

public class VerbalSurvey : MonoBehaviour
{

    [System.Serializable]
    public class SurveyQuestion
    {
        [TextArea] public string questionText; 
        public List<string> options = new List<string>();
    }

    public static VerbalSurvey Instance;
    public TextMeshProUGUI questionText;
    public GameObject toggleOptionPrefab;
    public ToggleGroup answerOptionsGroup;
    public Button nextButton;
    public Button submitButton;

    [Header("Survey Setup")]
    public List<SurveyQuestion> surveyQuestions = new List<SurveyQuestion>();
    private int currentQuestionIndex = 0;
    private List<string> userAnswers = new List<string>();
    public Scrollbar surveySlider;

    public Dictionary<string, string> finalSurveyAnswers = new Dictionary<string, string>();

    void Awake()
    {
        Instance = this;
	}

    void Start()
    {
        DisplayCurrentQuestion();

        nextButton.onClick.AddListener(NextQuestion);
        submitButton.onClick.AddListener(SubmitSurvey);
        submitButton.gameObject.SetActive(false);
    }

    void LoadQuestions()
    {
        surveyQuestions = new List<SurveyQuestion>
        {
            new SurveyQuestion { questionText = "Overall, how mentally demanding was this task?", options = new List<string> {
            "Extremely challenging",
            "Challenging",
            "Moderately difficult",
            "Slightly difficult",
            "Very easy" } },
            new SurveyQuestion { questionText = "How would you describe your emotional state during the task?", options = new List<string> {
            "Calm and in control",
            "Slightly tense but manageable",
            "Neutral",
            "Stressed but still performing",
            "Overwhelmed" } },
            new SurveyQuestion { questionText = "Did the time spent solving riddles (before each SAT question) impact how you approached the main questions?", options = new List<string> {
            "Yes, it made me more prepared and engaged",
            "Yes, it made me more tired or distracted",
            "Neutral",
            "No impact, I treated them separately",
            "I skipped riddles, so this didn’t apply" } },
            new SurveyQuestion { questionText = "Please enter your current heart rate (BPM).", options = new List<string> {
            "Riddle-based",
            "Direct questions" } },
            new SurveyQuestion { questionText = "On a scale of 1–10, how high is your stress level right now?", options = new List<string> {
            "Riddle-based",
            "Direct questions" } },
            new SurveyQuestion { questionText = "On a scale of 1–10, how strong is your focus level right now?", options = new List<string> {
            "Riddle-based",
            "Direct questions" } },
            new SurveyQuestion { questionText = "Compared to before the task, do you feel:", options = new List<string> {
            "Less stressed",
            "About the same",
            "More stressed" } },
            new SurveyQuestion { questionText = "How familiar did this school/university environment feel to you?", options = new List<string> {
            "Very familiar (like my own past or current studies)",
            "Somewhat familiar",
            "Neutral",
            "Somewhat unfamiliar",
            "Very unfamiliar" } },
            new SurveyQuestion { questionText = "Did being in this school/university setting make you reflect on your own education or career journey?", options = new List<string> {
            "Strongly yes",
            "Somewhat yes",
            "Neutral",
            "Not really",
            "Not at all" } },
            new SurveyQuestion { questionText = "While completing the task, did you imagine yourself in:", options = new List<string> {
            "A high school environment",
            "A university environment",
            "A workplace/career environment",
            "None of these" } },
            new SurveyQuestion { questionText = "How often did you use hints for riddles?", options = new List<string> {
            "Always",
            "Often",
            "Sometimes",
            "Rarely",
            "Never" } },
            new SurveyQuestion { questionText = "Did you skip riddles to move forward faster?", options = new List<string> {
            "Yes, frequently",
            "Yes, occasionally",
            "No, I tried to solve them all" } },
            new SurveyQuestion { questionText = "What best describes your approach to the most difficult SAT-style questions?", options = new List<string> {
            "I pushed through even when stressed",
            "I took my time but finished",
            "I preferred to move on mentally if I felt stuck",
            "I lost motivation when stuck" } },
            new SurveyQuestion { questionText = "Did this task make you feel more aware of your strengths and weaknesses in problem-solving?", options = new List<string> {
            "Strongly yes",
            "Somewhat yes",
            "Neutral",
            "Not really",
            "Not at all" } },
            new SurveyQuestion { questionText = "How much do you think your performance in this task reflects your potential in a future career?", options = new List<string> {
            "Very strongly reflects it",
            "Somewhat reflects it",
            "Neutral",
            "Not much",
            "Not at all" } },
            new SurveyQuestion { questionText = "Did the setting (school/university) feel like a place where you could imagine your future self succeeding?", options = new List<string> {
            "Strongly yes",
            "Somewhat yes",
            "Neutral",
            "Not really",
            "Not at all" } }
        };
        
    }

    void DisplayCurrentQuestion()
    {
        foreach (Transform child in answerOptionsGroup.transform)
        {
            Destroy(child.gameObject);
        }

        var currentQ = surveyQuestions[currentQuestionIndex];
        questionText.text = currentQ.questionText;

        foreach (var option in currentQ.options)
        {
            GameObject toggleObj = Instantiate(toggleOptionPrefab, answerOptionsGroup.transform);
            Toggle toggle = toggleObj.GetComponent<Toggle>();
            Text label = toggleObj.GetComponentInChildren<Text>();
            label.text = option;
        }
    }

    void NextQuestion()
    {
        surveySlider.value = 1;
        string answer = GetSelectedAnswer();
        if (string.IsNullOrEmpty(answer)) return;

        userAnswers.Add(answer);
        currentQuestionIndex++;

        if (currentQuestionIndex >= surveyQuestions.Count)
        {
            nextButton.gameObject.SetActive(false);
            submitButton.gameObject.SetActive(true);
        }
        else
        {
            DisplayCurrentQuestion();
        }
    }
    
    string GetSelectedAnswer()
    {
        foreach (Toggle toggle in answerOptionsGroup.GetComponentsInChildren<Toggle>())
        {
            if (toggle.isOn)
            {
                return toggle.GetComponentInChildren<Text>().text;
            }
        }
        return null;
    }

    void SubmitSurvey()
    {
        for (int i = 0; i < surveyQuestions.Count; i++)
        {
            finalSurveyAnswers.Add(surveyQuestions[i].questionText, userAnswers[i]);
        }

        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "all_")
        {

            VerbalTaskManager.Instance.SaveResultsToDb(finalSurveyAnswers);
            return;
        }
        if (currentSceneName == "SciFi_Warehouse")
        {
            SceneLogicManager.Instance.AccessShowScoreboard(finalSurveyAnswers);
            return;
        }
        if (currentSceneName == "MemoryRecall")
        {
            string user_id = PlayerPrefs.GetString("LoggedInUser");
            string quiteORend = PlayerPrefs.GetString($"{user_id}_QuitOrEnd", "");

            switch (quiteORend)
            {
                case "Quit":
                    TaskManager.Instance.ShowQuitScoreboard(finalSurveyAnswers);
                    break;
                case "End":
                    TaskManager.Instance.ShowScoreboard(finalSurveyAnswers);
                    break;
                default:
                    break;
            }

            return;
        }

        if (currentSceneName == "ReactionTimer" || currentSceneName == "PschyeTests")
        {
            TaskManager.Instance.ShowScoreboard(finalSurveyAnswers);

            return;
        }
        
        if (currentSceneName == "NavigationTestScene 1")
        {
            MainSceneLogic.Instance.ShowScoreboard(finalSurveyAnswers);
                
            return;
        }
    }

    public Dictionary<string, string> GetResults()
    {
        return finalSurveyAnswers;
    }
}
