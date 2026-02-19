using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;

public class CustomGrabMath : MonoBehaviour
{
    public static CustomGrabMath Instance;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private bool hasMoved = false;
    private RiddleZone riddleZone;

    void Awake()
    {
        Instance = this;
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable == null)
        {
            return;
        }
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }


    private void OnGrabbed(SelectEnterEventArgs args)
    {
        hasMoved = false; 
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        if (hasMoved)
        {
            OnObjectMoved();
        }
    }

    private Vector3 lastPosition;

    void Update()
    {
        if (grabInteractable.isSelected)
        {
            if (!hasMoved && Vector3.Distance(transform.position, lastPosition) > 0.001f)
            {
                hasMoved = true;
            }
        }

        lastPosition = transform.position;
    }

    public void OnObjectMoved()
    {        
        riddleZone = GetComponent<RiddleZone>();
        if (riddleZone != null)
        {
            string sceneName = "";
            SceneLogicManager.Instance.OnRiddleZoneReached(riddleZone.questionId);
            
            switch (riddleZone.questionId)
            {
                case 1:
                    sceneName = "MathQuiz";
                    break;
                default:
                    gameObject.SetActive(false);
                    break;
            }
            SceneManager.LoadScene(sceneName);            
        }
    }
}
