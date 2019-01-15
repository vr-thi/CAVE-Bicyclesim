using Cave;
using System;
using System.Collections.Generic;
using UnityEngine;


/* SpawnableObject is Generic and need to reference to the class attached to the root of the spawnable prefab, and needs to be inheriting from DataDistributor
 * 
 */

public class PlayerNavigation: SpawnableObject
{

    //Navgation
    public NavigationPath nextPath = null;
    public char previousTurnSign = '\0';
    public char nextTurnSign = '\0';
    public int distanceToNextIntersection = -1;

    private Animation rightStraightAnimation, leftStraightAnimation, straightLeftAnimmation, straigthRightAnimation;

    private static int distanceBetweenWaypoints;

    private int displayDistanceMinimum = 80;

    private bool changePath = false;

    public char currentIndication = '\0';

    private int nextID = -2;

    private bool arrived = false;

    

    private Camera mainCamera;

    public GameObject holder;

    private DateTime timeNow;

    public Material arrowLeft, arrowRight, arrowStraight, transparent;

    public Renderer naviRenderer;

    public BycicleBehaviour bb;

    public GameObject cycle;

    private float vel = 0;

    public TextMesh myDistanceText, myVelocityText, myTimeText;
    public String oldDistanceText  = "", oldVelocityText = "", oldTimeText = "";


    /* You need to adjust the Class Type of your SyncScript */

    private string myDistanceTag = "distanceText";
    private string myVelocityTag = "velocityText";
    private string myTimeTag = "timeText";
    private string myPositionTag = "myPosition";
    private string myRotationTag = "myRotation";
    private string myDirectionTag = "myDirection";
    private string mySelfReferenceTag = "this";

    private void Start()
    {

        Init();
        //in Awake you need to register all parameter representations which need to be updated
        
        mySyncScript.AddToSyncDictionary(ref myDistanceTag, this);
        mySyncScript.AddToSyncDictionary(ref myVelocityTag, this);
        mySyncScript.AddToSyncDictionary(ref myTimeTag, this);
        mySyncScript.AddToSyncDictionary(ref myPositionTag, this);
        mySyncScript.AddToSyncDictionary(ref myRotationTag, this);
        mySyncScript.AddToSyncDictionary(ref myDirectionTag, this);
        mySyncScript.AddToSyncDictionary(ref mySelfReferenceTag, this);



        distanceBetweenWaypoints = NavigationPath.DISTANCE_BETWEEN_POINTS;
        arrived = false;
        cycle = GameObject.Find("Cycle");
        bb = cycle.GetComponent<BycicleBehaviour>();

        myTimeText.text = "xx:xx";
        myVelocityText.text = "xx km/h";

        naviRenderer.material = transparent;

        if(ines.isServer)
            InvokeRepeating("MyUpdate", 0, 0.02f);
    }


    public override void MyUpdate()
    {

        if (bb != null)
        {
            vel = bb.GetSpeed();
            String newText = vel.ToString("00") + " km/h";

                myVelocityText.text = newText;
                mySyncScript.RpcUpdateString(myVelocityTag, myVelocityText.text);
                oldVelocityText = newText;
           
            newText = DateTime.Now.Hour.ToString("D2") + ":" + DateTime.Now.Minute.ToString("D2");

                myTimeText.text = newText;
                mySyncScript.RpcUpdateString(myTimeTag, myTimeText.text);
                oldTimeText = newText;

        }

        mySyncScript.RpcUpdateVector3(myPositionTag, this.transform.position);
        mySyncScript.RpcUpdateQuaternion(myRotationTag, this.transform.rotation);

        if (nextPath != null)
            mySyncScript.RpcUpdateString(myDistanceTag, CalculateDistanceToIntersection());
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
                    {
                        naviRenderer.material = arrowStraight;

                    }
                    else if (currentIndication == 'r')
                        naviRenderer.material = arrowRight;
                    else if (currentIndication == 'l')
                        naviRenderer.material = arrowLeft;
                    else {
                        naviRenderer.material = transparent;
                        currentIndication = 't';
                    }
                    mySyncScript.RpcUpdateChar(myDirectionTag, currentIndication);

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

    public string CalculateDistanceToIntersection()
    {

        Vector3[] positions = nextPath.GetDistancePointsPosition().ToArray();
        List<float> distances = new List<float>();

        foreach (Vector3 position in positions)
            distances.Add((position - transform.position).magnitude);
        
        float minimum = Mathf.Min(distances.ToArray());
        int minIndex = distances.IndexOf(minimum);

        distanceToNextIntersection = (minIndex) * distanceBetweenWaypoints;
        myDistanceText.text = distanceToNextIntersection.ToString() + " m";

        return myDistanceText.text;
    }


    /* Setter go here!
     * Setter are virtual and defined for all primitive datatypes (as well as Vector3 and Quaternion (defined in Spawnableobject)
     * Used setters need to be overwritten!
     */


    public override void SetValue(string key, string value)
    {

        if (key.Equals(myDistanceTag))
        {
            myDistanceText.text = value.ToString();
            return;
        }
        else if (key.Equals(myVelocityTag))
        {
            myVelocityText.text = value.ToString();
            return;
        }
        else if (key.Equals(myTimeTag))
        {
            myTimeText.text = value.ToString();
            return;
        }
    }

    public override void SetValue(string key, Vector3 value)
    {
        if (key.Equals(myPositionTag))
        {
            this.transform.position = value;
            mySyncScript.CmdAcknowledge(mySelfReferenceTag, myPositionTag);
        }
    }

    public override void SetValue(string key, Quaternion value)
    {
        if (key.Equals(myRotationTag))
        {
            this.transform.rotation = value;
        }
    }

    public override void SetValue(string key, char value)
    {
        if (key.Equals(myDistanceTag))
        {
            if (value == 's')
                naviRenderer.material = arrowStraight;
            else if (value == 'r')
                naviRenderer.material = arrowRight;
            else if (value == 'l')
                naviRenderer.material = arrowLeft;
            else
                naviRenderer.material = transparent;
        }
    }

}
