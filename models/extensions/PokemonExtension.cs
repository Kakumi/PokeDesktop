using PKHeX.Core;
using System.Collections.Generic;
using System.Linq;

public static class PokemonExtension
{
    public static bool HasType(this PKM pkm, MoveType type)
    {
        return (MoveType)pkm.PersonalInfo.Type1 == type || (MoveType)pkm.PersonalInfo.Type2 == type;
    }

    public static string GetName(this PKM pkm)
    {
        return pkm.IsNicknamed ? pkm.Nickname : GameInfo.Strings.Species.ElementAt(pkm.Species);
    }

    public static MovementType GetMovementType(this PKM pokemon)
    {
        if (pokemon == null) return MovementType.None;

        Species sp = (Species)pokemon.Species;

        var TELEPORTERS = new HashSet<Species>
        {
            Species.Abra,
            Species.Kadabra,
            Species.Alakazam,
            Species.Ralts,
            Species.Kirlia,
            Species.Gardevoir,
            Species.Gallade,
            Species.Natu,
            Species.Xatu,
            Species.Claydol,
            Species.Deoxys,
            Species.Lunala,
            Species.Solgaleo,
            Species.Elgyem,
            Species.Beheeyem,
            Species.Cosmog,
            Species.Cosmoem
        };

        var DIGGERS = new HashSet<Species>
        {
            Species.Sandshrew,
            Species.Sandslash,
            Species.Diglett,
            Species.Dugtrio,
            Species.Onix,
            Species.Steelix,
            Species.Trapinch,
            Species.Vibrava,
            Species.Flygon,
            Species.Gible,
            Species.Gabite,
            Species.Garchomp,
            Species.Hippopotas,
            Species.Hippowdon,
            Species.Drilbur,
            Species.Excadrill,
            Species.Sandile,
            Species.Krokorok,
            Species.Krookodile
        };

        var HOVERERS = new HashSet<Species>
        {
            Species.Magnemite, Species.Magneton, Species.Magnezone,
            Species.Rotom,
            Species.Koffing, Species.Weezing,
            Species.Gastly, Species.Haunter, Species.Gengar,
            Species.Misdreavus, Species.Mismagius,
            Species.Unown,
            Species.Eelektrik, Species.Eelektross,
            Species.Leavanny, Species.Pelipper, Species.Cramorant
        };

        var BOUNCERS = new HashSet<Species>
        {
            Species.Spoink, Species.Grumpig
        };

        var SWIM = new HashSet<Species>
        {
            Species.Gyarados, Species.Mantine
        };

        if (TELEPORTERS.Contains(sp)) return MovementType.Teleport;
        if (DIGGERS.Contains(sp)) return MovementType.Dig;
        if (SWIM.Contains(sp)) return MovementType.Swim;
        if (HOVERERS.Contains(sp)) return MovementType.Fly;
        if (BOUNCERS.Contains(sp)) return MovementType.Bouncing;

        if (HasType(pokemon, MoveType.Flying))
            return MovementType.Fly;

        if (HasType(pokemon, MoveType.Water))
            return MovementType.Swim;

        return MovementType.Walk;
    }
}