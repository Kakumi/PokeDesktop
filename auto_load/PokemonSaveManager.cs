using Godot;
using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class PokemonSaveManager : Node
{
    [Export] public PackedScene PokemonWindow;

    public static PokemonSaveManager Instance { get; private set; }

    private List<PokemonWindow> _popups;

    private SaveFile _currentSav;
    private Random _rng = new Random();

    public override void _Ready()
    {
        _popups = new List<PokemonWindow>();

        SettingsManager.Instance.SettingsChanged += Instance_SettingsChanged;

        if (SettingsManager.Instance.Loaded)
        {
            Instance_SettingsChanged(SettingsManager.Instance.Settings);
        }
    }

    private void Instance_SettingsChanged(Settings settings)
    {
        foreach (var item in GetChildren().Where(x => x is PokemonWindow))
        {
            item.QueueFree();
        }

        LoadPokemons();
    }

    private void LoadPokemons()
    {
        string path = SettingsManager.Instance.Settings.SaveFilePath;
        if (path == null)
        {
            Logger.Instance.Error(TranslationServer.Translate("ERROR_MISSING_SAVE_FILE"));
            return;
        }

        _currentSav = SaveUtil.GetSaveFile(path);
        if (_currentSav == null)
        {
            Logger.Instance.Error(TranslationServer.Translate("ERROR_INVALID_SAVE_FILE"));
            return;
        }

        Logger.Instance.Debug($"Creating a backup file...");
        try
        {
            File.Copy(SettingsManager.Instance.Settings.SaveFilePath, $"{SettingsManager.Instance.Settings.SaveFilePath}.bak", true);
            Logger.Instance.Debug($"Backup file created!");
        }
        catch (Exception ex)
        {
            Logger.Instance.Error(TranslationServer.Translate("ERROR_SAVE_BACKUP_FILE"), ex);
        }

        Logger.Instance.Info(string.Format(TranslationServer.Translate("RELOADING_SAVE_FILE"), path));
        Logger.Instance.Debug($"Loading {_currentSav.PartyCount} Pokémons for trainer {_currentSav.OT}");
        Logger.Instance.Info(string.Format(TranslationServer.Translate("WELCOME_TRAINER"), _currentSav.OT));

        int max = 1; // SettingsManager.Instance.Settings.MaxVisible;
        for (int i = 0; i < max && i < _currentSav.PartyCount; i++)
        {
            var pkm = _currentSav.PartyData[i];
            Logger.Instance.Debug($"Loading pokémon {pkm.GetName()} with friendship {pkm.CurrentFriendship}");
            _popups.Add(CreatePokemonWindow(new PartyPokemon(i, pkm)));
        }
    }

    private PokemonWindow CreatePokemonWindow(PartyPokemon pokemon)
    {
        var window = PokemonWindow.Instantiate<PokemonWindow>();
        AddChild(window);

        window.Init(pokemon);

        return window;
    }

    public void UpdateFriendship(PartyPokemon partyPokemon, byte amount)
    {
        var newFriendship = (byte)Math.Min(255, partyPokemon.Pokemon.CurrentFriendship + amount);
        if (newFriendship != partyPokemon.Pokemon.CurrentFriendship)
        {
            var message = string.Format(
                TranslationServer.Translate("GIFT_FRIENDSHIP"),
                partyPokemon.Pokemon.GetName(),
                amount,
                partyPokemon.Pokemon.CurrentFriendship,
                newFriendship
            );

            Logger.Instance.Info(message);
            partyPokemon.Pokemon.CurrentFriendship = newFriendship;
            UpdatePokemon(partyPokemon);
        }
        else
        {
            var message = string.Format(
                TranslationServer.Translate("GIFT_FRIENDSHIP_FULL"),
                partyPokemon.Pokemon.GetName()
            );

            Logger.Instance.Info(message);
        }
    }

    public void AddMoney(PartyPokemon pokemon, uint amount)
    {
        var newMoney = (uint)Math.Min(_currentSav.MaxMoney, _currentSav.Money + amount);
        if (_currentSav.Money != newMoney)
        {
            var message = string.Format(
                TranslationServer.Translate("GIFT_MONEY"),
                pokemon.Pokemon.GetName(),
                amount.ToString("C"),
                _currentSav.Money.ToString("C"),
                newMoney.ToString("C")
            );

            Logger.Instance.Info(message);
            _currentSav.Money = newMoney;

            SaveChanges();
        }
        else
        {
            var message = string.Format(
                TranslationServer.Translate("GIFT_MONEY_FULL"),
                pokemon.Pokemon.GetName(),
                amount.ToString("C")
            );

            Logger.Instance.Info(message);
        }
    }

    public void AddRandomItem(PartyPokemon pokemon, int amount)
    {
        string[] items = [.. GameInfo.Strings.GetItemStrings(_currentSav.Context, _currentSav.Version)];
        var pouches = _currentSav.Inventory;
        var allowedCategories = new InventoryType[] {
            InventoryType.TMHMs,
            InventoryType.Items,
            InventoryType.Medicine,
            InventoryType.Balls,
            InventoryType.Candy,
            InventoryType.BattleItems,
            InventoryType.Berries,
            InventoryType.Ingredients
        };

        var allowedPouches = pouches.Where(x => allowedCategories.Contains(x.Type));
        if (allowedPouches.Count() > 0)
        {
            var pouchIndex = _rng.Next(allowedPouches.Count());
            var pouch = pouches[pouchIndex];
            var allowedItems = pouch.GetAllItems().ToArray();
            if (allowedItems.Length > 0)
            {
                var itemIndex = _rng.Next(allowedItems.Length);
                var itemID = allowedItems[itemIndex];

                string item = items[itemID];
                var oldCount = pouch.Items.FirstOrDefault(x => x.Index == itemID)?.Count ?? 0;
                var newCount = pouch.GiveItem(_currentSav, itemID, amount);
                if (newCount > 0)
                {
                    var message = string.Format(
                        TranslationServer.Translate("GIFT_ITEM"),
                        pokemon.Pokemon.GetName(),
                        amount,
                        item,
                        oldCount,
                        newCount
                    );

                    Logger.Instance.Info(message);
                    _currentSav.Inventory = pouches;

                    SaveChanges();
                }
                else
                {
                    var message = string.Format(
                        TranslationServer.Translate("GIFT_ITEM_FULL"),
                        pokemon.Pokemon.GetName(),
                        amount,
                        item
                    );

                    Logger.Instance.Info(message);
                }
            }
        }
    }

    private void UpdatePokemon(PartyPokemon pokemon)
    {
        _currentSav.SetPartySlotAtIndex(pokemon.Pokemon, pokemon.Index);

        SaveChanges();
    }

    private void SaveChanges()
    {
        try
        {
            File.WriteAllBytes(SettingsManager.Instance.Settings.SaveFilePath, _currentSav.Write().ToArray());
            Logger.Instance.Debug($"Changed saved to player's save file.");
        }
        catch (Exception e)
        {
            Logger.Instance.Error(TranslationServer.Translate("ERROR_SAVING_SAVE_FILE"), e);
        }
    }

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
