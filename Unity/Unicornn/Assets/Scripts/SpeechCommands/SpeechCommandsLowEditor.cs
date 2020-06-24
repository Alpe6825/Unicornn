using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpeechCommandsLow))]
public class SpeechCommandsLowEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SpeechCommandsLow myCommands = (SpeechCommandsLow)target;
        if (GUILayout.Button("Send Informations"))
        {
            myCommands.SendWords();
        }
    }
}
