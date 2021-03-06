﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ini;
using System;
using System.Threading;
using UnityEngine.Networking;

public class TCP_Stream : MonoBehaviour {
	Thread tavr;
	Thread ttrd;
	Test_ReadData trd;

	AVR_NetClient avr;

    public Speed speed; 
	// Use this for initialization
	void Start () {
        if (GetComponent<NetworkIdentity>().isServer)
        {
            IniFile ini = new IniFile("Assets\\Scripts\\FahrradCom.EXE.ini");


            string ip1 = ini.IniReadValue("TCP", "IP");


            avr = new AVR_NetClient(ini.IniReadValue("TCP", "IP"),
                                    Int32.Parse(ini.IniReadValue("TCP", "Port")),
                                    Int32.Parse(ini.IniReadValue("TCP", "BufferSize")));

            Debug.Log("avr" + avr);

            speed = new Speed(Int32.Parse(ini.IniReadValue("Speed", "SensorCount")),
                Int32.Parse(ini.IniReadValue("Speed", "SpeedSample")),
                Double.Parse(ini.IniReadValue("Speed", "WheelLine")));

            //Debug.Log (ini.IniReadValue ("Speed", "SensorCount") + ini.IniReadValue ("Speed", "SpeedSample") + ini.IniReadValue ("Speed", "WheelLine"));

            // Winkel
            Angle angle = new Angle(Int32.Parse(ini.IniReadValue("Angle", "SensorMinValue")),
                Int32.Parse(ini.IniReadValue("Angle", "SensorMaxValue")),
                Int32.Parse(ini.IniReadValue("Angle", "SensorRange")),
                ini.IniReadValue("Angle", "SwitchDirection") == "0" ? true : false);


            // Daten zum lesen via TCP - die werden immer abgefragt
            NetData rd = new NetData();
            // Zum Beispiel:
            rd.addData(CONST.getANGLE());   //Winkel
            rd.addData(CONST.getSPEED());   //Geschwindigkeit
                                            // rd.addData(CONST.getSTATE());	//Status


            // Zuordnung der Datensets für die Kommunikation mit AVR
            avr.setDataToRead(rd);



            /* Ein Thread für die Kommunikation.
               Ist wichtig vor allem für die Geschwindigkeitsberechnung.
               Der SpeedSensor liefert eigentlich nur FALSE, wenn die MetallStücke am Rad (9 Stück)
               vorbei "fahren" und TRUE wenn "die Luft rein ist". Die Abstände sind gleich (?),
               deswegen auf Grund bekantes Radumfangs, auch die Teilstrecken (zwischen 2 Metallstücken)
               einfach zu berechnen.
               Für die Geschwindigkeit fehlt nun nur die Zeit. Die bekommt man, in dem man die Zeituterschiede
               in Millisekunden zwischen 2 SpeedSensor-Aktivierungen ablesen kann. Je genauer desto besser.
            */
            tavr = new Thread(avr.startThread);
            tavr.Start();

            // Thread zum Lesen von Daten von AVR
            trd = new Test_ReadData(speed, angle, rd, Int32.Parse(ini.IniReadValue("Speed", "SpeedResetAfter")));
            ttrd = new Thread(trd.StartThread);
            ttrd.Start();

        }
	}
//
//	private void OnApplicationQuit() {
//		tavr.Abort ();
//		ttrd.Abort ();
//	}

//	private void OnApplicationQuit() { 
//		foreach (var client in ClientsManager.Clients) {
//			try { 
//				((TcpClient) client).GetStream().Close(); ((TcpClient) client).Close(); 
//			} 
//			catch (Exception exception) { 
//				Util.logError(exception.ToString()); 
//			} 
//		} 
//	}
	
	// Stop all threads
	void OnApplicationQuit() {
			Debug.Log ("Abort Threads");


			trd.stopThread = true;
			Debug.Log (trd.stopThread);

			avr.stop ();
			Debug.Log (avr.stop ());

	}

	void Update() {
		if (Input.GetKeyDown (KeyCode.T)) {
			Debug.Log ("Abort Threads");


			trd.stopThread = true;
			Debug.Log (trd.stopThread);

			avr.stop ();
			Debug.Log (avr.stop ());
		}
	}

}
