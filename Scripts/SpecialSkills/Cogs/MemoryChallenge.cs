using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MemoryChallenge : MonoBehaviour
{
    [Header("Setup")]
    public Button[] buttons;             
    public Light sequenceLight;         
    public Slider progressBar;           

    [Header("Difficulty Settings")]
    public int sequenceLength = 4;       
    public float flashDuration = 0.5f;   
    public float delayBetweenFlashes = 0.2f;

    [Header("UI")]
    public TMPro.TextMeshProUGUI countdownText;
    private List<int> sequence = new List<int>();
    private int currentIndex = 0;

    void Start()
    {
        StartCoroutine(CountdownAndStart());
    }

    private IEnumerator CountdownAndStart()
    {
        SetButtonsInteractable(false);
        int countdown = 3; 
        while (countdown > 0)
        {
            if (countdownText != null)
                countdownText.text = countdown.ToString();

            yield return new WaitForSeconds(1f);
            countdown--;
        }

        if (countdownText != null)
            countdownText.text = "GO!";

        yield return new WaitForSeconds(1f);

        if (countdownText != null)
            countdownText.text = "";

        StartChallenge();
    }

    public void StartChallenge()
    {
        GenerateSequence();
        UpdateProgressBar();
    }

    private void GenerateSequence()
    {
        sequence.Clear();
        currentIndex = 0;

        for (int i = 0; i < sequenceLength; i++)
        {
            sequence.Add(Random.Range(0, buttons.Length));
        }

        StartCoroutine(PlaySequence());
        UpdateProgressBar();
    }

    private IEnumerator PlaySequence()
    {
        SetButtonsInteractable(false);

        foreach (int index in sequence)
        {
            buttons[index].image.color = Color.red;
            if (sequenceLight != null)
            {
                sequenceLight.intensity = Random.Range(3f, 8f);
                sequenceLight.color = new Color(Random.value, Random.value, Random.value);
            }
            yield return new WaitForSeconds(flashDuration);
            buttons[index].image.color = Color.white;
            if (sequenceLight != null)
                sequenceLight.intensity = 0f;

            yield return new WaitForSeconds(delayBetweenFlashes);
        }
        SetButtonsInteractable(true);
        TaskManager.Instance.StartTaskTimer();
    }

    public void PlayerSelection(int selectedIndex)
    {
        if (selectedIndex == sequence[currentIndex])
        {
            currentIndex++;
            UpdateProgressBar();

            if (currentIndex >= sequence.Count)
            {
                TaskManager.Instance.StartNextTask();
            }
        }
        else
        {
            TaskManager.Instance.AddPenalty(5f);
            TaskManager.Instance.StopTaskTimerWithoutSave();
            currentIndex = 0;
            GenerateSequence();
        }
    }

    private void UpdateProgressBar()
    {
        if (progressBar != null && sequence.Count > 0)
        {
            progressBar.value = (float)currentIndex / sequence.Count;
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        foreach (var button in buttons)
        {
            button.interactable = interactable;
            ColorBlock colors = button.colors;
            colors.disabledColor = colors.normalColor;
            button.colors = colors;
        }
    }
    public void ConfigureChallenge(int newLength, float newFlashDuration, float newDelay)
    {
        sequenceLength = newLength;
        flashDuration = newFlashDuration;
        delayBetweenFlashes = newDelay;
    }

    private void KeepButtonsVisible()
    {
        foreach (var button in buttons)
        {
            ColorBlock colors = button.colors;
            colors.disabledColor = colors.normalColor;
            button.colors = colors;
        }
    }
}
