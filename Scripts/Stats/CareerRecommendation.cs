using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using System.IO;

using System;
using System.Linq;
using Newtonsoft.Json.Linq;


[System.Serializable]
public class CareerRecommendation
{
    public string FieldName; 
    public string RoleName; 
    public float FitScore; 
    public float RIASECCongruence; 
    public string Description;
    public string ExplanationTemplate;
    public DomainScores ParticipantProfile;

    public string GetPersonalizedExplanation()
    {
        if (ParticipantProfile == null) return ExplanationTemplate;

        string explanation = ExplanationTemplate;

        explanation = explanation.Replace("{dominantRIASEC}", ParticipantProfile.DominantRIASECCode);

        var topTraits = ParticipantProfile.BigFive
            .OrderByDescending(x => x.Value)
            .Take(2)
            .Select(x => GetTraitName(x.Key));
        explanation = explanation.Replace("{topBigFiveTraits}", string.Join(" and ", topTraits));

        return explanation;
    }

    private string GetTraitName(string traitLetter)
    {
        switch (traitLetter)
        {
            case "O": return "high Openness";
            case "C": return "high Conscientiousness";
            case "E": return "high Extraversion";
            case "A": return "high Agreeableness";
            case "N": return "low Neuroticism (emotional stability)";
            default: return traitLetter;
        }
    }
}