# PokéDesktop

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![GitHub release](https://img.shields.io/github/v/release/Kakumi/PokeDesktop?include_prereleases)](https://github.com/Kakumi/PokeDesktop/releases)
[![Open Issues](https://img.shields.io/github/issues/Kakumi/PokeDesktop)](https://github.com/Kakumi/PokeDesktop/issues)
[![Godot](https://img.shields.io/badge/Godot-4.5-478CBF?logo=godot-engine&logoColor=white)](https://godotengine.org/)
[![C#](https://img.shields.io/badge/C%23-.NET-blueviolet?logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Discord](https://img.shields.io/discord/1178966789477126176?label=Discord&logo=discord&logoColor=white&color=5865F2)](https://discord.gg/wvQKYmuMnK)

<a href="https://discord.gg/wvQKYmuMnK">
  <img src="https://discordapp.com/api/guilds/1178966789477126176/widget.png?style=banner2" alt="Discord server">
</a>

---

## 🧩 Short Description

**PokéDesktop** is a “pet-desktop” style application built with **Godot 4.5 (C#)** that brings your Pokémon team to life on your computer desktop.  
Using your game save (compatible up to **Pokémon Scarlet/Violet**), your Pokémon walk across your taskbar, express emotions, and can even gift you random items or money. Interacting with them increases their friendship and strengthens your bond!

▶️ **[Watch the PokéDesktop Demo on YouTube](https://www.youtube.com/watch?v=ipPdtFGySok)**

---

## 📚 Table of Contents

* [Dependencies](#dependencies)  
* [Features](#features)  
	* [Languages](#languages)  
	* [Movements](#movements)
	* [Show Player Party Pokémon](#show-player-party-pokémon)  
	* [Emotions System](#emotions-system)  
	* [Improve Friendship](#improve-friendship)  
	* [Drop System](#drop-system)  
	* [Images](#images)  
* [Todo & Ideas](#todo-ideas)  
* [Sources](#sources)  
* [License](#license)

---

## <a name="dependencies"></a>🧱 Dependencies

- **.NET 8**  
- **Godot Engine 4.5 (C# support enabled)**  
- **PKHeX.Core** (for reading/saving player save data)

---

## <a name="features"></a>✨ Features

### <a name="languages"></a>Languages
**French** and **English** are currently supported.

### <a name="movements"></a>Movements
By default, Pokémon use one of several available smart movement systems depending on their type or behavior:

* Walk – Standard ground movement (default)
* Fly / Swim – The Pokémon hovers or floats (e.g. Rayquaza, Gyarados)
* Teleport – The Pokémon teleports from place to place (e.g. Abra)
* Bounce – The Pokémon walks and bounces (e.g. Spoink, Grumpig)
* Dig – For ground-dwelling Pokémon (e.g. Diglett)

You can disable smart movement entirely to make all Pokémon use simple walking instead.

### <a name="show-player-party-pokémon"></a>Show Player Party Pokémon
Displays the Pokémon from the player's current save file directly on the desktop/taskbar. (Between 1 to 6 Pokémon)

### <a name="emotions-system"></a>Emotions System
Pokémon periodically display emotions based on their friendship level.  

| Image | Emotion | Friendship Range | Description |
|----|----------|------------------|--------------|
| ![Angry emotion](/assets/images/emotions/angry/angry_3.png) | 😠 **Angry** | 0–49 | Upset Pokémon |
| ![Depressed emotion](/assets/images/emotions/depressed/depressed_3.png) | 😞 **Depressed** | 50–99 | Sad or unmotivated |
| ![Sad emotion](/assets/images/emotions/sad/sad_3.png) | 😢 **Sad** | 100–149 | Slightly unhappy |
| ![Happy emotion](/assets/images/emotions/happy/happy_3.png) | 😊 **Happy** | 150–199 | Cheerful and playful |
| ![Love emotion](/assets/images/emotions/love/love_3.png) | 💖 **Love** | 200–255 (max) | Maximum friendship and affection |
| ![Sleep emotion](/assets/images/emotions/sleep/sleep_3.png) | 💤 **Sleep** | Random | Occasionally takes a nap |
| ![Angry emotion](/assets/images/emotions/gift/gift_2.png) | 🎁 **Gift (Lucky)** | Random | Gives a random item or money |

### <a name="improve-friendship"></a>Improve Friendship
Clicking on a Pokémon emotion increases its friendship by **+3**.

### <a name="drop-system"></a>Drop System
Clicking while gift emotion is played will also trigger a **drop** event:
- +5 friendship boost  
- Random loot (item or money)
	- Money is between 200 and 2000
	- Item can be TMHMs, Items, Medicine, Balls, Candy, BattleItems, Berries or Ingredients between 1 and 3

### <a name="images"></a>🖼️ Images
You can currently choose between **animated Pokémon** (default) or a **pixel art version** — both available in the application settings.

---

## <a name="todo-ideas"></a>📝 Todo & Ideas

- [ ] Click on Pokémon to move it somewhere else
- [ ] Multi-monitor Pokémon roaming 
- [ ] Expand emotion pool with more expressions  
- [ ] Start the app on startup

---

## <a name="sources"></a>📦 Sources

- [Godot Engine](https://godotengine.org/)  
- [PKHeX.Core](https://github.com/kwsch/PKHeX)

---

## <a name="license"></a>📄 License

**PokéDesktop** is distributed under the **MIT License** — you may use it freely for any purpose permitted under the license.  
See [LICENSE](LICENSE) for details.

---

> _“Bring your Pokémon to life — right on your desktop!”_