using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Pokemon> wildPokemons;

    public Pokemon GetWildEncounter()
    {
        var wildEncounter = wildPokemons[Random.Range(0, wildPokemons.Count)];
        wildEncounter.Init();

        return wildEncounter;
    }
}
