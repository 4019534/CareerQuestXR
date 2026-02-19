using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PersonalityProfiler : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI questionText;
    public Button option1Button, option2Button, option3Button, option4Button;
    public TextMeshProUGUI option1Text, option2Text, option3Text, option4Text; 
    private int currentQuestionIndex = 0;
    private Dictionary<string, string> results = new Dictionary<string, string>();

    private (string question, string[] options)[] questions =
    {
        ("Which type of work do you prefer?", new string[] {
        "Hands-on, practical, using tools or equipment",
        "Investigating ideas, research, problem-solving",
        "Creative and expressive, working with imagination",
        "Helping people, making a positive impact" }),
        ("Which work environment suits you best?", new string[] {
        "Competitive and ambitious, leading others",
        "Structured and organized, following clear procedures",
        "Flexible and creative, room for expression",
        "Supportive and collaborative, helping others succeed" }),
        ("What personal value matters most in your career?", new string[] {
        "Stability and security, clear expectations",
        "Creativity and innovation, trying new things",
        "Helping and supporting others, making a difference",
        "Leadership and influence, driving results" }),
        ("How do you usually approach new challenges?", new string[] {
        "With curiosity and experimentation, eager to explore",
        "With caution and planning, step-by-step",
        "With enthusiasm and energy, diving right in",
        "With hands-on action, learning by doing" }),
        ("How do you prefer to make decisions?", new string[] {
        "Based on facts and analysis, thorough research",
        "Based on intuition and creativity, new perspectives",
        "Based on values and impact on others, what's best for people",
        "Based on efficiency and results, what works best" }),
        ("When given freedom in work, what do you do?", new string[] {
        "Create something new and original",
        "Organize and structure tasks systematically",
        "Explore new knowledge or conduct research",
        "Build relationships and support teamwork" }),
        ("What gives you the most satisfaction in work or study?", new string[] {
        "Solving complex problems and understanding how things work",
        "Expressing creativity and producing original work",
        "Helping someone succeed or overcome challenges",
        "Meeting goals and achieving visible results" }),
        ("When working on a project, you prefer to", new string[] {
        "Work independently with tools or technical equipment",
        "Lead and coordinate a team toward a goal",
        "Follow established procedures and maintain accuracy",
        "Experiment with creative solutions and new ideas" }),
        ("In a team setting, what role naturally fits you?", new string[] {
        "The innovator who suggests creative approaches",
        "Expressing creativity and producing original work",
        "The supporter who helps others and maintains harmony",
        "The organizer who keeps everything on track" }),
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
            TaskManager.Instance.SaveTaskResults("PersonalityProfiler", results);
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
