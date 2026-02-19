using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro; 
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class ReactionTimeTest : MonoBehaviour
{
    public InputActionProperty triggerAction;
    public Button reactionButton, penaltyButton, penaltyButton2;     
    public TextMeshProUGUI reactionTimeText;     
    public TextMeshProUGUI attemptsLeftText;     
    public TextMeshProUGUI avgReactionTimeText;   
    public TextMeshProUGUI penaltyTimeText;      
    public TextMeshProUGUI finalAvgReactionTimeText;
    public TextMeshProUGUI penaltyText;           
    public Image reactionPanel;       
    public GameObject reactionTimerPanel, surveyPanel;
    private float timeStart;           
    private bool canClick = false;       
    private int attemptCount = 0;       
    private List<float> reactionTimes = new List<float>(); 
    private float totalPenalty = 0f;    
    private const int maxAttempts = 5; 
    private const float penaltyAmount = 5f;
    private int penaltyTotal = 0;

    [Header("Countdown UI")]
    public TextMeshProUGUI countdownText;

    public static ReactionTimeTest Instance;
    private bool uiButtonPressed = false;
    private Button lastPressedButton = null;

    void Awake()
    {
        Instance = this;
        penaltyButton.onClick.AddListener(() => OnUIButtonClicked(penaltyButton));
        penaltyButton2.onClick.AddListener(() => OnUIButtonClicked(penaltyButton2));
        reactionButton.onClick.AddListener(() => OnUIButtonClicked(reactionButton));

	}

    void Start()
    {
        UpdateAttemptsLeftUI();
        StartCoroutine(StartReactionTest());
    }

    private void OnUIButtonClicked(Button pressedButton)
    {
        lastPressedButton = pressedButton;
    }

    void Update()
    {
        if (lastPressedButton == null)
            return;

        Button pressed = lastPressedButton;
        lastPressedButton = null; 

        penaltyTimeText.text = "";
        penaltyText.text = "";

 
        if (pressed == reactionButton)
        {
            if (canClick)
            {
                PlayerClicked();
            }
            else
            {
                totalPenalty += penaltyAmount;
                penaltyTotal++;

                penaltyText.text = "Clicked Too Early! +5ms Penalty!";
                penaltyTimeText.text = $"Total Penalty: {totalPenalty}ms | Penalty Clicks: {penaltyTotal}";

                StartCoroutine(ClearPenaltyText());
            }
        }

        else if (pressed == penaltyButton || pressed == penaltyButton2)
        {
            totalPenalty += penaltyAmount;
            penaltyTotal++;

            penaltyText.text = "Wrong Click! +5ms Penalty!";
            penaltyTimeText.text = $"Total Penalty: {totalPenalty}ms | Penalty Clicks: {penaltyTotal}";

            StartCoroutine(ClearPenaltyText());
        }
    }

    IEnumerator StartReactionTest()
    {
        if (attemptCount >= maxAttempts)
        {
            EndTest();
            yield break;
        }

        reactionButton.interactable = false;
        
        penaltyText.text = ""; 
        canClick = false;

        int countdown = 3;
        while (countdown > 0)
        {
            if (countdownText != null)
                countdownText.text = countdown.ToString();

            yield return new WaitForSeconds(1f);
            countdown--;
        }

        if (countdownText != null)
            countdownText.text = "GO!";

        yield return new WaitForSeconds(1f);

        if (countdownText != null)
            countdownText.text = "";


        reactionPanel.color = Color.red;
        penaltyButton.gameObject.SetActive(true);
        penaltyButton2.gameObject.SetActive(false);
        reactionTimeText.text = "Wait for Green...";
        float waitTime = Random.Range(2f, 5f);
        yield return new WaitForSeconds(waitTime);

        reactionPanel.color = Color.green;
        penaltyButton.gameObject.SetActive(false);
        penaltyButton2.gameObject.SetActive(true);
        reactionButton.interactable = true;
        timeStart = Time.time;
        canClick = true;
    }

    void PlayerClicked()
    {
        if (canClick)
        {
            float reactionTime = (Time.time - timeStart) * 1000; 
            reactionTimes.Add(reactionTime);
            attemptCount++;

            reactionTimeText.text = $"Reaction Time: {reactionTime:F1} ms";
            canClick = false;
            reactionButton.interactable = false;
            penaltyButton2.gameObject.SetActive(false);

            if (attemptCount < maxAttempts)
            {
                UpdateAttemptsLeftUI();
                StartCoroutine(StartReactionTest());
            }
            else
            {
                EndTest();
            }
        }
    }

    IEnumerator ClearPenaltyText()
    {
        yield return new WaitForSeconds(1f);
        penaltyText.text = "";
        penaltyTimeText.text = "";
    }

    void UpdateAttemptsLeftUI()
    {
        if (attemptsLeftText != null)
        { 
            attemptsLeftText.text = $"Attempts Left: {maxAttempts - attemptCount}";
        }
    }

    void EndTest()
    {
        float avgReactionTime = CalculateAverageTime();
        float finalReactionTime = avgReactionTime + totalPenalty / maxAttempts; 

        gameObject.SetActive(false);

        TaskManager.Instance.SaveReactionTimeResults(avgReactionTime, finalReactionTime, totalPenalty, penaltyTotal);
        reactionTimerPanel.SetActive(false);
        surveyPanel.SetActive(true);

    }

    float CalculateAverageTime()
    {
        if (reactionTimes.Count == 0) return 0;

        float sum = 0;
        foreach (float time in reactionTimes)
        {
            sum += time;
        }
        return sum / reactionTimes.Count;
    }
}
