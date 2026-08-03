using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using DG.Tweening;
using TMPro;

using System;

public class MenuSequence : MenuBase
{
    [Header("References (Internal)")]
    [SerializeField] VideoPlayer sequenceVideoPlayer;
    [SerializeField] RawImage sequenceImageRenderer;
    [SerializeField] TextMeshProUGUI sequenceTextField;

    // Data
    TextureResource slideshowImage = null;
    Sequence slideshowSequence = null;

    // Events
    public event Action SequenceComplete;

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
        // Free slide show resources...
        slideshowImage?.Free();

        // Slideshow sequence termination.
        slideshowSequence = null;

        base.Shutdown();
    }

    /// <summary>
    /// Plays a legacy type sequence
    /// </summary>
    public void PlaySequence(SoMSequence sequence)
    {
        switch (sequence.mode)
        {
            case SoMSequenceMode.Video:

                if (!ResourceManager.Find(sequence.file, out string foundSeqVideo))
                    SequenceComplete?.Invoke();
                else
                {
                    sequenceVideoPlayer.gameObject.SetActive(true);
                    sequenceVideoPlayer.enabled = true;
                    sequenceVideoPlayer.source = VideoSource.Url;
                    sequenceVideoPlayer.url = foundSeqVideo;

                    sequenceVideoPlayer.loopPointReached += OnVideoPlayerLoopPointReached;
                    sequenceVideoPlayer.Play();
                }   
                break;

            case SoMSequenceMode.SlideShow:

                if (!ResourceManager.Find(sequence.file, out string foundSeqSlideshow))
                    SequenceComplete?.Invoke();
                else
                {
                    if (!SoMSlideshow.LoadFromFile(foundSeqSlideshow, out SoMSlideshow slideshow))
                        SequenceComplete?.Invoke();

                    // Convert SlideShow to DOTween sequence
                    slideshowSequence = DOTween.Sequence();

                    // Each frame becomes a mess of tweens
                    for (int i = 0; i < slideshow.NumberOfSlides; ++i)
                    {
                        SoMSlideshowSlide slide = slideshow.Slides[i];

                        // Set up image and text.
                        slideshowSequence.AppendCallback(() =>
                        {
                            // Clear sequence text
                            sequenceTextField.SetText(string.Empty);

                            // If a slideshow image is already loaded, free it.
                            slideshowImage?.Free();

                            // Should this frame have an image?
                            if (!slide.imageFileName.Equals("NO_BMP", StringComparison.InvariantCultureIgnoreCase))
                            {
                                // Load the image to display...
                                if (ResourceManager.Find($"DATA\\PICTURE\\{slide.imageFileName}", out string slideShowImage))
                                {
                                    ulong slideshowImageName = ResourceManager.Load<TextureResource>(slideShowImage);

                                    // Get it...
                                    slideshowImage = ResourceManager.Get<TextureResource>(slideshowImageName);

                                    // Get the native unity resource
                                    sequenceImageRenderer.texture = slideshowImage.Get();

                                    sequenceImageRenderer.color = new Color(1, 1, 1, 0);
                                }
                                else
                                {
                                    sequenceImageRenderer.texture = null;
                                    sequenceImageRenderer.color   = new Color(0, 0, 0, 0);
                                }
                            }
                            else
                                // Where no image is wanted, display black...
                                sequenceImageRenderer.color = Color.black;
                        });

                        // Fade image in for one second...
                        slideshowSequence.Append(sequenceImageRenderer.DOFade(1F, 1.5F));

                        // Wait for (around) 1 second ?..
                        //
                        // These timings are all slightly off... They're close? 
                        // EDIT: actually, maybe they're fine.
                        //
                        slideshowSequence.AppendInterval(2F);

                        // Show the text, then wait for the slide time
                        slideshowSequence.AppendCallback(() => sequenceTextField.SetText(slide.text));
                        slideshowSequence.AppendInterval(slide.displayTimeMilliSecs / 1000F);

                        // Remove text and fade out
                        slideshowSequence.AppendCallback(() => sequenceTextField.SetText(string.Empty));
                        slideshowSequence.Append(sequenceImageRenderer.DOFade(0F, 1.0F));

                        // Wait for (around) 1 second?..
                        slideshowSequence.AppendInterval(1.5F);
                    }

                    slideshowSequence.OnStart(() =>
                    {
                        // Load BGM - I mean... If you're uh... ready?
                        if (!slideshow.MusicFileName.Equals("NO_BGM", StringComparison.InvariantCultureIgnoreCase))
                            MusicManager.Instance.Play(slideshow.MusicFileName, false);

                        // Enable sequence objects
                        sequenceImageRenderer.gameObject.SetActive(true);
                        sequenceTextField.gameObject.SetActive(true);
                    });

                    slideshowSequence.OnComplete(() =>
                    {
                        // Disable sequence objects
                        sequenceImageRenderer.gameObject.SetActive(false);
                        sequenceImageRenderer.texture = null;
                        sequenceTextField.gameObject.SetActive(false);

                        // Free slide show resources...
                        slideshowImage?.Free();
                        slideshowImage = null;

                        // Stop music
                        MusicManager.Instance.Stop();

                        // Call complete event
                        SequenceComplete?.Invoke();
                    });

                    // Play the built slideshow sequence.
                    slideshowSequence.Play();
                }
                break;

            case SoMSequenceMode.None:
                SequenceComplete?.Invoke();
                break;
        }
    }

    /// <summary>
    /// Immediately stop a sequence.
    /// </summary>
    public void StopSequence()
    {
        if (sequenceVideoPlayer.isPlaying)
        {
            // Video
            sequenceVideoPlayer.Stop();
        }
        else
        {
            // Slide show
            if (slideshowSequence != null)
                slideshowSequence.Kill();

            sequenceImageRenderer.color = Color.black;
            sequenceTextField.SetText(string.Empty);
        }

        // Stop sequence music from further playback
        MusicManager.Instance.Stop();
        
        // Fire off the complete sequence event
        SequenceComplete?.Invoke();
    }

    /// <summary>
    /// Event Callback.<br/>
    /// Called when a video sequence has completed playback.
    /// </summary>
    void OnVideoPlayerLoopPointReached(VideoPlayer source)
    {
        sequenceVideoPlayer.loopPointReached -= OnVideoPlayerLoopPointReached;

        // Raise sequence complete
        SequenceComplete?.Invoke();
    }
}
