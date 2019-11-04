using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Piece : MonoBehaviour {

    GameStateManager gameState;
    Board board;

    public int playerID;


    Vector3 targetPosition;
    float smoothDistance = 0.01f;
    Vector3 velocity;
    float horizontalTime = 0.1f;
    float verticalTime = 0.25f;

    List<Vector3> moveQueue;
    bool toBeRemoved = false;

	// Use this for initialization
	void Start () {
        gameState = FindObjectOfType<GameStateManager>();
        board = FindObjectOfType<Board>();
        moveQueue = new List<Vector3>();
	}
	
	// Update is called once per frame
	void Update () {

	    if(moveQueue.Count == 0)
        {
            return;
        }

        targetPosition = moveQueue[0];

        //If we're at the target position, pop the movement queue
        if (Vector3.Distance(this.transform.position, targetPosition) < smoothDistance)
        {
            moveQueue.RemoveAt(0);

            //If we pop the final position, notify the game state we're no longer animating (this piece)
            if (moveQueue.Count == 0)
            {
                gameState.animationsPlaying--;

                //If this piece is slated for removal (i.e. it has been taken)
                //then destroy it now that it's off screen
                if (toBeRemoved)
                {
                    //Destroy(gameObject);

                    //Make the piece affected by gravity/motion
                    Rigidbody rb = GetComponent<Rigidbody>();
                    rb.isKinematic = false;

                    //Add a nudge of force as it falls?
                    float mod = (playerID == 0) ? 1 : -1;
                    rb.AddForce(new Vector3(10f * mod, 0f, 0f));
                    //Add some random rotational force too?
                    rb.AddTorque(new Vector3(
                        Random.Range(-10, 10),
                        Random.Range(-5, 5),
                        Random.Range(-10, 10)
                    ));

                    //Remove the SCRIPT on the gameobject so that this piece doesn't get moved/taken/counted/etc
                    Destroy(this);

                }
            }
        }
        //If we're not, move towards the target position 
        else
        {
            float timeSmooth;
            //Depending whether we're moving horizontally or vertically, set the time to smooth
            if (this.transform.position.y == targetPosition.y)
            {
                timeSmooth = horizontalTime;
            }
            else
            {
                timeSmooth = verticalTime;
            }

            this.transform.position = Vector3.SmoothDamp(
                this.transform.position,
                targetPosition,
                ref velocity,
                timeSmooth
            );
        }
	}

    void OnMouseUp()
    {
        //TODO: check the mouse isn't over the UI


        if (gameState.state == GameStateManager.State.MovePiece)
        {
            SetAsPieceToMove();
        }
        else if (gameState.state == GameStateManager.State.TakePiece)
        {
            RemovePiece();
        }
        else
        {
            //If we're not in one of these states, it's not something we care about
            return;
        }
    }

    public void SetAsPieceToMove()
    {
        //If the player is trying to move someone else's piece, ignore and return
        if (gameState.currentPlayerID != this.playerID)
        {
            return;
        }

        //If this piece can't be legally moved, don't let the player move this piece
        if (!CanMove())
        {
            return;
        }

        gameState.pieceToMove = this;
        //TODO: highlight this piece
        //TODO: highlight valid spaces?
    }

    public bool MovePiece(Spot moveTo)
    {
        gameState.animationsPlaying++;

        //Lift piece
        moveQueue.Add(new Vector3(this.transform.position.x, 1f, this.transform.position.z));
        //Move piece
        moveQueue.Add(new Vector3(moveTo.transform.position.x, 1f, moveTo.transform.position.z));
        //Lower piece
        moveQueue.Add(moveTo.transform.position);

        this.transform.SetParent(moveTo.transform);


        if (this.InMill())
        {
            gameState.state = GameStateManager.State.TakePiece;
            //TODO: highlight valid pieces?
        }
        else
        {
            gameState.state = GameStateManager.State.NextTurn;
        }

        return this.InMill();
    }

    public bool InMill()
    {
        foreach (Mill mill in board.Mills)
        {
            if (ArrayUtility.Contains(mill.spotObjects, this.transform.parent.gameObject) && mill.IsComplete())
            {
                return true;
            }
        }
        return false;
    }

    public void RemovePiece()
    {
        if (!CanRemove())
        {
            return;
        }

        this.transform.SetParent(null);
        gameState.animationsPlaying++;
        //moveQueue.Add(new Vector3(transform.position.x, 100, transform.position.z));
        moveQueue.Add(new Vector3(this.transform.position.x, 2f, this.transform.position.z));
        float mod = (playerID == 0) ? 1 : -1;
        moveQueue.Add(new Vector3(10f * mod, 2f, 0f));
        toBeRemoved = true;
        if(Piece.Count(playerID) <= 2)
        {
            gameState.GameOver(gameState.currentPlayerID);
        }
        else
        {
            gameState.state = GameStateManager.State.NextTurn;
        }
    }

    public bool CanMove()
    {
        Spot spot = GetComponentInParent<Spot>();

        //If the piece is not yet on the board, it can be moved onto the board
        if (spot == null)
        {
            return true;
        }
        //If there are pieces not on the board yet, move those first
        else if (!Piece.AllOnBoard(playerID))
        {
            return false;
        }

        //If "flying" is enabled, and the player is down to their last three pieces
        if(GameStateManager.ALLOW_FLYING && Piece.Count(playerID) == 3)
        {
            return true;
        }
        
        foreach (GameObject spotObject in spot.Neighbours)
        {
            //If there is an empty neighbour, we can move there
            if (spotObject.GetComponentInChildren<Piece>() == null)
            {
                return true;
            }
        }

        return false;
    }

    public bool CanRemove()
    {
        //If the player is trying to remove their OWN piece, return
        if (gameState.currentPlayerID == this.playerID)
        {
            return false;
        }
        //Can't remove pieces that are not on the board yet!
        if (GetComponentInParent<Spot>() == null)
        {
            return false;
        }

        //If there are NO pieces not in mills, remove any piece
        if (gameState.HasFullMills(this.playerID))
        {
            return true;
        }
        //Otherwise, only remove this piece if NOT in a mill
        else if (!this.InMill())
        {
            return true;
        }

        return false;
    }

    public static int Count(int playerID)
    {
        int total = 0;
        foreach (Piece p in FindObjectsOfType<Piece>())
        {
            if (p.playerID == playerID)
            {
                total++;
            }
        }
        return total;
    }

    public static bool AllOnBoard(int playerID)
    {
        Piece[] pieces = FindObjectsOfType<Piece>();
        foreach (Piece p in pieces)
        {
            if (p.playerID == playerID && p.GetComponentInParent<Spot>() == null)
            {
                return false;
            }
        }
        return true;
    }
}
