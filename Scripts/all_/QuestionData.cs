using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class QuestionData {
    public int id;
    public string question;
    public List<string> answers;
    public int correctIndex;
    public List<string> hints = new List<string>();
    public bool usedHint = false;
    public int hintCount = 0;
}
