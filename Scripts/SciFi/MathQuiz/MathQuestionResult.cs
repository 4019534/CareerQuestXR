using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using System.IO;

[System.Serializable]
public class MathQuestionResult
{
    public int levelNumber;
    public int correctAnswers;
    public int totalQuestions;
    public float levelTime;
    public string testType;
    public List<MathQuestionResult> questionResults = new List<MathQuestionResult>();
}
