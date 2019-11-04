using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAI : AIPlayer
{
    protected override Piece MovePiece()
    {
        Piece[] pieces = GetLegalPieces();
        Piece piece = pieces[Random.Range(0, pieces.Length - 1)];

        Spot[] spots = GetLegalSpots(piece);
        Spot spot = spots[Random.Range(0, spots.Length - 1)];
        piece.MovePiece(spot);

        return piece;
    }

    protected override void TakePiece()
    {
        Piece[] pieces = GetTakeablePieces();
        Piece piece = pieces[Random.Range(0, pieces.Length - 1)];
        piece.RemovePiece();
    }

    
}
