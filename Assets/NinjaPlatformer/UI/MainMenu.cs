using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    // Use this for initialization
    [SerializeField]
    private GameObject MainMenuPanel;

    private void ShowMainMenu()
    {
        MainMenuPanel.SetActive(true);
    }
    void Start()
    {
        ShowMainMenu();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnStartBtnDown()
    {
        //Application.LoadLevel(1);也行，但是说是过时了
        Application.LoadLevel(1);
    }
    //
    public void OnEndBtnDown()
    {
        Application.Quit();
    }
    //
    public void OnSettingBtnDown()
    {
    }
}

