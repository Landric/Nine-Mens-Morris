using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThoughfulAI : AIPlayer
{
    protected class Move
    {
        public Piece p;
        public Spot s;
        public Move(Piece p, Spot s)
        {
            this.p = p;
            this.s = s;
        }
    }
    protected override Piece MovePiece()
    {
        float bestWeight = Mathf.NegativeInfinity;
        List<Move> bestMoves = new List<Move>();
        
        foreach (Piece p in GetLegalPieces())
        {
            foreach (Spot s in GetLegalSpots(p))
            {
                float weight = WeightSpot(p, s);
                if(weight > bestWeight)
                {
                    bestWeight = weight;
                    bestMoves.Clear();
                    bestMoves.Add(new Move(p, s));
                }
                else if (weight == bestWeight)
                {
                    bestMoves.Add(new Move(p, s));
                }
            }
        }

        Move bestMove = bestMoves[Random.Range(0, bestMoves.Count - 1)];
        bestMove.p.MovePiece(bestMove.s);
        return bestMove.p;
    }

    float WeightSpot(Piece piece, Spot spot)
    {
        float weight = 0f;
        Mill[] mills;

        Spot currentSpot = piece.GetComponentInParent<Spot>();
        
        if (currentSpot != null)
        {
            //What effects does leaving the current spot have?
            mills = currentSpot.GetMills();
            foreach (Mill mill in mills)
            {
                //Resist moving away from a mill with our own pieces
                weight -= 10f * mill.ContainsPieces(piece.playerID);
                //Resist moving away from a mill with opponent pieces
                weight -= 50f * mill.ContainsPieces((piece.playerID+1)%GameStateManager.NUMBER_OF_PLAYERS);
            }

            //Resist moving away from spots that are part of multiple mills
            if (mills.Length > 1)
            {
                weight -= 10f;
            }
        }
        //What effect would moving to the new spot have?
        mills = spot.GetMills();
        foreach (Mill mill in mills)
        {
            //Preferentially move to complete own mills
            int ownPieces = mill.ContainsPieces(piece.playerID);
            weight += 30f * ownPieces;

            //REALLY prefer completing mills
            if(ownPieces == 2)
            {
                weight += 1000f;
            }
            //Preferentially move to block opponent mills
            int opponentPieces = mill.ContainsPieces((piece.playerID + 1) % GameStateManager.NUMBER_OF_PLAYERS);
            
            //If they're about to get a mill, REALLY prefer blocking it
            if(opponentPieces == 2)
            {
                weight += 100f;
            }
            //If it's already blocked, we care less
            else if(opponentPieces == ownPieces)
            {
                weight -= 100f;
            }

            else if(opponentPieces == 1)
            {
                weight += 10f;
            }
            

            //TODO: reeeeally balance some of these weights better

            //TODO: for that matter, make them parameters of the constructor,
            //and subclass "EasyAI", "MediumAI", etc.
            //or "AggressiveAI", "BlockerAI", etc;
        }

        //TODO: Prefer moving out of a mill, if we can move back in with no chance of being blocked or taken

        //TODO: ESPECIALLY if we are moving straight into another mill

        //Preferentially move towards spots that are part of multiple mills
        if(mills.Length > 1)
        {
            weight += 10f;
        }

        return weight;
    }

    protected override void TakePiece()
    {
        float bestWeight = Mathf.NegativeInfinity;
        List<Piece> bestPieces = new List<Piece>();
        foreach (Piece p in GetTakeablePieces())
        {
            float weight = WeightPiece(p);
            if (weight > bestWeight)
            {
                bestWeight = weight;
                bestPieces.Clear();
                bestPieces.Add(p);
            }
            else if(weight == bestWeight)
            {
                bestPieces.Add(p);
            }
        }
        
        bestPieces[Random.Range(0, bestPieces.Count-1)].RemovePiece();
    }

    float WeightPiece(Piece piece)
    {
        float weight = 0f;
        Spot spot = piece.GetComponentInParent<Spot>();

        //Is this mill almost complete?
        Mill[] mills = spot.GetMills();
        foreach (Mill mill in mills)
        {
            //Prioritise taking pieces in almost complete mills
            weight += 20f * mill.ContainsPieces(piece.playerID);

            //Prioritise taking pieces that are blocking our mills
            weight += 5f * mill.ContainsPieces((piece.playerID+1)%GameStateManager.NUMBER_OF_PLAYERS);
        }

        //If all mills are complete, DON'T take something that can be
        //immediately replaced
        if (piece.InMill())
        {
            foreach (GameObject spotObject in piece.GetComponentInParent<Spot>().Neighbours)
            {
                //(EVERYTHING will have at least two neighbours in this case
                //but that's alright!)
                if(spotObject.GetComponentInChildren<Piece>() != null)
                {
                    weight -= 1000f;
                }
            }
        }

        return weight;
    }


}
