using PKHeX.Core;
using System.Collections.Generic;

public static class PokemonExtension
{
    public static bool HasType(this PKM pkm, MoveType type)
    {
        return (MoveType)pkm.PersonalInfo.Type1 == type || (MoveType)pkm.PersonalInfo.Type2 == type;
    }

    public static MovementType GetMovementType(this PKM pokemon)
    {
        if (pokemon == null) return MovementType.None;

        Species sp = (Species)pokemon.Species;

        var TELEPORTERS = new HashSet<Species>
        {
            Species.Abra, Species.Kadabra, Species.Alakazam,
            Species.Ralts, Species.Kirlia, Species.Gardevoir, Species.Gallade,
            Species.Natu, Species.Xatu,
            Species.Elgyem, Species.Beheeyem,
            Species.Cosmog, Species.Cosmoem
        };

        var DIGGERS = new HashSet<Species>
        {
            Species.Diglett, Species.Dugtrio,
            Species.Drilbur, Species.Excadrill,
            Species.Sandshrew, Species.Sandslash,
            Species.Trapinch,
            Species.Onix, Species.Steelix,
            Species.Sandygast, Species.Palossand
        };

        var HOVERERS = new HashSet<Species>
        {
            Species.Magnemite, Species.Magneton, Species.Magnezone,
            Species.Rotom, // (les formes de Rotom sont identifiées par forme, ici on simplifie)
            Species.Koffing, Species.Weezing,
            Species.Gastly, Species.Haunter, Species.Gengar,
            Species.Misdreavus, Species.Mismagius,
            Species.Unown,
            Species.Eelektrik, Species.Eelektross,
            Species.Leavanny
    };

        var AQUATIC_FLYING_PREFER_SWIM = new HashSet<Species>
        {
            Species.Gyarados, Species.Mantine, Species.Pelipper, Species.Cramorant
        };

        if (TELEPORTERS.Contains(sp)) return MovementType.Teleport;
        if (DIGGERS.Contains(sp)) return MovementType.Dig;

        if (HasType(pokemon, MoveType.Flying) && !AQUATIC_FLYING_PREFER_SWIM.Contains(sp))
            return MovementType.Fly;

        if (HOVERERS.Contains(sp))
            return MovementType.Fly;

        if (HasType(pokemon, MoveType.Water))
            return MovementType.Swim;

        return MovementType.Walk;
    }
}