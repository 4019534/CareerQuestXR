[System.Serializable]
public class QuestionResult {
    public int questionId;
    public string selectedAnswer;
    public string correctAnswer;
    public float timeTaken;
    public bool isSkipped;
    public int confidence; 
    public bool wasCorrect;
    public bool usedHint;
    public int hintCount;
}
