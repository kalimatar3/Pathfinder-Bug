using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(TextMeshProUGUI))]
public abstract class BaseTextUI : baseUI
{
    [SerializeField] protected TextMeshProUGUI text;
    public TextMeshProUGUI Text {get {return text;}}
    protected void LoadText() {
        this.text = GetComponent<TextMeshProUGUI>();
        if(text == null) Debug.LogWarning("Cant found Text");
    }
    public abstract void ShowText();
}
