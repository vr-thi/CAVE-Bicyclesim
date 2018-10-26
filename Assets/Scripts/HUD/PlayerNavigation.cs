using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNavigation : MonoBehaviour {


    public NavigationPath nextPath = null;
    public char previousTurnSign = '\0';
    public char nextTurnSign = '\0';

    public int distanceToInter = -1;

    private Animation rightStraightAnimation, leftStraightAnimation, straightLeftAnimmation, straigthRightAnimation;
    private Material leftArrow, rightArrow, straighArrow;

    private float distanceToNextIntersection = -1f;
    private int displayedDistanceToNextIntersection = -1;

    private static int distanceBetweenWaypoints;

    private int displayDistanceMinimum = 80;

    private bool changePath = false;

    public char currentIndication = '\0';

    private int nextID = 0;
    

	// Use this for initialization
	void Start () {
        distanceBetweenWaypoints = NavigationPath.DISTANCE_BETWEEN_POINTS;
		//zuweisen von animations, materials, etc.
	}
	
	// Update is called once per frame
	void Update () {
        if(nextPath != null)
            CalculateDistanceToIntersection();
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Contains("Intersection"))
        {
            IntersectionProperties temp = other.gameObject.GetComponent<IntersectionProperties>();

            if (nextID == temp.intersectionID)
            {
                nextPath = temp.nextPath;
                nextID = temp.nextID;
                if (nextPath != null)
                {
                    nextTurnSign = temp.nextTurnSign;
                    currentIndication = 'l';
                    changePath = true;
                }
                else
                {
                    nextTurnSign = '\0';
                    currentIndication = 'z';
                }
            }
            else if (nextID == -1)
            {
                nextID = 0;
            }
        }

    }
    private void OnTriggerExit(Collider other)
    {
        
        if (changePath)
        {
            //start Animation

            //set straightSign
            currentIndication = 's';

            changePath = false;
        }
    }

    public void CalculateDistanceToIntersection()
    {

        Vector3[] positions =  nextPath.GetDistancePointsPosition().ToArray();
        List<float> distances = new List<float>();
        foreach(Vector3 position in positions)
        {
            distances.Add((position - transform.position).magnitude);
        }

        float minimum = Mathf.Min(distances.ToArray());
        int minIndex = distances.IndexOf(minimum);

        
        displayedDistanceToNextIntersection = (minIndex) * distanceBetweenWaypoints;

        if (displayedDistanceToNextIntersection <= displayDistanceMinimum)
        {
            //ARHUD display distance
            distanceToInter = displayedDistanceToNextIntersection;
        }
        else
        {
            //ARHUD display empty string ""
            distanceToInter = -1;
        }

        //distanceToInter = displayedDistanceToNextIntersection;

    }
}
