using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class SystemMessage : MonoBehaviour
{
    [SerializeField] SoMMenuAssets menuAssets;
    [SerializeField] Image messageFrame;
    [SerializeField] Image messageBackground;
    [SerializeField] TextMeshProUGUI messageField;

    public void Initialize()
    {
        messageBackground.sprite = menuAssets.FrameBackgroundInactive;
        messageFrame.sprite      = menuAssets.FrameBorderSprite;
        messageField.SetText(string.Empty);
    }

    public void Show(string message, Action callback = null)
    {
        // Get the current selected game object
        // GameObject lastSelection = EventSystem.current.currentSelectedGameObject;

        // Remove the selected game object
        EventSystem.current.SetSelectedGameObject(null);

        // Show the message
        messageField.SetText(message);
        gameObject.SetActive(true);

        DOVirtual.DelayedCall(3F, () =>
        {
            // Disable message box
            gameObject.SetActive(false);

            // Restore selection
            // EventSystem.current.SetSelectedGameObject(lastSelection);

            // Invoke callback
            callback?.Invoke();
        });

        // Play error sound
        SoundManager.Instance.Play2D(menuAssets.ErrorSound);
    }
}
