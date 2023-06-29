using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new pokemon")]
public class PokemonBase : ScriptableObject
{
    // Basic Pokemon info
    [SerializeField] string pokemonName;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    // Pokemon Stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] List<LearnableMove> movePool;

    // Properties
    public string Name { get { return pokemonName; } }
    public string Description { get { return description; } }
    public Sprite FrontSprite { get { return frontSprite; } }
    public Sprite BackSprite { get { return backSprite; } }
    public PokemonType Type1 { get { return type1; } }
    public PokemonType Type2 { get { return type2; } }

    public int MaxHp { get { return maxHp; } }
    public int Attack { get { return attack; } }
    public int Defense { get { return defense; } }
    public int SpAttack { get { return spAttack; } }
    public int SpDefense { get { return spDefense; } }
    public int Speed { get { return speed; } }

    public List<LearnableMove> MovePool { get { return movePool; } }
}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase MoveBase { get { return moveBase; } }
    public int Level { get { return level; } }
}

// All pokemon types
public enum PokemonType
{
    None,
    Bug,
    Dark,
    Dragon,
    Electic,
    Fairy,
    Fighting,
    Fire,
    Flying,
    Ghost,
    Grass,
    Ground,
    Ice,
    Normal,
    Poison,
    Psychic,
    Rock,
    Steel,
    Water
}

// Saving stats
public enum BoostableStats
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed,

    // Not active stats, they are only affected by moves and not by stats
    Accuracy,
    Evasion
}

// Chart for type compatibility
public class TypeChart
{
    static float[][] chart =
    {
        //                      Bug   Dar   Dra   Ele   Fai   Fig   Fir   Fly   Gho   Gra   Gro   Ice   Nor   Poi   Psy   Roc   Ste   Wat
        /* Non */ new float[] { 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  },
        /* Bug */ new float[] { 1f  , 2f  , 1f  , 1f  , 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 2f  , 1f  , 1f  , 1f  , 0.5f, 2f  , 1f  , 0.5f, 1f  },
        /* Dar */ new float[] { 1f  , 0.5f, 1f  , 1f  , 0.5f, 0.5f, 1f  , 1f  , 2f  , 1f  , 1f  , 1f  , 1f  , 1f  , 2f  , 1f  , 1f  , 1f  },
        /* Dra */ new float[] { 1f  , 1f  , 2f  , 1f  , 0f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 0.5f, 1f  },
        /* Ele */ new float[] { 1f  , 1f  , 0.5f, 0.5f, 1f  , 1f  , 1f  , 2f  , 1f  , 0.5f, 0f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 2f  },
        /* Fai */ new float[] { 1f  , 2f  , 2f  , 1f  , 1f  , 2f  , 0.5f, 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 0.5f, 1f  , 1f  , 0.5f, 1f  },
        /* Fig */ new float[] { 0.5f, 2f  , 1f  , 1f  , 0.5f, 1f  , 1f  , 0.5f, 0f  , 1f  , 1f  , 2f  , 2f  , 0.5f, 0.5f, 2f  , 2f  , 1f  },
        /* Fir */ new float[] { 2f  , 1f  , 0.5f, 1f  , 1f  , 1f  , 0.5f, 1f  , 1f  , 2f  , 1f  , 2f  , 1f  , 1f  , 1f  , 0.5f, 2f  , 0.5f},
        /* Fly */ new float[] { 2f  , 1f  , 1f  , 0.5f, 1f  , 2f  , 1f  , 1f  , 1f  , 2f  , 1f  , 1f  , 1f  , 1f  , 1f  , 0.5f, 0.5f, 1f  },
        /* Gho */ new float[] { 1f  , 0.5f, 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 2f  , 1f  , 1f  , 1f  , 0f  , 1f  , 2f  , 1f  , 1f  , 1f  },
        /* Gra */ new float[] { 0.5f, 1f  , 0.5f, 1f  , 1f  , 1f  , 0.5f, 0.5f, 1f  , 0.5f, 2f  , 1f  , 1f  , 0.5f, 1f  , 2f  , 0.5f, 2f  },
        /* Gro */ new float[] { 0.5f, 1f  , 1f  , 2f  , 1f  , 1f  , 2f  , 0f  , 1f  , 0.5f, 1f  , 1f  , 1f  , 2f  , 1f  , 2f  , 2f  , 1f  },
        /* Ice */ new float[] { 1f  , 1f  , 2f  , 1f  , 1f  , 1f  , 0.5f, 2f  , 1f  , 2f  , 2f  , 0.5f, 1f  , 1f  , 1f  , 1f  , 0.5f, 0.5f},
        /* Nor */ new float[] { 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 0f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 0.5f, 0.5f, 1f  },
        /* Poi */ new float[] { 1f  , 1f  , 1f  , 1f  , 2f  , 1f  , 1f  , 1f  , 0.5f, 2f  , 0.5f, 1f  , 1f  , 0.5f, 1f  , 0.5f, 0f  , 1f  },
        /* Psy */ new float[] { 1f  , 0f  , 1f  , 1f  , 1f  , 2f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 1f  , 2f  , 0.5f, 1f  , 0.5f, 1f  },
        /* Roc */ new float[] { 2f  , 1f  , 1f  , 1f  , 1f  , 0.5f, 2f  , 2f  , 1f  , 1f  , 0.5f, 2f  , 1f  , 1f  , 1f  , 1f  , 0.5f, 1f  },
        /* Ste */ new float[] { 1f  , 1f  , 1f  , 0.5f, 2f  , 1f  , 0.5f, 1f  , 1f  , 1f  , 1f  , 2f  , 1f  , 1f  , 1f  , 2f  , 0.5f, 0.5f},
        /* Wat */ new float[] { 1f  , 1f  , 0.5f, 1f  , 1f  , 1f  , 2f  , 1f  , 1f  , 0.5f, 2f  , 1f  , 1f  , 1f  , 1f  , 2f  , 1f  , 0.5f}
    };

    public static float GetTypeAdvantage(PokemonType moveToHit, PokemonType firstDefendingType, PokemonType secondDefendingType)
    {
        int row = (int)moveToHit;
        int col1 = (int)firstDefendingType - 1;

        float effectiveness = chart[row][col1];

        if (secondDefendingType != PokemonType.None)
        {
            int col2 = (int)secondDefendingType - 1;
            effectiveness *= chart[row][col2];
        }

        return effectiveness;
    }
}
