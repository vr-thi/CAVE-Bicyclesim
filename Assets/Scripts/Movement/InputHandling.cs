using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandling : MonoBehaviour
{
    [Tooltip("As its natural to jiggle a little with the Handlebar while paddeling, its possible to reduce the amplitude of the steerangle")]
    public bool smoothSteerAngleAtCenter;
    
    public float smoothIntensityAtCenter = 0.2f;

    [Tooltip("If the Cycles raw Sensor Input of the Steering Angle should be Smoothed")]
    public bool smoothRawSteerAngle;
    [Tooltip("Defines the length of the Array, of which an average angle is calcualted --> the larger the number the larger the smoothing factor")]
    public int smoothIntensityAngle;
    private float[] angleMovingAverage;
    private int currAngle = 0;


    [Tooltip("If the Cycles raw Sensor Input of the speed should be Smoothed")]
    public bool smoothRawSpeed;
    [Tooltip("Defines the length of the Array, of which an average speed is calcualted --> the larger the number the larger the smoothing factor")]
    public int smoothIntensitySpeed;
    private float[] speedMovingAverge;
    private int currSpeed = 0;

    private void Start()
    {
        angleMovingAverage = new float[smoothIntensityAngle];
        speedMovingAverge = new float[smoothIntensitySpeed];
    }

    public float SmoothRawAngle(float input)
    {

        // fill the array with samples
        angleMovingAverage[currAngle] = input;
        currAngle++;
        if (currAngle >= angleMovingAverage.Length)
        {
            currAngle = 0;
        }

        float average = 0;
        for (int i = 0; i < angleMovingAverage.Length; i++)
        {
            average += angleMovingAverage[i];
        }
        average = average / angleMovingAverage.Length;
        return average;
    }



    public float SmoothRawSpeed(float input)
    {
        // fill the array with samples
        speedMovingAverge[currSpeed] = input;
        currSpeed++;
        if (currSpeed >= speedMovingAverge.Length)
        {
            currSpeed = 0;
        }

        float average = 0;
        for (int i = 0; i < speedMovingAverge.Length; i++)
        {
            average += speedMovingAverge[i];
        }
        average = average / speedMovingAverge.Length;
        return average;
    }

    // smoothing the angle more intens towards the center (steering ahead 0°)
    public float SmoothSteerAngleAtCenter(float angle)
    {
        float xAngle = 0;
        if (angle < 0)
        {
            xAngle = -(angle * (angle / 40));
        }
        else if (angle > 0)
        {
            xAngle = (angle * (angle / 40));
        }
        // calculate the difference between the real angle and the decreased angle
        float dif = angle - xAngle;
        //return 
        return (angle - (dif / smoothIntensityAtCenter) );
    }
}


