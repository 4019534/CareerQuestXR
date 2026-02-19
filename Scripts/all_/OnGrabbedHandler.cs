using UnityEngine;

public class OnGrabbedHandler : MonoBehaviour
{
    public RiddleZone riddleZone;

    private void Awake()
    {
        if (riddleZone == null)
        {
            riddleZone = GetComponent<RiddleZone>();
        }
    }

    public void HandleGrab()
    {
        if (riddleZone != null)
        {
            VerbalTaskManager.Instance.OnRiddleZoneReached(riddleZone.questionId);
            gameObject.SetActive(false);
        }
        else
        {
            return;
        }
    }
}
