using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera mainCamera;

    GameState gameState;

    private void Awake()
    {
        ConditionsDB.Init();
    }

    private void Start()
    {
        playerController.OnEncauntered += StartBattle;
        battleSystem.OnBattleOver += EndBattle;

        DialogueManager.Instance.OnStartDialogue += () =>
        {
            gameState = GameState.Dialogue;
        };
        DialogueManager.Instance.OnEndDialogue += () =>
        {
            if(gameState == GameState.Dialogue)
                gameState = GameState.Freeroam;
        };
    }

    private void Update()
    {
        if (gameState == GameState.Freeroam)
        {
            playerController.HandleUpdate();
        }
        else if (gameState == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (gameState == GameState.Dialogue)
        {
            DialogueManager.Instance.HandleUpdate();
        }
    }

    void StartBattle()
    {
        //playerController.FadeOut();

        gameState = GameState.Battle;

        battleSystem.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetWildEncounter();

        battleSystem.StartBattle(playerParty, wildPokemon);
    }

    void EndBattle(bool victory)
    {
        gameState = GameState.Freeroam;
        mainCamera.gameObject.SetActive(true);
        battleSystem.gameObject.SetActive(false);
    }
}

// Game states
public enum GameState
{
    Freeroam,
    Dialogue,
    Battle
}
