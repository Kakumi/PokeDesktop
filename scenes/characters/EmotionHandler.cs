using Godot;
using PKHeX.Core;
using System;
using System.Linq;

public partial class EmotionHandler : Node
{
    [Export] public TextureButton Bubble { get; set; }
    [Export] public Emotion[] Emotions { get; set; }
    [Export(PropertyHint.Range, "0,1,0.1")] public double ShowTrainerEmotion { get; set; } = 0.5;
    [Export(PropertyHint.Range, "0,1,0.1")] public double ShowSleep { get; set; } = 0.8;

    public Timer ClearTimer { get; private set; }
    public RandomTimer EmotionTimer { get; private set; }

    private EmotionType _currentEmotion = EmotionType.None;
    private PKM _pkm;

    private Random _rng = new Random();

    public override void _Ready()
    {
        EmotionTimer = GetNode<RandomTimer>("EmotionTimer");
        ClearTimer = GetNode<Timer>("ClearTimer");

        Bubble.Visible = false;
        Bubble.Pressed += Bubble_Pressed;

        EmotionTimer.Timeout += EmotionTimer_Timeout;
        ClearTimer.Timeout += ClearTimer_Timeout;

        if (SettingsManager.Instance.Settings.ShowEmotion)
        {
            EmotionTimer.StartNewTimer();
        }
    }

    public void Init(PKM pokemon)
    {
        _pkm = pokemon;
    }

    private void EmotionTimer_Timeout()
    {
        GenerateEmotion();

        ClearTimer.Start();
    }

    private void ClearTimer_Timeout()
    {
        Reset();

        EmotionTimer.StartNewTimer();
    }

    private void Bubble_Pressed()
    {
        //_pkm.CurrentFriendship = Math.Max(_pkm.CurrentFriendship + 3, 255);

        Reset();
    }

    private EmotionType GetEmotionTowardsTrainer(byte friendship)
    {
        if (friendship <= 49)
        {
            return EmotionType.Angry;
        }

        if (friendship <= 99)
        {
            return EmotionType.Depressed;
        }

        if (friendship <= 149)
        {
            return EmotionType.Sad;
        }

        if (friendship <= 199)
        {
            return EmotionType.Happy;
        }

        if (friendship <= 255)
        {
            return EmotionType.Love;
        }

        return EmotionType.Happy;
    }

    public void GenerateEmotion()
    {
        if (_pkm == null)
        {
            return;
        }

        double roll = _rng.NextDouble();
        if (roll < ShowTrainerEmotion)
        {
            SetEmotion(GetEmotionTowardsTrainer(_pkm.CurrentFriendship));
        }
        else if (roll < ShowSleep)
        {
            SetEmotion(EmotionType.Sleep);
        }
        else
        {
            SetEmotion(EmotionType.Gift);
        }
    }

    public void SetEmotion(EmotionType type)
    {
        var emotion = Emotions.FirstOrDefault(x => x.Type == type);
        if (emotion != null)
        {
            _currentEmotion = type;
            Bubble.TextureNormal = emotion.Texture;
            Bubble.Visible = true;
        }
        else
        {
            Reset();
        }
    }

    public void Reset()
    {
        _currentEmotion = EmotionType.None;
        Bubble.Visible = false;
        Bubble.TextureNormal = null;
    }
}
