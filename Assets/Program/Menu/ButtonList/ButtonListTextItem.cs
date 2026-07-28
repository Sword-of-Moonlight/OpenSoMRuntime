using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ButtonListTextItem : ButtonListItem
{
    [Header("References (External)")]
    [SerializeField] SoMMenuAssets MenuAssets;

    [Header("References (Internal)")]
    [SerializeField] Image backgroundImage;
    [SerializeField] Image frameImage;
    [SerializeField] TextMeshProUGUI labelField;
    [SerializeField] Animator animator;


    /// <summary>
    /// Set label data on the button
    /// </summary>
    public void SetLabel(string label, TextAlignmentOptions labelAlignment)
    {
        labelField.SetText(label);
        labelField.alignment = labelAlignment;
    }

    /// <summary>
    /// MonoBehaviour Implementation.<br/>
    /// Used to set default button skin
    /// </summary>
    protected override void Awake()
    {
        // Skinning
        frameImage.sprite      = MenuAssets.FrameBorderSprite;
        backgroundImage.sprite = MenuAssets.FrameBackgroundInactive;

        animator.Play("Idle");

        transition = Transition.None;
    }

    /// <summary>
    /// MonoBehaviour Implementation.<br/>
    /// Used to set default button skin
    /// </summary>
    protected override void OnEnable() =>
        Awake();

    public override void OnSelect(BaseEventData eventData)
    {
        // Skinning
        frameImage.sprite      = MenuAssets.FrameBorderSprite;
        backgroundImage.sprite = MenuAssets.FrameBackgroundActive;

        // Start Selected Animation
        animator.Play("Selected");

        // Play Select Sound
        SoundManager.Instance.Play2D(MenuAssets.SelectSound);

        base.OnSelect(eventData);
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        // Skinning
        frameImage.sprite      = MenuAssets.FrameBorderSprite;
        backgroundImage.sprite = MenuAssets.FrameBackgroundInactive;

        // Start Idle Animation
        animator.Play("Idle");

        base.OnDeselect(eventData);
    }

    public override void OnPressed()
    {
        // Play Confirm Sound
        if (Disabled)
            SoundManager.Instance.Play2D(MenuAssets.ErrorSound);
        else
            SoundManager.Instance.Play2D(MenuAssets.ConfirmSound);

        base.OnPressed();
    }
}
