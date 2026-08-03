using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;
using TMPro;

using System;
using System.IO;


public class MenuTitle : MenuBase
{
    [Header("References (Internal)")]
    [SerializeField] RawImage backgroundRenderer;
    [SerializeField] ButtonList buttonListA;
    [SerializeField] ButtonList buttonListB;

    /// <summary>Stores the title background resource</summary>
    TextureResource backgroundResource;

    // These are our actual button references
    ButtonListItem pressStartButton;
    ButtonListItem newGameButton;
    ButtonListItem continueButton;

    public event Action NewGame;
    public event Action ContinueGame;

    /// <summary>
    /// Load title assets
    /// </summary>
    public override void Initialize()
    {
        // Load the title background
        if (ResourceManager.Find($"\\DATA\\PICTURE\\{GameManager.Instance.ProjectData.titleBackgroundFile}", out string backgroundFile))
        {
            // Attempt to load the background image and assign it to the background renderer
            try
            {
                ulong backgroundResourceName = ResourceManager.Load<TextureResource>(backgroundFile);
                backgroundResource           = ResourceManager.Get<TextureResource>(backgroundResourceName);
                backgroundRenderer.texture   = backgroundResource.Get();
            }
            catch (Exception ex)
            {
                // Clean up...
                backgroundRenderer.texture = null;

                // Log out what happened
                Logger.Error(ex.Message);
            }  
        }

        // TO-DO: We've done text based buttons, but we still need to do image buttons and a flag to switch which are used...
        pressStartButton = buttonListA.CreateButton("PRESS START BUTTON", TextAlignmentOptions.Center);
        newGameButton    = buttonListB.CreateButton("NEW GAME", TextAlignmentOptions.Center);
        continueButton   = buttonListB.CreateButton("CONTINUE", TextAlignmentOptions.Center);

        // We now want to bind our button events...
        pressStartButton.Pressed += OnStartButtonPressed;
        newGameButton.Pressed    += OnNewGameButtonPressed;
        continueButton.Pressed   += OnContinueButtonPressed;

        base.Initialize();
    }

    /// <summary>
    /// Event Callback.<br/>
    /// Called when the "press to start" button is pressed
    /// </summary>
    void OnStartButtonPressed()
    {
        buttonListA.gameObject.SetActive(false);
        buttonListB.gameObject.SetActive(true);

        // Depending on if save data exists, this should highlight continue instead...
        buttonListB.Select(0);
    }

    /// <summary>
    /// Event Callback.<br/>
    /// Called when the "new game" button is pressed
    /// </summary>
    void OnNewGameButtonPressed()
    {
        // Disable everything to begin with...
        buttonListA.gameObject.SetActive(false);
        buttonListB.gameObject.SetActive(false);

        canvasGroup.alpha = 1F;
        canvasGroup.DOFade(0F, 1F)
            .OnComplete(() =>
            {
                NewGame?.Invoke();
            });
    }

    /// <summary>
    /// Event Callback.<br/>
    /// Called when the "continue" button is pressed
    /// </summary>
    void OnContinueButtonPressed()
    {
        // Disable everything to begin with...
        buttonListA.gameObject.SetActive(false);
        buttonListB.gameObject.SetActive(false);

        canvasGroup.alpha = 1F;
        canvasGroup.DOFade(0F, 1F)
            .OnComplete(() =>
            {
                ContinueGame?.Invoke();
            });
    }

    public override void Open()
    {
        base.Open();

        // Disable everything to begin with...
        buttonListA.gameObject.SetActive(false);
        buttonListB.gameObject.SetActive(false);

        canvasGroup.alpha = 0F;
        canvasGroup.DOFade(1F, 1F)
            .OnComplete(() =>
            {
                buttonListA.gameObject.SetActive(true);
                buttonListA.Select(0);

                buttonListB.gameObject.SetActive(false);
            });
    }

    public override void Shutdown()
    {
        // Free Assets
        backgroundResource?.Free();

        base.Shutdown();
    }
}
