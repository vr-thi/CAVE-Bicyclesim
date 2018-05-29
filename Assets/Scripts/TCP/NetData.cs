using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//import java.util.HashMap;
//import java.util.Set;


public class NetData
{



	private class NetDataSet
	{
		public String value = "";
		public long date = 0;
		/*
		public NetDataSet() {
		}

		public NetDataSet(String aValue) {
			this.value = aValue;
		}
*/
		public NetDataSet (String aValue, long aDate)
		{
			this.value = aValue;
			this.date = aDate;
		}
	}


	private Dictionary<String, NetDataSet> fData = new Dictionary<String, NetDataSet> ();

	/**
	 * Collection of data to read perpetually.
	 * @param aDataKeyName - The name of data key.
	 */
	public void addData (String aDataKeyName)
	{
		if (!this.fData.ContainsKey (aDataKeyName)) {
			this.fData.Add (aDataKeyName, new NetDataSet ("", 0));
		} else {
			Debug.Log ("'%s' exitst already on this list!\n" + aDataKeyName.ToString());
		}
	}

	/**
	 * Set a collection of data to write to the server. The data will be removed
	 * from collection after acknowlidged send to server.
	 * @param aDataKeyName - The name of data key.
	 * @param aValue - The value of ths data.
	 */
	public void addData (String aDataKeyName, String aValue)
	{
		this.fData.Add (aDataKeyName, new NetDataSet (aValue, 0));
	}

	/**
	 * Get values of received data (value and date).
	 * @param aDataKeyName - The name of data key.
	 * @return Values of received data (value as string and date as long.)
	 */
	/*public NetDataSet getData(String aDataKeyName) {
		if (this.fData.containsKey(aDataKeyName))
			return this.fData.get(aDataKeyName);
		return new NetDataSet();
	}
	*/
	/**
	* Set data to collection (value and date).
	* @param aDataKeyName - The name of data key.
	* @param aValue - Value to set.
	*/
	/*public void setData(String aDataKeyName, NetDataSet aValue) {
	this.fData.Add(aDataKeyName, aValue);
}
*/
	/**
* Get value of collection with aDataKeyName.
* @param aDataKeyName - The name of data key.
* @return Value of collection.
	*/
	public String getValue (String aDataKeyName)
	{
		return this.fData [(aDataKeyName)].value;
	}

	/**
	 * Set value of collection with aDataKeyName.
	 * @param aDataKeyName - The name of data key.
	 * @param aValue - The value to set.
	 */
	public void setValue (String aDataKeyName, String aValue)
	{
		this.fData [(aDataKeyName)].value = aValue;
	}

	/**
	 * Get date of collection with aDataKeyName.
	 * @param aDataKeyName - The name of data key.
	 * @return Value of date.
	 */
	public long getDate (String aDataKeyName)
	{
		return this.fData [(aDataKeyName)].date;
	}

	/**
	 * Set date of collection with aDataKeyName.
	 * @param aDataKeyName - The name of data key.
	 * @param aValue - The value of date to set.
	 */
	public void setDate (String aDataKeyName, long aValue)
	{
		this.fData [(aDataKeyName)].date = aValue;
	}

	/**
	 * Delete the key with aDataKeyName from collection.
	 * @param aDataKeyName - The key to remove.
	 */
	public void remove (String aDataKeyName)
	{
		if (this.fData.ContainsKey (aDataKeyName))
			this.fData.Remove (aDataKeyName);
	}

	/**
	 * Remove the collection.
	 */
	public void clear ()
	{
		this.fData.Clear ();
	}

	/**
	 * Check if this collection is empty.
	 * @return Return true if this collection is empty.
	 */
	public bool isEmpty ()
	{
		return this.fData.Count == 0;
	}

	/**
	 * Get a set of the keys contained in this collection.
	 * @return A set of the keys contained in this collection.
	 */
	//eventuell falscher return typ
	public HashSet<string> getKeySet() {
		HashSet<string> returnKeys = new HashSet<string> (); 
		//Dictionary<string, Int16>.KeyCollection keys = fData.Keys;
		foreach (string key in fData.Keys)
			returnKeys.Add (key);
		
		return returnKeys;
	}
}


