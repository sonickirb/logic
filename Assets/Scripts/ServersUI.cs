using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class ServersUI : MonoBehaviour
{
    
    [Header("References")]
    public GameObject ui;
    public TMP_InputField addressField;
    public TMP_InputField portField;
    public Button joinButton;

    void Start()
    {
        joinButton.onClick.AddListener(() =>
        {
            ui.SetActive(false);
            UnityTransport transport = NetworkManager.Singleton.transform.GetComponent<UnityTransport>();
            transport.ConnectionData.Address = addressField.text;
            transport.ConnectionData.Port = ushort.Parse(portField.text);

            NetworkManager.Singleton.StartClient();
        });
    }
}