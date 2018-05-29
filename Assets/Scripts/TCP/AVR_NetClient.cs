using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System;

public class AVR_NetClient {


	Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
	IPAddress ipAdd = System.Net.IPAddress.Parse("192.168.76.50"); //change to ini.readdata
	TcpClient tcp; 
	IPEndPoint remoteEP; 

	StreamReader reader;
	StreamWriter streamwriter;

	private string fIP;
	private int fPort;
	private int fBuffSize; 
	private bool fDoit = false;
	private bool fTestR = false;
	private DateTime datetime;

	public NetData fReadData;

	public AVR_NetClient(string aIp, int aPort) {
		this.fIP = aIp;
		this.fPort = aPort;
	}


	public AVR_NetClient(string aIp, int aPort, int aBuffSize) {
		this.fIP = aIp;
		this.fPort = aPort;
		this.fBuffSize = aBuffSize;
	}


	public void startThread() {
		tcp = new TcpClient (this.fIP, this.fPort);
		remoteEP = new IPEndPoint (ipAdd, this.fPort); 

		try {
			socket.Connect (remoteEP);// verbindet sich mit Server
		} catch (SocketException ex) {
			Debug.Log (ex.Message); 
		}
		if (socket.Connected)
			Debug.Log ("Connected");



		this.fDoit = true; 

		while (this.fDoit) {
			if (!this.fReadData.isEmpty()) {
				foreach (string keyName in this.fReadData.getKeySet() ) {
					send(tcp, keyName);
					if (this.fTestR) Debug.Log("READ-gesendet: %s\n" + keyName.ToString());
					this.fReadData.setValue(keyName, recv(tcp)); 
					this.fReadData.setDate(keyName, DateTime.Now.Ticks );
					if (this.fTestR) Debug.Log("READ-bekommen: %s [%s - %d]\n" + keyName.ToString() + this.fReadData.getValue(keyName) + this.fReadData.getDate(keyName));
					//this.fReadData.setData(keyName, this.fReadData.netDataSet);
				}
			}
		}


	}




	private string recv(TcpClient aClient){
		reader = new StreamReader (aClient.GetStream());
//		BufferedReader bufferedReader =
//			new BufferedReader(new InputStreamReader(aSocket.getInputStream()));
		char[] buffer = new char[this.fBuffSize];
		// blockiert bis Nachricht empfangen
		int count = reader.Read(buffer, 0, this.fBuffSize); 
		count = count - 2; // löscht die CR + LF Zeichen

		return new string(buffer, 0, count);
	}

	private void send(TcpClient aClient, String aMessage){
		streamwriter = new StreamWriter (aClient.GetStream ());
//		PrintWriter printWriter =
//			new PrintWriter(new OutputStreamWriter(aSocket.getOutputStream()));
		streamwriter.Write(aMessage + "\n");
		streamwriter.Flush (); 
	}


	public void setDataToRead(NetData aData) {
		this.fReadData = aData;
	}

//	private void OnApplicationQuit() {
//		socket.Close ();
//	}



	public string stop() {
		this.fDoit = false;
		reader.Dispose ();
		reader.Close ();
		streamwriter.Dispose ();
		streamwriter.Close ();
		tcp.Close ();

		if(!tcp.Connected)
			return "TCP disconnected";
		else
			return "TCP  NOT Disconnected";

		socket.Close ();
	}
		
	
	// Update is called once per frame
//	void Update () {
//		if (Input.GetKeyDown (KeyCode.Z)) {
//			Debug.Log ("Socket closed");
//			socket.Close();
//		}
//	}
}
