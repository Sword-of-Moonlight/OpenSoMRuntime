using System;
using UnityEngine;

/// <summary>
/// Storage container for SoM menu assets
/// </summary>
[CreateAssetMenu(fileName = "SoMMenuAssets", menuName = "Sword of Moonlight/Menu Assets")]
public class SoMMenuAssets : ScriptableObject
{
    /**
     * HUD or Gauge assets
    **/

    /// <summary>File name mapping of the various 'gage' textures.</summary>
    readonly string[] gaugeFilename = new string[]
    {
        null,
        "GAGE1.bmp",
        "GAGE2.bmp",
        "GAGE3.bmp",
        "GAGE4.bmp"
    };

    /// <summary>Resource for the loaded 'gage' texture file</summary>
    TextureResource menuGaugeTexture;

    public Sprite GaugeFrameSprite     { get; private set; }
    public Sprite GaugePowerBarSprite  { get; private set; }
    public Sprite GaugeFocusBarSprite  { get; private set; }
    public Sprite[] GaugeNumberSprites { get; private set; }
    public Sprite StatusPoisonSprite   { get; private set; }
    public Sprite StatusParalyzeSprite { get; private set; }
    public Sprite StatusBlindSprite    { get; private set; }
    public Sprite StatusCurseSprite    { get; private set; }
    public Sprite StatusSlowSprite     { get; private set; }
    public Sprite CompassFrameSprite   { get; private set; }
    public Sprite CompassArrowSprite   { get; private set; }


    /**
     * HUD or Gauge assets
    **/

    /// <summary>Resource for the loaded background texture file</summary>
    TextureResource menuBackgroundTexture;

    public Texture2D MenuBackground { get; private set; }

    /**
     * Frame assets
    **/

    /// <summary>File name mapping of the various 'gage' textures.</summary>
    readonly string[] frameFilename = new string[]
    {
        "NoFrame.bmp",
        "frameG1.bmp",
        "frameG2.bmp",
        "frameS1.bmp",
        "frameS2.bmp"
    };

    /// <summary>Resource for the loaded 'frame' texture file</summary>
    TextureResource menuFrameTexture;

    public Sprite FrameBorderSprite       { get; private set; }
    public Sprite FrameThumbSprite        { get; private set; }
    public Sprite FrameArrowUpSprite      { get; private set; }
    public Sprite FrameArrowDownSprite    { get; private set; }
    public Sprite FrameArrowLeftSprite    { get; private set; }
    public Sprite FrameArrowRightSprite   { get; private set; }
    public Sprite FrameBackgroundInactive { get; private set; }
    public Sprite FrameBackgroundActive   { get; private set; }
    public Sprite FrameBackgroundError    { get; private set; }


    /**
     * Sound assets
    **/
    public AudioClip ConfirmSound { get; private set; }
    public AudioClip SelectSound { get; private set; }
    public AudioClip CancelSound { get; private set; }
    public AudioClip ErrorSound { get; private set; }

    // Data
    AudioResource menuConfirmSound;
    AudioResource menuSelectSound;
    AudioResource menuCancelSound;
    AudioResource menuErrorSound;

    public void Initialize()
    {
        ulong resourceName;

        // Load Gauge Texture, 'GAGE1', 'GAGE2', 'GAGE3', 'GAGE4'
        if (GameManager.Instance.ProjectData.defaultGaugeType != 0)
        {
            // Load the gauge texture
            if (ResourceManager.Find($"DATA\\MENU\\GAGE{GameManager.Instance.ProjectData.defaultGaugeType:D1}.bmp", out string foundGaugeTexture))
            {
                resourceName = ResourceManager.Load<TextureResource>(foundGaugeTexture);
                menuGaugeTexture = ResourceManager.Get<TextureResource>(resourceName);

                // Parse gauge into sprites...
                Texture2D gaugeUnityTexture = menuGaugeTexture.Get();

                // Gauge
                GaugeFrameSprite = Sprite.Create(
                    gaugeUnityTexture,
                    new Rect(0, 187, 255, 69),
                    new Vector2(0.5F, 0.5F),
                    100F);

                GaugePowerBarSprite = Sprite.Create(
                    gaugeUnityTexture,
                    new Rect(120, 40, 128, 16),
                    new Vector2(0.5F, 0.5F),
                    100F);

                GaugeFocusBarSprite = Sprite.Create(
                    gaugeUnityTexture,
                    new Rect(120, 16, 128, 16),
                    new Vector2(0.5F, 0.5F),
                    100F);

                GaugeNumberSprites = new Sprite[10];

                for (int i = 0; i < 10; ++i)
                {
                    GaugeNumberSprites[i] = Sprite.Create(
                        gaugeUnityTexture,
                        new Rect(10 * i, 38, 10, 18),
                        new Vector2(0.5F, 0.5F),
                        100F);
                }


                // Status
                StatusPoisonSprite = Sprite.Create(
                    gaugeUnityTexture,
                    new Rect(0, 8, 24, 24),
                    new Vector2(0.5F, 0.5F),
                    100F);

                StatusParalyzeSprite = Sprite.Create(
                    gaugeUnityTexture,
                    new Rect(24, 8, 24, 24),
                    new Vector2(0.5F, 0.5F),
                    100F);

                StatusBlindSprite = Sprite.Create(
                    gaugeUnityTexture,
                    new Rect(48, 8, 24, 24),
                    new Vector2(0.5F, 0.5F),
                    100F);

                StatusCurseSprite = Sprite.Create(
                    gaugeUnityTexture,
                    new Rect(72, 8, 24, 24),
                    new Vector2(0.5F, 0.5F),
                    100F);

                StatusSlowSprite = Sprite.Create(
                    gaugeUnityTexture,
                    new Rect(96, 8, 24, 24),
                    new Vector2(0.5F, 0.5F),
                    100F);


                // Compass
                CompassFrameSprite = Sprite.Create(
                    gaugeUnityTexture,
                    new Rect(0, 62, 143, 122),
                    new Vector2(0.5F, 0.5F),
                    100F);

                CompassArrowSprite = Sprite.Create(
                    gaugeUnityTexture,
                    new Rect(148, 120, 64, 64),
                    new Vector2(0.5F, 0.34375F),
                    100F);
            }
        }

        // Load Menu Background.
        if (GameManager.Instance.ProjectData.menuBackgroundFile != string.Empty)
        {
            // Load the background texture
            if (ResourceManager.Find($"DATA\\PICTURE\\{GameManager.Instance.ProjectData.menuBackgroundFile}", out string foundBackgroundTexture))
            {
                // Get texture resource
                resourceName = ResourceManager.Load<TextureResource>(foundBackgroundTexture);
                menuBackgroundTexture = ResourceManager.Get<TextureResource>(resourceName);

                // Grab the unity texture
                MenuBackground = menuBackgroundTexture.Get();
            }
        }

        // Load Frame Assets. 'NoFrame', 'frameG1', 'frameG2', 'frameS1', 'frameS2'
        if (ResourceManager.Find($"DATA\\MENU\\{frameFilename[GameManager.Instance.ProjectData.defaultMenuStyle]}", out string foundFrameTexture))
        {
            // Get frame texture resource
            resourceName     = ResourceManager.Load<TextureResource>(foundFrameTexture);
            menuFrameTexture = ResourceManager.Get<TextureResource>(resourceName);

            // Grab our sprites from the frame texture
            Texture2D frameUnityTexture = menuFrameTexture.Get();

            // Helper function for creating sprites for the frame
            Sprite CreateFrameSprite(Texture2D baseTexture, Vector2 XY, Vector2 WH, Vector4 border) =>
                Sprite.Create(baseTexture, new Rect(XY, WH), new Vector2(.5F, .5F), 100F, 0, SpriteMeshType.Tight, border, false);

            // Create sprite definitions of the texture
            FrameBorderSprite       = CreateFrameSprite(frameUnityTexture, new Vector2(0, 136),   new Vector2(120, 120), new Vector4(40, 40, 40, 40));
            FrameThumbSprite        = CreateFrameSprite(frameUnityTexture, new Vector2(0, 176),   new Vector2(80, 80),   Vector4.zero);
            FrameArrowUpSprite      = CreateFrameSprite(frameUnityTexture, new Vector2(0, 96),    new Vector2(40, 40),   Vector4.zero);
            FrameArrowDownSprite    = CreateFrameSprite(frameUnityTexture, new Vector2(40, 96),   new Vector2(40, 40),   Vector4.zero);
            FrameArrowLeftSprite    = CreateFrameSprite(frameUnityTexture, new Vector2(80, 96),   new Vector2(40, 40),   Vector4.zero);
            FrameArrowRightSprite   = CreateFrameSprite(frameUnityTexture, new Vector2(120, 96),  new Vector2(40, 40),   Vector4.zero);
            FrameBackgroundInactive = CreateFrameSprite(frameUnityTexture, new Vector2(120, 216), new Vector2(40, 40),   Vector4.zero);
            FrameBackgroundActive   = CreateFrameSprite(frameUnityTexture, new Vector2(120, 176), new Vector2(40, 40),   Vector4.zero);
            FrameBackgroundError    = CreateFrameSprite(frameUnityTexture, new Vector2(120, 136), new Vector2(40, 40),   Vector4.zero);
        }

        // Load Menu Sounds
        if (GameManager.Instance.ProjectData.menuSoundType != 0)
        {
            int menuSoundBase = 1008 + (4 * (GameManager.Instance.ProjectData.menuSoundType - 1));

            // Error Sound
            if (ResourceManager.Find($"DATA\\SOUND\\SE\\{(menuSoundBase + 0):D4}.SND", out string foundErrorSound))
            {
                resourceName   = ResourceManager.Load<AudioResource>(foundErrorSound);
                menuErrorSound = ResourceManager.Get<AudioResource>(resourceName);
                ErrorSound     = menuErrorSound.Get();
            }

            // Confirm Sound
            if (ResourceManager.Find($"DATA\\SOUND\\SE\\{(menuSoundBase + 1):D4}.SND", out string foundConfirmSound))
            {
                resourceName     = ResourceManager.Load<AudioResource>(foundConfirmSound);
                menuConfirmSound = ResourceManager.Get<AudioResource>(resourceName);
                ConfirmSound     = menuConfirmSound.Get();
            }

            // Cancel Sound
            if (ResourceManager.Find($"DATA\\SOUND\\SE\\{(menuSoundBase + 2):D4}.SND", out string foundCancelSound))
            {
                resourceName    = ResourceManager.Load<AudioResource>(foundCancelSound);
                menuCancelSound = ResourceManager.Get<AudioResource>(resourceName);
                CancelSound     = menuCancelSound.Get();
            }

            // Select Sound
            if (ResourceManager.Find($"DATA\\SOUND\\SE\\{(menuSoundBase + 3):D4}.SND", out string foundSelectSound))
            {
                resourceName    = ResourceManager.Load<AudioResource>(foundSelectSound);
                menuSelectSound = ResourceManager.Get<AudioResource>(resourceName);
                SelectSound     = menuSelectSound.Get();
            }
        }
    }

    /// <summary>
    /// MonoBehaviour Implementation.<br/>
    /// Frees held resources.
    /// </summary>
    public void Free()
    {
        // Free Gauge Image
        menuGaugeTexture?.Free();

        // Free menu background
        menuBackgroundTexture?.Free();

        // Free Frame Image
        menuFrameTexture?.Free();

        // Free Sounds
        menuConfirmSound?.Free();
        menuSelectSound?.Free();
        menuCancelSound?.Free();
        menuErrorSound?.Free();
    }
}
