﻿using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public enum CPC_EManipulationModes
{
    Free,
    SelectAndTransform
}

public enum CPC_ENewWaypointMode
{
    LastWaypoint,
    WaypointIndex
}

[CustomEditor(typeof(NavigationPath))]
public class NavigationPath_Inspector : Editor
{
    private NavigationPath t;       //this is for accessing the attributes of NavigationPath
    private ReorderableList pointReorderableList;

    private bool pointsListEmpty = true;

    //Editor variables
    private bool visualFoldout;
    private bool manipulationFoldout;
    private CPC_EManipulationModes cameraTranslateMode;
    private CPC_EManipulationModes cameraRotationMode;
    private CPC_EManipulationModes handlePositionMode;
    private CPC_ENewWaypointMode waypointMode;
    private int waypointIndex = 1;

    //GUIContents
    private GUIContent addPointContent = new GUIContent("Add Point", "Adds a waypoint at the scene view camera's position/rotation");
    private GUIContent deletePointContent = new GUIContent("X", "Deletes this waypoint");
    private GUIContent gotoPointContent = new GUIContent("Goto", "Teleports the scene camera to the specified waypoint");
    private GUIContent relocateContent = new GUIContent("Relocate", "Relocates the specified camera to the current view camera's position/rotation");
    private GUIContent alwaysShowContent = new GUIContent("Always show", "When true, shows the curve even when the GameObject is not selected - \"Inactive path color\" will be used as path color instead");
    private GUIContent chainedContent = new GUIContent("o───o", "Toggles if the handles of the specified waypoint should be chained (mirrored) or not");
    private GUIContent unchainedContent = new GUIContent("o─x─o", "Toggles if the handles of the specified waypoint should be chained (mirrored) or not");

    //Serialized Properties
    private SerializedObject serializedObjectTarget;
    private SerializedProperty mSelfProperty;
    private SerializedProperty playOnAwakeProperty;
    private SerializedProperty visualPathProperty;
    private SerializedProperty visualInactivePathProperty;
    private SerializedProperty visualFrustumProperty;
    private SerializedProperty visualHandleProperty;
    private SerializedProperty loopedProperty;
    private SerializedProperty alwaysShowProperty;
    public SerializedProperty playerProperty;

    private int selectedIndex = -1;

    private float currentTime;
    private float previousTime;

    private bool hasScrollBar = false;

 


    void OnEnable()
    {
        EditorApplication.update += Update;

        t = (NavigationPath)target;
        if (t == null) return;
        

        SetupEditorVariables();
        GetVariableProperties();
        SetupReorderableList();
    }

    void OnDisable()
    {
        EditorApplication.update -= Update;
    }

    void Update()
    {
        if (t == null) return;
        currentTime = t.GetCurrentWayPoint();
        if (Math.Abs(currentTime - previousTime) > 0.0001f)
        {
            Repaint();
            previousTime = currentTime;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObjectTarget.Update();
        DrawBasicSettings();
        GUILayout.Space(5);
        GUILayout.Box("", GUILayout.Width(Screen.width - 20), GUILayout.Height(3));
        DrawVisualDropdown();
        GUILayout.Box("", GUILayout.Width(Screen.width - 20), GUILayout.Height(3));
        DrawManipulationDropdown();
        GUILayout.Box("", GUILayout.Width(Screen.width - 20), GUILayout.Height(3));
        GUILayout.Space(10);
        DrawWaypointList();
        GUILayout.Space(10);
        //DrawRawValues();
        serializedObjectTarget.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        if (t.points.Count >= 2)
        {
            for (int i = 0; i < t.points.Count; i++)
            {
                
                DrawHandles(i);
                Handles.color = Color.white;
            }
        }
    }

    void SelectIndex(int index)
    {
        selectedIndex = index;
        pointReorderableList.index = index;
        Repaint();
    }

    void SetupEditorVariables()
    {
        cameraTranslateMode = (CPC_EManipulationModes)PlayerPrefs.GetInt("CPC_cameraTranslateMode", 1);
        cameraRotationMode = (CPC_EManipulationModes)PlayerPrefs.GetInt("CPC_cameraRotationMode", 1);
        handlePositionMode = (CPC_EManipulationModes)PlayerPrefs.GetInt("CPC_handlePositionMode", 0);
        waypointMode = (CPC_ENewWaypointMode)PlayerPrefs.GetInt("CPC_waypointMode", 0);
    }

    void GetVariableProperties()
    {
        serializedObjectTarget = new SerializedObject(t);
        mSelfProperty = serializedObjectTarget.FindProperty("mSelf");
        playerProperty = serializedObject.FindProperty("player");
        visualPathProperty = serializedObjectTarget.FindProperty("visual.pathColor");
        visualInactivePathProperty = serializedObjectTarget.FindProperty("visual.inactivePathColor");
        visualFrustumProperty = serializedObjectTarget.FindProperty("visual.frustrumColor");
        visualHandleProperty = serializedObjectTarget.FindProperty("visual.handleColor");
        loopedProperty = serializedObjectTarget.FindProperty("looped");
        alwaysShowProperty = serializedObjectTarget.FindProperty("alwaysShow");
        playOnAwakeProperty = serializedObjectTarget.FindProperty("playOnAwake");
        
    }

    void SetupReorderableList()
    {
        pointReorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("points"), true, true, false, false);

        pointReorderableList.elementHeight *= 6;

        pointReorderableList.drawElementCallback = (rect, index, active, focused) =>
        {
            float startRectY = rect.y;
            if (index > t.points.Count - 1) return;
            rect.height -= 2;
            float fullWidth = rect.width - 16 * (hasScrollBar ? 1 : 0);
            rect.width = 40;
            fullWidth -= 40;
            rect.height /= 6;
            GUI.Label(rect, "#" + (index + 1));
            //rect.y += rect.height - 3;
            rect.x += rect.width +2;
            rect.width = (fullWidth -40+16)/4;
            if (GUI.Button(rect, t.points[index].chained ? chainedContent : unchainedContent))
            {
                Undo.RecordObject(t, "Changed chain type");
                t.points[index].chained = !t.points[index].chained;
            }


            rect.y = startRectY;
            rect.height *= 2;
            rect.x += rect.width +2;
            rect.width = (fullWidth-20)/4;
            rect.height = rect.height / 2 - 1;
            if (GUI.Button(rect, gotoPointContent))
            {
                pointReorderableList.index = index;
                selectedIndex = index;
                SceneView.lastActiveSceneView.pivot = t.points[pointReorderableList.index].waypointPosition;
                SceneView.lastActiveSceneView.size = 3;
                SceneView.lastActiveSceneView.Repaint();
            }
            //rect.y += rect.height + 2;
            rect.x += rect.width + 2;
            if (GUI.Button(rect, relocateContent))
            {
                Undo.RecordObject(t, "Relocated waypoint");
                pointReorderableList.index = index;
                selectedIndex = index;
                t.points[pointReorderableList.index].waypointPosition = SceneView.lastActiveSceneView.camera.transform.position;
                t.points[pointReorderableList.index].waypointRotation = SceneView.lastActiveSceneView.camera.transform.rotation;
                SceneView.lastActiveSceneView.Repaint();
            }
            rect.height = (rect.height);
            rect.y = startRectY;
            rect.x += rect.width + 2;
            //rect.width = 20;

            if (GUI.Button(rect, deletePointContent))
            {
                Undo.RecordObject(t, "Deleted a waypoint");
                //if(t.points[index].GetWaypointObject() != null)
                //    DestroyImmediate(t.points[index].GetWaypointObject());
                t.points.Remove(t.points[index]);
                SceneView.RepaintAll();
            }
            rect.y = rect.y + rect.height+2;
            rect.x = 20;
            rect.width = (fullWidth +40);
            //GUI.BeginGroup(rect);

            var i = t.points[index];
            EditorGUI.BeginChangeCheck();
            //GUILayout.BeginVertical("Box");
            Vector3 pos = EditorGUI.Vector3Field(rect, "Waypoint Position", i.waypointPositionLocal);
            rect.y = rect.y + rect.height + 2;
            Quaternion rot = Quaternion.Euler(EditorGUI.Vector3Field(rect, "Waypoint Rotation", i.waypointRotation.eulerAngles));
            rect.y = rect.y + rect.height + 2;
            Vector3 posp = EditorGUI.Vector3Field(rect, "Previous Handle Offset", i.waypointHandlePrev);
            rect.y = rect.y + rect.height + 2;
            Vector3 posn = EditorGUI.Vector3Field(rect, "Next Handle Offset", i.waypointHandleNext);
            rect.y = rect.y + 2;

            //GUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(t, "Changed waypoint transform");

                i.waypointPosition = pos + t.gameObject.transform.position;
                i.waypointPositionLocal = pos;
                i.waypointRotation = rot;
                i.waypointHandleNext = posn;
                i.waypointHandlePrev = posp;

                SceneView.RepaintAll();
            }
        };

    }

    

    void DrawBasicSettings()
    {
        GUILayout.BeginHorizontal();
        playerProperty.objectReferenceValue = (GameObject)EditorGUILayout.ObjectField("PlayerReference", playerProperty.objectReferenceValue, typeof(Transform), true);
        GUI.enabled = true;
        GUILayout.EndHorizontal();
    }

    void DrawVisualDropdown()
    {
        EditorGUI.BeginChangeCheck();
        GUILayout.BeginHorizontal();
        visualFoldout = EditorGUILayout.Foldout(visualFoldout, "Visual");
        alwaysShowProperty.boolValue = GUILayout.Toggle(alwaysShowProperty.boolValue, alwaysShowContent);
        GUILayout.EndHorizontal();
        if (visualFoldout)
        {
            GUILayout.BeginVertical("Box");
            visualPathProperty.colorValue = EditorGUILayout.ColorField("Path color", visualPathProperty.colorValue);
            visualInactivePathProperty.colorValue = EditorGUILayout.ColorField("Inactive path color", visualInactivePathProperty.colorValue);
            visualFrustumProperty.colorValue = EditorGUILayout.ColorField("Frustum color", visualFrustumProperty.colorValue);
            visualHandleProperty.colorValue = EditorGUILayout.ColorField("Handle color", visualHandleProperty.colorValue);
            if (GUILayout.Button("Default colors"))
            {
                Undo.RecordObject(t, "Reset to default color values");
                t.visual = new CPC_Visual();
            }
            GUILayout.EndVertical();
        }
        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
    }

    void DrawManipulationDropdown()
    {
        manipulationFoldout = EditorGUILayout.Foldout(manipulationFoldout, "Transform manipulation modes");
        EditorGUI.BeginChangeCheck();
        if (manipulationFoldout)
        {
            GUILayout.BeginVertical("Box");
            cameraTranslateMode = (CPC_EManipulationModes)EditorGUILayout.EnumPopup("Waypoint Translation", cameraTranslateMode);
            cameraRotationMode = (CPC_EManipulationModes)EditorGUILayout.EnumPopup("Waypoint Rotation", cameraRotationMode);
            handlePositionMode = (CPC_EManipulationModes)EditorGUILayout.EnumPopup("Handle Translation", handlePositionMode);
            GUILayout.EndVertical();
        }
        if (EditorGUI.EndChangeCheck())
        {
            PlayerPrefs.SetInt("CPC_cameraTranslateMode", (int)cameraTranslateMode);
            PlayerPrefs.SetInt("CPC_cameraRotationMode", (int)cameraRotationMode);
            PlayerPrefs.SetInt("CPC_handlePositionMode", (int)handlePositionMode);
            SceneView.RepaintAll();
        }
    }

    void DrawWaypointList()
    {
        serializedObject.Update();
        pointReorderableList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
        Rect r = GUILayoutUtility.GetRect(Screen.width - 16, 18);
        //r.height = 18;
        r.y -= 10;
        GUILayout.Space(-30);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(addPointContent))
        {
            Undo.RecordObject(t, "Added camera path point");
            switch (waypointMode)
            {
                case CPC_ENewWaypointMode.LastWaypoint:
                    if (t.points.Count > 0) {
                        
                        t.points.Add(new CPC_Point(t.gameObject.transform.position, t.gameObject.transform.rotation));
                        
                    }
                    else
                    {
                        t.points.Add(new CPC_Point(t.gameObject.transform.position, t.gameObject.transform.rotation));
                        Debug.LogWarning("No previous waypoint found to place this waypoint, defaulting position to world center");
                        pointsListEmpty = false;
                    }
                    break;
                default:
                    Debug.Log("No points available.");
                    pointsListEmpty = true;
                    break;
                    //throw new ArgumentOutOfRangeException();
            }
            selectedIndex = t.points.Count - 1;
            SceneView.RepaintAll();
        }
        GUILayout.Label("at", GUILayout.Width(20));
        EditorGUI.BeginChangeCheck();
        waypointMode = (CPC_ENewWaypointMode)EditorGUILayout.EnumPopup(waypointMode, waypointMode == CPC_ENewWaypointMode.WaypointIndex ? GUILayout.Width(Screen.width / 4) : GUILayout.Width(Screen.width / 2));
        if (waypointMode == CPC_ENewWaypointMode.WaypointIndex)
        {
            waypointIndex = EditorGUILayout.IntField(waypointIndex, GUILayout.Width(Screen.width / 4));
        }
        if (EditorGUI.EndChangeCheck())
        {
            PlayerPrefs.SetInt("CPC_waypointMode", (int)waypointMode);
        }
        GUILayout.EndHorizontal();
    }

    void DrawHandles(int i)
    {
        DrawHandleLines(i);
        Handles.color = t.visual.handleColor;
        DrawNextHandle(i);
        DrawPrevHandle(i);
        DrawWaypointHandles(i);
        DrawSelectionHandles(i);
    }

    void DrawHandleLines(int i)
    {
        Handles.color = t.visual.handleColor;
        if (i < t.points.Count - 1 || t.looped == true)
            Handles.DrawLine(t.points[i].waypointPosition, t.points[i].waypointPosition + t.points[i].waypointHandleNext);
        if (i > 0 || t.looped == true)
            Handles.DrawLine(t.points[i].waypointPosition, t.points[i].waypointPosition + t.points[i].waypointHandlePrev);
        Handles.color = Color.white;
    }

    void DrawNextHandle(int i)
    {
        if (i < t.points.Count - 1 || loopedProperty.boolValue)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 posNext = Vector3.zero;
            float size = HandleUtility.GetHandleSize(t.points[i].waypointPosition + t.points[i].waypointHandleNext) * 0.1f;
            if (handlePositionMode == CPC_EManipulationModes.Free)
            {
#if UNITY_5_5_OR_NEWER
                posNext = Handles.FreeMoveHandle(t.points[i].waypointPosition + t.points[i].waypointHandleNext, Quaternion.identity, size, Vector3.zero, Handles.SphereHandleCap);
#else
                posNext = Handles.FreeMoveHandle(t.points[i].obj.transform.position + t.points[i].obj.GetComponent<BezierWaypointHandles>().waypointHandleNext, Quaternion.identity, size, Vector3.zero, Handles.SphereCap);
#endif
            }
            else
            {
                if (selectedIndex == i)
                {
#if UNITY_5_5_OR_NEWER
                    Handles.SphereHandleCap(0, t.points[i].waypointPosition + t.points[i].waypointHandleNext, Quaternion.identity, size, EventType.Repaint);
#else
                    Handles.SphereCap(0, t.points[i].obj.transform.position + t.points[i].obj.GetComponent<BezierWaypointHandles>().waypointHandleNext, Quaternion.identity, size);
#endif
                    posNext = Handles.PositionHandle(t.points[i].waypointPosition + t.points[i].waypointHandleNext, Quaternion.identity);
                }
                else if (Event.current.button != 1)
                {
#if UNITY_5_5_OR_NEWER
                    if (Handles.Button(t.points[i].waypointPosition + t.points[i].waypointHandleNext, Quaternion.identity, size, size, Handles.CubeHandleCap))
                    {
                        SelectIndex(i);
                    }
#else
                    if (Handles.Button(t.points[i].obj.transform.position + t.points[i].obj.GetComponent<BezierWaypointHandles>().waypointHandleNext, Quaternion.identity, size, size, Handles.CubeCap))
                    {
                        SelectIndex(i);
                    }
#endif
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed Handle Position");
                t.points[i].waypointHandleNext = posNext - t.points[i].waypointPosition;
                if (t.points[i].chained)
                    t.points[i].waypointHandlePrev = t.points[i].waypointHandleNext * -1;
            }
        }

    }

    void DrawPrevHandle(int i)
    {
        if (i > 0 || loopedProperty.boolValue)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 posPrev = Vector3.zero;
            float size = HandleUtility.GetHandleSize(t.points[i].waypointPosition + t.points[i].waypointHandlePrev) * 0.1f;
            if (handlePositionMode == CPC_EManipulationModes.Free)
            {
#if UNITY_5_5_OR_NEWER
                posPrev = Handles.FreeMoveHandle(t.points[i].waypointPosition + t.points[i].waypointHandlePrev, Quaternion.identity, 0.1f * HandleUtility.GetHandleSize(t.points[i].waypointPosition+ t.points[i].waypointHandlePrev), Vector3.zero, Handles.SphereHandleCap);
#else
                posPrev = Handles.FreeMoveHandle(t.points[i].obj.transform.position + t.points[i].handleprev, Quaternion.identity, 0.1f * HandleUtility.GetHandleSize(t.points[i].obj.transform.position + t.points[i].handleprev), Vector3.zero, Handles.SphereCap);
#endif
            }
            else
            {
                if (selectedIndex == i)
                {
#if UNITY_5_5_OR_NEWER
                    Handles.SphereHandleCap(0, t.points[i].waypointPosition + t.points[i].waypointHandlePrev, Quaternion.identity, 0.1f * HandleUtility.GetHandleSize(t.points[i].waypointPosition + t.points[i].waypointHandleNext), EventType.Repaint);
#else
                    Handles.SphereCap(0, t.points[i].obj.transform.position + t.points[i].handleprev, Quaternion.identity,
                        0.1f * HandleUtility.GetHandleSize(t.points[i].obj.transform.position + t.points[i].obj.GetComponent<BezierWaypointHandles>().waypointHandleNext));
#endif
                    posPrev = Handles.PositionHandle(t.points[i].waypointPosition + t.points[i].waypointHandlePrev, Quaternion.identity);
                }
                else if (Event.current.button != 1)
                {
#if UNITY_5_5_OR_NEWER
                    if (Handles.Button(t.points[i].waypointPosition + t.points[i].waypointHandlePrev, Quaternion.identity, size, size, Handles.CubeHandleCap))
                    {
                        SelectIndex(i);
                    }
#else
                    if (Handles.Button(t.points[i].obj.transform.position + t.points[i].handleprev, Quaternion.identity, size, size,
                        Handles.CubeCap))
                    {
                        SelectIndex(i);
                    }
#endif
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed Handle Position");

                t.points[i].waypointHandlePrev = posPrev - t.points[i].waypointHandlePrev;
                if (t.points[i].chained)
                    t.points[i].waypointHandleNext = t.points[i].waypointHandlePrev * -1;
            }
        }
    }

    void DrawWaypointHandles(int i)
    {
        if (Tools.current == Tool.Move)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 pos = Vector3.zero;
            if (cameraTranslateMode == CPC_EManipulationModes.SelectAndTransform)
            {
                if (i == selectedIndex) pos = Handles.PositionHandle(t.points[i].waypointPosition, (Tools.pivotRotation == PivotRotation.Local) ? t.points[i].waypointRotation : Quaternion.identity);
            }
            else
            {
#if UNITY_5_5_OR_NEWER
                pos = Handles.FreeMoveHandle(t.points[i].waypointPosition, (Tools.pivotRotation == PivotRotation.Local) ? t.points[i].waypointRotation : Quaternion.identity, HandleUtility.GetHandleSize(t.points[i].waypointPosition) * 0.2f, Vector3.zero, Handles.RectangleHandleCap);
#else
                pos = Handles.FreeMoveHandle(t.points[i].obj.transform.position, (Tools.pivotRotation == PivotRotation.Local) ? t.points[i].obj.transform.rotation : Quaternion.identity, HandleUtility.GetHandleSize(t.points[i].obj.transform.position) * 0.2f, Vector3.zero, Handles.RectangleCap);
#endif
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Moved Waypoint");
                t.points[i].waypointPositionLocal = pos - t.transform.position;
                t.points[i].waypointPosition = pos;
            }
        }
        else if (Tools.current == Tool.Rotate)
        {

            EditorGUI.BeginChangeCheck();
            Quaternion rot = Quaternion.identity;
            if (cameraRotationMode == CPC_EManipulationModes.SelectAndTransform)
            {
                if (i == selectedIndex) rot = Handles.RotationHandle(t.points[i].waypointRotation, t.points[i].waypointPosition);
            }
            else
            {
                rot = Handles.FreeRotateHandle(t.points[i].waypointRotation, t.points[i].waypointPosition, HandleUtility.GetHandleSize(t.points[i].waypointPosition) * 0.2f);
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Rotated Waypoint");
                t.points[i].waypointRotation = rot;
            }
        }
    }

    void DrawSelectionHandles(int i)
    {
        if (Event.current.button != 1 && selectedIndex != i)
        {
            if (cameraTranslateMode == CPC_EManipulationModes.SelectAndTransform && Tools.current == Tool.Move
                || cameraRotationMode == CPC_EManipulationModes.SelectAndTransform && Tools.current == Tool.Rotate)
            {
                float size = HandleUtility.GetHandleSize(t.points[i].waypointPosition) * 0.2f;
#if UNITY_5_5_OR_NEWER
                if (Handles.Button(t.points[i].waypointPosition, Quaternion.identity, size, size, Handles.CubeHandleCap))
                {
                    SelectIndex(i);
                }
#else
                if (Handles.Button(t.points[i].obj.transform.position, Quaternion.identity, size, size, Handles.CubeCap))
                {
                    SelectIndex(i);
                }
#endif
            }
        }
    }

    void DrawRawValues()
    {

            foreach (var i in t.points)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginVertical("Box");
                Vector3 pos = EditorGUILayout.Vector3Field("Waypoint Position", i.waypointPositionLocal);
                Quaternion rot = Quaternion.Euler(EditorGUILayout.Vector3Field("Waypoint Rotation", i.waypointRotation.eulerAngles));
                Vector3 posp = EditorGUILayout.Vector3Field("Previous Handle Offset", i.waypointHandlePrev);
                Vector3 posn = EditorGUILayout.Vector3Field("Next Handle Offset", i.waypointHandleNext);
                
                // bool displayTrajectory = EditorGUILayout.Toggle("Trajectory displayed", i.handleTrajectoryDisplayed);
                //bool stopSign = EditorGUILayout.Toggle("Stop Sign Active", i.handleStopSign);
                GUILayout.EndVertical();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(t, "Changed waypoint transform");
                    i.waypointPositionLocal = pos;
                    i.waypointPosition = pos + t.gameObject.transform.position;
                    i.waypointRotation = rot;
                    i.waypointHandlePrev = posp;
                    i.waypointHandleNext = posn;
                    
                    SceneView.RepaintAll();
                }
            }
        
    }
}