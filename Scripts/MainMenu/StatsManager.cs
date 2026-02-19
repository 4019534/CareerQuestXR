using UnityEngine;
using TMPro; // TextMeshPro namespace
using System.Collections;
using DentedPixel;

public class StatsManager : MonoBehaviour
{
    public TextMeshProUGUI statsText; 
    private float timePlayed;   
    private int tasksCompleted; 

    void Start()
    {
        timePlayed = 0f;
        tasksCompleted = 0; 
        UpdateStatsUI();
    }

    void Update()
    {
        timePlayed += Time.deltaTime;
        UpdateStatsUI();
    }

    public void IncrementTasks()
    {
        tasksCompleted++;
        Vector3 originalScale = statsText.rectTransform.localScale;
        Vector3 targetScale = originalScale * 1.2f;

        LeanTween.scale(statsText.gameObject, targetScale, 0.2f)
        .setEasePunch()
        .setOnComplete(() => statsText.rectTransform.localScale = originalScale); 
    }

    private void UpdateStatsUI()
    {
        statsText.text = $"Time Played: {FormatTime(timePlayed)}\nTasks Completed: {tasksCompleted}";
        
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}

