using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using TMPro;

public class SpawnPointManager : MonoBehaviour
{
    public Transform xrRig; 
    public List<Transform> spawnPoints = new List<Transform>();
    private int currentIndex = 0;

    void Start()
    {
        if (spawnPoints.Count == 0 || xrRig == null)
        {
            return;
        }

        MoveToSpawn(currentIndex);
    }

    public void OnRoomCompleted()
    {
        int questionIndex = VerbalTaskManager.Instance.currentQuestionIndex;

        if (currentIndex < spawnPoints.Count && 
            (
                questionIndex == 4 ||
                questionIndex == 7 ||
                questionIndex == 9 ||
                questionIndex == 10 ||
                questionIndex == 11
            ))
            
        {
            currentIndex++; 
            MoveToSpawn(currentIndex);
        }
      
    }

    private void MoveToSpawn(int index)
    {
        Vector3 targetPos = spawnPoints[index].position;
        Quaternion targetRot = spawnPoints[index].rotation;

        xrRig.position = targetPos;
        xrRig.rotation = targetRot;

    }
}
