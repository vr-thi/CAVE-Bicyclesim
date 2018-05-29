

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class SteeringWheelcollider : MonoBehaviour
{

    //changes 
    // Cycle: Mass 60, Freeze Position Y 
    // Wheels: Mass 20, Wheel damping rate 0.25, Suspension distance 0 
    // Spring Default ... rest default 
    // keine Anpassung der Geschwindigkeit 
    // 

    public WheelCollider HL;
    public WheelCollider HR;
    public WheelCollider VL;
    public WheelCollider VR;

    public GameObject wheelV;
    public GameObject wheelH;
    public GameObject handlebar;
    public GameObject frame;

    public GameObject camera;
    public GameObject camLerpPoint;

    public bool wheelcolliderPhysics;

    public bool showBycicle;
    public bool debugging;

    private float speed;
    private float angle;

    [Range(-40.0f, 40.0f)]
    public float angleDebug = 21f;
    [Range(0f, 30f)]
    public float speedDebug = 0f;



    // measured tire spacing is roughly 109 cm
    private float tirespacing = 1.09f;

    private Vector3 startPos;
    private Vector3 centerofMass;
    private float speedLog;


    //
    private float[] speedSample = new float[100];
    int i = 0;
    bool standing;
    bool CoroutineRunning = false;

    Vector3 mLastPosition = Vector3.zero;

    // Use this for initialization
    void Start()
    {
        standing = false;
        startPos = transform.position;

        // lower the center of mass in order to make the bike more stable 
        centerofMass = this.GetComponent<Rigidbody>().centerOfMass;
        centerofMass.y = -1.5f;
        this.GetComponent<Rigidbody>().centerOfMass = centerofMass;



        if (showBycicle)
        {
            wheelV.SetActive(true);
            wheelH.SetActive(true);
            handlebar.SetActive(true);
            frame.SetActive(true);
        }
        else
        {
            wheelV.SetActive(false);
            wheelH.SetActive(false);
            handlebar.SetActive(false);
            frame.SetActive(false);
        }

    }

    // Update is called once per frame
    void Update()
    {
        // take Values from Sensor or from Editor
        if (!debugging)
        {
            angle = (float)Test_ReadData.AngleForMono;
            angle = angle / 1.7f; // smoothing the angle because real input degrees are to rough for my motionsystem
            speed = (float)Test_ReadData.speedForMono * 40000; // smoothing the input because its seems too large
        }
        else
        {
            angle = angleDebug;
            speed = speedDebug;
        }

        if (wheelcolliderPhysics)
        {
            RotateWithWheelcolliders();
        }
        else
        {
            RotateWithRotateAround();
        }
        if (showBycicle)
        {
            ShowVirtualBycicle();
        }


        //Breaking
        if (speed > 0.5 && !CoroutineRunning)
        {
            StartCoroutine(CompareSpeedSamples());
            CoroutineRunning = true;
        }

        //Lerp Camera smoothly along with the cyclist, attaching the gameObject like this reduces jitter drastically 
        camera.transform.position = Vector3.Lerp(camera.gameObject.transform.position, camLerpPoint.transform.position, .5f);
        camera.transform.rotation = Quaternion.Lerp(camera.gameObject.transform.rotation, camLerpPoint.transform.rotation, .5f);



    }


    IEnumerator CompareSpeedSamples()
    {
        bool standingStill = true;
        float time = Time.time;
        float lastSpeed = speed;
        //Debug.Log("Last Speed" + lastSpeed + " Speed " + speed);
        while ((Time.time - time) < 0.3f)
        {
            if (speed != lastSpeed)
            {
                standingStill = false;
            }
            lastSpeed = speed;
            yield return null;
        }
        standing = standingStill;
        CoroutineRunning = false;
        yield return null;
    }



    void RotateWithWheelcolliders()
    {
        Debug.Log(standing);
        if (!standing)
        { //driving
            HR.brakeTorque = 0;
            HL.brakeTorque = 0;
            HR.motorTorque = speed;
            HL.motorTorque = speed;
        }
        else
        { // braking
            HR.brakeTorque = 30;
            HL.brakeTorque = 30;
            HL.motorTorque = 0;
            HR.motorTorque = 0;
        }
        VL.steerAngle = angle;
        VR.steerAngle = angle;
        speedLog = ((transform.position - mLastPosition).magnitude / Time.deltaTime) * 3.6f;
        mLastPosition = transform.position;
        if (speedLog > speed)
        {
            HR.motorTorque = 0;
            HL.motorTorque = 0;
        }
    }


    void RotateWithRotateAround()
    {
        speed = speedDebug * (30 / 1.88f);

        Vector3 turningCenter;
        float turnRadius;
        // find the radius of a turn calculated with the tirespacing
        turnRadius = tirespacing / Mathf.Sin(Mathf.Abs(angle) * Mathf.Deg2Rad);
        // find the Middle of the turning circle 
        turningCenter = (transform.position + (transform.right.normalized * turnRadius));

        if (angle < 0)
        {
            Vector3 curDirection = turningCenter - transform.position;
            turningCenter = transform.position - curDirection;
            transform.RotateAround(turningCenter, Vector3.up, -(speed / turnRadius) * Time.deltaTime);
        }
        else
        {
            transform.RotateAround(turningCenter, Vector3.up, (speed / turnRadius) * Time.deltaTime);
        }




        speedLog = ((transform.position - mLastPosition).magnitude / Time.deltaTime) * 3.6f;
        mLastPosition = transform.position;

        Debug.Log(speedLog);
    }


 //   void Update()
 //   {
 //       if (Input.GetKeyDown(KeyCode.R))
 //       {
 //           transform.position = startPos;
 //       }
 //   }


    void ShowVirtualBycicle()
    {

        if (wheelcolliderPhysics)
        {
            wheelV.transform.Rotate(VL.rpm / 60 * 360 * Time.deltaTime, 0, 0);
            wheelH.transform.Rotate(HR.rpm / 60 * 360 * Time.deltaTime, 0, 0);
            Vector3 helpWheelRotation = new Vector3(wheelV.transform.localEulerAngles.x, VL.steerAngle, wheelV.transform.localEulerAngles.z);
            wheelV.transform.localEulerAngles = helpWheelRotation;
            Vector3 helpHandlebar = new Vector3(0, VL.steerAngle, wheelV.transform.localEulerAngles.z);
            handlebar.transform.localEulerAngles = helpHandlebar;
        }
        else
        {
            Vector3 steeringAngle = new Vector3(0, angle, 0);
            handlebar.transform.localEulerAngles = steeringAngle;
            wheelV.transform.localEulerAngles = steeringAngle;
        }
    }


    // Draw The Expected turningcircle in Editor (according to the wheelspacing of given bike)  
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {

        // find the radius of a turn calculated with the tirespacing
        float turnRadius = tirespacing / Mathf.Sin(Mathf.Abs(angle) * Mathf.Deg2Rad);

        // find the Middle of the turning circle 
        Vector3 turningCenter = (transform.position + (transform.right.normalized * turnRadius));

        if (angle < 0)
        {
            Vector3 curDirection = turningCenter - transform.position;
            turningCenter = transform.position - curDirection;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(turningCenter, 0.3f);
            Gizmos.DrawWireSphere(turningCenter, turnRadius);

        }
        else
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(turningCenter, 0.3f);
            Gizmos.DrawWireSphere(turningCenter, turnRadius);
        }
    }
#endif



}



