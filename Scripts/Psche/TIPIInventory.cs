using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TIPIInventory : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI questionText;
    public Button option1Button, option2Button, option3Button, option4Button, option5Button,option6Button, option7Button;
    public TextMeshProUGUI option1Text, option2Text, option3Text, option4Text, option5Text, option6Text, option7Text; 
    private int currentQuestionIndex = 0;
    private Dictionary<string, string> results = new Dictionary<string, string>();
    private (string question, string[] options)[] questions =
    {
        ("I see myself as extraverted and enthusiastic", new string[] {
            "1 - Disagree strongly",
            "2 - Disagree moderately",
            "3 - Disagree slightly",
            "4 - Neither agree nor disagree",
            "5 - Agree slightly",
            "6 - Agree moderately",
            "7 - Agree strongly"
        }),

        ("I see myself as critical and quarrelsome", new string[] {
            "1 - Disagree strongly",
            "2 - Disagree moderately",
            "3 - Disagree slightly",
            "4 - Neither agree nor disagree",
            "5 - Agree slightly",
            "6 - Agree moderately",
            "7 - Agree strongly"
        }),

        ("I see myself as dependable and self-disciplined", new string[] {
            "1 - Disagree strongly",
            "2 - Disagree moderately",
            "3 - Disagree slightly",
            "4 - Neither agree nor disagree",
            "5 - Agree slightly",
            "6 - Agree moderately",
            "7 - Agree strongly"
        }),

        ("I see myself as anxious and easily upset", new string[] {
            "1 - Disagree strongly",
            "2 - Disagree moderately",
            "3 - Disagree slightly",
            "4 - Neither agree nor disagree",
            "5 - Agree slightly",
            "6 - Agree moderately",
            "7 - Agree strongly"
        }),

        ("I see myself as open to new experiences and complex", new string[] {
            "1 - Disagree strongly",
            "2 - Disagree moderately",
            "3 - Disagree slightly",
            "4 - Neither agree nor disagree",
            "5 - Agree slightly",
            "6 - Agree moderately",
            "7 - Agree strongly"
        }),

        ("I see myself as reserved and quiet", new string[] {
            "1 - Disagree strongly",
            "2 - Disagree moderately",
            "3 - Disagree slightly",
            "4 - Neither agree nor disagree",
            "5 - Agree slightly",
            "6 - Agree moderately",
            "7 - Agree strongly"
        }),

        ("I see myself as sympathetic and warm", new string[] {
            "1 - Disagree strongly",
            "2 - Disagree moderately",
            "3 - Disagree slightly",
            "4 - Neither agree nor disagree",
            "5 - Agree slightly",
            "6 - Agree moderately",
            "7 - Agree strongly"
        }),

        ("I see myself as disorganized and careless", new string[] {
            "1 - Disagree strongly",
            "2 - Disagree moderately",
            "3 - Disagree slightly",
            "4 - Neither agree nor disagree",
            "5 - Agree slightly",
            "6 - Agree moderately",
            "7 - Agree strongly"
        }),

        ("I see myself as calm and emotionally stable", new string[] {
            "1 - Disagree strongly",
            "2 - Disagree moderately",
            "3 - Disagree slightly",
            "4 - Neither agree nor disagree",
            "5 - Agree slightly",
            "6 - Agree moderately",
            "7 - Agree strongly"
        }),

        ("I see myself as conventional and uncreative", new string[] {
            "1 - Disagree strongly",
            "2 - Disagree moderately",
            "3 - Disagree slightly",
            "4 - Neither agree nor disagree",
            "5 - Agree slightly",
            "6 - Agree moderately",
            "7 - Agree strongly"
        })
    };


    void Start()
    {
        option1Button.onClick.AddListener(() => AnswerQuestion(0));
        option2Button.onClick.AddListener(() => AnswerQuestion(1));
        option3Button.onClick.AddListener(() => AnswerQuestion(2));
        option4Button.onClick.AddListener(() => AnswerQuestion(3));
        option5Button.onClick.AddListener(() => AnswerQuestion(4));
        option6Button.onClick.AddListener(() => AnswerQuestion(5));
        option7Button.onClick.AddListener(() => AnswerQuestion(6));
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
            option5Text.text = q.options[4];
            option6Text.text = q.options[5];
            option7Text.text = q.options[6];
        }
        else
        {
            TaskManager.Instance.SaveTaskResults("TIPIInventory", results);
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
