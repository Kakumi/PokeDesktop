using Godot;
using System;
using System.Reflection;
using System.Text;
using System.Text.Json;

public partial class VersionManager : Node
{
    public static string CurrentVersion { get; private set; }

    public HttpRequest HttpRequest { get; private set; }
    public AcceptDialog Dialog { get; private set; }
    public Label Message { get; private set; }

    private ReleaseResponse _response;

    public override void _Ready()
    {
        HttpRequest = GetNode<HttpRequest>("HTTPRequest");
        Dialog = GetNode<AcceptDialog>("AcceptDialog");
        Message = Dialog.GetNode<Label>("Panel/Label");

        Dialog.Confirmed += Dialog_Confirmed;

        CurrentVersion = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "0.0.0";

        Logger.Instance.Debug($"App version: {CurrentVersion}");

        GetGithubLatestRelease();
    }

    private void Dialog_Confirmed()
    {
        OS.ShellOpen(_response.Url);
    }

    private void GetGithubLatestRelease()
    {
        HttpRequest.RequestCompleted += HttpRequest_RequestCompleted;
        HttpRequest.Request($"https://api.github.com/repos/Kakumi/PokeDesktop/releases/latest");
    }

    private void HttpRequest_RequestCompleted(long result, long responseCode, string[] headers, byte[] body)
    {
        if (responseCode != 200)
        {
            Logger.Instance.Error(TranslationServer.Translate("ERROR_GET_RELEASE_VERSION"));
            return;
        }

        try
        {
            string json = Encoding.UTF8.GetString(body);
            _response = JsonSerializer.Deserialize<ReleaseResponse>(json);
            if (CurrentVersion != _response.Name)
            {
                Logger.Instance.Info(string.Format(TranslationServer.Translate("APP_UPDATE_LOG"), _response.Name));
                Message.Text = string.Format(TranslationServer.Translate("APP_UPDATE_MESSAGE"), _response.Name);
                Dialog.Show();
                Dialog.MoveToCenter();
            }
        }
        catch (Exception ex)
        {
            Logger.Instance.Error("");
        }
    }
}