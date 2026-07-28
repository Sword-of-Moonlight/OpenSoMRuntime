using System.IO;
using System.Text;
using UnityEngine;

public class SoMSlideshow
{
    public int NumberOfSlides { get; private set; }
    public SoMSlideshowSlide[] Slides { get; private set; }
    public string MusicFileName { get; private set; }

    /// <summary>
    /// Loads a SoM slide show sequence from file
    /// </summary>
    public static bool LoadFromFile(string filename, out SoMSlideshow slideshow)
    {
        // Create result
        slideshow = new SoMSlideshow();

        // Does the file exist?
        if (!File.Exists(filename))
            return false;

        // Read the slideshow file...
        using StreamReader sr = new (filename, EncodingExtensions.SJIS);

        // First is number of slides, as an int
        slideshow.NumberOfSlides = int.Parse(sr.ReadLine());
        slideshow.Slides         = new SoMSlideshowSlide[slideshow.NumberOfSlides];

        // Now read the slide show frames
        // NO_BMP = NO IMAGE
        for (int i = 0; i < slideshow.NumberOfSlides; ++i)
        {
            slideshow.Slides[i] = new SoMSlideshowSlide
            {
                imageFileName        = sr.ReadLine(),
                displayTimeMilliSecs = int.Parse(sr.ReadLine())
            };
        }

        // Now read the music file to play with the slide show.
        slideshow.MusicFileName = sr.ReadLine();

        // Now reach the text content for each frame
        for (int i = 0; i < slideshow.NumberOfSlides; ++i)
        {
            // Every text block begins with a newline?
            sr.ReadLine();

            // There are sixteen lines of text.
            StringBuilder slideTextBuilder = new StringBuilder();
            for (int j = 0; j < 16; ++j)
                slideTextBuilder.AppendLine(sr.ReadLine());

            SoMSlideshowSlide slide = slideshow.Slides[i];
            slide.text = slideTextBuilder.ToString();
            slideshow.Slides[i] = slide;
        }

        return true;
    }
}

public struct SoMSlideshowSlide
{
    public string imageFileName;
    public int displayTimeMilliSecs;
    public string text;
}
