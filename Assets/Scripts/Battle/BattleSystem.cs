using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogueBox dialogueBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;
    
    int currentAction = 0;
    int currentMove = 0;
    int currentMember = 0;
    int attemptToEscape = 0;
    int numberOfAttacksLeftThisTurn = 2;

    BattleState battleState;
    PokemonParty playerParty;
    Pokemon wildPokemon;

    /*--------------------------------------------------------- Set Up the battle ---------------------------------------------------------*/
    /*-------------------------------------------------------------------------------------------------------------------------------------*/
    // Start is called before the first frame update
    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetupBattle());
    }

    // Sets up the battle at the beginning only
    public IEnumerator SetupBattle()
    {
        playerUnit.Setup(playerParty.GetFirstHealthyPokemon());
        enemyUnit.Setup(wildPokemon);

        partyScreen.Init();

        dialogueBox.SetMoveNames(playerUnit.Pokemon.Moves);

        yield return dialogueBox.TypeDialogue($"A wild {enemyUnit.Pokemon.Template.Name} appeared!");
        yield return new WaitForSeconds(1f);

        PlayerActionState();
    }
    /*-------------------------------------------------------------------------------------------------------------------------------------*/

    /*-------------------------------------------- React to user inputs and displays the battle -------------------------------------------*/
    // Update the layout and listens to user input
    public void HandleUpdate()
    {
        if (battleState == BattleState.PlayerAction)
            HandleActionSelection();
        else if (battleState == BattleState.PlayerMove)
            HandleMoveSelection();
        else if (battleState == BattleState.PartyScreen)
            HandlePartyScreenSelection();
    }

    void BattleOver(bool won)
    {
        battleState = BattleState.BattleOver;
        playerParty.PokemonPartyMembers.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    void PlayerActionState()
    {
        battleState = BattleState.PlayerAction;
        dialogueBox.SetDialogue("Select an action.");
        dialogueBox.EnableActionSelector(true);
    }

    void OpenPartyScreen()
    {
        battleState = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.PokemonPartyMembers);
        partyScreen.gameObject.SetActive(true);
    }

    void PlayerMove()
    {
        battleState = BattleState.PlayerMove;
        dialogueBox.EnableActionSelector(false);
        dialogueBox.EnableDialogueText(false);

        dialogueBox.EnableMoveSelector(true);
    }
    /*-------------------------------------------------------------------------------------------------------------------------------------*/

    IEnumerator AttemptEscape()
    {
        battleState = BattleState.DisplayTurn;
        bool escapeSuccess = false;

        // some abilities or moves prevent a pokemon from escaping
        if (playerUnit.Pokemon.Speed > enemyUnit.Pokemon.Speed && playerUnit.Pokemon.CanSwitch())
            escapeSuccess = true;
        else
        {
            int random = UnityEngine.Random.Range(0, 255);
            int formulaToEscape = Mathf.FloorToInt((playerUnit.Pokemon.Speed * 128 / enemyUnit.Pokemon.Speed) + (30 * attemptToEscape));

            if (random < formulaToEscape)
                escapeSuccess = true;
            else
                attemptToEscape++;
        }

        if (escapeSuccess)
        {
            yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Template.Name} fled successfully.");
            BattleOver(true);
        }
        else
        {
            yield return dialogueBox.TypeDialogue($"{playerUnit.Pokemon.Template.Name} failed to flee.");

            numberOfAttacksLeftThisTurn = 1;
            yield return PerformMove(enemyUnit, enemyUnit.Pokemon.GetRandomMove(), playerUnit, null);
        }
    }

    // Both pokemon are attacking
    IEnumerator PlayTurn()
    {
        numberOfAttacksLeftThisTurn = 2;

        // Set up moves
        var playerMove = playerUnit.Pokemon.Moves[currentMove];
        var enemyMove = enemyUnit.Pokemon.GetRandomMove();

        // Check who goes first
        bool playerGoesFirst;

        if (playerMove.MoveBase.Priority > enemyMove.MoveBase.Priority)
            playerGoesFirst = true;
        else if (playerMove.MoveBase.Priority < enemyMove.MoveBase.Priority)
            playerGoesFirst = false;
        else if (playerUnit.Pokemon.Speed != enemyUnit.Pokemon.Speed)
            playerGoesFirst = playerUnit.Pokemon.Speed > enemyUnit.Pokemon.Speed;
        else {
            if (UnityEngine.Random.Range(0, 100) >= 50)
                playerGoesFirst = true;
            else
                playerGoesFirst = false;
        }

        if (playerGoesFirst)
            yield return PerformMove(playerUnit, playerUnit.Pokemon.Moves[currentMove], enemyUnit, enemyUnit.Pokemon.GetRandomMove());
        else
            yield return PerformMove(enemyUnit, enemyUnit.Pokemon.GetRandomMove(), playerUnit, playerUnit.Pokemon.Moves[currentMove]);
    }

    // Performs the turn based on the choice of moves for both the sides of the battle
    IEnumerator PerformMove(BattleUnit attackerUnit, Move attackerMove, BattleUnit defenderUnit, Move defenderMove)
    {
        battleState = BattleState.DisplayTurn;
        numberOfAttacksLeftThisTurn--;

        // Volatile counters needs to be decreased regardless
        foreach (var kvp in attackerUnit.Pokemon.VolatileStatuses)
            attackerUnit.Pokemon.VolatileStatusesTime[kvp.Key]--;

        // Check for conditions that might cause the pokemon to loose its turn
        bool canAttackStatus = attackerUnit.Pokemon.OnBeforeMove();
        if (!canAttackStatus)
            yield return ShowStatusChanges(attackerUnit.Pokemon);
        else
        {
            // Check for volatile conditions that might cause the pokemon to loose its turn
            bool volatileStatusTrigger = false;
            if (attackerUnit.Pokemon.VolatileStatuses.Count > 0)
            {
                // Avoid errors when modifying dictionary in real time
                Dictionary<ConditionsID, Condition> copyOfDictionary = new Dictionary<ConditionsID, Condition>(attackerUnit.Pokemon.VolatileStatuses);
                foreach (var kvp in copyOfDictionary)
                {
                    bool volatileEffect = attackerUnit.Pokemon.OnBeforeMoveVolatile(kvp.Key);

                    if (volatileEffect)
                        volatileStatusTrigger = true;
                }
            }

            if (volatileStatusTrigger)
            {
                yield return ShowStatusChanges(attackerUnit.Pokemon);
                yield return attackerUnit.Hud.UpdateHp();
            }
            else
            {
                yield return ShowStatusChanges(attackerUnit.Pokemon);

                attackerMove.Pp--;

                yield return dialogueBox.TypeDialogue($"{attackerUnit.Pokemon.Template.Name} used {attackerMove.MoveBase.Name}.");
                attackerUnit.PlayAttackAnimation();

                if (CheckIfMoveHits(attackerMove, attackerUnit, defenderUnit))
                {
                    defenderUnit.PlayHitAnimation();

                    // Status move
                    if (attackerMove.MoveBase.Category == MoveCategory.Status)
                    {
                        yield return PerformMoveEffect(attackerMove.MoveBase.MoveEffect, attackerMove.MoveBase.MoveTarget, attackerUnit, defenderUnit);
                    }
                    // Damaging move
                    else
                    {
                        // Some damaging move have secondary effects that might trigger
                        yield return PerformDamagingMove(attackerMove, attackerUnit, defenderUnit);
                    }
                    yield return PerformSecondaryMoveEffect(attackerMove, attackerUnit, defenderUnit);
                }
                else
                    yield return dialogueBox.TypeDialogue($"{attackerUnit.Pokemon.Template.Name}'s attack missed.");
            }
        }

        if (defenderUnit.Pokemon.CurrentHp <= 0)
        {
            yield return dialogueBox.TypeDialogue($"{defenderUnit.Pokemon.Template.Name} Fainted.");
            defenderUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);

            CheckForBattleOver(defenderUnit);
        }
        else if (numberOfAttacksLeftThisTurn > 0)
            yield return PerformMove(defenderUnit, defenderMove, attackerUnit, attackerMove);
        else
        {
            // Damage by status here
            yield return StatusDamageAtEndOfTurn(enemyUnit);
            yield return StatusDamageAtEndOfTurn(playerUnit);
            PlayerActionState();
        }
    }

    // Check if the battle is over when a pokemon faints
    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit == enemyUnit)
            BattleOver(true);
        else
        {
            var nextPokemon = playerParty.GetFirstHealthyPokemon();
            if (nextPokemon != null)
                OpenPartyScreen();
            else
                BattleOver(false);
        }
    }

    bool CheckIfMoveHits(Move move, BattleUnit attackerUnit, BattleUnit defenderUnit)
    {
        if (move.MoveBase.AlwaysHit)
            return true;
        else
        {
            float chanceToHit = move.MoveBase.Accuracy;

            int accuracy = attackerUnit.Pokemon.StatBoosts[BoostableStats.Accuracy];
            int evasion = defenderUnit.Pokemon.StatBoosts[BoostableStats.Evasion];

            var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

            if (accuracy > 0)
                chanceToHit *= boostValues[accuracy];
            else
                chanceToHit /= boostValues[-accuracy];

            if (evasion > 0)
                chanceToHit /= boostValues[evasion];
            else
                chanceToHit *= boostValues[-evasion];

            return UnityEngine.Random.Range(1, 100) < chanceToHit;
        }
    }

    // Deals with status moves, encapsulating this makes it easy to modify in the future
    IEnumerator PerformMoveEffect(MoveEffect effects, MoveTarget target, BattleUnit attackerUnit, BattleUnit defenderUnit)
    {
        // Stat Boosts
        if (effects.StatBoosts != null)
        {
            if (target == MoveTarget.Self)
                attackerUnit.Pokemon.ApplyBoosts(effects.StatBoosts);
            else
                defenderUnit.Pokemon.ApplyBoosts(effects.StatBoosts);
        }
        
        // Conditions
        if (effects.Status != ConditionsID.none)
            defenderUnit.Pokemon.SetStatus(effects.Status);
        // Volatile conditions
        if (effects.VolatileStatus != ConditionsID.none)
            defenderUnit.Pokemon.SetVolatileStatus(effects.VolatileStatus);

        yield return ShowStatusChanges(attackerUnit.Pokemon);
        yield return ShowStatusChanges(defenderUnit.Pokemon);
    }

    IEnumerator PerformSecondaryMoveEffect(Move move, BattleUnit attackerUnit, BattleUnit defenderUnit)
    {
        var effects = move.MoveBase.SecondaryEffects;

        // loop through every secondary effect in a similar manner to a status move
        foreach (var effect in effects)
        {
            if (UnityEngine.Random.Range(1, 100) < effect.PercentageOfSuccess)
            {
                yield return PerformMoveEffect(effect, effect.Target, attackerUnit, defenderUnit);
            }
        }
    }

    // Deals damage to opponent pokemon
    IEnumerator PerformDamagingMove(Move move, BattleUnit attackerUnit, BattleUnit defenderUnit)
    {
        var damageDetails = defenderUnit.Pokemon.TakeDamage(move, attackerUnit.Pokemon);
        yield return defenderUnit.Hud.UpdateHp();
        yield return ShowDamageDetails(damageDetails);
    }

    // Displays damage details to the dialogue box
    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogueBox.TypeDialogue("A Critical Hit!");
        if (damageDetails.TypeAdvantage > 1f)
            yield return dialogueBox.TypeDialogue("It's supereffective!");
        else if (damageDetails.TypeAdvantage < 1f)
            yield return dialogueBox.TypeDialogue("It's not that effective.");
    }

    // Displays the stats changes in the dialogue box
    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogueBox.TypeDialogue(message);
        }
    }

    // Applies damage dealth by status conditions
    IEnumerator StatusDamageAtEndOfTurn(BattleUnit unit)
    {
        unit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(unit.Pokemon);
        yield return unit.Hud.UpdateHp();
        if (unit.Pokemon.CurrentHp <= 0)
        {
            yield return dialogueBox.TypeDialogue($"{unit.Pokemon.Template.Name} Fainted.");
            unit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);

            CheckForBattleOver(unit);
        }
    }

    /*------------------------------------------------------------ User Inputs -------------------------------------------------------------*/
    // Handles the user input while choosing an action in battle
    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
            if (currentAction <= 1)
                currentAction += 2;
        if (Input.GetKeyDown(KeyCode.UpArrow))
            if (currentAction >= 2)
                currentAction -= 2;
        if (Input.GetKeyDown(KeyCode.RightArrow))
            if ((currentAction % 2) == 0)
                currentAction += 1;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            if ((currentAction % 2) == 1)
                currentAction -= 1;

        dialogueBox.UpdateActionSelector(currentAction);

        // Current Action has been selected
        if (Input.GetKeyDown(KeyCode.A))
        {
            // Fight was selected
            if (currentAction == 0)
                PlayerMove();
            if (currentAction == 1)
            {
                // Bag was selected
            }
            if (currentAction == 2)
            {
                // Party was selected and the current pokemon can switch
                if (playerUnit.Pokemon.CanSwitch())
                    OpenPartyScreen();
            }
            if (currentAction == 3)
                // Run was selected
                StartCoroutine(AttemptEscape());
        }
    }

    // Handle the user input while choosing a move in battle
    void HandleMoveSelection()
    {
        if (playerUnit.Pokemon.VolatileStatuses.ContainsKey(ConditionsID.encore))
            StartCoroutine(PlayTurn());
        else
        {
            if (Input.GetKeyDown(KeyCode.DownArrow) && (currentMove < playerUnit.Pokemon.Moves.Count - 2))
                if (currentMove <= 1)
                    currentMove += 2;
            if (Input.GetKeyDown(KeyCode.UpArrow))
                if (currentMove >= 2)
                    currentMove -= 2;
            if (Input.GetKeyDown(KeyCode.RightArrow) && (currentMove < playerUnit.Pokemon.Moves.Count - 1))
                if ((currentMove % 2) == 0)
                    currentMove += 1;
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                if ((currentMove % 2) == 1)
                    currentMove -= 1;

            dialogueBox.UpdateMoveSelector(currentMove, playerUnit.Pokemon.Moves[currentMove]);

            if (Input.GetKeyDown(KeyCode.A) && (playerUnit.Pokemon.Moves[currentMove].Pp > 0))
            {
                dialogueBox.EnableMoveSelector(false);
                dialogueBox.EnableDialogueText(true);

                //StartCoroutine(PerformPlayerMove());
                StartCoroutine(PlayTurn());
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                dialogueBox.EnableMoveSelector(false);
                dialogueBox.EnableDialogueText(true);
                PlayerActionState();
            }
        }
    }

    // Gives control for the player of the status screen
    void HandlePartyScreenSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow) && (currentMember < playerParty.PokemonPartyMembers.Count - 2))
            if (currentMember <= 3)
                currentMember += 2;
        if (Input.GetKeyDown(KeyCode.UpArrow))
            if (currentMember >= 2)
                currentMember -= 2;
        if (Input.GetKeyDown(KeyCode.RightArrow) && (currentMember < playerParty.PokemonPartyMembers.Count - 1))
            if ((currentMember % 2) == 0)
                currentMember += 1;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            if ((currentMember % 2) == 1)
                currentMember -= 1;

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.A))
        {
            var selectedMember = playerParty.PokemonPartyMembers[currentMember];
            if (selectedMember.CurrentHp <= 0)
                partyScreen.SetMessageText("You cannot send out a fainted pokemon!");
            else if (selectedMember == playerUnit.Pokemon)
                partyScreen.SetMessageText("This pokemon is already on the battlefield!");
            else
            {
                partyScreen.gameObject.SetActive(false);
                battleState = BattleState.DisplayTurn;

                StartCoroutine(SwitchPokemon(selectedMember));
            }
        }
        else if (Input.GetKeyDown(KeyCode.S) && (playerUnit.Pokemon.CurrentHp > 0))
        {
            partyScreen.gameObject.SetActive(false);
            PlayerActionState();
        }
    }

    // Can only be called from the party screen and switches out pokemon if possible
    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        var previousPokemon = playerUnit.Pokemon;

        if (previousPokemon.CurrentHp > 0)
        {
            yield return dialogueBox.TypeDialogue($"Come back {playerUnit.Pokemon.Template.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(1f);
        }

        playerUnit.Setup(newPokemon);

        dialogueBox.SetMoveNames(newPokemon.Moves);
        yield return dialogueBox.TypeDialogue($"Go {newPokemon.Template.Name}!");
        yield return new WaitForSeconds(1f);

        // Reset current move since we cannot know how many moves a pokemon has
        currentMove = 0;

        if (previousPokemon.CurrentHp > 0)
        {
            numberOfAttacksLeftThisTurn = 1;
            yield return PerformMove(enemyUnit, enemyUnit.Pokemon.GetRandomMove(), playerUnit, playerUnit.Pokemon.Moves[0]);
        }
        else
            PlayerActionState();
    }
}

// Store every battle state
public enum BattleState
{
    Start,
    PlayerAction,
    PlayerMove,
    DisplayTurn,
    PartyScreen,
    BattleOver
}