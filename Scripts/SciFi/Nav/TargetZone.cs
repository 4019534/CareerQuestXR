using UnityEngine;

public class TargetZone : MonoBehaviour
{
    public MainSceneLogic logic;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GridMovementManager.Instance.LoadNextLevel();
            Destroy(gameObject); 
        }
    }
}
