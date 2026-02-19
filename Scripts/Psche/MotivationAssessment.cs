using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MotivationAssessment : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI questionText;
    public Button option1Button, option2Button, option3Button, option4Button;
    public TextMeshProUGUI option1Text, option2Text, option3Text, option4Text;
    private int currentQuestionIndex = 0;
    private Dictionary<string, string> results = new Dictionary<string, string>();
    private (string question, string[] options)[] questions =
    {
        ("When working toward a difficult goal, what motivates you most?", new string[] {
        "The satisfaction of achievement",
        "Recognition or reward from others",
        "Pressure of deadlines or consequences",
        "Encouragement or expectations from others" }),
        ("A project will take months to complete. How do you feel?", new string[] {
        "Energized by the long-term challenge",
        "Neutral, I’ll handle it step by step",
        "Pressured, I prefer shorter goals",
        "Discouraged, it feels too far away" }),
        ("When you hit a roadblock in your work, what’s your first move?", new string[] {
        "Push through alone until I solve it",
        "Seek advice or help",
        "Step away and return later",
        "Shift focus to something else" }),
        ("Which best describes your work style?", new string[] {
        "Consistent and steady, even if progress is slow",
        "Energetic bursts of effort, then breaks",
        "Reactive, depending on pressure",
        "Driven mostly by others’ expectations" }),
        ("Imagine receiving no recognition for your work. How do you react?", new string[] {
        "Still satisfied if I did well",
        "A bit demotivated, recognition matters",
        "Frustrated, recognition is very important",
        "Indifferent, as long as it’s finished" }),
        ("What motivates you most in school, uni, or work?", new string[] {
        "Mastery and learning itself",
        "Praise and rewards",
        "Fear of failing",
        "Being part of a supportive team" })
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
            TaskManager.Instance.SaveTaskResults("MotivationAssessment", results);
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

