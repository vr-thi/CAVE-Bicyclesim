using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DebugSteering : MonoBehaviour {

	public WheelCollider HL;
	public WheelCollider HR;
	public WheelCollider VL;
	public WheelCollider VR;

	public Transform wheelV;
	public Transform wheelH;
	public Transform handlebar; 

	public GameObject camera; 
	public GameObject camLerpPoint;


	[Range(-40.0f,40.0f)]
	public float angle = 21f;
	[Range(0f,30f)]
	public float speed = 0f;

	Vector3 angleVector;
	private float turnRadius;
	//measured tire spacing is roughly 109 cm
	private float tirespacing = 1.09f;

	public float smooth = 0.1f; 
	private float anglePrev = 0; 

	private Vector3 centerCircle;
	private Vector3 startPos;
	private Vector3 centerofMass;
	private Vector3 turningCenter;
	float speedLog;

	Vector3 mLastPosition = Vector3.zero;  

	// Use this for initialization
	void Start ()
	{

		angleVector = Vector3.zero;
		centerCircle = Vector3.zero; 

		startPos = transform.position;
		centerofMass = this.GetComponent<Rigidbody> ().centerOfMass;
		centerofMass.y = -1.5f; 
		this.GetComponent<Rigidbody> ().centerOfMass = centerofMass;

	}

	// Update is called once per frame
	void FixedUpdate ()
	{
		//transform.position += transform.forward * Time.deltaTime * speed;
		//angleVector.y = angle;
		//transform.eulerAngles = angleVector;

		// calculate the middle of a circle 
		//centerCircle.x = transform.position.x + turnRadius * Mathf.Cos(Mathf.Abs(angle)* Mathf.Deg2Rad) * directionalVector.x; 
		//centerCircle.z = transform.position.z + turnRadius * Mathf.Sin(Mathf.Abs(angle)* Mathf.Deg2Rad) * directionalVector.z; 



		// find the radius of a turn calculated with the tirespacing
		turnRadius = tirespacing / Mathf.Sin (Mathf.Abs (angle) * Mathf.Deg2Rad); 

		// find the Middle of the turning circle 
		turningCenter = (transform.position + (transform.right.normalized * turnRadius));


		//transform.position += transform.forward * Time.fixedDeltaTime * (speed/3.6f);
		HR.motorTorque = speed;
		HL.motorTorque = speed; 

		speedLog = ((transform.position - mLastPosition).magnitude / Time.deltaTime) * 3.6f;
		mLastPosition = transform.position;

		if (speedLog > speed) {
			HR.motorTorque = 0; 
			HL.motorTorque = 0;
		}

	

		Debug.Log (speedLog);

//		angle =  smooth * angle + ((1 - smooth) * anglePrev);
//		anglePrev = angle; 
		VL.steerAngle = angle;
		VR.steerAngle = angle; 

		wheelV.Rotate (VL.rpm / 60 * 360 * Time.deltaTime, 0, 0);
		wheelH.Rotate (HR.rpm / 60 * 360 * Time.deltaTime, 0, 0);
		Vector3 helpWheelV = new Vector3 (wheelV.localEulerAngles.x, VL.steerAngle, 0);
		wheelV.localEulerAngles = helpWheelV;
		handlebar.localEulerAngles = helpWheelV;
	



	
		//smoothly rotate Camre 
		camera.transform.position =  Vector3.Lerp(camera.gameObject.transform.position, camLerpPoint.transform.position, 0.5f); 
		camera.transform.rotation =  Quaternion.Lerp (camera.gameObject.transform.rotation, camLerpPoint.transform.rotation, 0.5f);



	}

	void Update ()
	{


		if (Input.GetKeyDown (KeyCode.R)) {
			transform.position = startPos; 
		}



	}

	#if UNITY_EDITOR

	void OnDrawGizmosSelected ()
	{


		if (angle < 0) {
			Vector3 curDirection = turningCenter - transform.position;
			turningCenter = transform.position - curDirection; 
			Gizmos.color = Color.red; 
			Gizmos.DrawSphere (turningCenter, 0.3f);
			Gizmos.DrawWireSphere (turningCenter, turnRadius);

		} else {
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere (turningCenter, 0.3f);
			Gizmos.DrawWireSphere (turningCenter, turnRadius);
		}
	}
	#endif


}
