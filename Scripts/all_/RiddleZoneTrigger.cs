using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class RiddleZoneTrigger : MonoBehaviour
{
    public static RiddleZoneTrigger Instance;
    public ParticleSystem ps;
    public int questionIndex;
    public Dictionary<int, GameObject> cubeMap = new Dictionary<int, GameObject>();
    public RiddleZone[] cubes;
    void Awake() => Instance = this;

    void Start()
    {
        foreach (var cube in cubes)
        {
            if (!cubeMap.ContainsKey(cube.questionId))
                cubeMap.Add(cube.questionId, cube.gameObject);
        }
    }
}
