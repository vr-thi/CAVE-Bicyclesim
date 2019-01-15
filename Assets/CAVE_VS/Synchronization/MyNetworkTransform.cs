using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyNetworkTransform : SpawnableObject {


    private string mySelfReferenceTag = "this";
    private string myRotationTag = "myRotation";
    private string myPositionTag = "myPosition";

    // Use this for initialization
    void Start () {

        Init();

        mySyncScript.AddToSyncDictionary(ref myPositionTag, this);
        mySyncScript.AddToSyncDictionary(ref myRotationTag, this);
        mySyncScript.AddToSyncDictionary(ref mySelfReferenceTag, this);

        if (ines.isServer)
            InvokeRepeating("MyUpdate", 0, 0.02f);
    }

    public override void MyUpdate()
    {
        mySyncScript.RpcUpdateVector3(myPositionTag, this.transform.position);
        mySyncScript.RpcUpdateQuaternion(myRotationTag, this.transform.rotation);
    }

    // Setter

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

}
