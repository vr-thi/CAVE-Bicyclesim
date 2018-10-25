using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class triggerDirection : MonoBehaviour {

    public string direction;
    public ArHudController ARHUD;
    private Sprite directionImage;

	// Use this for initialization
	void Start () {
        directionImage = Resources.Load<Sprite>("Images/" + direction);
	}

    private void OnTriggerEnter(Collider other)
    {
       // ARHUD.SetDirectionIndicator(true, directionImage);
    }

    private void OnTriggerExit(Collider other)
    {
       // ARHUD.SetDirectioniIndicator();
    }


}
