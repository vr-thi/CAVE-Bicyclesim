using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistancePointsProperties : MonoBehaviour {

    [SerializeField]
    private int distanceToNextTurn;
	// Use this for initialization

    public void SetDistance(int dist)
    {
        distanceToNextTurn = dist;
    }

    public int GetDistance()
    {
        return distanceToNextTurn;
    }
}
