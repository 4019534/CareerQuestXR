using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

[System.Serializable]
public class CareerRecommendationResult
{
    public RecommendationType Type;
    public List<CareerRecommendation> Recommendations;
    public string Message;
    public DomainScores ParticipantScores;
    
    public CareerRecommendationResult()
    {
        Recommendations = new List<CareerRecommendation>();
    }
}

[System.Serializable]
public enum RecommendationType
{
    Success,              
    PartialSuccess,      
    NearMiss,           
    InsufficientAcademic, 
    NoMatch             
}

public static class CareerMapper
{
    private static JObject careerFieldDB = null;
    private const float ACADEMIC_READINESS_THRESHOLD = 40f;    
    private const float DEGREE_RECOMMENDATION_THRESHOLD = 40f;   
    private const float ALTERNATIVE_PATHWAY_THRESHOLD = 35f;    

    public static CareerRecommendationResult GenerateRecommendations(
        DomainScores participantScores, 
        Dictionary<string, Dictionary<string, object>> userResults = null)
    {
        LoadCareerFieldDBIfNeeded();
        
        var result = new CareerRecommendationResult
        {
            ParticipantScores = participantScores
        };
        
        if (careerFieldDB == null || careerFieldDB["fields"] == null)
        {
            result.Type = RecommendationType.NoMatch;
            result.Message = "Error: Career database not loaded.";
            return result;
        }

        if (!MeetsAcademicReadiness(participantScores, out float academicReadiness))
        {
            
            result.Type = RecommendationType.InsufficientAcademic;
            result.Message = $"Your academic readiness score is {academicReadiness:F1}/100. " +
                            $"University degree programs require a minimum academic readiness of {ACADEMIC_READINESS_THRESHOLD}. " +
                            $"We recommend:\n\n" +
                            $"• Exploring diploma or certificate programs to build foundational skills\n" +
                            $"• Strengthening your academic abilities through bridging courses\n" +
                            $"• Considering vocational and technical training pathways\n" +
                            $"• Revisiting degree programs after completing preparatory education\n\n" +
                            $"Focus on developing your Intellectual (currently {participantScores.Intellectual:F1}) " +
                            $"and Cognitive (currently {participantScores.Cognitive:F1}) capabilities.";
            
            return result;
        }
        
        var allRecommendations = new List<CareerRecommendation>();
        var fields = (JArray)careerFieldDB["fields"];

        foreach (var fieldToken in fields)
        {
            var field = (JObject)fieldToken;
            string fieldName = field["name"].ToString();

            var fieldRIASEC = (JObject)field["riasec"];
            float fieldRiasecCongruence = CalculateRIASECHexagonalMatch(participantScores.RIASEC, fieldRIASEC);
            
            var roles = (JArray)field["roles"];
            foreach (var roleToken in roles)
            {
                var role = (JObject)roleToken;
                string roleName = role["name"].ToString();

                float fitScore = CalculateCompositeFitScore(
                    participantScores,
                    (JObject)role["domainWeights"],
                    (JObject)role["riasec"],
                    (JObject)role["bigfive"]
                );

                allRecommendations.Add(new CareerRecommendation
                {
                    FieldName = fieldName,
                    RoleName = roleName,
                    FitScore = fitScore,
                    RIASECCongruence = CalculateRIASECHexagonalMatch(participantScores.RIASEC, (JObject)role["riasec"]),
                    Description = role["description"]?.ToString() ?? "",
                    ExplanationTemplate = role["explanationTemplate"]?.ToString() ?? "",
                    ParticipantProfile = participantScores
                });
            }
        }

        var degreeQualified = allRecommendations
            .Where(r => r.FitScore >= DEGREE_RECOMMENDATION_THRESHOLD)
            .OrderByDescending(r => r.FitScore)
            .Take(5)
            .ToList();
        
        var nearMisses = allRecommendations
            .Where(r => r.FitScore >= ALTERNATIVE_PATHWAY_THRESHOLD && 
                    r.FitScore < DEGREE_RECOMMENDATION_THRESHOLD)
            .OrderByDescending(r => r.FitScore)
            .Take(5)
            .ToList();

        
        if (degreeQualified.Count >= 5)
        {
            result.Type = RecommendationType.Success;
            result.Recommendations = degreeQualified;
            result.Message = $"Great news! We found {degreeQualified.Count} excellent degree program matches for you. " +
                            $"These recommendations are based on your academic readiness ({academicReadiness:F1}), " +
                            $"interests, and personality profile.";
            
        }
        else if (degreeQualified.Count > 0)
        {
            result.Type = RecommendationType.PartialSuccess;
            result.Recommendations = degreeQualified;
            result.Message = $"We found {degreeQualified.Count} degree program(s) that match your profile well. " +
                            $"While we typically recommend 5 options, these programs show the strongest alignment " +
                            $"with your academic readiness ({academicReadiness:F1}), interests, and personality.";
            
            if (nearMisses.Count > 0)
            {
                result.Message += $"\n\nNote: We also identified {nearMisses.Count} alternative pathway option(s) " +
                                $"that you could pursue through diploma/certificate programs first.";
            }
            
        }
        else if (nearMisses.Count > 0)
        {
            result.Type = RecommendationType.NearMiss;
            result.Recommendations = nearMisses;
            result.Message = $"Based on your profile, we recommend starting with diploma or certificate programs " +
                            $"in the following fields, then upgrading to degree programs:\n\n" +
                            $"Your academic readiness ({academicReadiness:F1}) is good, but these programs are slightly " +
                            $"outside your strongest fit range. Building experience through shorter qualifications first " +
                            $"will increase your success rate and provide a pathway to bachelor's degrees.\n\n" +
                            $"Consider:\n" +
                            $"• Starting with a National Diploma (2-3 years)\n" +
                            $"• Completing relevant certificates\n" +
                            $"• Building practical experience\n" +
                            $"• Upgrading to a degree program after 1-2 years";
            
        }
        else
        {
            result.Type = RecommendationType.NoMatch;
            result.Message = $"We were unable to identify university degree programs that strongly match your profile. " +
                            $"This doesn't mean you can't succeed academically!\n\n" +
                            $"Your academic readiness ({academicReadiness:F1}) is sufficient for tertiary education. " +
                            $"We recommend:\n\n" +
                            $"• Exploring vocational and technical training programs\n" +
                            $"• Considering apprenticeships and workplace training\n" +
                            $"• Investigating certificate programs in areas you're passionate about\n" +
                            $"• Meeting with a career counselor to discuss alternative pathways\n\n" +
                            $"Many successful careers don't require traditional university degrees!";
            
        }

        return result;
    }

    private static float CalculateRIASECHexagonalMatch(
        Dictionary<string, float> participantRIASEC,
        JObject careerRIASEC)
    {
        var hexDistance = new Dictionary<(string, string), int>
        {
            {("R","R"), 0}, {("I","I"), 0}, {("A","A"), 0},
            {("S","S"), 0}, {("E","E"), 0}, {("C","C"), 0},
            
            {("R","I"), 1}, {("I","R"), 1},
            {("I","A"), 1}, {("A","I"), 1},
            {("A","S"), 1}, {("S","A"), 1},
            {("S","E"), 1}, {("E","S"), 1},
            {("E","C"), 1}, {("C","E"), 1},
            {("C","R"), 1}, {("R","C"), 1},
            
            {("R","A"), 2}, {("A","R"), 2},
            {("I","S"), 2}, {("S","I"), 2},
            {("A","E"), 2}, {("E","A"), 2},
            {("S","C"), 2}, {("C","S"), 2},
            {("E","R"), 2}, {("R","E"), 2},
            {("C","I"), 2}, {("I","C"), 2},
            
            {("R","S"), 3}, {("S","R"), 3},
            {("I","E"), 3}, {("E","I"), 3},
            {("A","C"), 3}, {("C","A"), 3}
        };
        
        float totalSimilarity = 0f;
        float totalWeight = 0f;
        
        foreach (var careerType in careerRIASEC)
        {
            string careerCode = careerType.Key;
            float careerWeight = careerType.Value.Value<float>();
            
            if (careerWeight <= 0) continue;
            
            float careerTypeSimilarity = 0f;
            
            foreach (var participantType in participantRIASEC)
            {
                string participantCode = participantType.Key;
                float participantScore = participantType.Value;
                
                int distance = hexDistance[(participantCode, careerCode)];
                float similarity = 100f - (distance * 25f);
                
                careerTypeSimilarity += similarity * (participantScore / 100f);
            }
            
            totalSimilarity += careerTypeSimilarity * careerWeight;
            totalWeight += careerWeight;
        }
        
        return totalWeight > 0 ? (totalSimilarity / totalWeight) : 50f;
    }

    private static bool MeetsDomainThresholds(
        DomainScores participantScores,
        JObject fieldDomainWeights)
    {
        var thresholds = new Dictionary<string, float>();

        foreach (var domain in fieldDomainWeights)
        {
            float weight = domain.Value.Value<float>();
            
            thresholds[domain.Key] = weight > 0.25f ? 40f : 30f;
        }

        if (thresholds.ContainsKey("I") && participantScores.Intellectual < thresholds["I"])
        {
            return false;
        }

        if (thresholds.ContainsKey("C") && participantScores.Cognitive < thresholds["C"])
        {
            return false;
        }

        if (thresholds.ContainsKey("P") && participantScores.Psychological < thresholds["P"])
        {
            return false;
        }

        if (thresholds.ContainsKey("B") && participantScores.Behavioral < thresholds["B"])
        {
            return false;
        }

        return true;
    }

    private static float CalculateCompositeFitScore(
        DomainScores participantScores,
        JObject roleDomainWeights,
        JObject roleRIASEC,
        JObject roleBigFive)
    {
        float domainComponent = 0f;
        float totalDomainWeight = 0f;

        foreach (var domain in roleDomainWeights)
        {
            float weight = domain.Value.Value<float>();
            float score = 0f;

            switch (domain.Key)
            {
                case "I": score = participantScores.Intellectual; break;
                case "C": score = participantScores.Cognitive; break;
                case "P": score = participantScores.Psychological; break;
                case "B": score = participantScores.Behavioral; break;
            }

            domainComponent += score * weight;
            totalDomainWeight += weight;
        }

        if (totalDomainWeight > 0)
        {
            domainComponent /= totalDomainWeight;
        }

        float riasecComponent = CalculateRIASECHexagonalMatch(participantScores.RIASEC, roleRIASEC);

        float bigFiveComponent = CalculateBigFiveEuclideanDistance(participantScores.BigFive, roleBigFive);

        float finalScore = (riasecComponent * 0.4f) +  
                        (domainComponent * 0.35f) +     
                        (bigFiveComponent * 0.25f); 

        return Mathf.Clamp(finalScore, 0f, 100f);
    }

    private static float CalculateBigFiveEuclideanDistance(
        Dictionary<string, float> participantBigFive,
        JObject careerBigFive)
    {
        float sumSquaredDiff = 0f;
        int traitCount = 0;
        
        foreach (var trait in new[] { "O", "C", "E", "A", "N" })
        {
            if (!participantBigFive.ContainsKey(trait)) continue;
            if (careerBigFive[trait] == null) continue;
            
            float participantScore = participantBigFive[trait];
            float careerWeight = careerBigFive[trait].Value<float>();
            
            float idealScore = (careerWeight + 1f) * 50f;
            
            float diff = participantScore - idealScore;
            sumSquaredDiff += diff * diff;
            traitCount++;
        }
        
        if (traitCount == 0) return 50f;
        
        float distance = Mathf.Sqrt(sumSquaredDiff / traitCount);
        float similarity = 100f * (1f - Mathf.Clamp01(distance / 70f));
        return similarity;
    }
    
    private static void LoadCareerFieldDBIfNeeded()
    {
        if (careerFieldDB != null) return;

        string[] possiblePaths = new string[]
        {
            Path.Combine(Application.persistentDataPath, "CareerFieldDB.json"),
            Path.Combine(Application.persistentDataPath, "ConfigFiles", "CareerFieldDB.json"),
            Path.Combine(Application.streamingAssetsPath, "CareerFieldDB.json"),
            Path.Combine(Application.streamingAssetsPath, "ConfigFiles", "CareerFieldDB.json")
        };

        foreach (var path in possiblePaths)
        {
            try
            {
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    careerFieldDB = JObject.Parse(json);
                    return;
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }

        return;
    }

    private static bool MeetsAcademicReadiness(DomainScores participantScores, out float academicReadiness)
    {
        float intellectualWeight = 0.35f;
        float cognitiveWeight = 0.30f;
        float totalWeight = intellectualWeight + cognitiveWeight;
        
        academicReadiness = (participantScores.Intellectual * intellectualWeight + 
                           participantScores.Cognitive * cognitiveWeight) / totalWeight;
        
        
        bool meetsThreshold = academicReadiness >= ACADEMIC_READINESS_THRESHOLD;        
        return meetsThreshold;
    }
}