using UnityEngine;
using UnityEngine.UI;

public class MenuInGame : MenuBase
{
    [Header("References (Internal)")]
    [SerializeField] RawImage backgroundRenderer;

    [Header("References (External)")]
    [SerializeField] SoMMenuAssets menuAssets;

    public override void Initialize()
    {
        // Set up background...
        if (menuAssets.MenuBackground == null)
            backgroundRenderer.color = new Color32(0x00, 0x00, 0x00, 0x80);
        else
        {
            // We actually have an image to use.
            backgroundRenderer.color   = new Color32(0x80, 0x80, 0x80, 0xFF);
            backgroundRenderer.texture = menuAssets.MenuBackground;
        }

        base.Initialize();
    }
}
