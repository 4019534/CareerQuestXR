using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundManager : MonoBehaviour
{
    public GameObject office;
    public GameObject sky;
    public GameObject city;
    public GameObject country;
    public GameObject sea;

    void Start()
    {
        string currentUsername = PlayerPrefs.GetString("LoggedInUser", "");
        string selectedBackground = PlayerPrefs.GetString($"{currentUsername}_SelectedBackground", "office");

        office.SetActive(false);
        sky.SetActive(false);
        city.SetActive(false);
        country.SetActive(false);
        sea.SetActive(false);

        switch (selectedBackground.ToLower())
        {
            case "office":
                office.SetActive(true);
                break;
            case "sky":
                sky.SetActive(true);
                break;
            case "city":
                city.SetActive(true);
                break;
            case "country":
                country.SetActive(true);
                break;
            case "sea":
                sea.SetActive(true);
                break;
        }
    }
}
