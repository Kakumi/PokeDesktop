using Godot;
using PKHeX.Core;
using System;
using System.IO;
using System.Linq;

public partial class PokemonSpriteCache : Node
{
    [Signal] public delegate void TextureReadyEventHandler(Texture2D texture);
    [Signal] public delegate void TextureFailedEventHandler(string error);

    private string GetPokemonFilename(PKM pkm)
    {
        return $"{pkm.Species}";
    }

    private string GetDownloadUrl(PKM pkm, bool isGif)
    {
        var name = GetPokemonFilename(pkm);
        var isShiny = pkm.IsShiny(pkm.PID, pkm.Generation);

        if (isGif)
        {
            if (isShiny)
            {
                return $"https://raw.githubusercontent.com/PokeAPI/sprites/refs/heads/master/sprites/pokemon/other/showdown/shiny/{name}.gif";
            }

            return $"https://raw.githubusercontent.com/PokeAPI/sprites/refs/heads/master/sprites/pokemon/other/showdown/{name}.gif";
        }

        if (isShiny)
        {
            return $"https://raw.githubusercontent.com/PokeAPI/sprites/refs/heads/master/sprites/pokemon/shiny/{name}.png";
        }

        return $"https://raw.githubusercontent.com/PokeAPI/sprites/refs/heads/master/sprites/pokemon/{name}.png";
    }

    public void LoadOrDownloadTexture(PKM pkm, bool isGif)
    {
        var dirPath = "user://data/sprites";
        var isShiny = pkm.IsShiny(pkm.PID, pkm.Generation);
        if (isShiny)
        {
            dirPath = $"{dirPath}/shiny";
        }
        DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(dirPath));

        var filename = GetPokemonFilename(pkm);
        var url = GetDownloadUrl(pkm, isGif);
        var extension = isGif ? "gif" : "png";
        var filePath = $"{dirPath}/{filename}.{extension}";

        if (Godot.FileAccess.FileExists(filePath))
        {
            var texture = LoadTextureFromFile(filePath, isGif);
            if (texture != null)
            {
                EmitSignal(SignalName.TextureReady, texture);
            }
        }

        StartDownload(filePath, isGif, url);
    }

    private void StartDownload(string filename, bool isGif, string url)
    {
        var req = new HttpRequest();
        req.RequestCompleted += (r, rc, h, b) =>
        {
            req.QueueFree();
            Req_RequestCompleted(filename, isGif, r, rc, h, b);
        };

        AddChild(req);

        var err = req.Request(url);
        if (err != Error.Ok)
        {
            EmitSignal(SignalName.TextureFailed, $"Error while downloading sprite: {err} ({url})");
        }
    }

    private void Req_RequestCompleted(string path, bool isGif, long result, long responseCode, string[] headers, byte[] body)
    {
        if (responseCode < 200 || responseCode >= 300)
        {
            EmitSignal(SignalName.TextureFailed, $"Download failed: result={result}, code={responseCode}");
        }

        using (var fa = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write))
        {
            fa.StoreBuffer(body);
        }

        var texture = LoadTextureFromFile(path, isGif);
        if (texture != null)
        {
            EmitSignal(SignalName.TextureReady, texture);
        }
        else
        {
            EmitSignal(SignalName.TextureFailed, $"Download failed: result={result}, code={responseCode}");
        }
    }

    private Texture2D LoadTextureFromFile(string path, bool isGif)
    {
        if (isGif)
        {
            var buffer = Godot.FileAccess.GetFileAsBytes(path);
            return LoadGifAsAnimatedTexture(buffer);
        }

        var img = Godot.Image.LoadFromFile(path);
        if (img == null)
        {
            return null;
        }

        return ImageTexture.CreateFromImage(img);
    }

    private AnimatedTexture LoadGifAsAnimatedTexture(byte[] buffer)
    {
        using var ms = new MemoryStream(buffer);
        var frames = GifSplitter.GifToRgbaBuffers(ms);
        var frameCount = frames.Count;
        var images = frames.Select(x => Godot.Image.CreateFromData(x.Width, x.Height, false, Godot.Image.Format.Rgba8, x.RgbaBytes));

        var animated = new AnimatedTexture
        {
            Frames = frameCount,
            OneShot = false,
            Pause = false,
        };

        for (int i = 0; i < images.Count(); i++)
        {
            var image = images.ElementAt(i);
            var tex = ImageTexture.CreateFromImage(image);

            animated.SetFrameTexture(i, tex);

            float seconds = (float)Math.Max(0.001, frames[i].Delay.TotalSeconds);
            animated.SetFrameDuration(i, seconds);
        }

        return animated;
    }
}
