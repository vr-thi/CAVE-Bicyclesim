using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiltingObject : MonoBehaviour {

    public BycicleBehaviour bb;
    public GameObject cycle; 
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (bb.angle > 0)
        {

            //Vector3 angles = new Vector3(0, 0, -bb.tilt);
            Vector3 angles = new Vector3(0,0, -20);
            this.transform.localRotation = Quaternion.Euler(angles);
        }
        else
        {
            Vector3 angles = new Vector3(0, 0, 20);
            this.transform.localRotation = Quaternion.Euler(angles);
        }
    }
}
