using TMPro;
using UnityEngine;

public class MenuEventMessage : MenuBase
{
    [SerializeField] TextMeshProUGUI textField;

    public override void Initialize()
    {
        base.Initialize();
    }

    public void SetText(string text) =>
        textField.SetText(text);
}
