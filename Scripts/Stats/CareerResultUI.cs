using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CareerResultUI : MonoBehaviour
{
    public TMP_Text fieldText;
    public TMP_Text roleText;
    public TMP_Text scoreText;
    public TMP_Text explanationText;
    public Button button;
    public Image background;

    private bool expanded = false;
    private string fullExplanation;
    private string shortExplanation;

    public void Setup(CareerRecommendation rec, bool isEvenRow)
    {
        fieldText.text = rec.FieldName;
        roleText.text = rec.RoleName;

        scoreText.text = $"{rec.FitScore}/100 (RIASEC {rec.RIASECCongruence}/100)";

        fullExplanation = rec.GetPersonalizedExplanation();

        shortExplanation = fullExplanation.Length > 80
            ? fullExplanation.Substring(0, 80) + "..."
            : fullExplanation;

        explanationText.text = shortExplanation;

        button.onClick.AddListener(ToggleExpand);
    }

    private void ToggleExpand()
    {
        expanded = !expanded;
        explanationText.text = expanded ? fullExplanation : shortExplanation;
    }
}
