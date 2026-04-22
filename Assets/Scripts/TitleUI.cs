using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleUI : MonoBehaviour
{

    [Header("References")]
    public GameObject worldsMenu;
    public Button worldsButton;
    public Button exitWorldsButton;
    public GameObject serversMenu;
    public Button serversButton;
    public Button exitServersButton;
    public Button quitButton;

    // Start is called before the first frame update
    void Start()
    {
        worldsButton.onClick.AddListener(() =>
        {
            worldsMenu.SetActive(true);
        });
        exitWorldsButton.onClick.AddListener(() =>
        {
            worldsMenu.SetActive(false);
        });
        serversButton.onClick.AddListener(() =>
        {
            serversMenu.SetActive(true);
        });
        exitServersButton.onClick.AddListener(() =>
        {
            serversMenu.SetActive(false);
        });
        quitButton.onClick.AddListener(Application.Quit);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
