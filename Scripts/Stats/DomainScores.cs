using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using System.IO;

[System.Serializable]
public class DomainScores
{
    public float Intellectual;
    public float Cognitive;
    public float Psychological;
    public float Behavioral;
    public Dictionary<string, float> RIASEC; 
    public Dictionary<string, float> BigFive; 
    public string DominantRIASECCode; 
}