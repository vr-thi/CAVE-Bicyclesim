using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DisplaySensorData : MonoBehaviour {

	public Text angleText; 
	public Text speedText; 


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		angleText.text = Test_ReadData.AngleForMono.ToString ();
		speedText.text = Test_ReadData.speedForMono.ToString (); 
	}
}
