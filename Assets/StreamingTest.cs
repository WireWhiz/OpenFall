using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class StreamingTest : MonoBehaviour
{
    public ServerVariables.PilotSettings settings;
}
#if UNITY_EDITOR
[CustomEditor(typeof(StreamingTest))]
public class StreamingTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Get Values"))
            ((StreamingTest)target).settings = ServerVariables.GetPlayerMovementValues();
    }
}
#endif
