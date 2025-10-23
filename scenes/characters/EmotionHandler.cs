using Godot;
using System;
using System.Linq;

public partial class EmotionHandler : Node
{
    [Export] public TextureButton Bubble { get; set; }
    [Export] public Emotion[] Emotions { get; set; }
    [Export(PropertyHint.Range, "0,1,0.1")] public double ShowTrainerEmotion { get; set; } = 0.5;
    [Export(PropertyHint.Range, "0,1,0.1")] public double ShowSleep { get; set; } = 0.8;
    [Export(PropertyHint.Range, "0,1,0.1")] public double ChanceDropItem { get; set; } = 0.7;
    [Export(PropertyHint.Range, "1,10,1")] public int DropItemMinAmount { get; set; } = 1;
    [Export(PropertyHint.Range, "1,10,1")] public int DropItemMaxAmount { get; set; } = 3;
    [Export(PropertyHint.Range, "0,10000,1")] public int DropMoneyMinAmount { get; set; } = 200;
    [Export(PropertyHint.Range, "0,10000,1")] public int DropMoneyMaxAmount { get; set; } = 2000;
    [Export(PropertyHint.Range, "0,255,1")] public byte FriendshipValue { get; set; } = 3;
    [Export(PropertyHint.Range, "0,255,1")] public byte FriendshipGiftValue { get; set; } = 5;

    public Timer ClearTimer { get; private set; }
    public RandomTimer EmotionTimer { get; private set; }

    private EmotionType _currentEmotion = EmotionType.None;
    private PartyPokemon _pkm;

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

    public void Init(PartyPokemon pokemon)
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
        if (_currentEmotion == EmotionType.Gift)
        {
            PokemonSaveManager.Instance.UpdateFriendship(_pkm, FriendshipGiftValue);

            double roll = _rng.NextDouble();
            if (SettingsManager.Instance.Settings.DropItem && (roll < ChanceDropItem || !SettingsManager.Instance.Settings.DropMoney))
            {
                var amount = _rng.Next(DropItemMinAmount, DropItemMaxAmount);
                PokemonSaveManager.Instance.AddRandomItem(_pkm, amount);
            }
            else if (SettingsManager.Instance.Settings.DropMoney)
            {
                var amount = (uint)_rng.Next(DropMoneyMinAmount, DropMoneyMaxAmount);
                PokemonSaveManager.Instance.AddMoney(_pkm, amount);
            }
        }
        else
        {
            PokemonSaveManager.Instance.UpdateFriendship(_pkm, FriendshipValue);
        }

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

        var hasGift = SettingsManager.Instance.Settings.DropItem || SettingsManager.Instance.Settings.DropMoney;
        double roll = _rng.NextDouble();
        if (roll < ShowTrainerEmotion)
        {
            SetEmotion(GetEmotionTowardsTrainer(_pkm.Pokemon.CurrentFriendship));
        }
        else if (roll < ShowSleep || !hasGift)
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
