using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class ProgressManager : MonoBehaviour {
    public static float GetProgressForTestArea(string testArea) {
        if (PlayerPrefs.HasKey(testArea + "_Progress"))
            return PlayerPrefs.GetFloat(testArea + "_Progress");
        return 0;
    }

    public static void SetProgress(string testArea, float value) {
        PlayerPrefs.SetFloat(testArea + "_Progress", value);
    }
}
