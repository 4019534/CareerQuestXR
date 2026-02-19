using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Linq;

public static class ScoreCalculator
{
    private static JObject weightsConfig = null;
    private static JObject bigFiveConfig = null;
    
    private static readonly Dictionary<string, string> TraitNameToCode = new Dictionary<string, string>
    {
        { "Openness", "O" },
        { "Conscientiousness", "C" },
        { "Extraversion", "E" },
        { "Agreeableness", "A" },
        { "Neuroticism", "N" }
    };
    
    private static readonly string[] possibleConfigPaths = new string[]
    {
        Path.Combine(Application.persistentDataPath, "WeightsConfig.json"),
        Path.Combine(Application.persistentDataPath, "ConfigFiles", "WeightsConfig.json"),
        Path.Combine(Application.persistentDataPath, "ConfigFiles", "BigFiveConfig.json"),
        Path.Combine(Application.persistentDataPath, "WeightsConfig.json")
    };

    public static DomainScores ComputeDomainScores(Dictionary<string, Dictionary<string, object>> userResults)
    {

        if (userResults == null)
        {
            return new DomainScores();
        }



        LoadWeightsConfigIfNeeded();
        LoadBigFiveConfigIfNeeded();



        

        var domainTotals = new Dictionary<string, float>
        {
            { "intellectual", 0f },
            { "cognitive", 0f },
            { "psychological", 0f },
            { "behavioral", 0f }
        };
        var domainWeights = new Dictionary<string, float>
        {
            { "intellectual", 0f },
            { "cognitive", 0f },
            { "psychological", 0f },
            { "behavioral", 0f }
        };

        var bigFiveTIPITotals = new Dictionary<string, List<float>>
        {
            { "O", new List<float>() },
            { "C", new List<float>() },
            { "E", new List<float>() },
            { "A", new List<float>() },
            { "N", new List<float>() }
        };

        var bigFiveBehavioralTotals = new Dictionary<string, float>
        {
            { "O", 0f }, { "C", 0f }, { "E", 0f }, { "A", 0f }, { "N", 0f }
        };
        var bigFiveBehavioralWeights = new Dictionary<string, float>
        {
            { "O", 0f }, { "C", 0f }, { "E", 0f }, { "A", 0f }, { "N", 0f }
        };

        var riasecTotals = new Dictionary<string, float>
        {
            { "R", 0f }, { "I", 0f }, { "A", 0f },
            { "S", 0f }, { "E", 0f }, { "C", 0f }
        };
        var riasecCounts = new Dictionary<string, int>
        {
            { "R", 0 }, { "I", 0 }, { "A", 0 },
            { "S", 0 }, { "E", 0 }, { "C", 0 }
        };

        DomainScores scores = new DomainScores
        {
            Intellectual = 0f,
            Cognitive = 0f,
            Psychological = 0f,
            Behavioral = 0f,
            RIASEC = new Dictionary<string, float>(riasecTotals.Keys.ToDictionary(k => k, k => 0f)),
            BigFive = new Dictionary<string, float>(bigFiveBehavioralTotals.Keys.ToDictionary(k => k, k => 0f))
        };

        if (weightsConfig == null)
        {
            return scores;
        }
        
        foreach (var testEntry in userResults)
        {
            string testName = testEntry.Key;
            var testData = testEntry.Value;


            JToken testConfigToken = weightsConfig[testName];
            if (testConfigToken == null)
            {
                continue;
            }
            
            if (testData.ContainsKey("results") && testConfigToken["metrics"] != null)
            {
                var metricsDict = testData["results"] as Dictionary<string, object>;

                foreach (var metricProp in (JObject)testConfigToken["metrics"])
                {
                    string metricKey = metricProp.Key;
                    JToken domainMap = metricProp.Value;

                    float rawVal = TryGetMetricRawValue(metricsDict, metricKey, 0f);
                    float normalized = NormalizeMetricForScoring(metricKey, rawVal, domainMap as JObject);


                    foreach (var dom in (JObject)domainMap)
                    {
                        string domainName = dom.Key;
                        if (domainName == "normMin" || domainName == "normMax" || domainName == "bigfive" || domainName == "note") 
                            continue;

                        float weight = ToFloat(dom.Value);
                        float contribution = normalized * weight;
                        
                        AccumulateDomain(domainTotals, domainWeights, domainName, contribution, weight);
                    }

                    if (domainMap["bigfive"] != null)
                    {
                        foreach (var bf in (JObject)domainMap["bigfive"])
                        {
                            string traitName = bf.Key;
                            float weight = ToFloat(bf.Value);
                            
                            if (TraitNameToCode.TryGetValue(traitName, out string traitCode))
                            {
                                float contribution = normalized * weight;
                                AccumulateBigFiveBehavioral(bigFiveBehavioralTotals, bigFiveBehavioralWeights, traitCode, contribution, weight);
                            }
                        }
                    }
                }
            }

            
            if (testData.ContainsKey("survey") && testConfigToken["survey"] != null)
            {
                var surveyDict = testData["survey"] as Dictionary<string, string>;
                if (surveyDict != null)
                {
                    foreach (var surveyPair in (JObject)testConfigToken["survey"])
                    {
                        string surveyKey = surveyPair.Key;
                        JObject surveyFieldConfig = (JObject)surveyPair.Value;

                        if (!TryGetStringFromDict(surveyDict, surveyKey, out string answerStr))
                        {
                            continue;
                        }

                        float numeric = MapSurveyAnswer(surveyFieldConfig, answerStr);

                        if (surveyFieldConfig["domains"] != null)
                        {
                            foreach (var dom in (JObject)surveyFieldConfig["domains"])
                            {
                                string domainName = dom.Key;
                                float weight = ToFloat(dom.Value);
                                float contribution = numeric * weight;
                                
                                AccumulateDomain(domainTotals, domainWeights, domainName, contribution, weight);
                            }
                        }

                        if (surveyFieldConfig["bigfive"] != null)
                        {
                            foreach (var bf in (JObject)surveyFieldConfig["bigfive"])
                            {
                                string traitName = bf.Key;
                                float weight = ToFloat(bf.Value);
                                
                                if (TraitNameToCode.TryGetValue(traitName, out string traitCode))
                                {
                                    float contribution = numeric * weight;
                                    AccumulateBigFiveBehavioral(bigFiveBehavioralTotals, bigFiveBehavioralWeights, traitCode, contribution, weight);
                                }
                            }
                        }
                    }
                }
            }
            
            foreach (var child in (JObject)testConfigToken)
            {
                string childName = child.Key;
                if (childName == "metrics" || childName == "survey" || childName == "note") 
                    continue;

                JObject subtaskConfig = (JObject)child.Value;
                if (!testData.ContainsKey(childName)) continue;

                var subtaskObj = testData[childName];
                var subtaskDict = subtaskObj as Dictionary<string, string>;

                if (subtaskDict == null && subtaskObj is Dictionary<string, object> tempObjDict)
                {
                    subtaskDict = tempObjDict.ToDictionary(k => k.Key, k => k.Value?.ToString() ?? "");
                }

                if (subtaskDict == null) continue;
                
                if (childName == "TIPIInventory")
                {
                    ProcessTIPIInventory(subtaskDict, subtaskConfig, bigFiveTIPITotals);
                    continue;
                }
                
                if (childName == "PersonalityProfiler")
                {
                    ProcessPersonalityProfiler(subtaskDict, subtaskConfig, riasecTotals, riasecCounts);
                    continue;
                }
                
                foreach (var q in subtaskConfig)
                {
                    string questionKey = q.Key;
                    if (questionKey == "note") continue;
                    
                    JObject qConfig = (JObject)q.Value;

                    if (!TryGetStringFromDict(subtaskDict, questionKey, out string ansStr))
                    {
                        continue;
                    }

                    float numeric = MapSurveyAnswer(qConfig, ansStr);

                    if (qConfig["domains"] != null)
                    {
                        foreach (var dom in (JObject)qConfig["domains"])
                        {
                            string domainName = dom.Key;
                            float weight = ToFloat(dom.Value);
                            float contribution = numeric * weight;
                            
                            AccumulateDomain(domainTotals, domainWeights, domainName, contribution, weight);
                        }
                    }

                    if (qConfig["bigfive"] != null)
                    {
                        foreach (var bf in (JObject)qConfig["bigfive"])
                        {
                            string traitName = bf.Key;
                            float weight = ToFloat(bf.Value);
                            
                            if (TraitNameToCode.TryGetValue(traitName, out string traitCode))
                            {
                                float contribution = numeric * weight;
                                AccumulateBigFiveBehavioral(bigFiveBehavioralTotals, bigFiveBehavioralWeights, traitCode, contribution, weight);
                            }
                        }
                    }
                }
            }
        }

        Debug.Log("----- DOMAIN TOTALS BEFORE NORMALIZE -----");
        Debug.Log($"Intellectual -> Total: {domainTotals["intellectual"]}, Weight: {domainWeights["intellectual"]}");
        Debug.Log($"Cognitive -> Total: {domainTotals["cognitive"]}, Weight: {domainWeights["cognitive"]}");
        Debug.Log($"Psychological -> Total: {domainTotals["psychological"]}, Weight: {domainWeights["psychological"]}");
        Debug.Log($"Behavioral -> Total: {domainTotals["behavioral"]}, Weight: {domainWeights["behavioral"]}");


        scores.Intellectual = NormalizeDomain(domainTotals["intellectual"], domainWeights["intellectual"]);
        scores.Cognitive = NormalizeDomain(domainTotals["cognitive"], domainWeights["cognitive"]);
        scores.Psychological = NormalizeDomain(domainTotals["psychological"], domainWeights["psychological"]);
        scores.Behavioral = NormalizeDomain(domainTotals["behavioral"], domainWeights["behavioral"]);

        foreach (var letter in riasecTotals.Keys.ToList())
        {
            scores.RIASEC[letter] = NormalizeRIASEC(riasecTotals[letter], riasecCounts[letter]);
        }
        scores.DominantRIASECCode = GetDominantRIASECCode(scores.RIASEC);

        var bigFiveTIPI = CalculateBigFiveFromTIPI(bigFiveTIPITotals);

        var bigFiveBehavioral = new Dictionary<string, float>();
        foreach (var trait in bigFiveBehavioralTotals.Keys)
        {
            bigFiveBehavioral[trait] = NormalizeBigFiveBehavioral(bigFiveBehavioralTotals[trait], bigFiveBehavioralWeights[trait]);
        }

        scores.BigFive = CombineBigFiveScores(bigFiveTIPI, bigFiveBehavioral);

        float totalWeight = domainWeights["intellectual"] + 
                        domainWeights["cognitive"] + 
                        domainWeights["psychological"] + 
                        domainWeights["behavioral"];

        float intellectualPct = (domainWeights["intellectual"] / totalWeight) * 100f;
        float cognitivePct = (domainWeights["cognitive"] / totalWeight) * 100f;
        float psychologicalPct = (domainWeights["psychological"] / totalWeight) * 100f;
        float behavioralPct = (domainWeights["behavioral"] / totalWeight) * 100f;

        Debug.Log("----- FINAL DOMAIN SCORES -----");
        Debug.Log($"Intellectual: {scores.Intellectual}");
        Debug.Log($"Cognitive: {scores.Cognitive}");
        Debug.Log($"Psychological: {scores.Psychological}");
        Debug.Log($"Behavioral: {scores.Behavioral}");
        Debug.Log("========== ComputeDomainScores END ==========");


        return scores;
    }
    
    private static void ProcessPersonalityProfiler(
        Dictionary<string, string> subtaskDict,
        JObject subtaskConfig,
        Dictionary<string, float> riasecTotals,
        Dictionary<string, int> riasecCounts)
    {

        foreach (var q in subtaskConfig)
        {
            string questionKey = q.Key;
            if (questionKey == "note") continue;
            
            JObject qConfig = (JObject)q.Value;

            if (!TryGetStringFromDict(subtaskDict, questionKey, out string answerStr))
            {
                continue;
            }


            if (qConfig["RIASEC"] == null)
            {
                continue;
            }

            var riasecMapping = (JObject)qConfig["RIASEC"];

            if (riasecMapping[answerStr] != null)
            {
                string mappedType = riasecMapping[answerStr].ToString();
                
                float score = 100f;
                if (qConfig["mapping"] != null)
                {
                    var mapping = (JObject)qConfig["mapping"];
                    if (mapping[answerStr] != null)
                    {
                        score = ToFloat(mapping[answerStr]);
                    }
                }

                riasecTotals[mappedType] += score;
                riasecCounts[mappedType]++;

            }
        }
    }
    
    private static void ProcessTIPIInventory(
        Dictionary<string, string> subtaskDict,
        JObject subtaskConfig,
        Dictionary<string, List<float>> bigFiveTIPITotals)
    {
        foreach (var q in subtaskConfig)
        {
            string questionKey = q.Key;
            if (questionKey == "note") continue;
            
            JObject qConfig = (JObject)q.Value;

            if (!TryGetStringFromDict(subtaskDict, questionKey, out string answerStr))
            {
                continue;
            }

            if (!float.TryParse(answerStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float response))
            {
                continue;
            }

            if (qConfig["BigFive"] != null)
            {
                foreach (var bf in (JObject)qConfig["BigFive"])
                {
                    string traitCode = bf.Key; 
                    float weight = ToFloat(bf.Value); 

                    float scoredResponse = weight > 0 ? response : (8f - response);

                    bigFiveTIPITotals[traitCode].Add(scoredResponse);
                }
            }
        }
    }
    
    private static Dictionary<string, float> CalculateBigFiveFromTIPI(Dictionary<string, List<float>> tipiTotals)
    {
        var scores = new Dictionary<string, float>();

        foreach (var trait in new[] { "O", "C", "E", "A", "N" })
        {
            if (tipiTotals[trait].Count > 0)
            {
                float average = tipiTotals[trait].Average(); 

                float normalized = ((average - 1f) / 6f) * 100f;
                scores[trait] = Mathf.Clamp(normalized, 0f, 100f);

            }
            else
            {
                scores[trait] = 0f; 
            }
        }

        return scores;
    }

    private static Dictionary<string, float> CombineBigFiveScores(
        Dictionary<string, float> tipiScores,
        Dictionary<string, float> behavioralScores)
    {
        var combined = new Dictionary<string, float>();
        float tipiWeight = 0.6f;
        float behavioralWeight = 0.4f; 

        foreach (var trait in new[] { "O", "C", "E", "A", "N" })
        {
            float tipi = tipiScores.ContainsKey(trait) ? tipiScores[trait] : 00f;
            float behavioral = behavioralScores.ContainsKey(trait) ? behavioralScores[trait] : 00f;

            combined[trait] = (tipi * tipiWeight) + (behavioral * behavioralWeight);
            combined[trait] = Mathf.Clamp(combined[trait], 0f, 100f);

        }

        return combined;
    }
    
    private static void AccumulateDomain(
        Dictionary<string, float> totals,
        Dictionary<string, float> weights,
        string domainName,
        float contribution,
        float weight)
    {
        string key = domainName.ToLowerInvariant();
        if (!totals.ContainsKey(key)) return;
        
        totals[key] += contribution;
        weights[key] += Mathf.Abs(weight);
    }

    private static void AccumulateBigFiveBehavioral(
        Dictionary<string, float> totals,
        Dictionary<string, float> weights,
        string traitCode,
        float contribution,
        float weight)
    {
        if (!totals.ContainsKey(traitCode)) return;
        
        float weightedContribution = contribution * weight;
        totals[traitCode] += weightedContribution;
        weights[traitCode] += Mathf.Abs(weight); 
    }

    
    private static float NormalizeDomain(float total, float weightSum)
    {
        if (weightSum <= 0f) return 0f;
        return Mathf.Clamp01(total / (weightSum * 100f)) * 100f;
    }

    private static float NormalizeRIASEC(float total, int count)
    {
        const int TOTAL_RIASEC_QUESTIONS = 9; 
                
        if (count <= 0)
        {
            return 0f;
        }

        float frequencyScore = ((float)count / TOTAL_RIASEC_QUESTIONS) * 100f;
                
        return frequencyScore;
    }

    private static float NormalizeBigFiveBehavioral(float total, float weightSum)
    {
        if (weightSum <= 0f) return 0f;
        
        float avgContribution = total / weightSum; 
        float normalized = 0f + (avgContribution / 2f);
        
        return Mathf.Clamp(normalized, 0f, 100f);
    }

    private static string GetDominantRIASECCode(Dictionary<string, float> riasecScores)
    {
        var sorted = riasecScores.OrderByDescending(x => x.Value).Take(3).Select(x => x.Key);
        return string.Join("", sorted);
    }
    
    private static void LoadWeightsConfigIfNeeded()
    {
        if (weightsConfig != null) return;

        foreach (var path in possibleConfigPaths)
        {
            try
            {
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    weightsConfig = JObject.Parse(json);
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

    private static void LoadBigFiveConfigIfNeeded()
    {
        if (bigFiveConfig != null) return;

        foreach (var path in possibleConfigPaths)
        {
            try
            {
                string bfPath = Path.Combine(Path.GetDirectoryName(path), "BigFiveConfig.json");
                if (File.Exists(bfPath))
                {
                    string json = File.ReadAllText(bfPath);
                    bigFiveConfig = JObject.Parse(json);
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

    private static float MapSurveyAnswer(JObject fieldConfig, object answerObj)
    {
        float fallback = 0f;
        if (fieldConfig == null) return fallback;

        string answerStr = answerObj?.ToString() ?? "";

        if (fieldConfig["mapping"] != null)
        {
            var mapping = (JObject)fieldConfig["mapping"];

            if (mapping[answerStr] != null)
            {
                return ToFloat(mapping[answerStr]);
            }

            foreach (var kv in mapping)
            {
                if (string.Equals(kv.Key, answerStr, StringComparison.OrdinalIgnoreCase))
                {
                    return ToFloat(kv.Value);
                }
            }
            return fallback;
        }

        if (fieldConfig["scale"] != null)
        {
            int min = fieldConfig["scale"]["min"]?.Value<int>() ?? 1;
            int max = fieldConfig["scale"]["max"]?.Value<int>() ?? 10;
            bool reverse = fieldConfig["reverse"]?.Value<bool>() ?? false;

            if (float.TryParse(answerStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed))
            {
                float value = parsed;
                
                if (reverse)
                {
                    value = (max + min) - parsed;
                    
                }                
                float normalized = Mathf.Clamp01((value - min) / (float)(max - min)) * 100f;
                return normalized;
            }
        }

        if (float.TryParse(answerStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
        {
            return NormalizeSurveyScaleValue(val);
        }

        return fallback;
    }

    private static float NormalizeSurveyScaleValue(float raw)
    {
        if (raw >= 0f && raw <= 1f) return raw * 100f;
        if (raw >= 1f && raw <= 10f) return (raw - 1f) / 9f * 100f;
        if (raw >= 0f && raw <= 100f) return raw;
        return Mathf.Clamp(raw, 0f, 100f);
    }
    
    private static float NormalizeMetricForScoring(string metricName, float raw, JObject domainMap = null)
    {
        if (float.IsNaN(raw) || float.IsInfinity(raw)) return 0f;

        if (domainMap != null)
        {
            float? min = domainMap["normMin"]?.Value<float>();
            float? max = domainMap["normMax"]?.Value<float>();
            if (min.HasValue && max.HasValue && max > min)
            {
                bool invertScore = metricName.ToLowerInvariant().Contains("time") ||
                                metricName.ToLowerInvariant().Contains("penalty") ||
                                metricName.ToLowerInvariant().Contains("attempt");
                
                if (invertScore)
                {
                    float norm = 1f - ((raw - min.Value) / (max.Value - min.Value));
                    return Mathf.Clamp01(norm) * 100f;
                }
                else
                {
                    float norm = (raw - min.Value) / (max.Value - min.Value);
                    return Mathf.Clamp01(norm) * 100f;
                }
            }
        }

        string key = metricName.ToLowerInvariant();

        if (key.Contains("percent")) return Mathf.Clamp(raw, 0f, 100f);

        if (key.Contains("score")) return Mathf.Clamp(raw, 0f, 100f);

        if (key.Contains("reaction"))
        {
            float maxMs = 1000f;
            return (1f - Mathf.Clamp01(raw / maxMs)) * 100f;
        }

        if (key.Contains("time"))
        {
            float maxSeconds = 600f;
            float normalized = (1f - Mathf.Clamp01(raw / maxSeconds)) * 100f;
            return normalized;
        }

        if (key.Contains("penalty"))
        {
            float maxPenalty = 300f;
            return (1f - Mathf.Clamp01(raw / maxPenalty)) * 100f;
        }

        if (key.Contains("attempt") || key.Contains("skip"))
        {
            float max = 10f;
            return (1f - Mathf.Clamp01(raw / max)) * 100f;
        }

        if (raw >= 0f && raw <= 1f) return raw * 100f;
        if (raw >= 0f && raw <= 100f) return raw;
        return Mathf.Clamp(raw, 0f, 100f);
    }

    private static bool TryGetStringFromDict(IDictionary<string, string> dict, string plainKey, out string value)
    {
        value = null;
        if (dict == null) return false;

        if (dict.TryGetValue(plainKey, out value)) return true;

        string suffix = "::" + plainKey;
        foreach (var kv in dict)
        {
            if (kv.Key.EndsWith(suffix, StringComparison.Ordinal))
            {
                value = kv.Value;
                return true;
            }
        }

        return false;
    }

    private static float TryGetMetricRawValue(Dictionary<string, object> metricsDict, string metricKey, float fallback = 0f)
    {
        if (metricsDict == null) return fallback;

        if (metricsDict.TryGetValue(metricKey, out object val))
        {
            return ConvertToFloat(val, fallback);
        }

        string suffix = "::" + metricKey;
        foreach (var kv in metricsDict)
        {
            if (kv.Key.EndsWith(suffix, StringComparison.Ordinal))
            {
                return ConvertToFloat(kv.Value, fallback);
            }
        }

        return fallback;
    }

    private static float ConvertToFloat(object val, float fallback = 0f)
    {
        if (val == null) return fallback;
        if (val is float f) return f;
        if (val is double d) return (float)d;
        if (val is int i) return i;
        if (float.TryParse(val.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed))
            return parsed;
        return fallback;
    }

    private static float ToFloat(JToken token)
    {
        if (token == null) return 0f;
        if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
            return token.Value<float>();
        if (float.TryParse(token.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
            return v;
        return 0f;
    }
}
