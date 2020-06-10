using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MicrophoneInput : MonoBehaviour
{
    private float[] samples;
    [SerializeField]
    private string selectedDevice;
    private bool isRecording;
    private AudioSource source;

    private void Start()
    {
        selectedDevice = Microphone.devices[0].ToString();
        source = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!isRecording)
                StartRecording();
            else
                StopRecording();
        }
    }

    private void StopRecording()
    {
        isRecording = false;
        Microphone.End(selectedDevice);
        source.Play();

        samples = new float[source.clip.samples * source.clip.channels];
        source.clip.GetData(samples, 0);
    }

    void StartRecording()
    {
        isRecording = true;
        source.clip = Microphone.Start(selectedDevice, true, 5, AudioSettings.outputSampleRate);
        source.Play();
    }
}
