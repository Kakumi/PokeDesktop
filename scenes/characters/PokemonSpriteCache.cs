using Godot;
using PKHeX.Core;
using System;
using System.IO;
using System.Linq;

public partial class PokemonSpriteCache : Node
{
    [Signal] public delegate void TextureReadyEventHandler(Texture2D texture);
    [Signal] public delegate void TextureFailedEventHandler(string error);

    private SpriteCdn _spriteSource;

    public override void _Ready()
    {
        _spriteSource = SettingsManager.Instance.GetSpriteCDN();
    }

    private string GetDownloadUrl(PKM pkm)
    {
        var isShiny = pkm.IsShiny(pkm.PID, pkm.Generation);
        var url = isShiny ? _spriteSource.ShinyUrl : _spriteSource.NormalUrl;
        var speciesNames = GameInfo.GetStrings("en").Species[pkm.Species].ToLower();

        return url.Replace("{species}", pkm.Species.ToString()).Replace("{name}", speciesNames);
    }

    public void LoadOrDownloadTexture(PKM pkm)
    {
        var dirPath = $"user://data/sprites/{_spriteSource.Folder}";
        var isShiny = pkm.IsShiny(pkm.PID, pkm.Generation);
        if (isShiny)
        {
            dirPath = $"{dirPath}/shiny";
        }
        DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(dirPath));

        var filename = $"{pkm.Species}";
        var url = GetDownloadUrl(pkm);
        var extension = Path.GetExtension(_spriteSource.NormalUrl).Substring(1);
        var isAnimated = extension.Contains("gif");
        var filePath = $"{dirPath}/{filename}.{extension}";

        if (Godot.FileAccess.FileExists(filePath))
        {
            var texture = LoadTextureFromFile(filePath, isAnimated);
            if (texture != null)
            {
                EmitSignal(SignalName.TextureReady, texture);
            }
        }

        StartDownload(filePath, isAnimated, url);
    }

    private void StartDownload(string filename, bool isAnimated, string url)
    {
        var req = new HttpRequest();
        req.RequestCompleted += (r, rc, h, b) =>
        {
            req.QueueFree();
            Req_RequestCompleted(filename, isAnimated, r, rc, h, b);
        };

        AddChild(req);

        var err = req.Request(url);
        if (err != Error.Ok)
        {
            EmitSignal(SignalName.TextureFailed, string.Format(TranslationServer.Translate("CACHE_SPRITE_REQUEST_FAILED"), err, url));
        }
    }

    private void Req_RequestCompleted(string path, bool isAnimated, long result, long responseCode, string[] headers, byte[] body)
    {
        if (responseCode < 200 || responseCode >= 300)
        {
            EmitSignal(SignalName.TextureFailed, string.Format(TranslationServer.Translate("CACHE_SPRITE_DOWNLOAD_FAILED"), result, responseCode));
            return;
        }

        try
        {
            using var fa = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);
            fa.StoreBuffer(body);
        }
        catch (Exception e)
        {
            EmitSignal(SignalName.TextureFailed, string.Format(TranslationServer.Translate("CACHE_SPRITE_SAVE_FAILED"), e.Message));
            return;
        }

        var texture = LoadTextureFromFile(path, isAnimated);
        if (texture != null)
        {
            EmitSignal(SignalName.TextureReady, texture);
        }
        else
        {
            EmitSignal(SignalName.TextureFailed, string.Format(TranslationServer.Translate("CACHE_SPRITE_DOWNLOAD_FAILED"), result, responseCode));
        }
    }

    private Texture2D LoadTextureFromFile(string path, bool isAnimated)
    {
        if (isAnimated)
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
