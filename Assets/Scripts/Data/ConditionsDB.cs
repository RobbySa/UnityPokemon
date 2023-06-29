using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionID = kvp.Key;
            var condition = kvp.Value;

            condition.id = conditionID;
        }
    }

    public static Dictionary<ConditionsID, Condition> Conditions { get; set; } = new Dictionary<ConditionsID, Condition>()
    {
        {
            // Conditions
            ConditionsID.psn,
            new Condition()
            {
                conditionColor = Color.magenta,
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHp(pokemon.Hp / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Template.Name} was hurt by poison");
                }
            }
        },
        {
            ConditionsID.brn,
            new Condition()
            {
                conditionColor = new Color(1f,0.53f,0.33f,1f),
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHp(pokemon.Hp / 16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Template.Name} was hurt by its burn");
                }
            }
        },
        {
            ConditionsID.par,
            new Condition()
            {
                conditionColor = Color.yellow,
                Name = "Paralysis",
                StartMessage = "has been paralized",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (Random.Range(1, 5) == 1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Template.Name} is paralysed and can't move");
                        return false;
                    }
                    else
                        return true;
                }
            }
        },
        {
            ConditionsID.frz,
            new Condition()
            {
                conditionColor = new Color(0.36f,0.98f,1f,1f),
                Name = "Freeze",
                StartMessage = "has been frozen",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (Random.Range(1, 6) == 1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Template.Name} is not frozen anymore");
                        return true;
                    }
                    else
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Template.Name} is frozen solid");
                        return false;
                    }
                }
            }
        },
        {
            ConditionsID.slp,
            new Condition()
            {
                conditionColor = new Color(0.67f,0.67f,0.67f,1f),
                Name = "Sleep",
                StartMessage = "has fallen asleep",
                OnStart = (Pokemon pokemon) =>
                {
                    pokemon.StatusTime = Random.Range(1, 4);
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.StatusTime <= 0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Template.Name} woke up!");
                        return true;
                    }
                    else
                    {
                        pokemon.StatusTime--;
                        pokemon.StatusChanges.Enqueue($"{pokemon.Template.Name} is fast asleep");
                        return false;
                    }
                }
            }
        },

        // Volatile Conditions
        {
            ConditionsID.bound,
            new Condition()
            {
                Name = "Bound",
                StartMessage = "became trapped due to the opponent's move",
                OnStart = (Pokemon pokemon) =>
                {
                    pokemon.VolatileStatusesTime.Add(ConditionsID.confusion, Random.Range(4, 6));
                },
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHp(pokemon.Hp / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Template.Name} is still bound");
                }
            }
        },
        {
            ConditionsID.confusion,
            new Condition()
            {
                Name = "Confusion",
                StartMessage = "has been confused",
                OnStart = (Pokemon pokemon) =>
                {
                    pokemon.VolatileStatusesTime.Add(ConditionsID.confusion, Random.Range(1, 5));
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.VolatileStatusesTime[ConditionsID.confusion] <= 0)
                    {
                        pokemon.CureVolatileStatus(ConditionsID.confusion);
                        pokemon.StatusChanges.Enqueue($"{pokemon.Template.Name} snapped out of confusion!");
                        return false;
                    }
                    else
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Template.Name} is confused");
                        if (Random.Range(1, 4) == 1)
                        {
                            pokemon.confusionDamage();
                            pokemon.StatusChanges.Enqueue($"{pokemon.Template.Name} hurt itself in confusion");
                            return true;
                        }
                        return false;
                    }
                }
            }
        },
        {
            ConditionsID.curse,
            new Condition()
            {
                Name = "Curse",
                StartMessage = "has been cursed",
                OnStart = (Pokemon pokemon) =>
                {
                    pokemon.VolatileStatusesTime.Add(ConditionsID.curse, 1000);
                },
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHp(pokemon.Hp / 4);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Template.Name} was by the curse!");
                }
            }
        },
        {
            ConditionsID.embargo,
            new Condition()
            {
                Name = "Embargo",
                StartMessage = "can't use items anymore!",
                OnStart = (Pokemon pokemon) =>
                {
                    pokemon.VolatileStatusesTime.Add(ConditionsID.embargo, 5);
                }
            }
        },
        {
            ConditionsID.encore,
            new Condition()
            {
                Name = "Encore",
                StartMessage = "received an encore!",
                OnStart = (Pokemon pokemon) =>
                {
                    pokemon.VolatileStatusesTime.Add(ConditionsID.encore, Random.Range(2, 6));
                }
            }
        },
        {
            ConditionsID.healBlock,
            new Condition()
            {
                Name = "Heal Block",
                StartMessage = "was prevented from healing!",
                OnStart = (Pokemon pokemon) =>
                {
                    pokemon.VolatileStatusesTime.Add(ConditionsID.healBlock, 5);
                }
            }
        }
    };
}

public enum ConditionsID
{
    // Standard
    none,brn,frz,par,psn,slp,
    // Volatile
    bound, cantEspace, confusion, curse, embargo, encore, flinch, healBlock, infatuation, leechSeed, nightmare, perishSong,
    taunt, telekinesis, torment, aquaRing, bracing, chargingTurn, centerOfAttention, rooting, magneticLevitation,
    magicCoat, minimize, protection, recharging, semiInvulnerable, substitute, withdrawing
}
