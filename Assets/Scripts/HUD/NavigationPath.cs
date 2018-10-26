using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class CPC_Visual
{
    public Color pathColor = Color.green;
    public Color inactivePathColor = Color.gray;
    public Color frustrumColor = Color.white;
    public Color handleColor = Color.yellow;
}

public class LookUpTable
{
    private List<float> distance;
    private List<float> tvalue;
    private List<float> tvalue_precalcDistance;

    private int lastMatch = 1;
    private int lastMatchProspective = 1;

    public void ResetLastMatch()
    {
        lastMatch = 1;
    }
    public void ResetLastMatchProspective()
    {
        lastMatchProspective = 1;
    }

    public LookUpTable()
    {
        distance = new List<float>();
        tvalue = new List<float>();
        tvalue_precalcDistance = new List<float>();

    }

    public void Add(float dist, float t)
    {
        distance.Add(dist);
        tvalue.Add(t);
    }

    public float GetTvalueAtDistance(float dist)
    {
        for(int i = lastMatch+1; i < distance.Count; i++)
        {
            if(distance[i] > dist)
            {
                lastMatch = i;
                float percentage = (dist - distance[lastMatch - 1])/(distance[lastMatch]-distance[lastMatch-1]) ;// x% of difference between point A and point B
                float diffTvalue = tvalue[lastMatch - 1] + (tvalue[lastMatch] - tvalue[lastMatch - 1])*percentage;
                tvalue_precalcDistance.Add( diffTvalue);

                return diffTvalue;
            }
            else if(distance[i] == dist)
            {
                lastMatch = i;
                tvalue_precalcDistance.Add(tvalue[lastMatch]);

                return tvalue[lastMatch];
            }
        }
        return 1;
    }

    public float GetTvalueAtDistanceProspective(float dist)
    {
        for (int i = lastMatchProspective + 1; i < distance.Count; i++)
        {
            if (distance[i] > dist)
            {
                lastMatchProspective = i;
                float percentage = (dist - distance[lastMatchProspective - 1]) / (distance[lastMatchProspective] - distance[lastMatchProspective - 1]);// x% of difference between point A and point B
                float diffTvalue = tvalue[lastMatchProspective - 1] + (tvalue[lastMatchProspective] - tvalue[lastMatchProspective - 1]) * percentage;
                tvalue_precalcDistance.Add(diffTvalue);

                return diffTvalue;
            }
            else if (distance[i] == dist)
            {
                lastMatchProspective = i;
                tvalue_precalcDistance.Add(tvalue[lastMatchProspective]);

                return tvalue[lastMatchProspective];
            }
        }

        return 1;
    }
}

[System.Serializable]
public class CPC_Point
{
    private GameObject waypoint;

    public Vector3 waypointHandlePrev;
    public Vector3 waypointHandleNext;
    public Vector3 waypointPosition;
    public Quaternion waypointRotation;
    public Vector3 waypointPositionLocal;

    public AnimationCurve rotationCurve;
    public AnimationCurve positionCurve;

    public bool chained;

    public CPC_Point(Vector3 pos, Quaternion rot)
    {
       
        waypointPosition = pos;
        waypointRotation = rot;

        waypointHandlePrev = Vector3.zero;
        waypointHandleNext = Vector3.zero;
        rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        positionCurve = AnimationCurve.Linear(0, 0, 1, 1);
        chained = true;
    }

}

public class NavigationPath : MonoBehaviour
{
    public static int DISTANCE_BETWEEN_POINTS = 10;
    //parent vehicle
    public Transform player; // this.gameObject

    public Transform _transform;

    private List<GameObject> distancePoints = new List<GameObject>();
    private List<Vector3> distancePointsPosition = new List<Vector3>();
    private int distanceInitCounter = 0;

    private int mPathCount = -1; //needed for access of trajectory represenation

    // visualization of trajectory points on HUD
    private GameObject mSelf;

    public bool lookAtTarget = false;
    public Transform target;

    //triggers vehicle movement
    public bool playOnAwake = false;

    // waypoints
    public List<CPC_Point> points = new List<CPC_Point>();


    //visualization of trajectory
    public CPC_Visual visual;

    public bool looped = false;
    public bool alwaysShow = true;

    private int currentWaypointIndex = 0;
    private float currentTimeInWaypoint = 0;

    public static bool inactive = false;
    public static bool active = false;
    private bool moving = false;

    public List<float> timeNeeded = new List<float>();
    public List<float> distanceOfPath = new List<float>();
    public List<float> timeNeededCalc = new List<float>();

    public List<float> LengthBetweenWaypoints = new List<float>();

    public List<Vector3> precalculatedPointsPosition = new List<Vector3>();
    public List<Quaternion> precalculatedPointsRotation = new List<Quaternion>();

    private float factor = 1.2f;

    int precalcDistance = 5;

    int currentWaypointIndexProspective = 0;

    float currentTimeInWaypointProspective = 0;

    List<float> timeBetweenWaypoints = new List<float>();

    List<float> accelerationList = new List<float>();

    float offsetDistance = 0; //offset to endpoint, to start at correct position in next waypoint for precalculation of trajectory representation transformation

    List<LookUpTable> LUT = new List<LookUpTable>();

    //Vector3 originPositionEgo;
    //Quaternion originRotationEgo;
    //Vector3 originPositionMapView;
    //Quaternion originRotationMapView;

    public float distanceTravelled = 0;

    public LineRenderer lineRenderer;

    private int curveCount = 0;
    private int layerOrder = 0;
    private int SEGMENT_COUNT = 50;


    private void Start()
    {

        _transform = (Transform)target;

        mSelf = this.gameObject;
        LengthBetweenWaypoints.Clear();
        precalculatedPointsPosition.Clear();
        precalculatedPointsRotation.Clear();
        distancePoints.Clear();
        distancePointsPosition.Clear();

        foreach (var index in points)
        {
            index.rotationCurve = AnimationCurve.Linear(0, 0, 1, 1);
            index.positionCurve = AnimationCurve.Linear(0, 0, 1, 1);
        }
        float offset = 0f;

        // precalculate points which are shown on HUD
        for (int i = 0; i < points.Count - 1; i++)
        {
            offset = CalculateLength(i, offset);
        }

        curveCount = (int)points.Count;

    }

    private float trackLength = 0f;


    /// Start, Pause, Resume, Stop path
    public void StartPath()
    {

        foreach (var obj in LUT)
        {
            obj.ResetLastMatch();
            obj.ResetLastMatchProspective();
        }


        CalculateDistance();
    }

    public void CalculateDistance()
    {
        active = true;
        StopAllCoroutines();
    }

    public void DeactivatePath()
    {
        active = false;
        moving = false;
        StopAllCoroutines();

        foreach (var lut in LUT)
        {
            lut.ResetLastMatch();
            lut.ResetLastMatchProspective();
        }
    }


    public bool IsActive()
    {
        return inactive;
    }

    public int GetCurrentWayPoint()
    {
        return currentWaypointIndex;
    }


    float timeNeededOld = 0;
    float timeNeededVeh = 0;
    float timeDelta = 0;
    float fractionMoved = 0;


    private void ResetVariables()
    {
        alreadyTriggered = false;
        timeNeededOld = 0;
        trackLength = 0f;

        timeDelta = 0;
        fractionMoved = 0;

        currentWaypointIndex = 0;
        currentTimeInWaypoint = 0;
        currentWaypointIndexProspective = 0;
        currentTimeInWaypointProspective = 0;//ScenarioSettings.previewTimeOffset;
                                             //if (this.gameObject.name.Equals("EgoVehicle"))
                                             //  currentTimeInWaypointProspective = ScenarioSettings.previewTimeOffset;
        distanceTravelled = 0;

    }

    private bool alreadyTriggered = false;

    private void Update()
    {
        if (!inactive) //inactive == false, if event is triggered
        {

        }
    }

   public List<Vector3> GetDistancePointsPosition()
    {
        return distancePointsPosition;
    }


    private float GetOffsetDistanceTravelled(int waypointIndex)
    {
        float temp = 0;

        if (waypointIndex >= 1)
            return LengthBetweenWaypoints[waypointIndex - 1] + GetOffsetDistanceTravelled(waypointIndex - 1);

        return temp;
    }

    int GetNextIndex(int index)
    {
        if (index == points.Count - 1)
            return 0;
        return index + 1;
    }

    public Quaternion GetLerpRotation(int pointIndex, float time)
    {
        return Quaternion.LerpUnclamped(points[pointIndex].waypointRotation, points[GetNextIndex(pointIndex)].waypointRotation, points[pointIndex].rotationCurve.Evaluate(time));
    }

    //t between 0 (start of curve) and 1 (end of curve)
    public Vector3 GetPointAtTime(int pointIndex, float t)
    {
        //start point
        Vector3 A = points[pointIndex].waypointPosition;
        Vector3 B = points[pointIndex].waypointHandleNext;

        int nextIndex = GetNextIndex(pointIndex);

        //end point
        Vector3 C = points[nextIndex].waypointHandlePrev;
        Vector3 D = points[nextIndex].waypointPosition;

        //return value
        Vector3 val;

        //calculate position on bezier curve depending on order of curve
        if (B == Vector3.zero && C == Vector3.zero)     //1st order
            val = LinearBezier(A, D, t);
        else if (B == Vector3.zero)                     //2nd order
            val = QuadBezier(A, C + D, D, t);
        else if (C == Vector3.zero)                     //2nd order
            val = QuadBezier(A, B + A, D, t);
        else                                            //3rd order
            val = CubicBezier(A, B + A, C + D, D, t);

        return val;
    }

    public float CalculateLength(int waypointIndex, float offset)
    {
        //start point
        Vector3 A = points[waypointIndex].waypointPosition;
        Vector3 B = points[waypointIndex].waypointHandleNext;

        int nextIndex = GetNextIndex(waypointIndex);

        //end point
        Vector3 C = points[nextIndex].waypointHandlePrev;
        Vector3 D = points[nextIndex].waypointPosition;

        //return value
        Vector3 val;

        //calculate position on bezier curve depending on order of curve
        if (B == Vector3.zero && C == Vector3.zero)     //1st order
            offset = LengthLinear(A, D, offset, waypointIndex);
        else if (B == Vector3.zero)                     //2nd order
            offset = LengthQuad(A, C + D, D, offset, waypointIndex);
        else if (C == Vector3.zero)                     //2nd order
            offset = LengthQuad(A, B + A, D, offset, waypointIndex);
        else                                            //3rd order
            offset = LengthCubic(A, B + A, C + D, D, offset, waypointIndex);

        return offset;
    }

    //Precalculation of Waypoints every 10 m for trajectory representation
    //void PrecalcGetPointsForWaypoint(int pointIndex)                                            //maybe needs to be refactored? compare to normal getpointsforwaypoint
    //{
    //    //startPoint
    //    Vector3 A = points[pointIndex].position;
    //    Vector3 B = points[pointIndex].handlenext;

    //    int nextIndex = GetNextIndex(pointIndex);

    //    //endPoint
    //    Vector3 C = points[nextIndex].handleprev;
    //    Vector3 D = points[nextIndex].position;


    //    //calculate position on bezier curve depending on order of curve
    //    if (B == Vector3.zero && C == Vector3.zero)     //1st order
    //        PrecalcLinearBezier(A, D, pointIndex);
    //    else if (B == Vector3.zero)                     //2nd order
    //        PrecalcQuadBezier(A, C + D, D, pointIndex);
    //    else if (C == Vector3.zero)                     //2nd order
    //        PrecalcQuadBezier(A, B + A, D, pointIndex);
    //    else                                            //3rd order
    //        PrecalcCubicBezier(A, B + A, C + D, D, pointIndex);
    //}

    //-------------------------------------------------------------------------------------------------- Bezier Curves (normal)
    Vector3 LinearBezier(Vector3 startPoint, Vector3 endPoint, float t)
    {
        Vector3 lin = t * (-startPoint + endPoint) + startPoint;
        return lin;
    }

    Vector3 QuadBezier(Vector3 startPoint, Vector3 handle, Vector3 endPoint, float t)
    {
        Vector3 quad = t * t * (startPoint - 2 * handle + endPoint) + t * (-2 * startPoint + 2 * handle) + startPoint;
        return quad;
    }

    Vector3 CubicBezier(Vector3 startPoint, Vector3 handleStart, Vector3 handleEnd, Vector3 endPoint, float t)
    {
        Vector3 cubic = t * t * t * (-startPoint + 3 * handleStart - 3 * handleEnd + endPoint) + t * t * (3 * startPoint - 6 * handleStart + 3 * handleEnd) + t * (-3 * startPoint + 3 * handleStart) + startPoint;
        return cubic;
    }

    public float LengthLinear(Vector3 startPoint, Vector3 endPoint, float offset, int waypointIndex)
    {
        float length = (-startPoint + endPoint).magnitude;
        LengthBetweenWaypoints.Add(length);

        LUT.Add(new LookUpTable());

        //calculate positions of points here and save to "distancePoints" list



        //catch if length of this part is smaller than DISTANCE_BETWEEN_POINTS
        Debug.Log("leng: " + length + ", offset " + offset);
        if (length - offset < DISTANCE_BETWEEN_POINTS && offset > length)
        {
            offset = DISTANCE_BETWEEN_POINTS - (length - offset);
            Debug.Log("offset as offset > length: " + offset);
            return offset;
        }


        float tFraction = 1f / length;

        //no LookUpTable needed for Linear!! but init an empty part of the LUT!
        for (float i = offset; i < length; i = i + DISTANCE_BETWEEN_POINTS)
        {
            //LUT[waypointIndex].Add(i, i * tFraction);
            //LUT[waypointIndex].GetTvalueAtDistance(i);
            Vector3 newPosition = LinearBezier(startPoint, endPoint, i * tFraction);
            GameObject newDistancePosition;
            newDistancePosition = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            newDistancePosition.name = "waypoint_distance_" + distanceInitCounter * DISTANCE_BETWEEN_POINTS;
            
           // GameObject newDistancePosition = new GameObject("waypoint_distance_" + distanceInitCounter * DISTANCE_BETWEEN_POINTS);
            distanceInitCounter++;
            newDistancePosition.transform.position = newPosition;
            newDistancePosition.transform.SetParent(this.gameObject.transform);
            distancePointsPosition.Add(newPosition);
            distancePoints.Add(newDistancePosition);
            Debug.Log("DistancePoint i, " + newDistancePosition.ToString());
            //newDistancePosition.AddComponent<DistancePointProperties>();
            //newDistancePosition.GetComponent<DistancePointProperties>().
        }


        Debug.Log("offsetCalculation; DBP " + DISTANCE_BETWEEN_POINTS + " -length " + length + " -offset " + offset);
        offset = DISTANCE_BETWEEN_POINTS - ((length - offset) - ((float)((int)((length - offset) / DISTANCE_BETWEEN_POINTS))) * DISTANCE_BETWEEN_POINTS);
        if (offset == DISTANCE_BETWEEN_POINTS)
            offset = 0;
        Debug.Log("offset: " + offset);


        return offset;
    }

    public float LengthQuad(Vector3 startPoint, Vector3 handle, Vector3 endPoint, float offset, int waypointIndex)
    {
        float length = 0;
        int _precision = 100000;

        //store the lengths between PointsAtTime in an array
        float[] arcLengths = new float[_precision + 1];
        LUT.Add(new LookUpTable());
        Vector3 oldPoint = QuadBezier(startPoint, handle, endPoint, 0);

        for (int p = 0; p <= _precision; p++)
        {
            //Vector3 newPoint = quadBezier(startPoint, handle, endPoint, (float)p / _precision); //get next point
            float t = (float)p / _precision;
            Vector3 newPoint = QuadBezier(startPoint, handle, endPoint, t);
            arcLengths[p] = Vector3.Distance(oldPoint, newPoint); //find distance to old point
            length += arcLengths[p]; //add it to the bezier's length
            LUT[waypointIndex].Add(length, t);
            oldPoint = newPoint; //new is old for next loop
        }

        LengthBetweenWaypoints.Add(length);

        Debug.Log("leng: " + length + ", offset " + offset);
        if (length - offset < DISTANCE_BETWEEN_POINTS && offset > length)
        {
            offset = DISTANCE_BETWEEN_POINTS - (length - offset);
            Debug.Log("offset as offset > length: " + offset);
            return offset;
        }

        float tFraction = 1 / length;

        //Calculate positions of points

        for (float i = offset; i < length; i = i + DISTANCE_BETWEEN_POINTS)
        {
            float tValue = LUT[waypointIndex].GetTvalueAtDistance(i);
            Vector3 newPosition = QuadBezier(startPoint, handle, endPoint, tValue);
            //GameObject newDistancePosition = new GameObject("waypoint_distance_" + distanceInitCounter * DISTANCE_BETWEEN_POINTS);
            GameObject newDistancePosition;
            newDistancePosition = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            newDistancePosition.name = "waypoint_distance_" + distanceInitCounter * DISTANCE_BETWEEN_POINTS;
            newDistancePosition.transform.SetParent(this.gameObject.transform);
            newDistancePosition.transform.position = newPosition;
            distancePointsPosition.Add(newPosition);
            distancePoints.Add(newDistancePosition);
            distanceInitCounter++;
        }



        Debug.Log("offsetCalculation; DBP " + DISTANCE_BETWEEN_POINTS + " -length " + length + " -offset " + offset);
        offset = DISTANCE_BETWEEN_POINTS - ((length - offset) - ((float)((int)((length - offset) / DISTANCE_BETWEEN_POINTS))) * DISTANCE_BETWEEN_POINTS);
        if (offset == DISTANCE_BETWEEN_POINTS)
            offset = 0;
        Debug.Log("offset: " + offset);

        //offset = DISTANCE_BETWEEN_POINTS - (length-offset) - (float)((int)length / DISTANCE_BETWEEN_POINTS) * DISTANCE_BETWEEN_POINTS;
        return offset;
    }


    public float LengthCubic(Vector3 startPoint, Vector3 handleStart, Vector3 handleEnd, Vector3 endPoint, float offset, int waypointIndex)
    {
        float length = 0;
        int _precision = 100000;
        //store the lengths between PointsAtTime in an array
        float[] arcLengths = new float[_precision + 1];

        Vector3 oldPoint = CubicBezier(startPoint, handleStart, handleEnd, endPoint, 0);
        LUT.Add(new LookUpTable());

        for (int p = 0; p <= _precision; p++)
        {
            //Vector3 newPoint = quadBezier(startPoint, handle, endPoint, (float)p / _precision); //get next point
            float t = (float)p / _precision;
            Vector3 newPoint = CubicBezier(startPoint, handleStart, handleEnd, endPoint, t);
            arcLengths[p] = Vector3.Distance(oldPoint, newPoint); //find distance to old point
            length += arcLengths[p]; //add it to the bezier's length
            LUT[waypointIndex].Add(length, t);
            oldPoint = newPoint; //new is old for next loop
        }

        LengthBetweenWaypoints.Add(length);

        Debug.Log("leng: " + length + ", offset " + offset);
        if (length - offset < DISTANCE_BETWEEN_POINTS && offset > length)
        {
            offset = DISTANCE_BETWEEN_POINTS - (length - offset);
            Debug.Log("offset as offset > length: " + offset);
            return offset;
        }

        float tFraction = 1 / length;

        for (float i = offset; i < length; i = i + DISTANCE_BETWEEN_POINTS)
        {
            float tValue = LUT[waypointIndex].GetTvalueAtDistance(i);
            Vector3 newPosition = CubicBezier(startPoint, handleStart, handleEnd, endPoint, tValue);
            //GameObject newDistancePosition = new GameObject("waypoint_distance_" + distanceInitCounter * DISTANCE_BETWEEN_POINTS);
            GameObject newDistancePosition;
            newDistancePosition = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            newDistancePosition.name = "waypoint_distance_" + distanceInitCounter * DISTANCE_BETWEEN_POINTS;
            newDistancePosition.transform.SetParent(this.gameObject.transform);
            newDistancePosition.transform.position = newPosition;
            distancePointsPosition.Add(newPosition);
            distancePoints.Add(newDistancePosition);
            distanceInitCounter++;
        }

        Debug.Log("offsetCalculation; DBP " + DISTANCE_BETWEEN_POINTS + " -length " + length + " -offset " + offset);
        offset = DISTANCE_BETWEEN_POINTS - ((length - offset) - ((float)((int)((length - offset) / DISTANCE_BETWEEN_POINTS))) * DISTANCE_BETWEEN_POINTS);
        if (offset == DISTANCE_BETWEEN_POINTS)
            offset = 0;
        Debug.Log("offset: " + offset);

        //offset = DISTANCE_BETWEEN_POINTS - (length-offset) - (float)((int)length / DISTANCE_BETWEEN_POINTS) * DISTANCE_BETWEEN_POINTS;
        return offset;
    }

    private void OnDestroy()
    {
        foreach (GameObject obj in distancePoints)
        {
            Destroy(obj);
        }
    }

    // ------------------------------------------------------------------------------------------------- Bezier Curves (precalculated)
    //void PrecalcLinearBezier(Vector3 startPoint, Vector3 endPoint,  int index)
    //{
    //    float fraction = precalcDistance / LengthBetweenWaypoints[index];

    //    Vector3 position;
    //    Quaternion rotation;
    //    float i = 0;
    //    for (i = offsetDistance / LengthBetweenWaypoints[index]; i < 1; i = i + fraction)
    //    {
    //        position = i * (-startPoint + endPoint) + startPoint;
    //        rotation = GetLerpRotation(index, i);
    //        precalculatedPointsPosition.Add(position);
    //        precalculatedPointsRotation.Add(rotation);
    //    }

    //    offsetDistance = (i - 1) * LengthBetweenWaypoints[index];
    //}

    //void PrecalcQuadBezier(Vector3 startPoint, Vector3 handle, Vector3 endPoint, int index)
    //{
    //    //float fraction = precalcDistance / LengthBetweenWaypoints[index]; //10 / length
    //    float i = 0;
    //    Vector3 position;
    //    Quaternion rotation;
    //    for (i = offsetDistance; i < LengthBetweenWaypoints[index]; i = i + DISTANCE_BETWEEN_POINTS)
    //    {
    //        float t = LUT[index].GetTvalueAtDistance(i);
    //        position = t * t * (startPoint - 2 * handle + endPoint) + t * (-2 * startPoint + 2 * handle) + startPoint;
    //        rotation = GetLerpRotation(index, t);
    //        precalculatedPointsPosition.Add(position);
    //        precalculatedPointsRotation.Add(rotation);
    //    }
    //    LUT[index].ResetLastMatch();
    //    offsetDistance = i - LengthBetweenWaypoints[index]; // in meter
    //}

    //void PrecalcCubicBezier(Vector3 startPoint, Vector3 handleStart, Vector3 handleEnd, Vector3 endPoint,  int index)
    //{
    //    //float fraction = precalcDistance / LengthBetweenWaypoints[index]; //10 / length
    //    float i = 0;
    //    Vector3 position;
    //    Quaternion rotation;
    //    for (i = offsetDistance; i < LengthBetweenWaypoints[index]; i = i + DISTANCE_BETWEEN_POINTS)
    //    {
    //        float t = LUT[index].GetTvalueAtDistance(i);
    //        position = t * t * t * (-startPoint + 3 * handleStart - 3 * handleEnd + endPoint) + t * t * (3 * startPoint - 6 * handleStart + 3 * handleEnd) + t * (-3 * startPoint + 3 * handleStart) + startPoint;
    //        rotation = GetLerpRotation(index, t);
    //        precalculatedPointsPosition.Add(position);
    //        precalculatedPointsRotation.Add(rotation);
    //    }
    //    LUT[index].ResetLastMatch();
    //    offsetDistance = i - LengthBetweenWaypoints[index]; // in meter

    //}

    //############################################################################################################################## EDITOR

#if UNITY_EDITOR
    public void OnDrawGizmos()
    {

        if (UnityEditor.Selection.activeGameObject == gameObject || alwaysShow)
        {
            if (points.Count >= 2)
            {
                DrawCurve();
                //for (int i = 0; i < points.Count; i++)
                //{
                //    if (i < points.Count - 1)
                //    {
                //        var index = points[i];
                //        var indexNext = points[i + 1];
                //        UnityEditor.Handles.DrawBezier(index.waypointPosition, indexNext.waypointPosition, index.waypointPosition + index.waypointHandleNext,
                //            indexNext.waypointPosition + indexNext.waypointHandlePrev, ((UnityEditor.Selection.activeGameObject == gameObject) ? visual.pathColor : visual.inactivePathColor), null, 5);
                //    }
                //}
            }

            //for (int i = 0; i < points.Count-1; i++)
            //{
            //    var index = points[i];
            //    Gizmos.matrix = Matrix4x4.TRS(index.waypointPosition, index.waypointRotation, Vector3.one);
            //    Gizmos.color = visual.frustrumColor;
            //    Gizmos.DrawFrustum(Vector3.zero, 90f, 0.25f, 0.01f, 1.78f);
            //    Gizmos.matrix = Matrix4x4.identity;
            //}
        }
    }
#endif


    ////--------------------------------------------------------------------------------------------------------------------------------------------------- CSV output for debugging of trajectories
    //    string filename;
    //    char delimiter = '$';

    //    StreamWriter sw;

    //    private void WriteToCSV(float fractionMoved, float time, Vector3 position, float speed, int index)
    //    {
    //        string temp = index + "," + fractionMoved.ToString("0.0000") + delimiter +
    //            Vector3ToString(transform.position) + delimiter + speed.ToString("0.0000") + delimiter + Time.realtimeSinceStartup.ToString("0.0000");
    //        sw.WriteLine(temp);
    //    }
    //    private void WriteToCSV(float fractionMoved, float time, int index, Vector3 position)
    //    {
    //        string temp = index + delimiter + fractionMoved.ToString("0.0000") + delimiter +
    //            time.ToString("0.0000") + delimiter + Vector3ToString(position) + delimiter;
    //        sw.WriteLine(temp);
    //    }





    //    string Vector3ToString(Vector3 vector3)
    //    {
    //        return vector3.x.ToString("0.0000") + delimiter + vector3.y.ToString("0.0000") + delimiter + vector3.z.ToString("0.0000");
    //    }

    //    private void OnDestroy()
    //    {
    //        if (sw != null)
    //            sw.Close();
    //    }

    void DrawCurve()
    {
        List<Vector3> pixels = new List<Vector3>();
        curveCount = points.Count;
        int seg = 1;
        Vector3 pixelFrom, pixelTo;
        // lineRenderer.positionCount = SEGMENT_COUNT*curveCount;
        for (int i = 0; i < curveCount; i++)
        {

            if (i < points.Count - 1)
            {
                for (seg = 0; seg < SEGMENT_COUNT; seg++)
                {

                    float tFrom = seg / (float)SEGMENT_COUNT;
                    float tTo = (seg+1) / (float)SEGMENT_COUNT;
                    int nodeIndex = seg * 3;
                    Vector3 A = points[i].waypointPosition;
                    Vector3 B = points[i].waypointHandleNext;

                    int nextIndex = GetNextIndex(i);

                    //end point
                    Vector3 C = points[nextIndex].waypointHandlePrev;
                    Vector3 D = points[nextIndex].waypointPosition;

                    //return value
                    Vector3 val;
                    

                    //calculate position on bezier curve depending on order of curve
                    if (B == Vector3.zero && C == Vector3.zero)
                    {     //1st order
                        pixelFrom = LinearBezier(A, D, tFrom);
                        pixelTo = LinearBezier(A, D, tTo);
                    }
                    else if (B == Vector3.zero)
                    {                   //2nd order
                        pixelFrom = QuadBezier(A, C + D, D, tFrom);
                        pixelTo = QuadBezier(A, C + D, D, tTo);
                    }
                    else if (C == Vector3.zero)
                    {              //2nd order
                        pixelFrom = QuadBezier(A, B + A, D, tFrom);
                        pixelTo = QuadBezier(A, B + A, D, tTo);
                    }
                    else
                    {                                   //3rd order
                        pixelFrom = CubicBezier(A, B + A, C + D, D, tFrom);
                        pixelTo = CubicBezier(A, B + A, C + D, D, tTo);
                    }

                    UnityEditor.Handles.color = Color.green;
                    UnityEditor.Handles.DrawLine(pixelFrom, pixelTo);

                    
                    
                    //lineRenderer.SetVertexCount(((i * SEGMENT_COUNT) + seg));
                    //lineRenderer.SetPosition(i * (SEGMENT_COUNT) + (seg - 1), pixel);

                }

            }

        }

    }
}

