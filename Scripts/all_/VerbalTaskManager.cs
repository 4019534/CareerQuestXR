// VerbalTaskManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.SceneManagement;

public class VerbalTaskManager : MonoBehaviour
{
    public static VerbalTaskManager Instance;
    public enum TestMode { Riddle, Direct }
    public TestMode selectedMode = TestMode.Riddle;
    public List<QuestionData> questions;
    public List<QuestionResult> results = new List<QuestionResult>();
    public int currentQuestionIndex = 0;
    private float questionStartTime;
    private float totalTimeTaken;
    public int countCorrect=0;
    public int totalSkipped=0;
    public int totalHints=0;
    private bool isSkipped = false;

    [Header("Settings")]
    public float timePerQuestion = 30f;
    public Slider timerSlider;
    public float timer;

    void Awake() => Instance = this;

    void Start()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == "all_")
        {
            LoadQuestions();
            
        }
    }

    void Update()
    {
        if (timerSlider.gameObject.activeSelf)
        {
            timer -= Time.deltaTime;
            timerSlider.value = timer / timePerQuestion;
           

            if (timer <= 0f)
            {
                timer = 0f;
                timerSlider.gameObject.SetActive(false);
                AutoSubmitTimeoutAnswer();
            }
        }
    }

    

    void LoadQuestions()
    {

        questions = new List<QuestionData>
        {
            new QuestionData { id = 1, question = "1. abcdefghijklmnopqrstuvwXY\nIf every third letter is omitted from the alphabet, which letter will then be eleventh in the alphabet?", answers = new List<string>{"k", "n", "p", "s", "o"}, correctIndex = 2, hints = new List<string> {
            "I guard secrets behind a thin wooden face,\nCount three along and you’ll find my place.\nNot the first, not the last, but in the middle I hide,\nA keeper of belongings locked up inside.",
            "I’m found in schools, lining the wall in a row.",
            "You’ll need to count — I’m not the first you see.",
            "I’m the third locker from the right." 
        } },
            new QuestionData { id = 2, question = "2. Which is the last word in the sentence which can be formed from the following words?\nHOUSE DID LIVE WHICH IN YOU?", answers = new List<string>{"HOUSE", "LIVE", "DID", "WHICH", "YOU"}, correctIndex = 1, hints = new List<string> {
            "I’m a window for words without glass or a frame,\nTeachers call me daily, and chalk knows my name.\nI’m green, not black, though that’s what they say,\nFind me on the wall where lessons stay.",
            "I hang where the teacher stands to teach.",
            "Chalk and dust are my constant companions.",
            "I’m the only green board in this classroom." 
        } },
            new QuestionData { id = 3, question = "3. abcdefghijklmnopqrstuvwxyz\nIf the letters f and m change places, which letter will then be in the middle between f and u?", answers = new List<string>{"m", "p", "r", "q", "n"}, correctIndex = 3, hints = new List<string> {
            "I glow without fire, I speak without breath,\nI sit on the teacher’s desk, avoiding rest.\nI answer your questions, I work at a click,\nWithout me today, school life is sick.",
            "Look to the teacher’s desk for a glowing friend.",
            "I’m used for typing and projecting ideas.",
            "I’m the computer on the teacher’s desk." 
            } },
            new QuestionData { id = 4, question = "4. Which possible deduction can be made from the following sentence?\nNIC IS TALLER THAN MARIO AND SHORTER THAN PAUL.", answers = new List<string>{"Mario is shorter than Paul.", "Paul is shorter than Mario.", "Nic is the tallest.", "Mario is taller than Paul.", "Nic is taller than Paul."}, correctIndex = 0, hints = new List<string> {
            "When danger strikes and students fall,\nI’m the box that answers the call.\nWith a cross of red or white on my skin,\nOpen me up and healing begins.",
            "I hang on the wall, not for books or coats.",
            "I’m marked with a medical plus sign.",
            "I’m the locker that stores the first aid kit."     
        } },
            new QuestionData { id = 5, question = "5. ..... is to ADVERSITY as UNHAPPINESS is to.....", answers = new List<string>{"IGNORANCE	KNOWLEDGE", "DEATH	LIFE", "MOTOR CAR	PERSON", "PAIN	HEALTH", "HAPPINESS	PROSPERITY"}, correctIndex = 4,  hints = new List<string> {
            "I carry worlds within my skin,\nGreen without, with pages thin.\nI wait on a chair, not a shelf this time,\nCount three from the left and you’ll find what’s mine.",
            "I’m a book — but look for my green cover.",       
            "I’m not on a shelf, but lying on a seat.",   
            "I’m on the third chair in the front row."   
        } },
            new QuestionData { id = 6, question = "6. Nine people are using a lift. On the third floor three men get out and two women get in. On the seventh floor two men and one woman get out. If there are four men and one woman left in the lift, how many men were there in the lift originally?", answers = new List<string>{"Five", "Six", "Seven", "Eight", "Nine"}, correctIndex = 4, hints = new List<string> {
            "I hide the light or let it through,\nI stretch across the window’s view.\nNext to where the teacher speaks,\nI’m the curtain with slats, not sheets.",
            "I live beside the teacher’s desk.",      
            "I’m made to block or reveal the sun.",    
            "I’m one of the large set of blinds."     
        } },
            new QuestionData { id = 7, question = "7. Which word is least associated with the other four?", answers = new List<string>{"Permanence", "Character", "Loyalty", "Perseverance", "Devotion"}, correctIndex = 1, hints = new List<string> {
            "I shout without a mouth,\nMy voice comes from the wall.\nHigh above, I call out loud,\nIn times of class or hall.",
            "I’m small and mounted high.",       
            "I’m used to project sound across the school.",    
            "I’m the school speaker on the front wall."    
        } },
            new QuestionData { id = 8, question = "8. Which word is least associated with the other four?", answers = new List<string>{"Request", "Stipulation ", "Fixation ", "Condition", "Limitation"}, correctIndex = 2, hints = new List<string> {
            "I should be filled with tales and lore,\nBut I stand empty, wanting more.\nAt the back, by light that streams,\nI wait alone without my dreams.",
            "Most of my kind hold books.",       
            "I’m in the library, by the window.",    
            "I’m the only empty book rack."     
        } },
            new QuestionData { id = 9, question = "9. Five boys stand in a row. John stands in front and Gerald at the back. Paul stands between Peter and William. William stands behind John. Who is fourth in the row?", answers = new List<string>{"Peter", "William", "Paul", "John", "Gerald"}, correctIndex = 1, hints = new List<string> {
            "I open to leave, I close to stay,\nI take you from books to lockers’ array.\nI stand tall at the library’s end,\nTo the hallway beyond, I’ll send.",
            "I connect the library to the hallway.",      
            "You must pass me to move on.",    
            "I’m the library door."     
        } },
            new QuestionData { id = 10, question = "10. Which word is always associated with ROOM?", answers = new List<string>{"PICTURES", "FLOOR", "KEY", "CARPET", "CURTAINS"}, correctIndex = 2, hints = new List<string> {
            "I carry trays but never eat,\nI stand in rows where students meet.\nI’m orange-pink, in the row by the door,\nSit with me first, don’t search for more.",
            "I’m in the cafeteria where meals are had.",       
            "There are five of us, but I’m in the front row.",     
            "I’m the first table by the door."    
        } },
            new QuestionData { id = 11, question = "11. abcdefghijklmnopqrstuvwxyz\nWhich pair of letters will follow the series given below?\nCX FU IR LO .....", answers = new List<string>{"OL", "PM", "MN", "PK", "OM"}, correctIndex = 0, hints = new List<string> {
            "I have no voice yet hold the wise,\nI rest below the board where answers rise.\nI’m not for students but one to lead,\nAt the front I wait, to serve that need.",
            "I’m placed by the whiteboard.",       
            "The teacher sits on me, not the pupils.",    
            "I’m the teacher’s chair."     
        } },
            new QuestionData { id = 12, question = "12. Which possible deduction can be made from the following sentence?\nDURING THE DAY THE CHILDREN IN TOGO ALSO PLAY HI-HI OUT OF DOORS.", answers = new List<string>{"Hi-hi is only played during the day.", "Hi-hi can only be played by children.", "Hi-hi can only be played out of doors.", "The children of Togo also play other games.", "In Togo no games are played at night."}, correctIndex = 3, hints = new List<string> {
            "In silence I wait for danger’s breath,\nOne pull and I shout of fire and death.\nSmall and red, I hide in plain sight,\nBy lockers’ end, to bring the light.",
            "I’m hidden at the end of the lockers.",       
            "I’m red, but not a locker.",    
            "I’m the fire alarm on the right wall."     
        } },
            new QuestionData { id = 13, question = "13. A man coming from the north reaches a cross-road. He is on the way to Mafeking. The road which leads off to the right goes to another place; the one straight ahead of him leads to a farm. In which direction must the man turn in order to reach his destination?", answers = new List<string>{"North", "South", "East", "West", "Impossible to tell"}, correctIndex = 2, hints = new List<string> {
            "Among my brothers, lined in rows,\nI stand apart with an orange glow.\nCount carefully in the second set,\nNine from the right is where we met.",
            "Look for lockers in groups against the right wall.",       
            "I’m not in the first group, but the second.",     
            "I’m the 9th locker from the right, orange in colour."    
        } },
            new QuestionData { id = 14, question = "14. Which one of the five concepts from A to E encompasses the following words?\nMONTH, DAY, YEAR, SCHOOL TERM, CENTURY", answers = new List<string>{"Seasons", "Decade", "Distance", "Date", "Period"}, correctIndex = 4, hints = new List<string> {
            "I hold the words without a sound,\nI pin bright colours all around.\nUnlike my twins, in white they stay,\nMy colours guide you on your way.",
            "There are three of me, but I’m different.",       
            "The others wear only white.",    
            "I’m the notice board with coloured papers."     
        } },
            new QuestionData { id = 15, question = "15. KITCHEN can be written in a certain code as MKVEJGP. How would TABLE be written in the same code?", answers = new List<string>{"SZAKD", "ELBAT", "VCNDF", "WDEOH", "VCDNG"}, correctIndex = 4, hints = new List<string> {
            "I welcome all and bid goodbye,\nThrough me, the halls and world align.\nTwo I stand, side by side,\nThe school’s great mouth open wide.",
            "I’m at the very front of the school.",        
            "You face me when you spawn in the hallway.",    
            "I’m the double doors by the boards and your only exit."     
        } }
        };
    }

    public void SaveResultsToDb(Dictionary<string, string> surveyAnswers)
    {
        foreach (float time in UIController.Instance.taskTimes) { totalTimeTaken += time; }
        float score = ((float)countCorrect / questions.Count) * 100f;
        string finalScore = $"{countCorrect} / {questions.Count} ({score:F1}%)";

        Dictionary<string, object> finalResults = new Dictionary<string, object>
        {
            { "totalTime", totalTimeTaken },
            { "score", finalScore },
            { "totalSkipped", totalSkipped },
            { "totalHints", totalHints },
            { "questionResults", results },
            { "surveyAnswers", surveyAnswers }
        };

        string json = JsonConvert.SerializeObject(finalResults);

        UIController.Instance.ShowScoreboard(
            "VerbalComprehensionTest_test2",
            finalResults,
            finalScore,
            totalTimeTaken,
            surveyAnswers,
            results,
            null
        );
        
    }

    public void StartTest()
    {
        currentQuestionIndex = 0;
        totalTimeTaken = 0f;
        ShowNextQuestion();
    }

    void ShowNextQuestion()
    {
        timer = timePerQuestion;
        timerSlider.value = 1f;

        questionStartTime = Time.time;
        if (currentQuestionIndex >= questions.Count)
        {
            timerSlider.gameObject.SetActive(false);
            UIController.Instance.ShowSurvey();
            return;
        }

        questionStartTime = Time.time;
        timerSlider.gameObject.SetActive(true);
        UIController.Instance.ShowQuestion(questions[currentQuestionIndex]);
    }

    private void AutoSubmitTimeoutAnswer()
    {
        var q = questions[currentQuestionIndex];
        SubmitAnswerTimedOut();
    }

    private void SubmitAnswerTimedOut()
    {
        var q = questions[currentQuestionIndex];
        float timeTaken = Time.time - questionStartTime;

        bool hasAnswer = UIController.Instance.HasSelectedAnswer();
        int chosenIndex = UIController.Instance.GetSelectedAnswer();
        int confidence = UIController.Instance.GetCurrentConfidence();
        bool isCorrect = chosenIndex == q.correctIndex;

        if (isCorrect)
        {
            countCorrect++;
        }
        if (hasAnswer)
        {
            results.Add(new QuestionResult
            {
                questionId = q.id,
                selectedAnswer = q.answers[chosenIndex],
                correctAnswer = q.answers[q.correctIndex],
                confidence = confidence,
                timeTaken = timeTaken,
                wasCorrect = isCorrect,
                isSkipped = isSkipped,
                usedHint = q.usedHint,
                hintCount = q.hintCount
            });

        }
        else
        {
            results.Add(new QuestionResult
            {
                questionId = q.id,
                selectedAnswer = "(No Answer)",
                correctAnswer = q.answers[q.correctIndex],
                confidence = 0,
                timeTaken = timeTaken,
                wasCorrect = false,
                isSkipped = isSkipped,
                usedHint = q.usedHint,
                hintCount = q.hintCount
            });
        }

        isSkipped = false;
        currentQuestionIndex++;

        if (currentQuestionIndex >= questions.Count)
        {
            QuestionResultList wrap = new QuestionResultList(results);
            string json = JsonUtility.ToJson(wrap, true);
            UIController.Instance.questionPanel.SetActive(false);
            UIController.Instance.ShowSurvey();
            return;
        }

        if (selectedMode == TestMode.Riddle)
        {
            UIController.Instance.ShowRiddle(questions[currentQuestionIndex]);
        }
        else
        {
            ShowNextQuestion();
        }
    }

    public void SubmitAnswer(int selectedIndex, int confidenceLevel)
    {
        
        UIController.Instance.confidenceSlider.value = 0;
        float timeTaken = Time.time - questionStartTime;
        timer = timePerQuestion;

        timerSlider.gameObject.SetActive(false);
        var q = questions[currentQuestionIndex];

        bool isCorrect = selectedIndex == q.correctIndex;

        if (isCorrect)
        {
            countCorrect++;
        }

        results.Add(new QuestionResult
        {
            questionId = q.id,
            selectedAnswer = q.answers[selectedIndex],
            correctAnswer = q.answers[q.correctIndex],
            confidence = confidenceLevel,
            timeTaken = timeTaken,
            wasCorrect = isCorrect,
            isSkipped = isSkipped,
            usedHint = q.usedHint,
            hintCount = q.hintCount
        });

        isSkipped = false;
        
        currentQuestionIndex++;
        if (currentQuestionIndex >= questions.Count)
        {
            QuestionResultList wrappedResults = new QuestionResultList(results);
            string json = JsonUtility.ToJson(wrappedResults, true);
            UIController.Instance.questionPanel.SetActive(false);
            UIController.Instance.ShowSurvey(); 
            return;
        }

        if (selectedMode == TestMode.Riddle)
        {
            UIController.Instance.ShowRiddle(questions[currentQuestionIndex]);
        }
        else
        {
            ShowNextQuestion();
        }
    }

    public void MarkHintUsed()
    {
        int currentQId = questions[currentQuestionIndex].id;
        questions[currentQuestionIndex].usedHint = true;
    }

    public void EndTest()
    {
        timerSlider.gameObject.SetActive(false);
        UIController.Instance.ShowSurvey();
    }

    public void SkipQuestion()
    {
        totalSkipped++;
        isSkipped = true;
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        foreach (GameObject trigger in UIController.Instance.riddleTriggers)
        {
            RiddleZone zone = trigger.GetComponent<RiddleZone>();
            if (zone != null && zone.questionId == currentQuestionIndex + 1 && currentSceneName == "all_")
            {
                trigger.SetActive(false);
                break;
            }
        }
        
        timerSlider.gameObject.SetActive(true);
        timer = timePerQuestion;
        timerSlider.value = 1f;

        questionStartTime = Time.time;
        UIController.Instance.ShowQuestion(questions[currentQuestionIndex]);
    }

    public void OnRiddleZoneReached(int zoneIndex)
    {
        ShowNextQuestion();
    }

}

[System.Serializable]
public class QuestionResultList
{
    public List<QuestionResult> results;

    public QuestionResultList(List<QuestionResult> results)
    {
        this.results = results;
    }
}
