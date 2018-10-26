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

    private bool arrived = false;

    public TextMesh time, velocity, direction, distance;


    // Use this for initialization
    void Start () {
        distanceBetweenWaypoints = NavigationPath.DISTANCE_BETWEEN_POINTS;
        arrived = false;
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
            if (other.name.Contains("Start"))
            {
                arrived = false;
            }
            IntersectionProperties temp = other.gameObject.GetComponent<IntersectionProperties>();

            if (nextID == temp.intersectionID)
            {
                nextPath = temp.nextPath;
                nextID = temp.nextID;
                if (nextPath != null)
                {
                    nextTurnSign = temp.nextTurnSign;
                    currentIndication = temp.nextTurnSign;
                    direction.text = currentIndication.ToString();
                    changePath = true;
                }
                else
                {
                    nextTurnSign = '\0';
                    currentIndication = 'z';
                    direction.text = "Ziel";
                }
            }
            else if (nextID == -1)
            {
                nextID = 0;
                arrived = true;
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
            direction.text = "s";

            changePath = false;
        }
        if (arrived)
        {
            //arrive message
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
            distance.text = distanceToInter.ToString() + " m";

        }
        else
        {
            //ARHUD display empty string ""
            distanceToInter = -1;
            distance.text = "";
        }

        //distanceToInter = displayedDistanceToNextIntersection;

    }
}
