using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Pokemon
{
    //////// VARS ////////
    [SerializeField] PokemonBase template;
    [SerializeField] int level;

    public PokemonBase Template { get { return template; } }
    public int Level { get { return level; } }

    public int CurrentHp { get; set; }
    public List<Move> Moves { get; set; }

    public Dictionary<BoostableStats, int> Stats { get; private set; }
    public Dictionary<BoostableStats, int> StatBoosts{ get; private set; }

    public Condition Status { get; private set; }
    public int StatusTime { get; set; }
    public Dictionary<ConditionsID, Condition> VolatileStatuses { get; set; }
    public Dictionary<ConditionsID, int> VolatileStatusesTime { get; set; }

    public Queue<string> StatusChanges { get; private set; } = new Queue<string>();
    public bool HpChanged { get; set; }

    public event System.Action OnStatusChanged;

    public Move previousMoveUsed;

    //////// FUNCTIONS ////////
    public void Init()
    {
        // Add the 4 most recent moves upon creation
        Moves = new List<Move>();
        for(int move = Template.MovePool.Count - 1; move >= 0; move--)
        {
            if (Template.MovePool[move].Level <= Level)
                Moves.Add(new Move(Template.MovePool[move].MoveBase));
            if (Moves.Count >= 4)
                break;
        }

        CalculateStats();
        CurrentHp = Hp;

        ResetStatBoosts();
        Status = null;
        VolatileStatuses = new Dictionary<ConditionsID, Condition>();
        VolatileStatusesTime = new Dictionary<ConditionsID, int>();
    }

    // Calculate the stats on pokemon creation
    void CalculateStats()
    {
        Stats = new Dictionary<BoostableStats, int>();
        Stats.Add(BoostableStats.Attack, Mathf.FloorToInt((2 * Template.Attack * Level / 100) + 5));
        Stats.Add(BoostableStats.Defense, Mathf.FloorToInt((2 * Template.Defense * Level / 100) + 5));
        Stats.Add(BoostableStats.SpAttack, Mathf.FloorToInt((2 * Template.SpAttack * Level / 100) + 5));
        Stats.Add(BoostableStats.SpDefense, Mathf.FloorToInt((2 * Template.SpDefense * Level / 100) + 5));
        Stats.Add(BoostableStats.Speed, Mathf.FloorToInt((2 * Template.Speed * Level / 100) + 5));

        Hp = Mathf.FloorToInt((2 * Template.MaxHp * Level / 100) + Level + 10);
    }

    void ResetStatBoosts()
    {
        StatBoosts = new Dictionary<BoostableStats, int>
        {
            { BoostableStats.Attack, 0 },
            { BoostableStats.Defense, 0 },
            { BoostableStats.SpAttack, 0 },
            { BoostableStats.SpDefense, 0 },
            { BoostableStats.Speed, 0 },
            { BoostableStats.Accuracy, 0 },
            { BoostableStats.Evasion, 0 }
        };
    }

    // Get stats together with the current modifier
    int GetStat(BoostableStats stat)
    {
        int statValue = Stats[stat];

        // Apply bonuses
        int boost = StatBoosts[stat];
        var boostValue = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost > 0)
            statValue = Mathf.FloorToInt(statValue * boostValue[boost]);
        else
            statValue = Mathf.FloorToInt(statValue / boostValue[-boost]);

        // Paralysis status halves speed
        if (stat == BoostableStats.Speed && Status?.id == ConditionsID.par)
            statValue = Mathf.FloorToInt(statValue / 2);

        return statValue;
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boostAmount;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if (boost == 1)
                StatusChanges.Enqueue($"{Template.Name}'s {stat} rose!");
            else if (boost > 1)
                StatusChanges.Enqueue($"{Template.Name}'s {stat} sharply rose!");
            else if (boost == -1)
                StatusChanges.Enqueue($"{Template.Name}'s {stat} fell!");
            else if (boost < -1)
                StatusChanges.Enqueue($"{Template.Name}'s {stat} sharply fell!");
        }
    }

    // Properties
    public int Hp { get; private set; }
    public int Attack { get { return GetStat(BoostableStats.Attack); } }
    public int Defense { get { return GetStat(BoostableStats.Defense); } }
    public int SpAttack { get { return GetStat(BoostableStats.SpAttack); } }
    public int SpDefense { get { return GetStat(BoostableStats.SpDefense); } }
    public int Speed { get { return GetStat(BoostableStats.Speed); } }

    // Make pokemon suffer damage from incoming move
    public DamageDetails TakeDamage(Move move, Pokemon attacker)
    {
        // Modifiers
        float Randomness = UnityEngine.Random.Range(0.85f, 1f);
        float CriticalHit = (UnityEngine.Random.value * 100f <= 6.25f) ? 1.5f : 1f;
        float TypeModifier = TypeChart.GetTypeAdvantage(move.MoveBase.Type, Template.Type1, Template.Type2);
        float StabModifier = ((move.MoveBase.Type == attacker.Template.Type1) || (move.MoveBase.Type == attacker.Template.Type2)) ? 1.5f : 1f;

        // Stats to be used, default are the physical stats
        int attackStat = (move.MoveBase.Category == MoveCategory.Physical) ? attacker.Attack : attacker.SpAttack;
        int defenseStat = (move.MoveBase.Category == MoveCategory.Physical) ? Defense : SpDefense;

        // Damage details to be returned
        var damageDetails = new DamageDetails()
        {
            Fainted = false,
            Critical = CriticalHit,
            TypeAdvantage = TypeModifier
        };

        float damageTaken = (((2 * attacker.Level / 5) + 2) * attackStat * move.MoveBase.Power / (50 * defenseStat) + 2) * StabModifier * TypeModifier * Randomness * CriticalHit;

        // Burn halves the damage dealth by physical moves
        if (attacker.Status?.id == ConditionsID.brn && move.MoveBase.Category == MoveCategory.Physical)
            damageTaken /= 2;

        UpdateHp(Mathf.FloorToInt(damageTaken));
        
        return damageDetails; 
    }

    // Pokemon it itself in confusion
    public void confusionDamage()
    {
        float Randomness = UnityEngine.Random.Range(0.85f, 1f);

        float damageTaken = (((2 * level / 5) + 2) * Attack * 40 / (50 * Defense) + 2) * Randomness;

        // Burn halves the damage dealth by confusion damage
        if (Status?.id == ConditionsID.brn)
            damageTaken /= 2;

        UpdateHp(Mathf.FloorToInt(damageTaken));
    }

    // At the end of the turn update hp due to status
    public void UpdateHp(int damage)
    {
        CurrentHp = Mathf.Clamp(CurrentHp - damage, 0, Hp);
        HpChanged = true;
    }

    // Get a random move from the enemy pokemon
    public Move GetRandomMove()
    {
        var stillAvailableMoves = Moves.Where(x => x.Pp > 0).ToList();

        int r = UnityEngine.Random.Range(0, stillAvailableMoves.Count);
        return stillAvailableMoves[r];
    }

    // Returns if the pokemon can switch
    public bool CanSwitch()
    {
        bool canSwitch = true;

        if (VolatileStatuses.ContainsKey(ConditionsID.bound) && Template.Type1 != PokemonType.Ghost && Template.Type2 != PokemonType.Ghost)
            canSwitch = false;
        else if (VolatileStatuses.ContainsKey(ConditionsID.cantEspace) && Template.Type1 != PokemonType.Ghost && Template.Type2 != PokemonType.Ghost)
            canSwitch = false;
        else if (VolatileStatuses.ContainsKey(ConditionsID.rooting) && Template.Type1 != PokemonType.Ghost && Template.Type2 != PokemonType.Ghost)
            canSwitch = false;

        return canSwitch;
    }

    //////// Status related functions ////////
    public void SetStatus(ConditionsID conditionID)
    {
        // Cannot have more than one status at a time
        if (Status != null)
            return;

        Status = ConditionsDB.Conditions[conditionID];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Template.Name} {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    public void SetVolatileStatus(ConditionsID conditionID)
    {
        // Cannot inflict a volatile status already present on the pokemon
        if (VolatileStatuses.ContainsKey(conditionID))
            return;

        VolatileStatuses[conditionID] = ConditionsDB.Conditions[conditionID];
        VolatileStatuses[conditionID].OnStart(this);

        StatusChanges.Enqueue($"{Template.Name} {VolatileStatuses[conditionID].StartMessage}");
    }

    public void CureVolatileStatus(ConditionsID conditionID)
    {
        VolatileStatuses.Remove(conditionID);
        VolatileStatusesTime.Remove(conditionID);
    }

    public bool OnBeforeMove()
    {
        if (Status?.OnBeforeMove != null)
        {
            return Status.OnBeforeMove(this);
        }

        return true;
    }

    public bool OnBeforeMoveVolatile(ConditionsID conditionID)
    {
        if (VolatileStatuses[conditionID].OnBeforeMove != null)
        {
            return VolatileStatuses[conditionID].OnBeforeMove(this);
        }

        return true;
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
    }

    //////// end ////////

    // Resets Stat boosts when the battle ends
    public void OnBattleOver()
    {
        VolatileStatuses?.Clear();
        ResetStatBoosts();
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeAdvantage { get; set; }
}