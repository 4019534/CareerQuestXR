using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MultitaskingAssessment : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI questionText;
    public Button option1Button, option2Button, option3Button, option4Button;
    public TextMeshProUGUI option1Text, option2Text, option3Text, option4Text;
    private int currentQuestionIndex = 0;
    private Dictionary<string, string> results = new Dictionary<string, string>();
    private (string question, string[] options)[] questions =
    {
        ("You’re under deadline pressure when a friend asks for urgent help. What do you do?", new string[] {
        "Drop my task to help immediately",
        "Try to balance both at once",
        "Explain I’ll help once I finish my priority",
        "Find someone else who can assist" }),
        ("You’re juggling several roles in a project. How do you handle it?", new string[] {
        "Focus fully on one before switching",
        "Switch frequently between roles",
        "Ask others to share responsibilities",
        "Only do the urgent parts, leave the rest" }),
        ("You’re in a timed exam with many sections. What’s your strategy?", new string[] {
        "Start with the easiest section to secure marks",
        "Start with the hardest to get it done first",
        "Skim all, then divide time evenly",
        "Go with instinct, answering as I go" }),
        ("At work, two urgent requests arrive at once. What’s your instinct?", new string[] {
        "Prioritize based on deadlines",
        "Prioritize based on importance to others",
        "Try to start both and see how far I get",
        "Wait for clarification from a supervisor" }),
        ("In a group project, everyone depends on you for updates. How do you cope?", new string[] {
        "Keep organized lists and complete tasks systematically",
        "Juggle many things at once, even if stressful",
        "Ask for help to share the load",
        "Focus on the most visible tasks first" }),
        ("You’re interrupted constantly while working. How do you respond?", new string[] {
        "Politely ask for time to focus",
        "Try to handle interruptions immediately",
        "Get stressed but adapt",
        "Ignore interruptions if possible" }),
    };

    void Start()
    {
        option1Button.onClick.AddListener(() => AnswerQuestion(0));
        option2Button.onClick.AddListener(() => AnswerQuestion(1));
        option3Button.onClick.AddListener(() => AnswerQuestion(2));
        option4Button.onClick.AddListener(() => AnswerQuestion(3));
        UpdateQuestion();
    }

    void UpdateQuestion()
    {
        if (currentQuestionIndex < questions.Length)
        {
            var q = questions[currentQuestionIndex];
            questionText.text = q.question;

            option1Text.text = q.options[0];
            option2Text.text = q.options[1];
            option3Text.text = q.options[2];
            option4Text.text = q.options[3];
        }
        else
        {
            TaskManager.Instance.SaveTaskResults("Multitasking", results);
            TaskManager.Instance.StartNextTask();
        }
    }

    void AnswerQuestion(int choiceIndex)
    {
        var q = questions[currentQuestionIndex];
        results[q.question] = q.options[choiceIndex];
        currentQuestionIndex++;
        UpdateQuestion();
    }
}
