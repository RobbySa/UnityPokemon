using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemonParty;

    public List<Pokemon> PokemonPartyMembers { get { return pokemonParty; } }

    public void Start()
    {
        foreach (var pokemon in pokemonParty)
            pokemon.Init();
    }

    // Return a healthy pokemon if found
    public Pokemon GetFirstHealthyPokemon()
    {
        foreach (var pokemon in pokemonParty)
            if (pokemon.CurrentHp > 0)
                return pokemon;

        return null;
    }
}
