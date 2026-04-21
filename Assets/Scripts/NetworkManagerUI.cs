using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    
    [SerializeField]Button serverBtn;
    [SerializeField]Button hostBtn;
    [SerializeField]Button clientBtn;

    void Awake()
    {
        serverBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });
        hostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });
        clientBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });
        NetworkManager.Singleton.OnConnectionEvent += OnConnected;
    }

    public void OnAddressChanged(string address)
    {
        UnityTransport transport = NetworkManager.Singleton.transform.GetComponent<UnityTransport>();

        transport.ConnectionData.Address = address;
    }
    public void OnPortChanged(string port)
    {
        UnityTransport transport = NetworkManager.Singleton.transform.GetComponent<UnityTransport>();

        transport.ConnectionData.Port = ushort.Parse(port);
    }

    public void OnConnected(NetworkManager net, ConnectionEventData eventData)
    {
        gameObject.SetActive(false);
    }
    public void OnDisconnected()
    {
        gameObject.SetActive(true);
    }
}
