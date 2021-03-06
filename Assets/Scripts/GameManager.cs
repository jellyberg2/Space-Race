﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(PhotonView))]
public class GameManager : Photon.MonoBehaviour {

	public int numPolicySlots = 5;
	public int numOperativeSlots = 6;
	public GameObject cardPrefab;
	public Hand myHand;
	public Hand enemyHand;
	[HideInInspector]
	public int localPlayerNum;
	[HideInInspector]
	public ActionQueue actionQueue;
	[HideInInspector]
	public TurnManager turnManager;
	[HideInInspector]
	public SlotManager slotManager;
	public PlayerFunds myFunds;
	public PlayerFunds enemyFunds;

	private Deck localDeck;
	private DeckDisplay deckDisplay;


	void Start() {
		deckDisplay = GetComponent<DeckDisplay>();
		slotManager = GetComponent<SlotManager>();
		turnManager = GetComponent<TurnManager>();
		actionQueue = GetComponent<ActionQueue>();

		myFunds.SetGM(this);
		enemyFunds.SetGM(this);
	}

	void OnJoinedRoom () {
		localPlayerNum = PhotonNetwork.isMasterClient ? 1 : 2;

		// temp: just make a deck here
		localDeck = new Deck(new List<CardID>() {new CardID("ScienceFunding"),
												 new CardID("ScienceFunding"),
												 new CardID("ScienceFunding"),
												 new CardID("ScienceFunding"),
												 new CardID("ScienceFunding"),
												 new CardID("InvestigativeJournalist"),
												 new CardID("InvestigativeJournalist"),
												 new CardID("InvestigativeJournalist"),
												 new CardID("InvestigativeJournalist"),
												 new CardID("InvestigativeJournalist")});
		localDeck.Shuffle();
		
		deckDisplay.UpdateRemaining(localDeck.GetCount(), true);
		slotManager.SetupSlots(numPolicySlots, numOperativeSlots);

		turnManager.StartGame(); // todo: wait until other player has joined and is ready
	}

	public int GetEnemyPlayerNum() {
		return GetOtherPlayerNum(localPlayerNum);
	}

	public int GetOtherPlayerNum(int p) {
		return (p == 1) ? 2 : 1;
	}
	
	public Card CreateCardGO(CardID ID, int authorPlayerNum) {
		GameObject GO = Instantiate(cardPrefab);
		Card c = GO.GetComponent<Card>();

		c.Setup(ID, this, authorPlayerNum);
		return c;
	}

	public Card CreateConcealedCardGO() {
		GameObject GO = Instantiate(cardPrefab);
		Card c = GO.GetComponent<Card>();
		c.ConcealedSetup(this, GetEnemyPlayerNum());
		return c;
	}

	public Card DrawPlayersCard(int playerNum) {
		if (playerNum == localPlayerNum) {
			return DrawMyCard();
		} else {
			return DrawEnemyCard();
		}
	}

	public Card DrawMyCard() {
		Card c = deckDisplay.DrawCard(localDeck, this);

		if (c == null) {
			// deck is empty
			return null;
		}

		myHand.AddToHand(c);
		return c;
	}

	public Card DrawEnemyCard() {
		deckDisplay.DecrementEnemyRemaining();
		Card c = deckDisplay.DrawConcealedCard(this);
		enemyHand.AddToHand(c);
		return c;
	}
}
