using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;

/**
 * 
 * @author ToJa
 * @version 1.0.4
 * 
 */
public class Angle {
	private int fZero;			// Sensor-Wert für 0 Grad (berechnet)
	private int fMin;			// Sensor-Wert für Minimum
	private int fMax;			// Sensor-Wert für Maximum
	private int fAngleRange; 	// Aktive Spannweite von Sensor in Grad
	private int fValue; 		// Aktueller Sensorwert
	private double fFactor;		// Anteil des Winkels pro Sensor-Einheit

	/**
	 * Constructor of Angle.
	 * @param aMinSensorValue - Min. Value of Sensor.
	 * @param aMaxSensorValue - Max. Value of Sensor.
	 * @param aSensorRange - Aktive range of sensor in Grad.
	 */
	public Angle(int aMinSensorValue, int aMaxSensorValue, int aSensorRange) {
		this.create(aMinSensorValue, aMaxSensorValue, aSensorRange, false);
	}

	/**
	 * Constructor of Angle.
	 * @param aMinSensorValue - Min. Value of Sensor.
	 * @param aMaxSensorValue - Max. Value of Sensor.
	 * @param aSensorRange - Aktive range of sensor in Grad.
	 * @param aSwitchDir - The result angle will be switch if true (angle = -result). 
	 */
	public Angle(int aMinSensorValue, int aMaxSensorValue, int aSensorRange, bool aSwitchDir) {
		this.create(aMinSensorValue, aMaxSensorValue, aSensorRange, aSwitchDir);
	}

	private void create(int aMinSensorValue, int aMaxSensorValue, int aSensorRange, bool aSwitchDir) {
		this.fAngleRange = aSensorRange;
		this.fMax = aMaxSensorValue;
		this.fMin = aMinSensorValue;
		this.fZero = (aMaxSensorValue - aMinSensorValue) / 2 + aMinSensorValue;
		this.fFactor = (double)this.fAngleRange / (this.fMax - this.fMin);
		if (aSwitchDir) this.fFactor *= -1;
	}

	/**
	 * Set the current value of angle's sensor.
	 * @param aSensorValue - Current value of angle's sensor. 
	 */
	public void setValue(int aSensorValue) {
		this.fValue = aSensorValue;
	}

	/**
	 * Set the current value of angle's sensor.
	 * @param aSensorValue - Current value of angle's sensor. 
	 */
	public void setValue(string aSensorValue) {
		string pat = @"-?\d+(\.\d+)?"; //Regex r = new Regex(pat, RegexOptions.IgnoreCase);
		Regex r = new Regex(pat); // 
		if (aSensorValue.Length> 0 && r.IsMatch(aSensorValue))  //aSensorValue.matches("-?\\d+(\\.\\d+)?"))
			this.fValue = Int32.Parse(aSensorValue);
	}

	/**
	 * Angle of handle bars.
	 * @return Angle of handle bars in degree.
	 */
	public double getAngle() {
		if (this.fValue > 0) {
			return ((this.fValue - this.fZero) * this.fFactor);
		} else {
			return 0.0;
		}
	}

	/**
	 * Angle of handle bars.
	 * @return Angle of handle bars in radian.
	 */
	public double getAngleRAD() {
		return this.getAngle() * Math.PI / 180;
	}
}
