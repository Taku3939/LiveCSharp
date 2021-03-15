using System.Collections.Generic;
using LiveClient;
using LiveCoreLibrary;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// エモートを送信するためのクラス
/// </summary>
public class EmotePresenter : MonoBehaviour
{
    [SerializeField] private List<Button> emoteButtons = new List<Button>();
    private Client client;
    private void Start()
    {
        client = VLLNetwork.Client;
        for (int i = 0; i < emoteButtons.Count; i++)
        {
            var index = i;
            emoteButtons[i].onClick.AddListener(() => client.SendAsync(new EmoteMessage(index)));
        }
    }
}

