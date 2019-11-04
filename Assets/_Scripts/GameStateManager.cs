using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour {

    public const int NUMBER_OF_PLAYERS = 2;
    public const bool ALLOW_FLYING = true;
    public enum State { MovePiece, TakePiece, NextTurn, GameOver }
    public State state;
    public int currentPlayerID;
    public int animationsPlaying;

    AIPlayer[] AIPlayers;

    

    public Piece pieceToMove;

	// Use this for initialization
	void Start () {
        state = State.MovePiece;
        currentPlayerID = 0;
        animationsPlaying = 0;

        AIPlayers = new AIPlayer[NUMBER_OF_PLAYERS];

        AIPlayers[0] = null;    // Is a human player
        //AIPlayers[0] = new ThoughfulAI();
        AIPlayers[1] = new ThoughfulAI();

        if(AIPlayers[currentPlayerID] != null)
        {
            AIPlayers[currentPlayerID].TakeTurn();
        }
    }
	
	// Update is called once per frame
	void Update() {
        if(state == State.NextTurn && animationsPlaying == 0)
        {
            NextTurn();
        }	
	}

    private void NextTurn()
    {
        currentPlayerID = (currentPlayerID + 1) % 2;

        if (!HasLegalMoves(currentPlayerID))
        {
            GameOver((currentPlayerID + 1) % 2);
            return;
        }

        pieceToMove = null;
        
        state = State.MovePiece;

        if (AIPlayers[currentPlayerID] != null)
        {
            AIPlayers[currentPlayerID].TakeTurn();
        }
    }

    public bool HasLegalMoves(int playerID)
    {
        if(Piece.Count(playerID) <= 2)
        {
            return false;
        }

        foreach(Piece p in FindObjectsOfType<Piece>())
        {
            if(p.playerID == playerID && p.CanMove())
            {
                return true;
            }
        }
        return false;
    }

    //Are all of this player's pieces in a (full) mill?
    public bool HasFullMills(int playerID)
    {
        foreach (Piece p in FindObjectsOfType<Piece>())
        {
            if (p.playerID == playerID && !p.InMill() && p.GetComponentInParent<Spot>() != null)
            {
                return false;
            }
        }

        return true;
    }

    public void GameOver(int winnerID)
    {
        Debug.Log("Game over!");
        Debug.Log("Player " + (winnerID + 1) + " wins!");
        state = State.GameOver;
        //TODO: Play some sore of animation, etc.
        //TODO: Restart/new game/bacck to menu
    }
}
