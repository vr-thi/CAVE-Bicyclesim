using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.Networking;
using Cave; 
#if UNITY_EDITOR
using UnityEditor;
#endif


public class BycicleBehaviour : MonoBehaviour
{

    //These gameObjects represent the inGame bycicle, which is activated with the bool "showbycicle"
    //this inGame bycicle can also be used to align the real cycle with the 3D-model inside of the cave
    public GameObject wheelV;
    public GameObject wheelH;
    public GameObject handlebar;
    public GameObject frame;




    [Tooltip("The Node Manager Prefab of the Cave")]
    public GameObject nodeManager;
    [Tooltip("The default Unity Camera, which is active in Debug mode")]
    public GameObject defaultCamera;
    [Tooltip("The CameraHolder")]
    public GameObject cameraHolder;
    [Tooltip("The point towards which the Camera is Lerped in the Update Method")]
    public GameObject camLerpPoint;
   // [Tooltip("Connection to the bycicle TCP_Stream")]
    
    [Tooltip("The script, which handles and smoothes the raw sensor data")]
    public InputHandling inputHandling;


    //For Debugging
    //[Tooltip("if the 3D-models of the cycle should be rendered")]
    //public bool showBycicle;
    //[Tooltip("if in debug mode, or in Cave mode")]
    //public bool debugging;
    [Tooltip("The steerAngle from -40° to 40°")]
    [Range(-40.0f, 40.0f)]
    public float angleDebug = 21f;
    [Tooltip("The speed from 0 to 30 km/h")]
    [Range(0f, 30f)]
    public float speedDebug = 0f;


    [Tooltip("angle, which gets input from the TCP stream ")]
    [HideInInspector]
    public float angle;
    [Tooltip("speed, which gets input from TCP stream")]
    private float speed = 0f;


    [Tooltip("If the Cycle should tilt while driving a corner")]
    public bool tiltCycle;
    [Tooltip("The Intensity: 1 = realistic Tilt, Anything larger decreases the amount ")]
    public float smoothIntensityTilt;

    public Controller controller; 

    // the amount of which the Cycle tilts in corners
    private float tilt;
    // the amount from last frame, to smooth the tiltOverTime
    private float tiltFromLastFrame = 0;
    // the angles of the calculated tilt in relation to the Cycle
    private Vector3 tiltAngles;

    private TCP_Stream dataInput;

    // measured tire spacing is roughly 109 cm
    private float tirespacing = 1.09f;
    //center of mass of the Cycle
    private Vector3 centerofMass;
    // if the cycle was moving in the last frame 
    private bool wasinMotion;
    // if the Cycle is standing at the moment
    private bool standing;
    // if the Coroutine is currently runnung
    private bool CoroutineRunning = false;

    Vector3 posLastFrame;

    int framecounter;
    float timer; 


    // Enable / Disable various Components to enable debugging without the cave 
    private void Awake()
    {
        if (controller.debugging)
        {
            defaultCamera.SetActive(true);
            nodeManager.SetActive(false);
        }
        else
        {
            defaultCamera.SetActive(false);
            nodeManager.SetActive(true);
        }
    }



    void Start()
    {
        posLastFrame = Vector3.zero;
        // Sets the sampleCount = 2;  --> normal Sample count is 7 - 9
        // With only 2 samples the cycle starts quicker
        // the sample count gets increased after the initial movement to the maxSampleCount 
        // and reset to 2 every time the cycle comes to a halt
        // --> this happens in the script Speed.cs in Method: SetSensorTime(long aSensorTime)
        if (GetComponent<NetworkIdentity>().isServer || controller.vive)
        {
            dataInput = this.gameObject.AddComponent<TCP_Stream>();
            dataInput.speed.changeSampleCount(2);
        }
        
        // Cycle didnt move yet
        wasinMotion = false;

        // lower the center of mass in order to make the bike more stable 
        centerofMass = this.GetComponent<Rigidbody>().centerOfMass;
        centerofMass.y = -1.5f;
        this.GetComponent<Rigidbody>().centerOfMass = centerofMass;
        framecounter = 0;
        timer = Time.time; 
    }



    // Update is called once per frame
    void LateUpdate()
    {


        // The Cave will automaticall distribute the script and run it from all PCs, 
        // to avoid this behaviour we need to check if the executing PC is the Master 
        // Otherwise the Cave will freeze 
        if (!controller.debugging && (GetComponent<NetworkIdentity>().isServer || controller.vive))
        {
            // take steeringAngle from sensor
            angle = (float)Test_ReadData.AngleForMono;


            //smoothing the angle in 3 steps

            // 1. smoothing the angle amplitude over all because real input degrees are to rough... (I get motion sickness)
            //angle = angle / 1.7f; 
            // 2. Smooth the angle over time with an  average of the array of the rawSensorinput
            if (inputHandling.smoothRawSteerAngle)
                angle = inputHandling.SmoothRawAngle(angle);
            // 3. Decrease the amplitude at the center of Steering (to decrease Jiggle)
            if (inputHandling.smoothSteerAngleAtCenter)
                angle = inputHandling.SmoothSteerAngleAtCenter(angle);

            // somehow the sensor Input gets decreased by aprox. 50000 if connected to the cave
            speed = (float)Test_ReadData.speedForMono;// * 50000; 
                                                      // One step to smooth the speed (same as SmoothRawAngle)
            if (inputHandling.smoothRawSpeed)
                speed = inputHandling.SmoothRawSpeed(speed);

            // Apply the data to the ingame Cycle
            ApplySensorDataToBycicle();

          //  velocity.text = speed.ToString() + " km/h";

            //Lerp the Debug Camera smoothly along with the cyclist, attaching the gameObject like this reduces jitter drastically 
            cameraHolder.transform.position = Vector3.Lerp(cameraHolder.gameObject.transform.position, camLerpPoint.transform.position, .5f);
            cameraHolder.transform.rotation = Quaternion.Lerp(cameraHolder.gameObject.transform.rotation, camLerpPoint.transform.rotation, .5f);

            // if the cylce is shown, various gameObjects are set active, or inactive
            ShowVirtualBicycle();
        }






        // if in Debug Mode, take the Data from inEditorValues
        else if (controller.debugging)
        {
            angle = angleDebug;
            speed = speedDebug;
        }


        if (Time.time - timer <= 1.0f)
        {
            framecounter++;
        }
        else if (Time.time - timer > 1.0f)
        {
            Debug.Log("Frames/s" + framecounter);
            framecounter = 0;
            timer = Time.time; 
        }


    }

    public float GetSpeed()
    {

            return speed;

    }


    void ApplySensorDataToBycicle()
    {


        float originalSpeed = speed;

        // check if Breaking
        // not a clean solution, but it works 
        // a Coroutine is started every 0.4s, if during this time period the sensor input doesnt return any change in the speed, the cycle is standing still.
        if (!controller.debugging && !CoroutineRunning)
        {
            StartCoroutine(CompareSpeedSamples());
            CoroutineRunning = true;
        }

        Vector3 turningCenter;
        float turnRadius;
        // find the radius of a turn calculated with the tirespacing
        turnRadius = 2 * tirespacing / (Mathf.Sin(Mathf.Abs(angle) * Mathf.Deg2Rad));

        // find the Middle of the turning circle 
        turningCenter = (transform.position + (transform.right.normalized * turnRadius));

        // distinguish between heading left or right
        if (angle < 0f && !standing)
        {
            // if the angle is negativ (steering left) the turning center is flipped to the left side
            Vector3 curDirection = turningCenter - transform.position;
            turningCenter = transform.position - curDirection;


            // rotate the Cycle around the turningCenter with the given speed
            float curTurnRadius = turnRadius / Mathf.Rad2Deg;
            transform.RotateAround(turningCenter, Vector3.up, -(originalSpeed / curTurnRadius) * TimeSynchronizer.deltaTime);



            // you can try tilting but for me it made motion sickness occur stronger
            if (tiltCycle)
            {
                tilt = CalculateTilt(originalSpeed, turnRadius);
                tiltAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, -tilt);
                tiltAngles.z = -tiltAngles.z;
                transform.eulerAngles = tiltAngles;
            }
        }
        else if (angle > 0f && !standing)
        {
            // rotate the Cycle around the turningCenter with the given speed
            float curTurnRadius = turnRadius / Mathf.Rad2Deg;
            transform.RotateAround(turningCenter, Vector3.up, (originalSpeed / curTurnRadius)  * TimeSynchronizer.deltaTime);

            // you can try tilting but for me it made motion sickness occur stronger
            if (tiltCycle)
            {
                tilt = CalculateTilt(originalSpeed, turnRadius);
                tiltAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, -tilt);
                transform.eulerAngles = tiltAngles;
            }
        }


        float cycleSpeed = ((this.transform.position - posLastFrame).magnitude) / TimeSynchronizer.deltaTime;
        posLastFrame = this.transform.position;

        Debug.Log("currentSpeed: " + speed * 3.6f + " SpeedINgameCycle: " + cycleSpeed * 3.6f + " faktor" + (originalSpeed / cycleSpeed)); 


    }



    //checking if the cycle is standing still
    IEnumerator CompareSpeedSamples()
    {
        bool standingStill = true;
        float time = Time.time;
        float lastSpeed = speed;
        while ((Time.time - time) < 0.4f)
        {
            // if the speedSamples are different once, the cycle is still moving and therefore the coroutine is stopped
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
        // standing is only true if the while loop didnt catch a single differing speedsample
        standing = standingStill;
        if (standing && wasinMotion)
        {
            // the speed Array is flushed
            dataInput.speed.resetSpeed();
            //sampleCount is set to 2 (like in start)
            dataInput.speed.changeSampleCount(2);
            wasinMotion = false;
        }
        CoroutineRunning = false;
        yield return null;
    }



    private float CalculateTilt(float v, float r)
    {
        // formula to caluclate the tile (taken from motorcycle)
        float tiltAngle = (v * v) / (r * 9.81f);
        // smoothing the tilt with the tiltFromLastFrame
        float a = 0.5f;
        tiltAngle = tiltAngle * a + (1 - a) * tiltFromLastFrame;
        tiltFromLastFrame = tiltAngle;

        // intensity can be controlled with the public int smoothIntensityTilt
        if (smoothIntensityTilt == 0)
            return tiltAngle;
        else
            return tiltAngle / smoothIntensityTilt;
    }

       //activate / deactivate various components
    void ShowVirtualBicycle()
    {
        handlebar.SetActive(controller.showVirualBycicle);
        frame.SetActive(controller.showVirualBycicle);
        wheelV.SetActive(controller.showVirualBycicle);
        wheelH.SetActive(controller.showVirualBycicle);

        if (controller.showVirualBycicle)
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





