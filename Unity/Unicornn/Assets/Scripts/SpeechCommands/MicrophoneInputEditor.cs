using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MicrophoneInput))]
public class MicrophoneInputEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MicrophoneInput myMicrophoneInput = (MicrophoneInput)target;
        if (GUILayout.Button("Start Recording"))
        {
            myMicrophoneInput.StartRecording();
        }
        if(GUILayout.Button("Stop Recording"))
        {
            myMicrophoneInput.StopRecording();
        }
        if(GUILayout.Button("Play first word"))
        {
            if (myMicrophoneInput.firstWord.Count > 0) 
                myMicrophoneInput.PlayRecording(myMicrophoneInput.firstWord.ToArray());
        }
        if(GUILayout.Button("Play second word"))
        {
            if (myMicrophoneInput.secondWord.Count > 0) 
                myMicrophoneInput.PlayRecording(myMicrophoneInput.secondWord.ToArray());
        }
        if(GUILayout.Button("Play All"))
        {
            if (myMicrophoneInput.wholeClip.Count > 0)
                myMicrophoneInput.PlayRecording(myMicrophoneInput.wholeClip.ToArray());
        }
    }
}
