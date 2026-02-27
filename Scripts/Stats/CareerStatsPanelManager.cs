using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text; 
using System.Globalization; 


public class CareerStatsPanelManager : MonoBehaviour
{
    public static CareerStatsPanelManager Instance;
    [Header("UI Elements")]
    public Slider progressBar;
    public TMP_Text progressText;
    public Button loadButton;
    public Button surveyButton;
    public Transform careerListParent;
    public GameObject careerResultPrefab;
    public Scrollbar scrollbar;
    public TMP_Text placeholderText;

    [Header("Admin Panel UI")]
    public TMP_Text adminStatusText;
    public TMP_Text adminInfoText;
    public TMP_Text adminStatsText;
    public Slider adminProgressSlider;
    public ScrollRect adminScrollView;
    public Button adminPanelButton;
    [Header("CSV Export Buttons")]
    public Button exportDomainScoresButton;
    public Button exportCareerRecsButton;
    public Button exportDomainCorrelationsButton;  
    public Button exportBigFiveScoresButton;
    public Button exportCompDataButton;
    public Button exportBigFiveCorrelationsButton;  
    public Button generateReportButton;
    [Header("Settings")]
    public string username;
    public string rootPath;
    private readonly string[] testFolders = {
        "VerbalComprehensionTest_test2",
        "MentalMathsTest_test2",
        "MemoryRecallTest_test2",
        "ReactionTimerTest_test2",
        "NavigationGrid_test2",
        "PsychologicalEvaluationTest_test2",
        "OverallEvaluation"      
    };

    private void Start()
    {
        rootPath = Application.persistentDataPath;
        username = PlayerPrefs.GetString("LoggedInUser", "");


        loadButton.onClick.AddListener(OnLoadButtonClicked);
        exportDomainScoresButton.onClick.AddListener(ExportAllParticipantsDomainScores);
        exportCareerRecsButton.onClick.AddListener(ExportAllParticipantsCareerRecommendations);
        exportDomainCorrelationsButton.onClick.AddListener(ExportDomainCorrelationAnalysis); 
        exportBigFiveScoresButton.onClick.AddListener(ExportAllParticipantsBigFiveScores);
        exportCompDataButton.onClick.AddListener(ExportComprehensiveParticipantData);
        exportBigFiveCorrelationsButton.onClick.AddListener(ExportBigFiveCorrelationAnalysis);
        generateReportButton.onClick.AddListener(GenerateMyPDFReport); 
        
        ConfigureAdminUI();
        UpdateProgressBar(0, testFolders.Length);
    }

    private void ConfigureAdminUI()
    {
        bool isAdmin = username.Equals("admin", System.StringComparison.OrdinalIgnoreCase);
                
        if (isAdmin)
        {
            
            if (adminStatusText != null) adminStatusText.gameObject.SetActive(false);
            if (adminInfoText != null) adminInfoText.gameObject.SetActive(false);
            if (adminStatsText != null) adminStatsText.gameObject.SetActive(false);
            if (adminProgressSlider != null) adminProgressSlider.gameObject.SetActive(false);
            if (adminScrollView != null) adminScrollView.gameObject.SetActive(false);
            if (adminPanelButton != null) adminPanelButton.gameObject.SetActive(false);
            if (generateReportButton != null) generateReportButton.gameObject.SetActive(false);
            
            if (exportDomainScoresButton != null) exportDomainScoresButton.gameObject.SetActive(true);
            if (exportCareerRecsButton != null) exportCareerRecsButton.gameObject.SetActive(true);
            if (exportDomainCorrelationsButton != null) exportDomainCorrelationsButton.gameObject.SetActive(true);
            if (exportBigFiveScoresButton != null) exportBigFiveScoresButton.gameObject.SetActive(true);
            if (exportCompDataButton != null) exportCompDataButton.gameObject.SetActive(true);
            if (exportBigFiveCorrelationsButton != null) exportBigFiveCorrelationsButton.gameObject.SetActive(true);
                        
        }
        else
        {
            
            if (adminStatusText != null) adminStatusText.gameObject.SetActive(true);
            if (adminInfoText != null) adminInfoText.gameObject.SetActive(true);
            if (adminStatsText != null) adminStatsText.gameObject.SetActive(true);
            if (adminProgressSlider != null) adminProgressSlider.gameObject.SetActive(true);
            if (adminScrollView != null) adminScrollView.gameObject.SetActive(true);
            if (adminPanelButton != null) adminPanelButton.gameObject.SetActive(true);
            
            if (exportDomainScoresButton != null) exportDomainScoresButton.gameObject.SetActive(false);
            if (exportCareerRecsButton != null) exportCareerRecsButton.gameObject.SetActive(false);
            if (exportDomainCorrelationsButton != null) exportDomainCorrelationsButton.gameObject.SetActive(false);
            if (exportBigFiveScoresButton != null) exportBigFiveScoresButton.gameObject.SetActive(false);
            if (exportCompDataButton != null) exportCompDataButton.gameObject.SetActive(false);
            if (exportBigFiveCorrelationsButton != null) exportBigFiveCorrelationsButton.gameObject.SetActive(false);
            
        }
        
    }
    

    private void OnLoadButtonClicked()
    {
        Dictionary<string, Dictionary<string, object>> userResults = UserResultsLoader.LoadUserResults(username, rootPath);

        foreach (var test in userResults)
        {

            if (test.Value == null)
            {
                continue;
            }
        }
        if (userResults.Count == 0)
            {
                if (placeholderText != null)
                {
                    placeholderText.text = $"No test data found for {username}!";
                    placeholderText.gameObject.SetActive(true);
                }
                return;
            }
        int completedTests = userResults.Count;
        UpdateProgressBar(completedTests, testFolders.Length);

        if (completedTests == 0)
        {
            DisplayCareers(new List<CareerRecommendation>());
            return;
        }

        DomainScores domainScores = ScoreCalculator.ComputeDomainScores(userResults);
        CareerRecommendationResult result = CareerMapper.GenerateRecommendations(domainScores, userResults);

        switch (result.Type)
        {
            case RecommendationType.Success:
            case RecommendationType.PartialSuccess:
                DisplayCareers(result.Recommendations, result.Message);
                break;
                
            case RecommendationType.NearMiss:
                DisplayNearMissCareers(result.Recommendations, result.Message);
                break;
                
            case RecommendationType.InsufficientAcademic:
            case RecommendationType.NoMatch:
                DisplayFailureMessage(result.Message, domainScores);
                break;
        }
    }

    public void ExportAllParticipantsDomainScores()
    {
        
        HashSet<string> allUsernames = new HashSet<string>();
        
        foreach (string testFolder in testFolders)
        {
            string testPath = Path.Combine(rootPath, testFolder);
            if (!Directory.Exists(testPath)) continue;
            
            string resultsPath = Path.Combine(testPath, "results.csv");
            if (File.Exists(resultsPath))
            {
                string[] lines = File.ReadAllLines(resultsPath);
                foreach (string line in lines.Skip(1)) 
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] cols = line.Split(',');
                    if (cols.Length > 0)
                    {
                        string user = cols[0].Trim('"');
                        if (!string.IsNullOrEmpty(user) && user != "Username")
                            allUsernames.Add(user);
                    }
                }
            }
            
            string surveyPath = Path.Combine(testPath, "survey.csv");
            if (File.Exists(surveyPath))
            {
                string[] lines = File.ReadAllLines(surveyPath);
                foreach (string line in lines.Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var cols = ParseCSVLine(line);
                    if (cols.Count > 0)
                    {
                        string user = cols[0].Trim('"');
                        if (!string.IsNullOrEmpty(user) && user != "Username")
                            allUsernames.Add(user);
                    }
                }
            }
            
            if (testFolder == "PsychologicalEvaluationTest_test2")
            {
                string[] psycheFiles = { 
                    "EmotionalResponses.csv", 
                    "MotivationAssessment.csv", 
                    "Multitasking.csv", 
                    "PersonalityProfiler.csv", 
                    "TIPIInventory.csv" 
                };
                
                foreach (string file in psycheFiles)
                {
                    string path = Path.Combine(testPath, file);
                    if (!File.Exists(path)) continue;
                    
                    string[] lines = File.ReadAllLines(path);
                    foreach (string line in lines.Skip(1))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        string[] cols = SplitCsvLine(line);
                        if (cols.Length > 0)
                        {
                            string user = cols[0].Trim('"');
                            if (!string.IsNullOrEmpty(user) && user != "Username")
                                allUsernames.Add(user);
                        }
                    }
                }
            }
        }
                
        if (allUsernames.Count == 0)
        {
            return;
        }
        
        StringBuilder csv = new StringBuilder();
        
        csv.AppendLine("Username,Intellectual,Cognitive,Psychological,Behavioral,OverallAverage");
        
        int successCount = 0;
        int failCount = 0;
        
        foreach (string user in allUsernames.OrderBy(u => u))
        {
            try
            {
                Dictionary<string, Dictionary<string, object>> userResults = UserResultsLoader.LoadUserResults(user, rootPath);
                
                if (userResults.Count == 0)
                {
                    failCount++;
                    continue;
                }
                
                DomainScores domainScores = ScoreCalculator.ComputeDomainScores(userResults);
                
                float overallAvg = (domainScores.Intellectual + 
                                domainScores.Cognitive + 
                                domainScores.Psychological + 
                                domainScores.Behavioral) / 4f;
                
                csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "{0},{1:F2},{2:F2},{3:F2},{4:F2},{5:F2}",
                    user,
                    domainScores.Intellectual,
                    domainScores.Cognitive,
                    domainScores.Psychological,
                    domainScores.Behavioral,
                    overallAvg));
                                
                successCount++;
            }
            catch (System.Exception ex)
            {
                failCount++;
            }
        }
        
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = $"DomainScores_AllParticipants_{timestamp}.csv";
        string filepath = Path.Combine(rootPath, filename);
        
        File.WriteAllText(filepath, csv.ToString());
                
        if (placeholderText != null)
        {
            placeholderText.text = $"Exported domain scores for {successCount} participants to:\n{filename}";
            placeholderText.gameObject.SetActive(true);
        }
    }

    public void ExportAllParticipantsCareerRecommendations()
    {
        HashSet<string> allUsernames = new HashSet<string>();
        
        foreach (string testFolder in testFolders)
        {
            string testPath = Path.Combine(rootPath, testFolder);
            if (!Directory.Exists(testPath)) continue;
            
            string resultsPath = Path.Combine(testPath, "results.csv");
            if (File.Exists(resultsPath))
            {
                string[] lines = File.ReadAllLines(resultsPath);
                foreach (string line in lines.Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] cols = line.Split(',');
                    if (cols.Length > 0)
                    {
                        string user = cols[0].Trim('"');
                        if (!string.IsNullOrEmpty(user) && user != "Username")
                            allUsernames.Add(user);
                    }
                }
            }
            
            string surveyPath = Path.Combine(testPath, "survey.csv");
            if (File.Exists(surveyPath))
            {
                string[] lines = File.ReadAllLines(surveyPath);
                foreach (string line in lines.Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var cols = ParseCSVLine(line);
                    if (cols.Count > 0)
                    {
                        string user = cols[0].Trim('"');
                        if (!string.IsNullOrEmpty(user) && user != "Username")
                            allUsernames.Add(user);
                    }
                }
            }
            
            if (testFolder == "PsychologicalEvaluationTest_test2")
            {
                string[] psycheFiles = { 
                    "EmotionalResponses.csv", 
                    "MotivationAssessment.csv", 
                    "Multitasking.csv", 
                    "PersonalityProfiler.csv", 
                    "TIPIInventory.csv" 
                };
                
                foreach (string file in psycheFiles)
                {
                    string path = Path.Combine(testPath, file);
                    if (!File.Exists(path)) continue;
                    
                    string[] lines = File.ReadAllLines(path);
                    foreach (string line in lines.Skip(1))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        string[] cols = SplitCsvLine(line);
                        if (cols.Length > 0)
                        {
                            string user = cols[0].Trim('"');
                            if (!string.IsNullOrEmpty(user) && user != "Username")
                                allUsernames.Add(user);
                        }
                    }
                }
            }
        }
                
        if (allUsernames.Count == 0)
        {
            return;
        }
        
        StringBuilder csv = new StringBuilder();
        
        csv.AppendLine("Username,Career_1,FitScore_1,Career_2,FitScore_2,Career_3,FitScore_3," +
                    "Career_4,FitScore_4,Career_5,FitScore_5,FitScoreAvg,RecommendationType," +
                    "AcademicReadiness,DominantRIASEC");
        
        int successCount = 0;
        int failCount = 0;
        int noRecsCount = 0;
        
        foreach (string user in allUsernames.OrderBy(u => u))
        {
            try
            {
                Dictionary<string, Dictionary<string, object>> userResults = UserResultsLoader.LoadUserResults(user, rootPath);
                
                if (userResults.Count == 0)
                {
                    failCount++;
                    continue;
                }
                
                DomainScores domainScores = ScoreCalculator.ComputeDomainScores(userResults);
                
                CareerRecommendationResult result = CareerMapper.GenerateRecommendations(domainScores, userResults);
                
                StringBuilder row = new StringBuilder();
                row.Append($"{user}");
                       
                for (int i = 0; i < 5; i++)
                {
                    if (i < result.Recommendations.Count)
                    {
                        var rec = result.Recommendations[i];
                        string careerName = rec.RoleName.Replace(",", ";");
                        string fitScore = rec.FitScore.ToString("F2", CultureInfo.InvariantCulture);
                        row.Append($",\"{careerName}\",{fitScore}");
                    }
                    else
                    {
                        row.Append($",,");
                    }
                }

                float avgFitScore = 0f;
                if (result.Recommendations.Count > 0)
                {
                    avgFitScore = result.Recommendations.Average(r => r.FitScore);
                }
                string avgFitScoreStr = avgFitScore.ToString("F2", CultureInfo.InvariantCulture);
                row.Append($",{avgFitScoreStr}");

                row.Append($",{result.Type.ToString()}");

                float academicReadiness = (domainScores.Intellectual + domainScores.Cognitive) / 2f;
                string academicReadinessStr = academicReadiness.ToString("F2", CultureInfo.InvariantCulture);
                row.Append($",{academicReadinessStr}");

                string dominantRIASEC = domainScores.DominantRIASECCode ?? "N/A";
                row.Append($",{dominantRIASEC}");
                
                csv.AppendLine(row.ToString());
                
                if (result.Recommendations.Count == 0)
                {
                    noRecsCount++;
                }
                else
                {
                    successCount++;
                }
            }
            catch (System.Exception ex)
            {
                failCount++;
            }
        }
        
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = $"CareerRecommendations_AllParticipants_{timestamp}.csv";
        string filepath = Path.Combine(rootPath, filename);
        
        File.WriteAllText(filepath, csv.ToString());
        
        if (placeholderText != null)
        {
            placeholderText.text = $"Exported career recommendations for {successCount} participants to:\n{filename}";
            placeholderText.gameObject.SetActive(true);
        }
    }

    public void ExportDomainCorrelationAnalysis()
    {
        
        HashSet<string> allUsernames = new HashSet<string>();
        
        foreach (string testFolder in testFolders)
        {
            string testPath = Path.Combine(rootPath, testFolder);
            if (!Directory.Exists(testPath)) continue;
            
            string resultsPath = Path.Combine(testPath, "results.csv");
            if (File.Exists(resultsPath))
            {
                string[] lines = File.ReadAllLines(resultsPath);
                foreach (string line in lines.Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] cols = line.Split(',');
                    if (cols.Length > 0)
                    {
                        string user = cols[0].Trim('"');
                        if (!string.IsNullOrEmpty(user) && user != "Username")
                            allUsernames.Add(user);
                    }
                }
            }
            
            string surveyPath = Path.Combine(testPath, "survey.csv");
            if (File.Exists(surveyPath))
            {
                string[] lines = File.ReadAllLines(surveyPath);
                foreach (string line in lines.Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var cols = ParseCSVLine(line);
                    if (cols.Count > 0)
                    {
                        string user = cols[0].Trim('"');
                        if (!string.IsNullOrEmpty(user) && user != "Username")
                            allUsernames.Add(user);
                    }
                }
            }
            
            if (testFolder == "PsychologicalEvaluationTest_test2")
            {
                string[] psycheFiles = { 
                    "EmotionalResponses.csv", 
                    "MotivationAssessment.csv", 
                    "Multitasking.csv", 
                    "PersonalityProfiler.csv", 
                    "TIPIInventory.csv" 
                };
                
                foreach (string file in psycheFiles)
                {
                    string path = Path.Combine(testPath, file);
                    if (!File.Exists(path)) continue;
                    
                    string[] lines = File.ReadAllLines(path);
                    foreach (string line in lines.Skip(1))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        string[] cols = SplitCsvLine(line);
                        if (cols.Length > 0)
                        {
                            string user = cols[0].Trim('"');
                            if (!string.IsNullOrEmpty(user) && user != "Username")
                                allUsernames.Add(user);
                        }
                    }
                }
            }
        }
        
        
        if (allUsernames.Count < 3)
        {
            return;
        }
        
        List<float> intellectualScores = new List<float>();
        List<float> cognitiveScores = new List<float>();
        List<float> psychologicalScores = new List<float>();
        List<float> behavioralScores = new List<float>();
        List<string> validUsernames = new List<string>();
        
        foreach (string user in allUsernames.OrderBy(u => u))
        {
            try
            {
                Dictionary<string, Dictionary<string, object>> userResults = UserResultsLoader.LoadUserResults(user, rootPath);
                
                if (userResults.Count == 0)
                {
                    continue;
                }
                
                DomainScores domainScores = ScoreCalculator.ComputeDomainScores(userResults);
                
                intellectualScores.Add(domainScores.Intellectual);
                cognitiveScores.Add(domainScores.Cognitive);
                psychologicalScores.Add(domainScores.Psychological);
                behavioralScores.Add(domainScores.Behavioral);
                validUsernames.Add(user);
                
            }
            catch (System.Exception ex)
            {
                return;
            }
        }
        
        int n = validUsernames.Count;
        
        if (n < 3)
        {
            return;
        }
        
        float r_I_C = CalculatePearsonCorrelation(intellectualScores, cognitiveScores);
        float r_I_P = CalculatePearsonCorrelation(intellectualScores, psychologicalScores);
        float r_I_B = CalculatePearsonCorrelation(intellectualScores, behavioralScores);
        float r_C_P = CalculatePearsonCorrelation(cognitiveScores, psychologicalScores);
        float r_C_B = CalculatePearsonCorrelation(cognitiveScores, behavioralScores);
        float r_P_B = CalculatePearsonCorrelation(psychologicalScores, behavioralScores);
        
        var stats = new Dictionary<string, (float mean, float sd, float min, float max)>
        {
            { "Intellectual", CalculateStats(intellectualScores) },
            { "Cognitive", CalculateStats(cognitiveScores) },
            { "Psychological", CalculateStats(psychologicalScores) },
            { "Behavioral", CalculateStats(behavioralScores) }
        };
        
        StringBuilder csv = new StringBuilder();
        
        csv.AppendLine("Domain Correlation Analysis");
        csv.AppendLine($"Generated: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine($"Sample Size: {n} participants");
        csv.AppendLine();
        
        csv.AppendLine("Descriptive Statistics");
        csv.AppendLine("Domain,Mean,SD,Min,Max");
        foreach (var kvp in stats)
        {
            csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "{0},{1:F2},{2:F2},{3:F2},{4:F2}",
                kvp.Key, kvp.Value.mean, kvp.Value.sd, kvp.Value.min, kvp.Value.max));
        }
        csv.AppendLine();
        
        csv.AppendLine("Pearson Correlation Matrix");
        csv.AppendLine("Domain,Intellectual,Cognitive,Psychological,Behavioral");
        
        csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
            "Intellectual,1.00,{0:F3},{1:F3},{2:F3}",
            r_I_C, r_I_P, r_I_B));
        
        csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
            "Cognitive,{0:F3},1.00,{1:F3},{2:F3}",
            r_I_C, r_C_P, r_C_B));
        
        csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
            "Psychological,{0:F3},{1:F3},1.00,{2:F3}",
            r_I_P, r_C_P, r_P_B));
        
        csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
            "Behavioral,{0:F3},{1:F3},{2:F3},1.00",
            r_I_B, r_C_B, r_P_B));
        
        csv.AppendLine();
        
        csv.AppendLine("Interpretation Guide");
        csv.AppendLine("Correlation Strength,|r| Range,Interpretation");
        csv.AppendLine("Very Weak,0.00 - 0.19,Negligible relationship");
        csv.AppendLine("Weak,0.20 - 0.39,Weak relationship");
        csv.AppendLine("Moderate,0.40 - 0.59,Moderate relationship");
        csv.AppendLine("Strong,0.60 - 0.79,Strong relationship");
        csv.AppendLine("Very Strong,0.80 - 1.00,Very strong relationship (potential multicollinearity)");
        csv.AppendLine();
        
        csv.AppendLine("Statistical Interpretation");
        csv.AppendLine("Pair,Correlation,Strength,Distinctness");
        
        InterpretCorrelation(csv, "Intellectual-Cognitive", r_I_C);
        InterpretCorrelation(csv, "Intellectual-Psychological", r_I_P);
        InterpretCorrelation(csv, "Intellectual-Behavioral", r_I_B);
        InterpretCorrelation(csv, "Cognitive-Psychological", r_C_P);
        InterpretCorrelation(csv, "Cognitive-Behavioral", r_C_B);
        InterpretCorrelation(csv, "Psychological-Behavioral", r_P_B);
        
        csv.AppendLine();
        
        csv.AppendLine("Overall Domain Distinctness Assessment");
        float avgCorrelation = (Mathf.Abs(r_I_C) + Mathf.Abs(r_I_P) + Mathf.Abs(r_I_B) + 
                            Mathf.Abs(r_C_P) + Mathf.Abs(r_C_B) + Mathf.Abs(r_P_B)) / 6f;
        
        int highCorrelations = 0;
        if (Mathf.Abs(r_I_C) > 0.7f) highCorrelations++;
        if (Mathf.Abs(r_I_P) > 0.7f) highCorrelations++;
        if (Mathf.Abs(r_I_B) > 0.7f) highCorrelations++;
        if (Mathf.Abs(r_C_P) > 0.7f) highCorrelations++;
        if (Mathf.Abs(r_C_B) > 0.7f) highCorrelations++;
        if (Mathf.Abs(r_P_B) > 0.7f) highCorrelations++;
        
        csv.AppendLine($"Average |r| across all pairs: {avgCorrelation:F3}");
        csv.AppendLine($"Pairs with |r| > 0.70: {highCorrelations}/6");
        csv.AppendLine();
        
        if (avgCorrelation < 0.4f)
        {
            csv.AppendLine("EXCELLENT: Domains show strong distinctness (avg |r| < 0.40)");
            csv.AppendLine("The four domains measure largely independent constructs.");
        }
        else if (avgCorrelation < 0.6f)
        {
            csv.AppendLine("GOOD: Domains show adequate distinctness (avg |r| < 0.60)");
            csv.AppendLine("Some overlap exists but domains remain sufficiently distinct.");
        }
        else if (avgCorrelation < 0.7f)
        {
            csv.AppendLine("MODERATE: Some domains show substantial overlap (avg |r| < 0.70)");
            csv.AppendLine("Consider reviewing domain definitions for potential redundancy.");
        }
        else
        {
            csv.AppendLine("CONCERNING: High multicollinearity detected (avg |r| ≥ 0.70)");
            csv.AppendLine("Domains may be measuring overlapping constructs. Review recommended.");
        }
        
        if (highCorrelations > 0)
        {
            csv.AppendLine($"Note: {highCorrelations} pair(s) show high correlation (|r| > 0.70).");
        }
        
        csv.AppendLine();
        csv.AppendLine("For thesis: Report correlation matrix and discuss domain independence.");
        csv.AppendLine("Expected: I-C should correlate moderately (both academic)");
        csv.AppendLine("Expected: P-B should correlate moderately (both non-academic)");
        csv.AppendLine("Expected: I/C-P/B should correlate weakly (academic vs non-academic)");
        
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = $"DomainCorrelations_{timestamp}.csv";
        string filepath = Path.Combine(rootPath, filename);
        
        File.WriteAllText(filepath, csv.ToString());
                
        if (placeholderText != null)
        {
            placeholderText.text = $"Domain correlation analysis complete for {n} participants\n" +
                                $"Average |r| = {avgCorrelation:F3}\n" +
                                $"Results saved to: {filename}";
            placeholderText.gameObject.SetActive(true);
        }
    }

    private float CalculatePearsonCorrelation(List<float> x, List<float> y)
    {
        if (x.Count != y.Count || x.Count == 0)
        {
            return 0f;
        }
        
        int n = x.Count;
        
        float meanX = x.Average();
        float meanY = y.Average();
        
        float covariance = 0f;
        float varianceX = 0f;
        float varianceY = 0f;
        
        for (int i = 0; i < n; i++)
        {
            float devX = x[i] - meanX;
            float devY = y[i] - meanY;
            
            covariance += devX * devY;
            varianceX += devX * devX;
            varianceY += devY * devY;
        }
        
        covariance /= (n - 1);
        varianceX /= (n - 1);
        varianceY /= (n - 1);
        
        float stdX = Mathf.Sqrt(varianceX);
        float stdY = Mathf.Sqrt(varianceY);
        
        if (stdX == 0f || stdY == 0f)
        {
            return 0f;
        }
        
        float r = covariance / (stdX * stdY);
        
        return Mathf.Clamp(r, -1f, 1f);
    }

    private (float mean, float sd, float min, float max) CalculateStats(List<float> values)
    {
        if (values.Count == 0)
            return (0f, 0f, 0f, 0f);
        
        float mean = values.Average();
        float min = values.Min();
        float max = values.Max();
        
        float variance = 0f;
        foreach (float val in values)
        {
            float dev = val - mean;
            variance += dev * dev;
        }
        variance /= (values.Count - 1);
        float sd = Mathf.Sqrt(variance);
        
        return (mean, sd, min, max);
    }

    private void InterpretCorrelation(StringBuilder csv, string pairName, float r)
    {
        float absR = Mathf.Abs(r);
        string strength;
        string distinctness;
        
        if (absR < 0.20f)
        {
            strength = "Very Weak";
            distinctness = "Highly Distinct";
        }
        else if (absR < 0.40f)
        {
            strength = "Weak";
            distinctness = "Distinct";
        }
        else if (absR < 0.60f)
        {
            strength = "Moderate";
            distinctness = "Moderately Distinct";
        }
        else if (absR < 0.80f)
        {
            strength = "Strong";
            distinctness = "Some Overlap";
        }
        else
        {
            strength = "Very Strong";
            distinctness = "High Overlap (Potential Multicollinearity)";
        }
        
        csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
            "{0},{1:F3},{2},{3}",
            pairName, r, strength, distinctness));
    }

    public void ExportAllParticipantsBigFiveScores()
    {        
        HashSet<string> allUsernames = new HashSet<string>();
        
        foreach (string testFolder in testFolders)
        {
            string testPath = Path.Combine(rootPath, testFolder);
            if (!Directory.Exists(testPath)) continue;
            
            string resultsPath = Path.Combine(testPath, "results.csv");
            if (File.Exists(resultsPath))
            {
                string[] lines = File.ReadAllLines(resultsPath);
                foreach (string line in lines.Skip(1)) 
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] cols = line.Split(',');
                    if (cols.Length > 0)
                    {
                        string user = cols[0].Trim('"');
                        if (!string.IsNullOrEmpty(user) && user != "Username")
                            allUsernames.Add(user);
                    }
                }
            }
            
            string surveyPath = Path.Combine(testPath, "survey.csv");
            if (File.Exists(surveyPath))
            {
                string[] lines = File.ReadAllLines(surveyPath);
                foreach (string line in lines.Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var cols = ParseCSVLine(line);
                    if (cols.Count > 0)
                    {
                        string user = cols[0].Trim('"');
                        if (!string.IsNullOrEmpty(user) && user != "Username")
                            allUsernames.Add(user);
                    }
                }
            }
            
            if (testFolder == "PsychologicalEvaluationTest_test2")
            {
                string[] psycheFiles = { 
                    "EmotionalResponses.csv", 
                    "MotivationAssessment.csv", 
                    "Multitasking.csv", 
                    "PersonalityProfiler.csv", 
                    "TIPIInventory.csv" 
                };
                
                foreach (string file in psycheFiles)
                {
                    string path = Path.Combine(testPath, file);
                    if (!File.Exists(path)) continue;
                    
                    string[] lines = File.ReadAllLines(path);
                    foreach (string line in lines.Skip(1))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        string[] cols = SplitCsvLine(line);
                        if (cols.Length > 0)
                        {
                            string user = cols[0].Trim('"');
                            if (!string.IsNullOrEmpty(user) && user != "Username")
                                allUsernames.Add(user);
                        }
                    }
                }
            }
        }
        
        
        if (allUsernames.Count == 0)
        {
            return;
        }
        
        StringBuilder csv = new StringBuilder();
        
        csv.AppendLine("Username,Openness,Conscientiousness,Extraversion,Agreeableness,Neuroticism,PersonalityAvg,DominantTrait,LowestTrait");
        
        int successCount = 0;
        int failCount = 0;
        
        foreach (string user in allUsernames.OrderBy(u => u))
        {
            try
            {
                Dictionary<string, Dictionary<string, object>> userResults = UserResultsLoader.LoadUserResults(user, rootPath);
                
                if (userResults.Count == 0)
                {
                    failCount++;
                    continue;
                }
                
                DomainScores domainScores = ScoreCalculator.ComputeDomainScores(userResults);
                
                float openness = domainScores.BigFive.ContainsKey("O") ? domainScores.BigFive["O"] : 0f;
                float conscientiousness = domainScores.BigFive.ContainsKey("C") ? domainScores.BigFive["C"] : 0f;
                float extraversion = domainScores.BigFive.ContainsKey("E") ? domainScores.BigFive["E"] : 0f;
                float agreeableness = domainScores.BigFive.ContainsKey("A") ? domainScores.BigFive["A"] : 0f;
                float neuroticism = domainScores.BigFive.ContainsKey("N") ? domainScores.BigFive["N"] : 0f;
                
                float personalityAvg = (openness + conscientiousness + extraversion + 
                                    agreeableness + neuroticism) / 5f;
                
                var traits = new List<(string name, float score)>
                {
                    ("Openness", openness),
                    ("Conscientiousness", conscientiousness),
                    ("Extraversion", extraversion),
                    ("Agreeableness", agreeableness),
                    ("Neuroticism", neuroticism)
                };
                
                var sortedTraits = traits.OrderByDescending(t => t.score).ToList();
                string dominantTrait = sortedTraits[0].name;
                string lowestTrait = sortedTraits[4].name;
                
                csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "{0},{1:F2},{2:F2},{3:F2},{4:F2},{5:F2},{6:F2},{7},{8}",
                    user,
                    openness,
                    conscientiousness,
                    extraversion,
                    agreeableness,
                    neuroticism,
                    personalityAvg,
                    dominantTrait,
                    lowestTrait));
                                
                successCount++;
            }
            catch (System.Exception ex)
            {
                failCount++;
            }
        }
        
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = $"BigFiveScores_AllParticipants_{timestamp}.csv";
        string filepath = Path.Combine(rootPath, filename);
        
        File.WriteAllText(filepath, csv.ToString());
                
        if (placeholderText != null)
        {
            placeholderText.text = $"Exported Big Five scores for {successCount} participants to:\n{filename}";
            placeholderText.gameObject.SetActive(true);
        }
    }

    public void ExportComprehensiveParticipantData()
    {
        HashSet<string> allUsernames = new HashSet<string>();
        
        foreach (string testFolder in testFolders)
        {
            string testPath = Path.Combine(rootPath, testFolder);
            if (!Directory.Exists(testPath)) continue;
            
            string resultsPath = Path.Combine(testPath, "results.csv");
            if (File.Exists(resultsPath))
            {
                string[] lines = File.ReadAllLines(resultsPath);
                foreach (string line in lines.Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] cols = line.Split(',');
                    if (cols.Length > 0)
                    {
                        string user = cols[0].Trim('"');
                        if (!string.IsNullOrEmpty(user) && user != "Username")
                            allUsernames.Add(user);
                    }
                }
            }
            
            string surveyPath = Path.Combine(testPath, "survey.csv");
            if (File.Exists(surveyPath))
            {
                string[] lines = File.ReadAllLines(surveyPath);
                foreach (string line in lines.Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var cols = ParseCSVLine(line);
                    if (cols.Count > 0)
                    {
                        string user = cols[0].Trim('"');
                        if (!string.IsNullOrEmpty(user) && user != "Username")
                            allUsernames.Add(user);
                    }
                }
            }
            
            if (testFolder == "PsychologicalEvaluationTest_test2")
            {
                string[] psycheFiles = { 
                    "EmotionalResponses.csv", 
                    "MotivationAssessment.csv", 
                    "Multitasking.csv", 
                    "PersonalityProfiler.csv", 
                    "TIPIInventory.csv" 
                };
                
                foreach (string file in psycheFiles)
                {
                    string path = Path.Combine(testPath, file);
                    if (!File.Exists(path)) continue;
                    
                    string[] lines = File.ReadAllLines(path);
                    foreach (string line in lines.Skip(1))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        string[] cols = SplitCsvLine(line);
                        if (cols.Length > 0)
                        {
                            string user = cols[0].Trim('"');
                            if (!string.IsNullOrEmpty(user) && user != "Username")
                                allUsernames.Add(user);
                        }
                    }
                }
            }
        }
                
        if (allUsernames.Count == 0)
        {
            return;
        }
        
        StringBuilder csv = new StringBuilder();
        
        csv.AppendLine("Username," +
                    "Intellectual,Cognitive,Psychological,Behavioral,DomainAvg," +
                    "Openness,Conscientiousness,Extraversion,Agreeableness,Neuroticism," +
                    "RIASEC_R,RIASEC_I,RIASEC_A,RIASEC_S,RIASEC_E,RIASEC_C," +
                    "DominantRIASEC,AcademicReadiness," +
                    "TopCareer,TopCareerFitScore,RecommendationType");
        
        int successCount = 0;
        int failCount = 0;
        
        foreach (string user in allUsernames.OrderBy(u => u))
        {
            try
            {
                Dictionary<string, Dictionary<string, object>> userResults = UserResultsLoader.LoadUserResults(user, rootPath);
                
                if (userResults.Count == 0)
                {
                    failCount++;
                    continue;
                }
                
                DomainScores domainScores = ScoreCalculator.ComputeDomainScores(userResults);
                
                float domainAvg = (domainScores.Intellectual + domainScores.Cognitive + 
                                domainScores.Psychological + domainScores.Behavioral) / 4f;
                
                float openness = domainScores.BigFive.ContainsKey("O") ? domainScores.BigFive["O"] : 0f;
                float conscientiousness = domainScores.BigFive.ContainsKey("C") ? domainScores.BigFive["C"] : 0f;
                float extraversion = domainScores.BigFive.ContainsKey("E") ? domainScores.BigFive["E"] : 0f;
                float agreeableness = domainScores.BigFive.ContainsKey("A") ? domainScores.BigFive["A"] : 0f;
                float neuroticism = domainScores.BigFive.ContainsKey("N") ? domainScores.BigFive["N"] : 0f;
                
                float riasec_r = domainScores.RIASEC.ContainsKey("R") ? domainScores.RIASEC["R"] : 0f;
                float riasec_i = domainScores.RIASEC.ContainsKey("I") ? domainScores.RIASEC["I"] : 0f;
                float riasec_a = domainScores.RIASEC.ContainsKey("A") ? domainScores.RIASEC["A"] : 0f;
                float riasec_s = domainScores.RIASEC.ContainsKey("S") ? domainScores.RIASEC["S"] : 0f;
                float riasec_e = domainScores.RIASEC.ContainsKey("E") ? domainScores.RIASEC["E"] : 0f;
                float riasec_c = domainScores.RIASEC.ContainsKey("C") ? domainScores.RIASEC["C"] : 0f;
                
                string dominantRIASEC = domainScores.DominantRIASECCode ?? "N/A";
                
                float academicReadiness = (domainScores.Intellectual + domainScores.Cognitive) / 2f;
                
                CareerRecommendationResult result = CareerMapper.GenerateRecommendations(domainScores, userResults);
                
                string topCareer = "N/A";
                float topFitScore = 0f;
                string recType = result.Type.ToString();
                
                if (result.Recommendations.Count > 0)
                {
                    topCareer = result.Recommendations[0].RoleName.Replace(",", ";");
                    topFitScore = result.Recommendations[0].FitScore;
                }
                
                csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "{0}," +                                          
                    "{1:F2},{2:F2},{3:F2},{4:F2},{5:F2}," +          
                    "{6:F2},{7:F2},{8:F2},{9:F2},{10:F2}," +         
                    "{11:F2},{12:F2},{13:F2},{14:F2},{15:F2},{16:F2}," + 
                    "{17},{18:F2}," +                                 
                    "\"{19}\",{20:F2},{21}",                         
                    user,
                    domainScores.Intellectual, domainScores.Cognitive, domainScores.Psychological, domainScores.Behavioral, domainAvg,
                    openness, conscientiousness, extraversion, agreeableness, neuroticism,
                    riasec_r, riasec_i, riasec_a, riasec_s, riasec_e, riasec_c,
                    dominantRIASEC, academicReadiness,
                    topCareer, topFitScore, recType));
                                
                successCount++;
            }
            catch (System.Exception ex)
            {
                failCount++;
            }
        }
        
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = $"ComprehensiveData_AllParticipants_{timestamp}.csv";
        string filepath = Path.Combine(rootPath, filename);
        
        File.WriteAllText(filepath, csv.ToString());
                
        if (placeholderText != null)
        {
            placeholderText.text = $"Exported comprehensive data for {successCount} participants to:\n{filename}";
            placeholderText.gameObject.SetActive(true);
        }
    }

    public void ExportBigFiveCorrelationAnalysis()
    {
        
        HashSet<string> allUsernames = new HashSet<string>();
        
        foreach (string testFolder in testFolders)
        {
            string testPath = Path.Combine(rootPath, testFolder);
            if (!Directory.Exists(testPath)) continue;
            
            string resultsPath = Path.Combine(testPath, "results.csv");
            if (File.Exists(resultsPath))
            {
                string[] lines = File.ReadAllLines(resultsPath);
                foreach (string line in lines.Skip(1))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] cols = line.Split(',');
                    if (cols.Length > 0)
                    {
                        string user = cols[0].Trim('"');
                        if (!string.IsNullOrEmpty(user) && user != "Username")
                            allUsernames.Add(user);
                    }
                }
            }
        }
        
        if (allUsernames.Count < 3)
        {
            return;
        }
        
        List<float> opennessScores = new List<float>();
        List<float> conscientiousnessScores = new List<float>();
        List<float> extraversionScores = new List<float>();
        List<float> agreeablenessScores = new List<float>();
        List<float> neuroticismScores = new List<float>();
        List<string> validUsernames = new List<string>();
        
        foreach (string user in allUsernames.OrderBy(u => u))
        {
            try
            {
                Dictionary<string, Dictionary<string, object>> userResults = UserResultsLoader.LoadUserResults(user, rootPath);
                
                if (userResults.Count == 0)
                {
                    continue;
                }
                
                DomainScores domainScores = ScoreCalculator.ComputeDomainScores(userResults);
                
                float openness = domainScores.BigFive.ContainsKey("O") ? domainScores.BigFive["O"] : 0f;
                float conscientiousness = domainScores.BigFive.ContainsKey("C") ? domainScores.BigFive["C"] : 0f;
                float extraversion = domainScores.BigFive.ContainsKey("E") ? domainScores.BigFive["E"] : 0f;
                float agreeableness = domainScores.BigFive.ContainsKey("A") ? domainScores.BigFive["A"] : 0f;
                float neuroticism = domainScores.BigFive.ContainsKey("N") ? domainScores.BigFive["N"] : 0f;
                
                opennessScores.Add(openness);
                conscientiousnessScores.Add(conscientiousness);
                extraversionScores.Add(extraversion);
                agreeablenessScores.Add(agreeableness);
                neuroticismScores.Add(neuroticism);
                validUsernames.Add(user);
                
            }
            catch (System.Exception ex)
            {
                return;
            }
        }
        
        int n = validUsernames.Count;
        
        if (n < 3)
        {
            return;
        }
        
        float r_O_C = CalculatePearsonCorrelation(opennessScores, conscientiousnessScores);
        float r_O_E = CalculatePearsonCorrelation(opennessScores, extraversionScores);
        float r_O_A = CalculatePearsonCorrelation(opennessScores, agreeablenessScores);
        float r_O_N = CalculatePearsonCorrelation(opennessScores, neuroticismScores);
        float r_C_E = CalculatePearsonCorrelation(conscientiousnessScores, extraversionScores);
        float r_C_A = CalculatePearsonCorrelation(conscientiousnessScores, agreeablenessScores);
        float r_C_N = CalculatePearsonCorrelation(conscientiousnessScores, neuroticismScores);
        float r_E_A = CalculatePearsonCorrelation(extraversionScores, agreeablenessScores);
        float r_E_N = CalculatePearsonCorrelation(extraversionScores, neuroticismScores);
        float r_A_N = CalculatePearsonCorrelation(agreeablenessScores, neuroticismScores);
        
        var stats = new Dictionary<string, (float mean, float sd, float min, float max)>
        {
            { "Openness", CalculateStats(opennessScores) },
            { "Conscientiousness", CalculateStats(conscientiousnessScores) },
            { "Extraversion", CalculateStats(extraversionScores) },
            { "Agreeableness", CalculateStats(agreeablenessScores) },
            { "Neuroticism", CalculateStats(neuroticismScores) }
        };
        
        StringBuilder csv = new StringBuilder();
        
        csv.AppendLine("Big Five Personality Correlation Analysis");
        csv.AppendLine($"Generated: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine($"Sample Size: {n} participants");
        csv.AppendLine();
        
        csv.AppendLine("Descriptive Statistics");
        csv.AppendLine("Trait,Mean,SD,Min,Max");
        foreach (var kvp in stats)
        {
            csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "{0},{1:F2},{2:F2},{3:F2},{4:F2}",
                kvp.Key, kvp.Value.mean, kvp.Value.sd, kvp.Value.min, kvp.Value.max));
        }
        csv.AppendLine();
        
        csv.AppendLine("Pearson Correlation Matrix - Big Five Traits");
        csv.AppendLine("Trait,O,C,E,A,N");
        
        csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
            "Openness (O),1.00,{0:F3},{1:F3},{2:F3},{3:F3}",
            r_O_C, r_O_E, r_O_A, r_O_N));
        
        csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
            "Conscientiousness (C),{0:F3},1.00,{1:F3},{2:F3},{3:F3}",
            r_O_C, r_C_E, r_C_A, r_C_N));
        
        csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
            "Extraversion (E),{0:F3},{1:F3},1.00,{2:F3},{3:F3}",
            r_O_E, r_C_E, r_E_A, r_E_N));
        
        csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
            "Agreeableness (A),{0:F3},{1:F3},{2:F3},1.00,{3:F3}",
            r_O_A, r_C_A, r_E_A, r_A_N));
        
        csv.AppendLine(string.Format(CultureInfo.InvariantCulture,
            "Neuroticism (N),{0:F3},{1:F3},{2:F3},{3:F3},1.00",
            r_O_N, r_C_N, r_E_N, r_A_N));
        
        csv.AppendLine();
        
        csv.AppendLine("Statistical Interpretation");
        csv.AppendLine("Pair,Correlation,Strength,Independence");
        
        InterpretCorrelation(csv, "Openness-Conscientiousness", r_O_C);
        InterpretCorrelation(csv, "Openness-Extraversion", r_O_E);
        InterpretCorrelation(csv, "Openness-Agreeableness", r_O_A);
        InterpretCorrelation(csv, "Openness-Neuroticism", r_O_N);
        InterpretCorrelation(csv, "Conscientiousness-Extraversion", r_C_E);
        InterpretCorrelation(csv, "Conscientiousness-Agreeableness", r_C_A);
        InterpretCorrelation(csv, "Conscientiousness-Neuroticism", r_C_N);
        InterpretCorrelation(csv, "Extraversion-Agreeableness", r_E_A);
        InterpretCorrelation(csv, "Extraversion-Neuroticism", r_E_N);
        InterpretCorrelation(csv, "Agreeableness-Neuroticism", r_A_N);
        
        csv.AppendLine();
        
        csv.AppendLine("Overall Trait Independence Assessment");
        float avgCorrelation = (Mathf.Abs(r_O_C) + Mathf.Abs(r_O_E) + Mathf.Abs(r_O_A) + Mathf.Abs(r_O_N) +
                            Mathf.Abs(r_C_E) + Mathf.Abs(r_C_A) + Mathf.Abs(r_C_N) +
                            Mathf.Abs(r_E_A) + Mathf.Abs(r_E_N) + Mathf.Abs(r_A_N)) / 10f;
        
        csv.AppendLine($"Average |r| across all trait pairs: {avgCorrelation:F3}");
        csv.AppendLine();
        
        if (avgCorrelation < 0.3f)
        {
            csv.AppendLine("EXCELLENT: Big Five traits show strong independence (avg |r| < 0.30)");
        }
        else if (avgCorrelation < 0.5f)
        {
            csv.AppendLine("GOOD: Big Five traits show adequate independence (avg |r| < 0.50)");
        }
        else
        {
            csv.AppendLine("MODERATE: Some traits show overlap - review measurement (avg |r| ≥ 0.50)");
        }
        
        csv.AppendLine();
        csv.AppendLine("Note: Big Five traits are theoretically independent orthogonal factors.");
        csv.AppendLine("Expected: Most correlations should be weak (|r| < 0.30)");
        csv.AppendLine("Expected: C-N negative correlation is common (conscientious people less neurotic)");
        csv.AppendLine("Expected: E-N negative correlation is common (extraverts less neurotic)");
        
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filename = $"BigFiveCorrelations_{timestamp}.csv";
        string filepath = Path.Combine(rootPath, filename);
        
        File.WriteAllText(filepath, csv.ToString());
        
        if (placeholderText != null)
        {
            placeholderText.text = $"Big Five correlation analysis complete for {n} participants\n" +
                                $"Average |r| = {avgCorrelation:F3}\n" +
                                $"Results saved to: {filename}";
            placeholderText.gameObject.SetActive(true);
        }
    }

    public void GenerateMyPDFReport()
    {        
        if (string.IsNullOrEmpty(username))
        {
            if (placeholderText != null)
            {
                placeholderText.text = "Error: No user logged in";
                placeholderText.gameObject.SetActive(true);
            }
            return;
        }
        
        try
        {
            Dictionary<string, Dictionary<string, object>> userResults = UserResultsLoader.LoadUserResults(username, rootPath);
            
            if (userResults.Count == 0)
            {
                if (placeholderText != null)
                {
                    placeholderText.text = $"No test data found for {username}";
                    placeholderText.gameObject.SetActive(true);
                }
                return;
            }
            
            DomainScores domainScores = ScoreCalculator.ComputeDomainScores(userResults);
            
            CareerRecommendationResult result = CareerMapper.GenerateRecommendations(domainScores, userResults);
            
            var reportData = new Dictionary<string, object>();
            
            reportData["username"] = username;
            reportData["date"] = System.DateTime.Now.ToString("dd/MM/yyyy");
            reportData["duration"] = "3 hrs 30";
            var domains = new Dictionary<string, float>
            {
                { "I", domainScores.Intellectual },
                { "C", domainScores.Cognitive },
                { "P", domainScores.Psychological },
                { "B", domainScores.Behavioral }
            };
            reportData["domains"] = domains;
            
            reportData["riasec"] = domainScores.RIASEC;
            reportData["dominant_riasec"] = domainScores.DominantRIASECCode ?? "N/A";
            
            reportData["bigfive"] = domainScores.BigFive;
            
            float academicReadiness = (domainScores.Intellectual + domainScores.Cognitive) / 2f;
            reportData["academic_readiness"] = academicReadiness;
            
            var recommendations = new List<Dictionary<string, object>>();
            foreach (var rec in result.Recommendations.Take(5))
            {
                var recDict = new Dictionary<string, object>
                {
                    { "career_name", rec.RoleName },
                    { "faculty", rec.FieldName },
                    { "fit_score", rec.FitScore },
                    { "explanation", GenerateExplanation(rec, domainScores) },
                    { "career_paths", "Various career opportunities available" } 
                };
                recommendations.Add(recDict);
            }
            reportData["recommendations"] = recommendations;
            
            string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(reportData, Newtonsoft.Json.Formatting.Indented);

            
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string jsonPath = Path.Combine(rootPath, $"temp_report_data_{timestamp}.json");
            File.WriteAllText(jsonPath, jsonData);
            
            string pdfPath = Path.Combine(rootPath, $"CareerReportNew_{username}_{timestamp}.pdf");
            
            string pythonScript = Path.Combine(Application.streamingAssetsPath, "Configs", "generate_career_report.py");
            
            if (!File.Exists(pythonScript))
            {
                pythonScript = Path.Combine(rootPath, "ConfigFiles", "generate_career_report.py");
                if (!File.Exists(pythonScript))
                {
                    if (placeholderText != null)
                    {
                        placeholderText.text = "Error: Report generator script not found";
                        placeholderText.gameObject.SetActive(true);
                    }
                    return;
                }
            }
            
            
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "/Library/Frameworks/Python.framework/Versions/3.10/bin/python3";
            process.StartInfo.Arguments = $"\"{pythonScript}\" \"{jsonPath}\" \"{pdfPath}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
                        
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string errors = process.StandardError.ReadToEnd();
            process.WaitForExit();
            
            if (process.ExitCode == 0)
            {
                
                File.Delete(jsonPath);
                
                if (placeholderText != null)
                {
                    placeholderText.text = $"PDF Report generated!\nSaved to: CareerReportNew_{username}_{timestamp}.pdf";
                    placeholderText.gameObject.SetActive(true);
                }
                
                #if UNITY_STANDALONE_OSX
                System.Diagnostics.Process.Start("open", pdfPath);
                #elif UNITY_STANDALONE_WIN
                System.Diagnostics.Process.Start(pdfPath);
                #elif UNITY_STANDALONE_LINUX
                System.Diagnostics.Process.Start("xdg-open", pdfPath);
                #endif
            }
            else
            {
                if (placeholderText != null)
                {
                    placeholderText.text = $"Error generating PDF report. Check console for details.";
                    placeholderText.gameObject.SetActive(true);
                }
            }
        }
        catch (System.Exception ex)
        {
            
            if (placeholderText != null)
            {
                placeholderText.text = $"Error: Can only be used in Unity Editor";
                placeholderText.gameObject.SetActive(true);
            }
        }
    }

    private string GenerateExplanation(CareerRecommendation rec, DomainScores scores)
    {
        StringBuilder explanation = new StringBuilder();
        
        var topDomains = new List<(string, float)>
        {
            ("Intellectual", scores.Intellectual),
            ("Cognitive", scores.Cognitive),
            ("Psychological", scores.Psychological),
            ("Behavioral", scores.Behavioral)
        };
        topDomains.Sort((a, b) => b.Item2.CompareTo(a.Item2));
        
        explanation.Append($"Your strong {topDomains[0].Item1} ({topDomains[0].Item2:F0}%) ");
        explanation.Append($"and {topDomains[1].Item1} ({topDomains[1].Item2:F0}%) scores ");
        explanation.Append($"align well with {rec.RoleName}'s requirements. ");
        

        explanation.Append($"Your {scores.DominantRIASECCode} RIASEC profile indicates strong fit for this field's work style. ");
        

        float academicReadiness = (scores.Intellectual + scores.Cognitive) / 2f;
        if (academicReadiness >= 70)
        {
            explanation.Append($"Your academic readiness ({academicReadiness:F0}%) shows you're well-prepared for university-level study.");
        }
        else if (academicReadiness >= 50)
        {
            explanation.Append($"Your academic readiness ({academicReadiness:F0}%) is adequate with room for development through bridging programs.");
        }
        else
        {
            explanation.Append($"Consider strengthening your academic foundation before pursuing degree-level study.");
        }
        
        return explanation.ToString();
    }

    private void DisplayFailureMessage(string message, DomainScores scores)
    {
        foreach (Transform child in careerListParent)
            Destroy(child.gameObject);

        placeholderText.gameObject.SetActive(false);

        GameObject item = Instantiate(careerResultPrefab, careerListParent);
        var ui = item.GetComponent<CareerResultUI>();

        ui.fieldText.text = "Career Guidance";
        ui.roleText.text = "";
        ui.scoreText.text = "";
        ui.explanationText.text = message;
    }


    public Dictionary<string, Dictionary<string, object>> LoadUserResults(string username)
    {
        var results = new Dictionary<string, Dictionary<string, object>>();

        string[] testFolders = {
            "VerbalComprehensionTest_test2",
            "MentalMathsTest_test2",
            "MemoryRecallTest_test2",
            "ReactionTimerTest_test2",
            "NavigationGrid_test2",
            "PsychologicalEvaluationTest_test2",
            "OverallEvaluation" 
        };

        foreach (string testFolder in testFolders)
        {
            string testPath = Path.Combine(rootPath, testFolder);
  
            if (!Directory.Exists(testPath)) continue;
            if (testFolder == "PsychologicalEvaluationTest_test2")
            {
                var psycheData = new Dictionary<string, object>();
                string[] psycheFiles = { "EmotionalResponses.csv", "MotivationAssessment.csv", "Multitasking.csv", "PersonalityProfiler.csv", "TIPIInventory.csv" };

                foreach (string file in psycheFiles)
                {
                    string path = Path.Combine(testPath, file);
                    if (!File.Exists(path)) continue;

                    var dict = ParsePsychEvalCSV(path, username);
                    string key = Path.GetFileNameWithoutExtension(file);
                    if (dict != null && dict.Count > 0)
                    {
                        psycheData[key] = dict;
                    }
                }
                if (psycheData.Count > 0)
                    results[testFolder] = psycheData;
            }

            else
            {
                string resultsPath = Path.Combine(testPath, "results.csv");
                string surveyPath = Path.Combine(testPath, "survey.csv");
                var combinedData = new Dictionary<string, object>();

                if (File.Exists(resultsPath))
                {
                    var resultsDict = ParseCSV(resultsPath, username);
                    if (resultsDict != null && resultsDict.Count > 0)
                        combinedData["results"] = resultsDict;
                }

                if (File.Exists(surveyPath))
                {
                    var surveyDict = ParseSurveyCSV(surveyPath, username);
                    if (surveyDict != null && surveyDict.Count > 0)
                        combinedData["survey"] = surveyDict;
                }
                if (combinedData.Count > 0)
                    results[testFolder] = combinedData;
            }
        }

        return results;
    }

    private static List<string> ParseCSVLine(string line)
    {
        var fields = new List<string>();
        bool inQuotes = false;
        var field = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '\"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                {
                    field.Append('\"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(field.ToString());
                field.Clear();
            }
            else
            {
                field.Append(c);
            }
        }

        fields.Add(field.ToString());
        return fields;
    }

    public Dictionary<string, string> ParsePsychEvalCSV(string path, string username)
    {
        var dict = new Dictionary<string, string>();
        if (!File.Exists(path))
        {
            return dict;
        }

        string[] lines = File.ReadAllLines(path);
        if (lines.Length < 2)
        {
            return dict;
        }

        string[] headers = SplitCsvLine(lines[0]);

        int rowCount = 0;
        foreach (string line in lines.Skip(1))
        {
            rowCount++;
            string[] cols = SplitCsvLine(line);

            if (cols.Length != headers.Length)
            {
                continue;
            }

            string rowUser = cols[0].Trim('"');
            if (!rowUser.Equals(username))
            {
                continue;
            }

            for (int i = 1; i < cols.Length; i++)
            {
                string key = headers[i].Trim('"');
                string val = cols[i].Trim('"');
                string namespacedKey = $"{Path.GetFileName(Path.GetDirectoryName(path))}::{key}";
                dict[namespacedKey] = val;
            }
        }
        return dict;
    }

    private string[] SplitCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var current = new System.Text.StringBuilder();

        foreach (char c in line)
        {
            if (c == '\"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());
        return result.ToArray();
    }

    private Dictionary<string, string> ParseSurveyCSV(string path, string username)
    {
        var dict = new Dictionary<string, string>();
        if (!File.Exists(path))
        {
            return dict;
        }

        string[] lines = File.ReadAllLines(path);
        if (lines.Length < 2) return dict;

        var headers = ParseCSVLine(lines[0]);

        foreach (string rawLine in lines.Skip(1))
        {
            var cols = ParseCSVLine(rawLine);

            if (cols.Count != headers.Count)
            {
                continue;
            }

            string rowUser = cols[0].Trim('"');
            if (!rowUser.Equals(username))
            {
                continue;
            }

            for (int i = 1; i < cols.Count; i++)
            {
                string key = headers[i].Trim('"');
                string val = cols[i].Trim('"');
                string namespacedKey = $"{Path.GetFileName(Path.GetDirectoryName(path))}::{key}";
                dict[namespacedKey] = val;
            }
        }
        return dict;
    }
    private Dictionary<string, object> ParseCSV(string path, string username)
    {
        var dict = new Dictionary<string, object>();
        string[] lines = File.ReadAllLines(path);

        if (lines.Length < 2) return dict;

        string[] headers = lines[0].Split(',');

        foreach (string line in lines.Skip(1))
        {
            string[] cols = line.Split(',');
            if (cols.Length != headers.Length) continue;

            if (!cols[0].Equals(username)) continue; 

            for (int i = 1; i < cols.Length; i++)
            {
                string key = headers[i];
                string val = cols[i];
                string namespacedKey = $"{Path.GetFileName(Path.GetDirectoryName(path))}::{key}";

                if (float.TryParse(val, out float f))
                    dict[namespacedKey] = f;
                else
                    dict[namespacedKey] = val;
            }
        }
        return dict;
    }


    private void UpdateProgressBar(int completed, int total)
    {
        float percent = (float)completed / total;
        progressBar.value = percent;
        progressText.text = $"Tests Completed: {completed} of {total} ({Mathf.RoundToInt(percent * 100)}%)";
        if (completed >= 7 && surveyButton != null)
        {
            surveyButton.interactable = true;
            progressText.text = $"Tests Completed: {completed} of {total} ({Mathf.RoundToInt(percent * 100)}%). Overall Survey is unlocked!";
        }
        else if (surveyButton != null)
        {
            surveyButton.interactable = false;
            progressText.text = $"Tests Completed: {completed} of {total} ({Mathf.RoundToInt(percent * 100)}%). Overall Survey remains locked.";
        }
    }

    private void DisplayCareers(List<CareerRecommendation> careers, string headerMessage = "")
    {
        scrollbar.value = 1;
        foreach (Transform child in careerListParent)
            Destroy(child.gameObject);

        placeholderText.gameObject.SetActive(careers.Count == 0);

        int count = 0;
        foreach (CareerRecommendation rec in careers)
        {
            if (count >= 5) break;
            GameObject item = Instantiate(careerResultPrefab, careerListParent);
            var ui = item.GetComponent<CareerResultUI>();
            ui.Setup(rec, count % 2 == 0);
            count++;
        }
    }

    private void DisplayNearMissCareers(List<CareerRecommendation> careers, string message)
    {
        scrollbar.value = 1;
        foreach (Transform child in careerListParent)
            Destroy(child.gameObject);

        placeholderText.gameObject.SetActive(false);
        
        GameObject headerItem = Instantiate(careerResultPrefab, careerListParent);
        var headerUI = headerItem.GetComponent<CareerResultUI>();
        headerUI.fieldText.text = "Alternative Pathway Recommendations";
        headerUI.roleText.text = "";
        headerUI.scoreText.text = "";
        headerUI.explanationText.text = message;

        int count = 0;
        foreach (CareerRecommendation rec in careers)
        {
            if (count >= 5) break;
            GameObject item = Instantiate(careerResultPrefab, careerListParent);
            var ui = item.GetComponent<CareerResultUI>();
            ui.Setup(rec, count % 2 == 0);
            
            ui.scoreText.text = $"{rec.FitScore:F1}/100 (Alternative Pathway)";
            
            count++;
        }
        
    }
}
