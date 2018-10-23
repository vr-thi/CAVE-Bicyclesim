using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ArHudController {

    enum CurrentIndicatedDirection { LEFT, RIGHT, STRAIGHT, NONE }

    private int distanceUntilTurn;
    private CurrentIndicatedDirection currentIndicatedDirection;
    private DateTime currentTime;
    private Transform self;

    private List<Vector3> waypoints10m;

    public ArHudController()
    {
        distanceUntilTurn = -1;
        currentIndicatedDirection = CurrentIndicatedDirection.NONE;
        currentTime = DateTime.Now;
        waypoints10m.Clear();
        waypoints10m = new List<Vector3>();
    }

    public int alculateNearestWaypoint()
    {
        for(int i = 0; i < waypoints10m.Count; i++)
        {
            if (Vector3.Distance(waypoints10m[i], self.position) < 10)
                return i * 10;
        }
        return -1;
    }


}
