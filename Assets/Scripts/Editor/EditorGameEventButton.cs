using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameEvent))]
public class EditorGameEventButton : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GameEvent myGameEvent = (GameEvent)target;

        if (GUILayout.Button("Raise()"))
        {
            myGameEvent.Raise();
        }
    }
}
