using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Newtonsoft.Json;

public class StartingScene : MonoBehaviour
{
    public static StartingScene Instance;

    void Awake() => Instance = this;

    public void SelectBackground(string backgroundName)
    {
        string username = PlayerPrefs.GetString("LoggedInUser", "");
        UserAuth.Instance.SaveBackgroundChoice(username, backgroundName);
        PlayerPrefs.SetString($"{username}_SelectedBackground", backgroundName);
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
