using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/* This script has to be used for synchronized communication between Master and Slaves and vice versa
 * 
 * Each spawned object has to have this script included on the root of the spawned prefab, as only the spawned root object is allowed to use the NetworkBehaviour script (due to Unity guidelines).
 */

public class DataDistributor : NetworkBehaviour
{

    public Cave.InstantiateNode ines;


    [SyncVar]
    public GameObject parent;

    protected Dictionary<string, SpawnableObject> dict = new Dictionary<string, SpawnableObject>();
    private bool dictionaryInitialized = false;


    void Start()
    {
        if (!dictionaryInitialized)
        {
            dict = new Dictionary<string, SpawnableObject>();
            dictionaryInitialized = true;
        }

        if (!this.transform.root.CompareTag("SpawnParent"))
        {
            this.transform.SetParent(parent.transform);
        }
        var g = GameObject.Find("NodeManager");
        ines = g.GetComponent<Cave.InstantiateNode>();
    }


    public void AddToSyncDictionary(ref string key, SpawnableObject obj)
    {
        int offset = -1;
        while (dict.ContainsKey(key))
        {
            offset++;
            key = key + offset;
            Debug.Log("The dictionary already contains a key with this value. Name got changed to " + key + " and is checked again.");
        }
        dict.Add(key, obj);
        
    }

    /*REMOTE PROCEDURE CALLS
*   
*   RPC calls are used to get data from the master to the slaves
*
* Structure of a RPC function. CAUTION, RPC functions must start with "Rpc", to let Unity know this function needs to be synchronized!
* 
*   [ClientRpc]
*   public void Rpc<FunctionName>(<Parameterlist){
*       //do something
*   }
* */

    [ClientRpc]
    public void RpcUpdateString(string key, string vel)
    {
        if (!dict.ContainsKey(key))
            return;
        SpawnableObject comp = dict[key];
        comp.SetValue(key, vel);
    }

    [ClientRpc]
    public void RpcUpdateFloat(string key, float vel)
    {
        if (!dict.ContainsKey(key))
            return;
        SpawnableObject comp = dict[key];
        comp.SetValue(key, vel);
    }

    [ClientRpc]
    public void RpcUpdateChar(string key, char vel)
    {
        if (!dict.ContainsKey(key))
            return;
        SpawnableObject comp = dict[key];
        comp.SetValue(key, vel);
    }

    [ClientRpc]
    public void RpcUpdateDouble(string key, double vel)
    {
        if (!dict.ContainsKey(key))
            return;
        SpawnableObject comp = dict[key];
        comp.SetValue(key, vel);
    }

    [ClientRpc]
    public void RpcUpdateVector3(string key, Vector3 value)
    {
        if (!dict.ContainsKey(key))
            return;
        SpawnableObject comp = dict[key];
        comp.SetValue(key, value);
        Debug.Log("Cmd ACK of message " + value.ToString());
    }

    [ClientRpc]
    public void RpcUpdateQuaternion(string key, Quaternion value)
    {
        if (!dict.ContainsKey(key))
            return;
        SpawnableObject comp = dict[key];
        comp.SetValue(key, value);
        Debug.Log("Cmd ACK of message " + value.ToString());
    }


    [ClientRpc]
    public void RpcAcknowledge(string key, string value)
    {
        if (!dict.ContainsKey(key))
            return;
        SpawnableObject comp = dict[key];
        comp.SetValue(key, value);
        Debug.Log("RPC ACK of message " + value);
    }



    /*COMMANDS
    *   
    *   Commands are used to get data from a slave to the master
    *   If data change of a slave is relevant for the master Commands are used to sync data with the master.
    *   If the data change is further relevant to the other slaves, data needs to be snychronized by RPC to the other clients.
    *
    *   Structure of a Command function. CAUTION, Command functions must start with "Cmd", to let Unity know this function needs to be synchronized!
    * 
    *   [Command]
    *   public void Cmd<FunctionName>(<Parameterlist){
    *       //do something
    *   }
    * */

    [Command]
    public void CmdAcknowledge(string key, string value)
    {
        if (!dict.ContainsKey(key))
            return;
        SpawnableObject comp = dict[key];
        comp.SetValue(key, value);
        Debug.Log("Cmd ACK of message " + value);
    }

}
