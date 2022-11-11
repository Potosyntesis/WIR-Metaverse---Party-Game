using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject MainPanel;

    // Start is called before the first frame update
    void Start()
    {
        playButton = GetComponent<Button>();
        settingButton = GetComponent<Button>();
        quitButton = GetComponent<Button>();

        playButton.onClick.AddListener(() => Play());
        settingButton.onClick.AddListener(() => Settings());
        quitButton.onClick.AddListener(() => Quit());
    }

    private void Play()
    {
        SceneManager.LoadScene(1);
    }

    private void Settings()
    {

    }

    private void Quit()
    {
        Application.Quit();
        Debug.Log("Quitting application");
    }

    private void OnDestroy()
    {
        playButton.onClick.RemoveListener(() => Play());
        settingButton.onClick.RemoveListener(() => Settings());
        quitButton.onClick.RemoveListener(() => Quit());
    }
}
