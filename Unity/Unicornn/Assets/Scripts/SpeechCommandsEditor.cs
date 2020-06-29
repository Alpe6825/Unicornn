using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpeechCommands))]
public class SpeechCommandsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SpeechCommands myCommands = (SpeechCommands)target;
        if (GUILayout.Button("Send Informations"))
        {
            myCommands.SendWords();
        }
    }
}
