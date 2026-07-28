using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;
using TMPro;

using System;
using System.IO;

public class MenuSplash : MenuBase
{
    [Header("References (Internal)")]
    [SerializeField] RawImage backgroundRenderer;
    [SerializeField] TextMeshProUGUI versionTextRenderer;

    public event Action DisplayComplete;

    /// <summary>
    /// Check to see if a splash override exists (TO-DO)
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
    }

    /// <summary>
    /// Unload any assets required by the menu
    /// </summary>
    public override void Shutdown()
    {
        base.Shutdown();
    }

    /// <summary>
    /// Open up the menu to the player
    /// </summary>
    public override void Open()
    {
        base.Open();

        // Clear out any old value
        versionTextRenderer.SetText(string.Empty);

        // Fade in the canvas
        canvasGroup.alpha = 0F;
        canvasGroup.DOFade(1F, 1F)
            .OnComplete(OnFadeInComplete);
    }

    /// <summary>
    /// Tween Callback.<br/>
    /// Called when the fade in is complete
    /// </summary>
    void OnFadeInComplete()
    {
        // Display the splash screen for five seconds...
        DOVirtual.DelayedCall(5F, OnSplashDisplayComplete);

        // Set version information
        versionTextRenderer.SetText($"Open SoM Runtime, Version {Application.version}\nBuilt with Unity {Application.unityVersion}");
    }

    /// <summary>
    /// Tween Callback.<br/>
    /// Called after the 5 seconds of splash display is complete
    /// </summary>
    void OnSplashDisplayComplete()
    {
        canvasGroup.alpha = 1F;
        canvasGroup.DOFade(0F, 1F)
            .OnComplete(() => DisplayComplete?.Invoke());
    }
}