using System.Collections;
using System.Collections.Generic;
using LiveClient;
using UnityEngine;

public class ClientTester : MonoBehaviour
{
    private Client client;
    async void Start()
    {
        client = VLLNetwork.Client;
        await client.ConnectAsync("127.0.0.1", 30000);
        client.ReceiveStart(100);
        client.HealthCheck(1000);
    }
    
    public void OnApplicationQuit()
    {
        this.client?.Close();
    }
}
