using System;
using System.Collections;
using System.Collections.Generic;
using LiveClient;
using LiveCoreLibrary;
using MessagePack;
using UniRx;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    private Client client;
    private Animator animator;
    [SerializeField] private Material _material;
    [SerializeField] private string colorName = "Color_BBCBD17C";
    private void Start()
    {
        animator = this.GetComponent<Animator>();
        client = VLLNetwork.Client;
        client.OnMessageReceived?
            .Where(e => e.Item1.type == typeof(PenLightMessage))
            .Subscribe(e => this.Received(MessagePackSerializer.Deserialize<PenLightMessage>(e.Item2)));
        
        
        _material.SetColor(colorName, Color.red);
    }

    public void Received(PenLightMessage message)
    {
        var color = message._colorRgb.ToUnityColor();
        _material.SetColor(colorName, color);
        animator.Play(message.mode.ToString());
        Debug.Log(message.mode.ToString());
    }
    
    
}
