using UnityEngine;
using UnityEngine.UI;
public class HUDController : MonoBehaviour
{
    [Header("References (External)")]
    [SerializeField] SoMMenuAssets menuAssets;

    [Header("References (Internal)")]
    [SerializeField] GameObject compassBase;
    [SerializeField] Image compassFrameImage;
    [SerializeField] Image compassArrowImage;

    [SerializeField] GameObject statusBase;
    [SerializeField] Image statusPoisonImage;
    [SerializeField] Image statusParalyzeImage;
    [SerializeField] Image statusBlindImage;
    [SerializeField] Image statusCurseImage;
    [SerializeField] Image statusSlowImage;

    [SerializeField] GameObject gaugeBase;
    [SerializeField] Image gaugeFrameImage;
    [SerializeField] Image gaugePowerBarImage;
    [SerializeField] Image gaugeFocusBarImage;
    [SerializeField] Image[] gaugeHPTickerImages;
    [SerializeField] Image[] gaugeMPTicketImages;

    /// <summary>
    /// Enable the HUD
    /// </summary>
    public void Enable()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Disable the HUD
    /// </summary>
    public void Disable()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// MonoBehaviour Implementation.<br/>
    /// </summary>
    void Awake()
    {
        // Compass Set up
        compassBase.SetActive(GameManager.Instance.ProjectData.defaultCompassType != 0);

        if (compassBase.activeInHierarchy)
        {
            compassFrameImage.sprite = menuAssets.CompassFrameSprite;
            compassArrowImage.sprite = menuAssets.CompassArrowSprite;
        }


        // Status Set up
        statusBase.SetActive(true); // No option for this - but we should add one...

        if (statusBase.activeInHierarchy)
        {
            statusPoisonImage.sprite   = menuAssets.StatusPoisonSprite;
            statusParalyzeImage.sprite = menuAssets.StatusParalyzeSprite;
            statusBlindImage.sprite    = menuAssets.StatusBlindSprite;
            statusCurseImage.sprite    = menuAssets.StatusCurseSprite;
            statusSlowImage.sprite     = menuAssets.StatusSlowSprite;
        }


        // Gauge Set up
        gaugeBase.SetActive(GameManager.Instance.ProjectData.defaultGaugeType != 0);

        if (gaugeBase.activeInHierarchy)
        {
            gaugeFrameImage.sprite    = menuAssets.GaugeFrameSprite;
            gaugePowerBarImage.sprite = menuAssets.GaugePowerBarSprite;
            gaugeFocusBarImage.sprite = menuAssets.GaugeFocusBarSprite;

            SetTickerNumber(gaugeHPTickerImages, 50);
            SetTickerNumber(gaugeMPTicketImages, 30);
        }
    }

    /// <summary>
    /// MonoBehaviour Implementation.<br/>
    /// </summary>
    void FixedUpdate()
    {
        // Bar pulses
        float pulseBaseTime = ((Time.time * 4F) % (2F * Mathf.PI));
        float pulsePowerMod = 0F;
        float pulseFocusMod = (Mathf.PI * 0.5F);

        gaugePowerBarImage.color = new Color(1, 1, 1, 0.375F + (Mathf.Cos(pulseBaseTime + pulsePowerMod) * 0.625F));
        gaugeFocusBarImage.color = new Color(1, 1, 1, 0.375F + (Mathf.Cos(pulseBaseTime + pulseFocusMod) * 0.625F));
    }

    void SetTickerNumber(Image[] images, int number)
    {
        // Seperate number into H, T and O
        int H = (number / 100);
        int T = (number / 10) % 10;
        int U = (number % 10);

        // Set images...
        images[0].sprite = menuAssets.GaugeNumberSprites[H];
        images[1].sprite = menuAssets.GaugeNumberSprites[T];
        images[2].sprite = menuAssets.GaugeNumberSprites[U];
    }
}
