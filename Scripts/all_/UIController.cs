using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIController : MonoBehaviour
{
    public static UIController Instance;
    public GameObject[] riddleTriggers;
    public GameObject startModePanel, riddlePanel, questionPanel, questionPartnerPanel, surveyPanel, scoreboardPanel, hintButtonCanvas;
    public Button questionSubmitButton;
    public TMP_Text questionText, riddleHintText, warningText;
    public List<Button> answerButtons;
    private Button currentlySelectedButton = null;
    public Color defaultAnswerColor = Color.white;
    public Color selectedAnswerColor = new Color(0.7f, 0.85f, 1f); 
    public Color selectedColor = new Color(0.7f, 0.85f, 1f);
    public Slider confidenceSlider;
    public Button hintButton;
    public TMP_Text hintButtonText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI totalTimeText;
    public TextMeshProUGUI surveyAnswersText;
    public TextMeshProUGUI questionResultsText;
    public TMP_Text questionRiddle;
    private bool isQuestionActive = false;
    public List<float> taskTimes = new List<float>();
    private float taskStartTime;
    public TMP_Text timerText;
 
    void Awake() => Instance = this;

    void Start()
    {
        DisableRiddleColliders();
        ShowStartModePanel();
        if (questionSubmitButton != null)
            questionSubmitButton.interactable = false;

        confidenceSlider.onValueChanged.AddListener(OnConfidenceChanged);
        }

    void Update()
    {
        if (isQuestionActive)
        {
            float remaining = VerbalTaskManager.Instance.timer;
            timerText.text = $"Time: {FormatTime(Mathf.Max(remaining, 0f))}";
        }
        else
        {
            if (timerText != null)
                timerText.text = "Time: 00:00:00";
        }
    }


    private string FormatTime(float timeInSeconds)
    {
        int hours = Mathf.FloorToInt(timeInSeconds / 3600f);
        int minutes = Mathf.FloorToInt((timeInSeconds % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        
        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }
    public void StartTaskTimer()
    {
        taskStartTime = Time.time;
        isQuestionActive = true;
        Debug.Log("Task Timer Started!");
        
    }

    public void StopTaskTimerAndSave()
    {
        if (isQuestionActive)
        {
            float elapsed = Time.time - taskStartTime;
            taskTimes.Add(elapsed);
        }
        isQuestionActive = false;
    }

    void DisableRiddleColliders()
    {
        foreach (GameObject trigger in riddleTriggers)
        {
            Transform torusCol = trigger.transform.Find("Torus_COL");
            if (torusCol != null)
            {
                torusCol.gameObject.SetActive(false);
            }
            else
            {
                continue;
            }
        }
    }

    public void EnableCurrentRiddleZone()
    {
        int targetQuestionId = VerbalTaskManager.Instance.currentQuestionIndex + 1;

        foreach (GameObject trigger in riddleTriggers)
        {
            RiddleZone riddleZone = trigger.GetComponent<RiddleZone>();
            if (riddleZone == null)
            {
                continue;
            }

            Transform torusCol = trigger.transform.Find("Torus_COL");
            if (torusCol == null)
            {
                continue;
            }

            if (riddleZone.questionId == targetQuestionId)
            {
                torusCol.gameObject.SetActive(true);
            }
            else
            {
                torusCol.gameObject.SetActive(false);
            }
        }
    }


 


    public void ShowStartModePanel()
    {
        startModePanel.SetActive(true);
        riddlePanel.SetActive(false);
        questionPanel.SetActive(false);
        questionPartnerPanel.SetActive(false);
        scoreboardPanel.SetActive(false);
    }

    public void OnHintButtonPressed()
    {
        VerbalTaskManager.Instance.totalHints++;
        var q = VerbalTaskManager.Instance.questions[VerbalTaskManager.Instance.currentQuestionIndex];

        int nextHintIndex = q.hintCount + 1;
        if (nextHintIndex < q.hints.Count)
        {
            string currentHint = q.hints[nextHintIndex];
            riddleHintText.text = currentHint;
            q.usedHint = true;
            q.hintCount++;
        }

        if (q.hintCount >= 3)
        {
            hintButton.interactable = false;
            hintButtonText.text = "No More Hints";
        }
    }

    public void ShowRiddle(QuestionData data)
    {
        riddlePanel.SetActive(true);
        questionRiddle.text = $"Riddle {data.id}";
        questionPanel.SetActive(false);
        questionPartnerPanel.SetActive(false);

        if (data.hints.Count > 0)
        {
            riddleHintText.text = data.hints[0];
        }
        
        hintButton.interactable = true;
        hintButtonText.text = "Need a Hint?";
        StopTaskTimerAndSave();
    }

    public void OnConfidenceChanged(float value)
    {
        if (value >= 1f)
            questionSubmitButton.interactable = true;
        else
            questionSubmitButton.interactable = false;
    }

    public void ShowQuestion(QuestionData data)
    {

        selectedAnswer = -1;

        if (currentlySelectedButton != null)
        {
            Image oldImg = currentlySelectedButton.GetComponent<Image>();
            if (oldImg != null)
            {
                oldImg.color = defaultAnswerColor;
            }

            currentlySelectedButton = null;
        }

        questionText.text = data.question;
        for (int i = 0; i < answerButtons.Count; i++)
        {
            int index = i; 
            answerButtons[i].GetComponentInChildren<TMP_Text>().text = data.answers[i];
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
        }

        riddlePanel.SetActive(false);
        hintButtonCanvas.SetActive(false);
        questionPanel.SetActive(true);
        questionPartnerPanel.SetActive(true);
        StartTaskTimer();
    }

    void HighlightButton(Button btn)
    {
        var colors = btn.colors;
        colors.normalColor = new Color(0.6f, 0.8f, 1f); 
        btn.colors = colors;
    }

    void ResetButtonColor(Button btn)
    {
        var colors = btn.colors;
        colors.normalColor = Color.white; 
        btn.colors = colors;
    }

    int selectedAnswer = -1;
    void OnAnswerSelected(int index)
    {

        selectedAnswer = index;

        if (currentlySelectedButton != null)
        {
            Image oldImg = currentlySelectedButton.GetComponent<Image>();
            if (oldImg != null)
            {
                oldImg.color = defaultAnswerColor;
            }
        }

        Button newBtn = answerButtons[index];
        Image newImage = newBtn.GetComponent<Image>();

        if (newImage != null)
        {
            newImage.color = selectedAnswerColor;
        }

        currentlySelectedButton = newBtn;
        warningText.gameObject.SetActive(false);
    }

    public void OnSubmitAnswer()
    {

        if (selectedAnswer == -1)
        {
            warningText.gameObject.SetActive(true);
            return; 
        }

        warningText.gameObject.SetActive(false);
        hintButton.interactable = true;
        hintButtonText.text = "Need a Hint?";
        int confidence = Mathf.RoundToInt(confidenceSlider.value);
        VerbalTaskManager.Instance.SubmitAnswer(selectedAnswer, confidence);
        selectedAnswer = -1;

        if (currentlySelectedButton != null)
        {
            currentlySelectedButton.GetComponent<Image>().color = defaultAnswerColor;
            currentlySelectedButton = null;
        }
    }

    public int GetSelectedAnswer() => selectedAnswer;
    public int GetCurrentConfidence() => Mathf.RoundToInt(confidenceSlider.value);
    public bool HasSelectedAnswer() => selectedAnswer != -1;
    public void ShowSurvey()
    {
        surveyPanel.SetActive(true);
        startModePanel.SetActive(false);
        riddlePanel.SetActive(false);
        questionPanel.SetActive(false);
        questionPartnerPanel.SetActive(false);
        StopTaskTimerAndSave();
        scoreboardPanel.SetActive(false);
    }

    public void ShowScoreboard(string testArea, Dictionary<string, object> finalResults, string score,float totalTime,Dictionary<string, string> surveyAnswers, List<QuestionResult> questionResults, List<MathQuestionResult> questionResultsMath)
    {
        scoreboardPanel.SetActive(true);

        ScoreboardRenderer scoreboard = FindObjectOfType<ScoreboardRenderer>();

        scoreboard.DisplayResults(testArea, finalResults, totalTime, score, surveyAnswers, questionResults, questionResultsMath);
      
        surveyPanel.SetActive(false);
        startModePanel.SetActive(false);
        riddlePanel.SetActive(false);
        questionPanel.SetActive(false);
        questionPartnerPanel.SetActive(false);
    }

}
