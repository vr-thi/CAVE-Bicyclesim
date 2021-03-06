﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speed {
	private int fSample = 6;				// SampleAnzahl  
	private int fSensorCnt = 9;				// Anzahl von Sensoren
	private double fWheelLine = 2.07471;	// Kreisumfang in Meter

	private int fCnt = 0;
	private long [] fSensorTime;
	private float fAverTimeDiff;			//[ms]
	private bool fSpeedAvailable = false;

    public int getSampleCount()
    {
        return this.fSample;
    }

    /**
	 * Creator of Speed-Class.
	 * @param sensorCount - Count of sensor's activations pro full turn of Wheel. 
	 * @param sampleNbr - Number of samples to calculation of speed. 
	 * @param wheelLine - Length of way pro full turn of Wheel.
	 */
    public Speed(int sensorCount, int sampleNbr, double wheelLine) {
		this.fSensorCnt = sensorCount;
		this.fSample = sampleNbr;		
		this.fWheelLine = wheelLine;
		this.fSensorTime = new long[this.fSample];
	}

    /**
	 * Set current time of sensor.
	 * @param aSensorTime
	 * 		Current time of sensor as a long value, e.g. date.getTime().  
	 */
    public void setSensorTime(long aSensorTime) {
        //Debug.Log ("called");
        //if (this.fCnt <= 2) {
        //    this.fSensorTime[this.fCnt] = aSensorTime;
        //    this.fCnt++;
            
        //}
        if (this.fCnt < this.fSample)
        {
            this.fSensorTime[this.fCnt] = aSensorTime;
            this.fCnt++;
            this.shiftTime(aSensorTime);
            this.averTimeDiff();
        } else {
			//wenn ZeitenVektor voll ist
			this.shiftTime(aSensorTime);
			this.averTimeDiff();
		}
    }

	/**
	 * Shift values of sensors' times.
	 * @param aNewTime
	 * 		Current time of sensor as a long value, e.g. date.getTime()
	 */
	private void shiftTime(long aNewTime) {
		for (int  i=0; i<this.fSample-1; i++) {
			//die Sensorzeiten werden verschoben (die "älteste" Zeit gelöscht) 
			this.fSensorTime[i] = this.fSensorTime[i+1]; 
		}
		//die neueste Sensorzeit auf die "jungste" Stelle
		this.fSensorTime[this.fSample-1] = aNewTime;
	}

	private void averTimeDiff() {
		int sum = 0;

		for (int i=0; i<this.fSample-1; i++) {
            //Summe von Zeitdifferenzen zwischen nacheinander kommenden Sensorzeiten
            //Debug.Log("diffTime: " + (int)(this.fSensorTime[i + 1] - this.fSensorTime[i]));
			sum += (int)(this.fSensorTime[i+1] - this.fSensorTime[i]);
		}

		//Mittelwert von Zeitdifferenzen
		this.fAverTimeDiff = sum / (this.fSample - 1);
		//wenn Mittelwert ist größer 0, kann man die Geschwindigkeit berchnen und ausgeben
		this.fSpeedAvailable = this.fAverTimeDiff > 0;
	}

	/** 	
	 * Current Speed.
	 * @return Speed in m/s.
	 */
	public double getSpeed() {
		if (this.fSpeedAvailable) {
			//Radumfang des Rades ist durch Anzahl von Sensoren geteilt
			//Millisekunden muss man in Sekunden uwandeln
            double temp = (this.fWheelLine / this.fSensorCnt) / (this.fAverTimeDiff / Mathf.Pow(10,7));
            //Debug.Log("res: " + temp + ", fAverTimeDiff: " + fAverTimeDiff + ", fAverTimeDiff/10^7: " + fAverTimeDiff / Mathf.Pow(10, 7));
            return temp;
		} else {
			return 0;
		}
	}

	/**
	 * Current Speed.
	 * @return Speed in mph.
	 */
	public double getSpeedMPH() {
		return this.getSpeed() * 2.236936292054402;
	}

	/**
	 * Current Speed.
	 * @return Speed in km/h.
	 */
	public double getSpeedKMH() {
		return this.getSpeed() * 3.6;
	}

	/**
	 * Set speed to zero.
	 */
	public void resetSpeed() {
		for (int i=0; i<this.fSample; i++) {
			this.fSensorTime[i] = 0; 
		}
		this.fCnt = 0;
		this.fSpeedAvailable = false;
	}

    public void changeSampleCount(int sample)
    {
        this.fSample = sample;
    }

    public void increaseSampleCount()
    {
        if(this.fSample < this.fSensorCnt)
        this.fSample += 1;
    }
}

