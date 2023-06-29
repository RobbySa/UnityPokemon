using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new move")]
public class MoveBase : ScriptableObject
{
    // Basic move info
    [SerializeField] string moveName;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] PokemonType type;
    [SerializeField] MoveCategory category;
    [SerializeField] MoveEffect moveEffect;
    [SerializeField] List<SecondaryEffect> secondaryEffects;
    [SerializeField] MoveTarget moveTarget;
    [SerializeField] int priority;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] bool alwaysHit;
    [SerializeField] int pp;

    // Properties
    public string Name { get { return moveName; } }
    public string Description { get { return description; } }
    public PokemonType Type { get { return type; } }
    public MoveCategory Category { get { return category; } }
    public MoveEffect MoveEffect { get { return moveEffect; } }
    public List<SecondaryEffect> SecondaryEffects { get { return secondaryEffects; } }
    public MoveTarget MoveTarget { get { return moveTarget; } }
    public int Priority { get { return priority; } }
    public int Power { get { return power; } }
    public int Accuracy { get { return accuracy; } }
    public bool AlwaysHit { get { return alwaysHit; } }
    public int Pp { get { return pp; } }
}

[System.Serializable]
public class MoveEffect
{
    [SerializeField] List<StatBoost> statBoosts;
    [SerializeField] ConditionsID status;
    [SerializeField] ConditionsID volatileStatus;

    public List<StatBoost> StatBoosts { get { return statBoosts; } }
    public ConditionsID Status { get { return status; } }
    public ConditionsID VolatileStatus { get { return volatileStatus; } }
}

[System.Serializable]
public class SecondaryEffect : MoveEffect
{
    [SerializeField] int percentageOfSuccess;
    [SerializeField] MoveTarget target;

    public int PercentageOfSuccess { get { return percentageOfSuccess; } }
    public MoveTarget Target { get { return target; } }
}

[System.Serializable]
public class StatBoost
{
    public BoostableStats stat;
    public int boostAmount;
}

public enum MoveCategory
{
    Status,
    Physical,
    Special
}

public enum MoveTarget
{
    Enemy,
    AllEnemies,
    Ally,
    AllAllies,
    Self,
    All,
    AllButSelf,
    Any
}