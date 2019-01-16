using UnityEngine;
using Cave;
using System;

public class SpawnableObject : MonoBehaviour
{
    public virtual void SetValue(string key, string value) { throw new NotImplementedException(); }

    public virtual void SetValue(string key, int value) { throw new NotImplementedException(); }

    public virtual void SetValue(string key, float value) { throw new NotImplementedException(); }

    public virtual void SetValue(string key, double value) { throw new NotImplementedException(); }

    public virtual void SetValue(string key, char value) { throw new NotImplementedException(); }

    public virtual void SetValue(string key, Vector3 value) { throw new NotImplementedException(); }

    public virtual void SetValue(string key, Quaternion value) { throw new NotImplementedException(); }

    public virtual void MyUpdate() { throw new NotImplementedException(); }

    protected InstantiateNode ines;

    protected DataDistributor mySyncScript;

    protected float updateFrequency = 1f / 60f;


    public void Init()
    {
        
        ines = GameObject.Find("NodeManager").GetComponent<InstantiateNode>();

        if (!ines.isServer)
            ines.SetClientOriginScreenplanes();

        //outsource search for parent?
        //maybe static SYNC.findParent?
        bool foundSyncScript = false;
        var findParent = this.gameObject;

        while (foundSyncScript == false && findParent != null)
        {

            if (findParent == null)
            {
                Debug.LogError("No parent of " + this.gameObject.name + " has a SyncScript.");
                return;
            }
            mySyncScript = findParent.GetComponent<DataDistributor>();
            if (mySyncScript != null)
            {
                foundSyncScript = true;
                Debug.Log("Found parent");
                return;
            }
            findParent = findParent.transform.parent.gameObject;
        }


    }


    //GENERIC SETUP

    //public virtual T Init<T>()
    //{
    //    ines = GameObject.Find("NodeManager").GetComponent<InstantiateNode>();

    //    T myVar = default(T);
    //    //outsource search for parent?
    //    //maybe static SYNC.findParent?
    //    bool foundSyncScript = false;
    //    var findParent = this.gameObject;

    //    while (foundSyncScript == false && findParent != null)
    //    {

    //        if (findParent == null)
    //        {
    //            Debug.LogError("No parent of " + this.gameObject.name + " has a SyncScript.");
    //            return myVar;
    //        }
    //        myVar = findParent.GetComponent<T>();
    //        if (myVar != null)
    //        {
    //            foundSyncScript = true;
    //            Debug.Log("Found parent");
    //            return myVar;
    //        }
    //        findParent = findParent.transform.parent.gameObject;
    //    }

    //    return myVar;

    //}




}