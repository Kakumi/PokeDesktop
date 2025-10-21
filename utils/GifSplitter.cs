using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;

public static class GifSplitter
{
    public sealed class GifFrameData
    {
        public int Width { get; init; }
        public int Height { get; init; }
        public byte[] RgbaBytes { get; init; } = Array.Empty<byte>();
        public TimeSpan Delay { get; init; }
    }

    public static List<GifFrameData> GifToRgbaBuffers(Stream gifStream)
    {
        using var image = Image.Load<Rgba32>(gifStream);
        var result = new List<GifFrameData>(image.Frames.Count);

        for (int i = 0; i < image.Frames.Count; i++)
        {
            var frame = image.Frames[i];

            var meta = frame.Metadata.GetGifMetadata();
            int cs = meta?.FrameDelay ?? 0;
            var delay = TimeSpan.FromMilliseconds(cs * 10);

            using var frameImg = image.Frames.CloneFrame(i);

            int w = frameImg.Width;
            int h = frameImg.Height;
            var bytes = new byte[w * h * 4];

            frameImg.CopyPixelDataTo(bytes);

            result.Add(new GifFrameData
            {
                Width = w,
                Height = h,
                RgbaBytes = bytes,
                Delay = delay
            });
        }

        return result;
    }
}