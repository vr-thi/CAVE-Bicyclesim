using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {

    public bool usingCave;
    public bool usingVive;
    public bool debugging;

    public GameObject cave;
    public GameObject vive;

    public bool showVirualBycicle; 



	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (usingCave)
        {
            cave.SetActive(true);
            vive.SetActive(false); 
        }


        if (usingVive)
        {
            cave.SetActive(false);
            vive.SetActive(true);
        }


	}
}
