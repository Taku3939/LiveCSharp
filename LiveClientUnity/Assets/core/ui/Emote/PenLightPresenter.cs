using System.Collections;
using System.Collections.Generic;
using LiveClient;
using LiveCoreLibrary;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PenLightPresenter : MonoBehaviour
{
    [SerializeField] private List<Button> emoteButtons;
    [SerializeField] private List<Button> colorButtons;
    private Client client;

    private ColorRgb CurrentColor = new ColorRgb(0, 0, 0);
    private PenLightMode Mode = PenLightMode.Pen_None;

    private void Start()
    {
        client = VLLNetwork.Client;

        //ペンライトのモード変更のイベント追加
        for (int i = 0; i < emoteButtons.Count; i++)
        {
            var index = i;
            emoteButtons[i].onClick.AddListener(() => Mode = (PenLightMode) index);
        }
        
        //ペンライトの色の変更のイベント追加
        foreach (var b in colorButtons) b.onClick.AddListener(() => CurrentColor = new ColorRgb(b.GetComponent<RawImage>().color));
        
        //Buttonに送信イベントの追加
        emoteButtons.ForEach(b => b.onClick.AddListener(() => client.SendAsync(new PenLightMessage(Mode, CurrentColor))));
        colorButtons.ForEach(b => b.onClick.AddListener(() => client.SendAsync(new PenLightMessage(Mode, CurrentColor))));
    }
}