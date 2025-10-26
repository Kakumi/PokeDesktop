using Godot;
using PKHeX.Core;
using System;
using System.IO;

public partial class PokemonCriesHandler : Node
{
    public AudioStreamPlayer AudioStreamPlayer { get; private set; }

    private CriesCdn _crySource;

    public override void _Ready()
    {
        AudioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");

        _crySource = SettingsManager.Instance.GetCryCDN();
    }

    // ---- UTILS ----
    private string GetDownloadUrl(PKM pkm)
    {
        var speciesNames = GameInfo.GetStrings("en").Species[pkm.Species].ToLower();

        return _crySource.Url.Replace("{species}", pkm.Species.ToString()).Replace("{name}", speciesNames);
    }

    // ---- API ----
    public void LoadOrDownloadSound(PKM pkm)
    {
        if (!SettingsManager.Instance.Settings.CriesOnEmotion && !SettingsManager.Instance.Settings.CriesOnClick)
        {
            //Options disabled, skipped
            return;
        }

        var dirPath = $"user://data/cries/{_crySource.Folder}";
        DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath(dirPath));

        var filename = $"{pkm.Species}";
        var extension = Path.GetExtension(_crySource.Url).Substring(1);
        var filePath = $"{dirPath}/{filename}.{extension}";
        var url = GetDownloadUrl(pkm);

        if (Godot.FileAccess.FileExists(filePath))
        {
            var stream = LoadSoundFromFile(filePath, extension);
            if (stream != null)
            {
                SetStream(stream);
                return;
            }
        }

        StartDownload(filePath, url, extension);
    }

    public void PlayCry()
    {
        if (AudioStreamPlayer.Stream != null && !AudioStreamPlayer.Playing)
        {
            AudioStreamPlayer.Play();
        }
    }

    // ---- DOWNLOAD ----
    private void StartDownload(string filePath, string url, string extension)
    {
        var req = new HttpRequest
        {
            Timeout = 30
        };

        req.RequestCompleted += (long result, long responseCode, string[] headers, byte[] body) =>
        {
            req.QueueFree();
            Req_RequestCompleted(filePath, extension, result, responseCode, headers, body);
        };

        AddChild(req);

        var err = req.Request(url);
        if (err != Error.Ok)
        {
            Logger.Instance.Error(string.Format(TranslationServer.Translate("CACHE_SOUND_REQUEST_FAILED"), err, url));
        }
    }

    private void Req_RequestCompleted(string path, string extension, long result, long responseCode, string[] headers, byte[] body)
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

        var stream = LoadSoundFromFile(path, extension);
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

    private AudioStream LoadSoundFromFile(string path, string extension)
    {
        if (extension.Contains("mp3"))
        {
            return AudioStreamMP3.LoadFromFile(path);
        }

        return AudioStreamOggVorbis.LoadFromFile(path);
    }
}
