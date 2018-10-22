using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Test_ReadData {


	public static double speedForMono;
	public static double AngleForMono;

	public bool stopThread; 

	public Speed speed;
	public Angle angle;
	public NetData rnd;
	private int fSpeedResetTime;
	private int fGrenze = 500;	//Speedsensor liefert eigentlich nur ca. 0V oder 5V
	//ist aber zur ADC angeschlossen. Von ADC bekommen wird 
	//entsprechend etwas um 9 und 1023.


	public Test_ReadData(Speed aSpeed, Angle aAngle, NetData aReadNetData, int aSpeedResetTime) {
		speed = aSpeed;
		angle = aAngle;
		rnd = aReadNetData;
		this.fSpeedResetTime = aSpeedResetTime*1000;
	}

	public void StartThread() {
		stopThread = false; 
		bool sensor = false;
		bool allowSpeedReset = true;
		DateTime dt = DateTime.Now;
		long resetSpeedStart = dt.Ticks;
		int val;
		double currWinkel;   	  
		double lastWinkel = -1000.0;
		Debug.Log("WORKER INSIDE");

		while (!stopThread) {

			// ****************************************
			// Winkel
			//*****************************************
			angle.setValue(rnd.getValue(CONST.getANGLE()));
			currWinkel = angle.getAngle(); 

			if (currWinkel != lastWinkel) {
				// um die Ausgabe der kleinsten Winkeländerungen zu vermeiden,  
				// darf der Winkelunterschied nicht in Bereich <-1,1> liegen
				if (!inRange(currWinkel - lastWinkel, -1, 1)) {
					AngleForMono = currWinkel;
					//Debug.Log("Winkel \n"+ currWinkel.ToString());
					lastWinkel = currWinkel;
				}
			}

			// ****************************************
			// Geschwindigkeit
			//*****************************************
			if (rnd.getValue(CONST.getSPEED()).Length > 0) {
                

                val = Int32.Parse(rnd.getValue(CONST.getSPEED()));
               // Debug.Log("string: " + rnd.getValue(CONST.getSPEED()) + " parsed: " + val);


                if (val < this.fGrenze && sensor == false) {
					sensor = true;
					speed.setSensorTime(rnd.getDate(CONST.getSPEED()));
					speedForMono = speed.getSpeed ();
					//Debug.Log("Speed m/s " + speedForMono.ToString());

					resetSpeedStart = rnd.getDate(CONST.getSPEED());
					allowSpeedReset = true;
				}

				if (val > this.fGrenze && sensor == true ) {
					sensor = false; 	
					resetSpeedStart = rnd.getDate(CONST.getSPEED());
					allowSpeedReset = true;
				}

				//Es kann passieren, dass das Rad sich nicht bewegt, aber auf Grund
				//gespeicherten Daten Geschwindichkeit > 0 ist.
				//Wenn solcher Zustand länger als 2 Sek. dauert, wird die
				//Geschwindigkeit genullt.
				if (rnd.getDate(CONST.getSPEED()) - resetSpeedStart > this.fSpeedResetTime && allowSpeedReset) {
					//speed.resetSpeed();
					resetSpeedStart = rnd.getDate(CONST.getSPEED());
					allowSpeedReset = false;
					//Debug.Log("Speed m/s\n" + speed.getSpeed().ToString());
				}
			}
		}
	}

	private bool inRange(double aValue, double aMin, double aMax) {
		return (aValue >= aMin) && (aValue <= aMax);
	}
}

