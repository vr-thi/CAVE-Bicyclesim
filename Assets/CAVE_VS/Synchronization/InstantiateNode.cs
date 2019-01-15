using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace Cave
{

    public class InstantiateNode : NetworkManager
    {
        public List<GameObject[]> testliste = new List<GameObject[]>();
        public GameObject origin;
        private GameObject originSlave;
        private TrackerSettings trackerSettings;
        public Transform originParent;

        public bool isServer = false;

        public string IpAddress;
        public string Port;
        public float GuiOffset;
        public GUILayout buttonServer, buttonClient;
        private bool _started;

        void Start()
        {
            _started = false;

            NodeInformation.Load();

            this.networkAddress = NodeInformation.master.ip;
            this.networkPort = NodeInformation.master.port;

            if (NodeInformation.IsMaster())
            {
                isServer = true;
                this.StartServer();
                //Instantiate the origin-GameObject
                GameObject objOrigin = Instantiate(origin, originParent);
                DataDistributor dd = objOrigin.GetComponent<DataDistributor>();
                dd.ines = this;
                dd.parent = originParent.gameObject;
                //Spawn the object for the clients
                NetworkServer.Spawn(objOrigin);

                //Test Master; if we remove this, then the master won't see anything (same goes for config)
                originSlave = FindOrigin();

                NodeInformation.SpawnScreenplanes(originSlave.transform.Find("ScreenPlanes"));

                trackerSettings = (TrackerSettings)originSlave.transform.Find("CameraHolder").GetComponent("TrackerSettings");
                trackerSettings.HostSettings = (TrackerHostSettings)GameObject.Find("NodeManager/TrackerHostSettings").GetComponent("TrackerHostSettings");

                trackerSettings = (TrackerSettings)originSlave.transform.Find("Flystick").GetComponent("TrackerSettings");
                trackerSettings.HostSettings = (TrackerHostSettings)GameObject.Find("NodeManager/TrackerHostSettings").GetComponent("TrackerHostSettings");
                //trackerSettings.gameObject.SetActive(false);

            }
            else
            {
                this.StartClient();
                //The camera manipulation happens in Update()
                //GameObject objOrigin = Instantiate(origin, originParent);
                //////Spawn the object for the clients
                //NetworkServer.Spawn(objOrigin);
                
                // deactivate music and sound for clients
                AudioListener.volume = 0f;
            }
        }

        public override void OnStopClient()
        {
            Application.Quit();
        }

        private void OnApplicationQuit()
        {
            if (isServer)
                this.StopServer();
            else
                this.StopClient();
        }



        public static GameObject FindOrigin()
        {
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("MainCamera");
            foreach (GameObject gameObject in gameObjects)
            {
                if (gameObject.transform.Find("CameraHolder") != null)
                    return gameObject;
            }
            return null;
        }

        public void SetClientOriginScreenplanes()
        {
            //The slave needs to turn the camera and adjust the screenplane
            if (!NodeInformation.IsMaster())
            {
                //Only do it once!
                if (originSlave == null)
                {
                    //the origin-GameObject has the tag "MainCamera" to find it easily
                    originSlave = FindOrigin();

                    //Since the client needs some time to get the object from the server, we can only continue if we have the origin
                    if (originSlave != null)
                    {
                        originSlave.transform.parent = originParent;

                        NodeInformation.SpawnScreenplanes(originSlave.transform.Find("ScreenPlanes"));

                        //Find the camera to turn it to the right direction
                        Transform CameraTransform = originSlave.transform.Find("CameraHolder/Camera");
                        CameraTransform.eulerAngles = NodeInformation.own.cameraRoation;

                        //Apply the interpupillary distance
                        //Maybe we need to change this a bit, so that the distance is calculated by considering the direction the User looks.
                        //We see that when we test it.   
                        if (NodeInformation.own.cameraEye == "left")
                        {
                            CameraTransform.localPosition += new Vector3(-0.03f, 0, 0);
                        }
                        else
                        {
                            CameraTransform.localPosition += new Vector3(0.03f, 0, 0);
                        }
                    }
                }
            }
        }

    }

    
}