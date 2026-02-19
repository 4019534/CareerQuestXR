using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class EmotionalResponseTest : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scenarioText;
    public Image scenarioImage; 
    public Button option1Button, option2Button, option3Button, option4Button;
    public TextMeshProUGUI option1Text, option2Text, option3Text, option4Text;

    [Header("Scenario Images")]
    public List<Sprite> scenarioImages;
    private int currentScenarioIndex = 0;
    private Dictionary<string, string> results = new Dictionary<string, string>();
    private (string scenario, string[] responses, int imageIndex)[] scenarios =
    {
        ("A teacher or boss criticizes your work harshly in public. What do you do?", 
            new string[] {
                "Stay calm, listen, and learn",
                "Feel upset but stay quiet",
                "Defend myself immediately",
                "Withdraw and lose motivation" }, -1),

        ("You’re excluded from a group activity you wanted to join. First reaction?", 
            new string[] {
                "Disappointed but move on",
                "Hurt, but try to understand why",
                "Angry, it feels unfair",
                "Indifferent, I didn’t care much" }, -1),

        ("You make a mistake that affects your team. What do you do?", 
            new string[] {
                "Admit it and suggest fixes",
                "Feel guilty but wait for others to raise it",
                "Quietly fix it if I can",
                "Blame circumstances or others" }, 0),

        ("You’re waiting in a very long queue. What’s your state of mind?", 
            new string[] {
                "Patient, I can wait",
                "Slightly irritated but calm",
                "Restless and impatient",
                "Frustrated, I want to leave" }, -1),

        ("Someone takes credit for your work. How do you react?", 
            new string[] {
                "Confront them calmly",
                "Feel upset but avoid conflict",
                "Bring it up later in private",
                "Leave it alone, not worth fighting" }, 1), 

        ("You’re under a lot of stress. How do you cope first?", 
            new string[] {
                "Break tasks into small parts",
                "Talk to someone I trust",
                "Distract myself with other activities",
                "Procrastinate or avoid the problem" }, -1),
    };

    void Start()
    {
        option1Button.onClick.AddListener(() => AnswerScenario(0));
        option2Button.onClick.AddListener(() => AnswerScenario(1));
        option3Button.onClick.AddListener(() => AnswerScenario(2));
        option4Button.onClick.AddListener(() => AnswerScenario(3));
        UpdateScenario();
    }

    void UpdateScenario()
    {
        if (currentScenarioIndex < scenarios.Length)
        {
            var s = scenarios[currentScenarioIndex];
            scenarioText.text = s.scenario;

            if (s.imageIndex >= 0 && s.imageIndex < scenarioImages.Count)
            {
                scenarioImage.sprite = scenarioImages[s.imageIndex];
                scenarioImage.gameObject.SetActive(true);
            }
            else
            {
                scenarioImage.gameObject.SetActive(false);
            }

            option1Text.text = s.responses[0];
            option2Text.text = s.responses[1];
            option3Text.text = s.responses[2];
            option4Text.text = s.responses[3];
        }
        else
        {

            TaskManager.Instance.SaveTaskResults("EmotionalResponses", results);
            TaskManager.Instance.StartNextTask();
        }
    }

    void AnswerScenario(int choiceIndex)
    {
        var s = scenarios[currentScenarioIndex];
        results[s.scenario] = s.responses[choiceIndex];
        currentScenarioIndex++;
        UpdateScenario();
    }
}
