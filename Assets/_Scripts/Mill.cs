using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Mill
{
    public GameObject[] spotObjects;

    public bool IsComplete()
    {
        int? playerID = null;
        foreach (GameObject spotObject in spotObjects)
        {
            Piece p = spotObject.GetComponentInChildren<Piece>();
            if(p == null)
            {
                return false;
            }

            if(playerID == null)
            {
                playerID = p.playerID;
            }
            else if(p.playerID != playerID)
            {
                return false;
            }
        }
        return true;
    }

    public int ContainsPieces(int playerID)
    {
        int count = 0;
        foreach (GameObject spotObject in spotObjects)
        {
            Piece p = spotObject.GetComponentInChildren<Piece>();
            if(p != null && p.playerID == playerID)
            {
                count++;
            }
        }
        return count;
    }
}
