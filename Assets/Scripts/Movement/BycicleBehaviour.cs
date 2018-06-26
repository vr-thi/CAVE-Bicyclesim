using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class BycicleBehaviour : MonoBehaviour
{


    public GameObject wheelV;
    public GameObject wheelH;
    public GameObject handlebar;
    public GameObject frame;

    public GameObject camera;
    public GameObject camLerpPoint;

    public test dataInput;

    public bool showBycicle;
    public bool debugging;


    private float speed;
    private float speedFromLastFrame = 0;
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
    private bool wasinMotion;

    bool standing;
    bool CoroutineRunning = false;

    Vector3 mLastPosition = Vector3.zero;


    // Use this for initialization
    void Start()
    {
        dataInput.speed.changeSampleCount(2);
        wasinMotion = false;
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
        // The Cave will automaticall distribute the script and run it from all PCs, 
        // to avoid this behaviour we need to check if the executing PC is the Master 
        // Otherwise the Cave will freeze 
        //if (!debugging && GetComponent<NetworkIdentity>().isServer)
        //{
            // take Values from Sensor or from Editor
            angle = (float)Test_ReadData.AngleForMono;
            //angle = angle 1.7f; // smoothing the angle because real input degrees are to rough (I get motion sickness)
            speed = (float)Test_ReadData.speedForMono * 50000; // somehow the cave divides the sensor input by aprox. 40000
            float a = 0.6f;
            speed = speed * a + (1 - a) * speedFromLastFrame;
            speedFromLastFrame = speed;

            // if in Debug Mode, take the Data from inEditorValues

            ApplySensorDataToBycicle();


            if (showBycicle)
                ShowVirtualBycicle();


            //Lerp Camera smoothly along with the cyclist, attaching the gameObject like this reduces jitter drastically 
            camera.transform.position = Vector3.Lerp(camera.gameObject.transform.position, camLerpPoint.transform.position, .5f);
            camera.transform.rotation = Quaternion.Lerp(camera.gameObject.transform.rotation, camLerpPoint.transform.rotation, .5f);
        //}else if (debugging)
        //{

        //    angle = angleDebug;
        //    speed = speedDebug;

        //    if (showBycicle)
        //        ShowVirtualBycicle();

        //    ApplySensorDataToBycicle();
        //    camera.transform.position = Vector3.Lerp(camera.gameObject.transform.position, camLerpPoint.transform.position, .5f);
        //    camera.transform.rotation = Quaternion.Lerp(camera.gameObject.transform.rotation, camLerpPoint.transform.rotation, .5f);
        //}
    }


    void ApplySensorDataToBycicle()
    {

        float speedx = speed;
        speed *= (30 / 1.88f);

        //check if Breaking
        if (!debugging && !CoroutineRunning)
        {
            StartCoroutine(CompareSpeedSamples());
            CoroutineRunning = true;
        }


        Vector3 turningCenter;
        float turnRadius;
        // find the radius of a turn calculated with the tirespacing
        turnRadius = tirespacing / Mathf.Sin(Mathf.Abs(angle) * Mathf.Deg2Rad);
        // find the Middle of the turning circle 
        turningCenter = (transform.position + (transform.right.normalized * turnRadius));

        //if (testScript.speed.getSampleCount() < 9)
        //{
        //    testScript.speed.increaseSampleCount();
        //}


        if (angle <= 0 && !standing)
        {
            Vector3 curDirection = turningCenter - transform.position;
            turningCenter = transform.position - curDirection;
            transform.RotateAround(turningCenter, Vector3.up, -(speed / turnRadius) * Time.deltaTime);
        }
        else if (angle > 0 && !standing)
        {
            transform.RotateAround(turningCenter, Vector3.up, (speed / turnRadius) * Time.deltaTime);
        }




        speedLog = ((transform.position - mLastPosition).magnitude / Time.deltaTime) * 3.6f;
        mLastPosition = transform.position;

        //Debug.Log("SensorDataSpeed = " + speed + "Bike Speedlog = " + speedLog + " ..... SensorSpeed without Calculation: " + speedx);
    }

    IEnumerator CompareSpeedSamples()
    {
        bool standingStill = true;
        float time = Time.time;
        float lastSpeed = speed;
        while ((Time.time - time) < 0.3f)
        {
            if (speed != lastSpeed)
            {
                wasinMotion = true;
                standingStill = false;
                standing = standingStill;
                CoroutineRunning = false;
                break;
            }
            lastSpeed = speed;
            yield return null;
        }
        standing = standingStill;
        if (standing && wasinMotion)
        {
            dataInput.speed.resetSpeed();
            dataInput.speed.changeSampleCount(2);
            wasinMotion = false;
        }
        CoroutineRunning = false;
        yield return null;

    }

    void ShowVirtualBycicle()
    {
        Vector3 steeringAngle = new Vector3(0, angle, 0);
        handlebar.transform.localEulerAngles = steeringAngle;
        wheelV.transform.localEulerAngles = steeringAngle;
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





