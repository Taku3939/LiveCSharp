using System;
using System.Collections.Generic;
using LiveClient;
using LiveCoreLibrary;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Windowをポップアップさせるための汎用クラス
/// </summary>
[RequireComponent(typeof(Animator))]
public class WindowPopup : MonoBehaviour
{
    [SerializeField] private Button popupButton;
    [SerializeField] private string showAnimName, hideAnimName;
    private Animator _animator;
    public bool isOpen;
    private void Start()
    {
        _animator = this.GetComponent<Animator>();
        popupButton.onClick.AddListener(() => isOpen = !isOpen);
        
        //isOpen変数の値に応じてタブのアニメーションを生成する
        this.ObserveEveryValueChanged(x => x.isOpen)
            .Subscribe(Animate);

    }

    public void Animate(bool b)
    {
        string animationName = b ? showAnimName : hideAnimName;
        _animator.Play(animationName, 0);
    }
}

