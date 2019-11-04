using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIPlayer {

    GameStateManager gameState;

    public AIPlayer()
    {
        gameState = GameObject.FindObjectOfType<GameStateManager>();
    }

    public void TakeTurn()
    {
        Piece piece = MovePiece();
        if (piece.InMill())
        {
            TakePiece();
        }
    }

    protected abstract Piece MovePiece();

    protected abstract void TakePiece();

    protected Piece[] GetLegalPieces()
    {
        List<Piece> legalPieces = new List<Piece>();

        foreach (Piece p in GameObject.FindObjectsOfType<Piece>())
        {
            if(p.playerID == gameState.currentPlayerID && p.CanMove())
            {
                legalPieces.Add(p);
            }
        }

        return legalPieces.ToArray();
    }

    protected Spot[] GetLegalSpots(Piece piece)
    {
        List<Spot> legalSpots = new List<Spot>();

        Spot currentSpot = piece.GetComponentInParent<Spot>();

        if (currentSpot == null || (GameStateManager.ALLOW_FLYING && Piece.Count(piece.playerID) == 3))
        {
            foreach (Spot spot in GameObject.FindObjectsOfType<Spot>())
            {
                if (spot.GetComponentInChildren<Piece>() == null)
                {
                    legalSpots.Add(spot);
                }
            }
        }
        else
        {
            foreach (GameObject spotObject in currentSpot.Neighbours)
            {
                if (spotObject.GetComponentInChildren<Piece>() == null)
                {
                    legalSpots.Add(spotObject.GetComponent<Spot>());
                }
            }
        }

        return legalSpots.ToArray();
    }

    protected Piece[] GetTakeablePieces()
    {
        List<Piece> takeablePieces = new List<Piece>();

        foreach (Piece p in GameObject.FindObjectsOfType<Piece>())
        {
            if (p.playerID != gameState.currentPlayerID && p.CanRemove())
            {
                takeablePieces.Add(p);
            }
        }

        return takeablePieces.ToArray();
    }
}
