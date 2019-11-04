using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Spot : MonoBehaviour {

    public GameObject[] Neighbours;
    
    GameStateManager gameState;
    Board board;
	// Use this for initialization
	void Start () {
        gameState = FindObjectOfType<GameStateManager>();
        board = FindObjectOfType<Board>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnMouseUp()
    {
        //If there is no piece to move, or it is not the players turn, it cannot be moved
        if (gameState.pieceToMove == null || gameState.pieceToMove.playerID != gameState.currentPlayerID)
        {
            return;
        }

        if (gameState.state == GameStateManager.State.MovePiece && IsValidMove())
        {
            gameState.pieceToMove.MovePiece(this);
        }
    }

    bool IsValidMove()
    {
        //If this spot holds a piece, this is not a valid move
        if(GetComponentInChildren<Piece>() != null)
        {
            return false;
        }

        //If the piece is not on the board, it can move (anywhere) onto the board
        if(gameState.pieceToMove.GetComponentInParent<Spot>() == null)
        {
            return true;
        }

        //If "flying" is allowed, any spot is valid
        if(GameStateManager.ALLOW_FLYING && Piece.Count(gameState.currentPlayerID) <= 3)
        {
            return true;
        }

        //Otherwise, the piece can only move to one of its neighbouring spots
        foreach (GameObject neighbour in Neighbours)
        {
            if (gameState.pieceToMove.transform.parent.gameObject == neighbour)
            {
                return true;
            }
        }

        return false;
    }

    public Mill[] GetMills()
    {
        List<Mill> mills = new List<Mill>();
        foreach (Mill mill in board.Mills)
        {
            if (ArrayUtility.Contains(mill.spotObjects, this.gameObject))
            {
                mills.Add(mill);
            }
        }
        return mills.ToArray();
    }
}
