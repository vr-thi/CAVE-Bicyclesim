using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class PlayerNavigation : MonoBehaviour {


    public NavigationPath nextPath = null;
    public char previousTurnSign = '\0';
    public char nextTurnSign = '\0';

    public int distanceToInter = -1;

    private Animation rightStraightAnimation, leftStraightAnimation, straightLeftAnimmation, straigthRightAnimation;


    private float distanceToNextIntersection = -1f;
    private int displayedDistanceToNextIntersection = -1;

    private static int distanceBetweenWaypoints;

    private int displayDistanceMinimum = 80;

    private bool changePath = false;

    public char currentIndication = '\0';

    private int nextID = -2;

    private bool arrived = false;

    public TextMesh time, velocity, direction, distance;

    private Camera mainCamera;

    public GameObject holder;

    private DateTime timeNow;

    public Material arrowLeft, arrowRight, arrowStraight, transparent;

    public Renderer naviRenderer;

    public BycicleBehaviour bb;

    


    // Use this for initialization
    void Start () {
        distanceBetweenWaypoints = NavigationPath.DISTANCE_BETWEEN_POINTS;
        arrived = false;
        //zuweisen von animations, materials, etc.

        //mainCamera = GameObject.FindObjectOfType<Camera>();
        //holder = GameObject.Find("Flystick");

        //this.transform.LookAt(holder.transform.position + holder.transform.forward * 2f);
        //this.transform.SetParent(holder.transform);
        //////  this.transform.rotation.SetEulerAngles(holder.transform.rotation.eulerAngles.z, -holder.transform.rotation.eulerAngles.y - 90f, holder.transform.rotation.eulerAngles.x);
        //this.transform.localPosition = new Vector3(0, 0, 3f);
        //this.transform.localRotation = Quaternion.identity;

     
        
        time.text = Cave.TimeSynchronizer.timeHour.ToString("D2") + ":" + Cave.TimeSynchronizer.timeMinute.ToString("D2");
        velocity.text = "xx km/h";

        naviRenderer.material = transparent;

    }
	
	// Update is called once per frame
	void Update () {

        //this.transform.rotation.SetEulerAngles(0f, (this.transform.rotation.eulerAngles.y +290)%360, this.transform.rotation.eulerAngles.z);

        //this.transform.rotation.SetEulerAngles(holder.transform.rotation.eulerAngles.z, -holder.transform.rotation.eulerAngles.y, holder.transform.rotation.eulerAngles.x);

        time.text =  Cave.TimeSynchronizer.timeHour.ToString("D2") + ":" + Cave.TimeSynchronizer.timeMinute.ToString("D2");

        
        velocity.text = Cave.VelocitySynchronizer.velocity.ToString("00") + " km/h";

        
        

        if (nextPath != null)
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
                    if (currentIndication == 's')
                        naviRenderer.material = arrowStraight;
                    else if (currentIndication == 'r')
                        naviRenderer.material = arrowRight;
                    else if (currentIndication == 'l')
                        naviRenderer.material = arrowLeft;
                    else
                        naviRenderer.material = transparent;
                    changePath = true;
                }
                else
                {
                    nextTurnSign = '\0';
                    currentIndication = 'z';
                    
                    naviRenderer.material = transparent;
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
            currentIndication = nextTurnSign;
            //direction.text = nextTurnSign.ToString();

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

        //if (displayedDistanceToNextIntersection <= displayDistanceMinimum)
        //{
            //ARHUD display distance
            distanceToInter = displayedDistanceToNextIntersection;
            distance.text = distanceToInter.ToString() + " m";

        //}
        //else
        //{
        //    //ARHUD display empty string ""
        //    distanceToInter = -1;
        //    distance.text = "empty";
        //}

        //distanceToInter = displayedDistanceToNextIntersection;

    }
}
