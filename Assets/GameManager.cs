﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameManager : MonoBehaviour {

    public GameSetting GameSettings;

    public PlayerController player;

    public Enemy enemy;

    public int turnNumber;

    public CardSpawner cardSpawner;

    // Dialog for summoning and upgrades
    public GameObject exorcismDialog;

    public GameEvent DisablePlayerInteractionEvent;

    public GameEvent EnablePlayerInteractionEvent;

    // List of all the cards
    private List<Card> allCards;

    // List of cards by their initiation, 
    // if something dies it should be removed from here
    private List<Card> initiationList;

    private List<Card> nextInitiationListQueue;

    private Card activeCard;

    // Use this for initialization
    void Start()
    {
        initiationList = new List<Card>();
        nextInitiationListQueue = new List<Card>();
        allCards = new List<Card>();

        // Start with the player disabled
        DisablePlayerInteractions();

        // Get next level
        Level nextLevel = GameSettings.AllLevels[GameSettings.CurrentLevelIndex];

        // Initialize the player
        player.SetState(GameSettings.PlayerState);
        player.Enemy = enemy;
        enemy.Enemy = player;
        
        // Create the list of all cards
        allCards = new List<Card>();

        SpawnPlayerMonsters(GameSettings.PlayerState.MyCards);
        SpawnEnemyMonsters(nextLevel.cardsToBeSpawned);

        // Show summon dialog if possible
        ShowPlayerSummonDialog();
    }

    private void SpawnEnemyMonsters(CardDefinition[] enemyCards)
    {
        // player.Cards = list of spawned cards
        enemy.Cards = cardSpawner.SpawnEnemyCards(enemy, player, enemyCards);
        allCards.AddRange(enemy.Cards);
    }

    private void SpawnPlayerMonsters(CardDefinition[] playerCards)
    {
        player.Cards = cardSpawner.SpawnPlayerCards(player, enemy, playerCards);
        allCards.AddRange(player.Cards);
    }

    public void ShowPlayerSummonDialog() {
        Debug.Log("Trying to show the player's summon dialog.");
        exorcismDialog.SetActive(true);
    }

    public void OnPlayerSummonDialogClosed() {
        Debug.Log("Summoning over.");
        
        // Summoning over

        // ...

        // Enable all the monsters and start of the game
        EnableAllTheMonsters();
    }


    private void EnableAllTheMonsters()
    {
        // TODO Enable all the monsters

        OnMonstersEnabled();
    }

    public void OnMonstersEnabled()
    {
        EnablePlayerInteractions();

        // TODO Start the game
        StartBattle();
        
    }

    public void StartBattle() {
        Debug.Log("Started new battle.");
        turnNumber = 0;
        NewTurn();
    }


    public void OnActionDone() {

        if (activeCard != null)
        {
            activeCard.isActiveCard = false;
            activeCard.DisableHighlight();
        }

        DisablePlayerInteractions();

        Debug.Log("Action done.");

        // Check if any player won
        if (enemy.Lost()) {
            ShowPlayerResurrectDialog();
            BattleFinished();
            return;
        }

        // Check if player lost
        if (player.Lost()) {
            // End the game
            ShowGameOverDialog();
        }

        NewTurn();
    }

   
    private void ShowGameOverDialog()
    {
        Debug.Log("Gameover. Showing gameover screen.");
    }

    private void ShowPlayerResurrectDialog()
    {
        Debug.Log("Player won, trying to show ressuret diaog.");
    }

    public void NewTurn() {
        turnNumber++;
        Debug.Log("Starting new turn:" + turnNumber);
        
        player.NewTurn(turnNumber);
        enemy.NewTurn(turnNumber);

        if (initiationList.Count == 0) {
            NextRound();
        }

        Debug.Log("Initiation order: " + InitiationListToString());


        activeCard = initiationList[0];
        activeCard.isActiveCard = true;
        initiationList.Remove(activeCard);
        activeCard.HighlightActiveCard();

        Debug.Log("Active card: " + activeCard.definition.Name);

        BoardPlayer activePlayer = activeCard.owner;

        if (activePlayer == player) {
            EnablePlayerInteractions();
        }

        activePlayer.Play(activeCard);

        // TODO: Highlight the active card

        Debug.Log("Player on turn: " + activePlayer.name);
    }

    private void NextRound()
    {
        InitializeInitiationList();

        if (turnNumber != 1)
        {
            foreach (Card c in allCards){
                c.NewRound();
            }

        }
    }

    private string InitiationListToString()
    {
        string inList = "";
        foreach (Card c in initiationList){
            inList += c.definition.Name + " ";
        }

        return inList;
    }

    private void InitializeInitiationList()
    {
        // Sort cards by initiation
        List<Card> sortedCards = allCards.OrderBy(card => card.definition.initiative).ToList();

        // Add them to the list with queue beeing first
        initiationList = new List<Card>();
        initiationList.AddRange(nextInitiationListQueue);
        initiationList.AddRange(sortedCards);

        // Empty the queue
        nextInitiationListQueue = new List<Card>();
    }

    public void BattleFinished() {

    }

    public void EnablePlayerInteractions()
    {
        EnablePlayerInteractionEvent.Raise();
    }

    public void DisablePlayerInteractions()
    {
        DisablePlayerInteractionEvent.Raise();
    }

    public void AddCard(Card card) {
        allCards.Add(card);
    }

    public void RemoveCard(Card card) {
        allCards.Remove(card);
        initiationList.Remove(card);
        nextInitiationListQueue.Remove(card);
    }

    public void AddCardToInitiationQueue(Card card)
    {
        nextInitiationListQueue.Add(card);
    }
}
