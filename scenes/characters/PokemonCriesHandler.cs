using Godot;
using PKHeX.Core;
using System;

public partial class PokemonCriesHandler : Node
{
    public AudioStreamPlayer AudioStreamPlayer { get; private set; }

    public override void _Ready()
    {
        AudioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
    }

    // ---- UTILS ----
    private string GetPokemonFilename(PKM pkm)
        => $"{pkm.Species}";

    private string GetDownloadUrl(PKM pkm)
    {
        var name = GetPokemonFilename(pkm);
        // PokeAPI cries - OGG
        return $"https://github.com/PokeAPI/cries/raw/refs/heads/main/cries/pokemon/latest/{name}.ogg";
    }

    // ---- API ----
    public void LoadOrDownloadSound(PKM pkm)
    {
        if (!SettingsManager.Instance.Settings.CriesOnEmotion && !SettingsManager.Instance.Settings.CriesOnClick)
        {
            //Options disabled, skipped
            return;
        }

        var dirPath = "user://data/cries";
        DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(dirPath));

        var filename = GetPokemonFilename(pkm);
        var filePath = $"{dirPath}/{filename}.ogg";
        var url = GetDownloadUrl(pkm);

        if (Godot.FileAccess.FileExists(filePath))
        {
            var stream = LoadSoundFromFile(filePath);
            if (stream != null)
            {
                SetStream(stream);
                return;
            }
        }

        StartDownload(filePath, url);
    }

    public void PlayCry()
    {
        if (AudioStreamPlayer.Stream != null && !AudioStreamPlayer.Playing)
        {
            AudioStreamPlayer.Play();
        }
    }

    // ---- DOWNLOAD ----
    private void StartDownload(string filePath, string url)
    {
        var req = new HttpRequest
        {
            Timeout = 30
        };

        req.RequestCompleted += (long result, long responseCode, string[] headers, byte[] body) =>
        {
            req.QueueFree();
            Req_RequestCompleted(filePath, result, responseCode, headers, body);
        };

        AddChild(req);

        var err = req.Request(url);
        if (err != Error.Ok)
        {
            Logger.Instance.Error(string.Format(TranslationServer.Translate("CACHE_SOUND_REQUEST_FAILED"), err, url));
        }
    }

    private void Req_RequestCompleted(string path, long result, long responseCode, string[] headers, byte[] body)
    {
        if (responseCode < 200 || responseCode >= 300 || body == null || body.Length == 0)
        {
            Logger.Instance.Error(string.Format(TranslationServer.Translate("CACHE_SOUND_DOWNLOAD_FAILED"), result, responseCode));
            return;
        }

        try
        {
            using var fa = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);
            fa.StoreBuffer(body);
        }
        catch (Exception e)
        {
            Logger.Instance.Error(string.Format(TranslationServer.Translate("CACHE_SOUND_SAVE_FAILED"), e.Message));
            return;
        }

        var stream = LoadSoundFromFile(path);
        if (stream != null)
        {
            SetStream(stream);
        }
        else
        {
            Logger.Instance.Error(string.Format(TranslationServer.Translate("CACHE_SOUND_DOWNLOAD_FAILED"), result, responseCode));
        }
    }

    private void SetStream(AudioStream stream)
    {
        AudioStreamPlayer.Stream = stream;
    }

    private AudioStream LoadSoundFromFile(string path)
    {
        return AudioStreamOggVorbis.LoadFromFile(path);
    }
}
