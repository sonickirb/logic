using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class WorldMenuUI : MonoBehaviour
{

    [Header("References")]
    public GameObject ui;
    public GameObject worldInfo;
    public TMP_Text worldInfo_Name;
    public RawImage worldInfo_Icon;
    public Button singleplayer;
    public Button hostServer;
    public Transform content;
    public GameObject content_NoWorlds;
    public GameObject worldPrefab;

    [Header("Runtime")]
    public List<string> worldPaths;
    public WorldMenuButton button;

    // Start is called before the first frame update
    void Start()
    {
        worldPaths = SaveSystem.GetAllWorlds();
        int i = 0;
        foreach (string path in worldPaths)
        {
            i++;
            FileInfo info = new FileInfo(path);
            string wrldName = info.Name.Split(".")[0];
            
            WorldMenuButton world = Instantiate(worldPrefab).GetComponent<WorldMenuButton>();
            world.wrldName = wrldName;
            world.filePath = path;
            world.tmp.text = wrldName;
            world.GetComponent<Button>().onClick.AddListener(() =>
            {
                button = world;
                SelectWorld();
            });
            world.transform.name = i.ToString();
            world.transform.parent = content;
        }
        if (i == 0)
            content_NoWorlds.SetActive(true);

        singleplayer.onClick.AddListener(Singleplayer);
        hostServer.onClick.AddListener(HostServer);
    }

    public void SelectWorld()
    {
        worldInfo_Name.text = button.tmp.text;
        worldInfo_Icon.texture = button.icon.texture;
        worldInfo.SetActive(true);
    }

    public void Singleplayer()
    {
        ui.SetActive(false);
        LogicManager.Instance.wrldName = button.wrldName;
        NetworkManager.Singleton.StartHost();
    }

    public void HostServer()
    {
        ui.SetActive(false);
        LogicManager.Instance.wrldName = button.wrldName;
        NetworkManager.Singleton.StartServer();
    }
}
