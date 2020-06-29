using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class MicrophoneInput : MonoBehaviour
{
    CreateWaveform firstWordUI;
    CreateWaveform secondWordUI;

    private float[] samples;

    [SerializeField]
    private string selectedDevice;
    private bool isRecording;
    private AudioSource source;

    [HideInInspector] public List<float> firstWord = new List<float>();
    [HideInInspector] public List<float> secondWord = new List<float>();

    private void Start()
    {
        selectedDevice = Microphone.devices[0].ToString();
        source = GetComponent<AudioSource>();
        firstWordUI = GameObject.Find("FirstWordUI").GetComponent<CreateWaveform>();
        secondWordUI = GameObject.Find("SecondWordUI").GetComponent<CreateWaveform>();
    }

    #region Recording Buttons
    public void StopRecording()
    {
        if (isRecording)
        {
            source.volume = 1;
            isRecording = false;
            Microphone.End(selectedDevice);

            samples = new float[source.clip.samples * source.clip.channels];
            source.clip.GetData(samples, 0);

            HandleAudioBuffer();
        }
    }
    public void StartRecording()
    {
        source.volume = 0;
        HandleNewRecording();
        isRecording = true;
        source.clip = Microphone.Start(selectedDevice, true, 5, AudioSettings.outputSampleRate);
        source.Play();
        Invoke("StopRecording", 4);
    }
    public void PlayRecording(float[] clip)
    {
        source.Stop();
        int channels = source.clip.channels;
        int freq = source.clip.frequency;
        AudioClip newAudioClip = AudioClip.Create("clip", clip.Length, channels, freq, false);
        newAudioClip.SetData(clip, 0);
        source.clip = newAudioClip;
        source.Play();
    }
    private void HandleNewRecording()
    {
        firstWord.Clear();
        secondWord.Clear();
    }
    #endregion


    #region Handle Audio splicing
    public int bufferSize = 1024;
    public float threshhold = 15;
    public float frontWrapperTimer = 0.1f;
    public float endWrapperTimer = 0.1f;
    private void HandleAudioBuffer()
    {
        List<Buffer> allBuffers = new List<Buffer>();

        int bufferCounter = 0;
        float bufferLoudness = 0;

        List<float> buffer = new List<float>();
        for (int i = 0; i < samples.Length; i++)
        {
            if(bufferCounter == 0)
            {
                buffer = new List<float>();
            }
            if (bufferCounter < bufferSize)
            {
                bufferLoudness += Mathf.Abs(samples[i]);
                bufferCounter++;
                buffer.Add(samples[i]);
            }
            if (bufferCounter >= bufferSize)
            {
                Buffer convertedBuffer = new Buffer(buffer, bufferLoudness);
                allBuffers.Add(convertedBuffer);
                bufferCounter = 0;
                bufferLoudness = 0;
            }
        }
        FindSeries(allBuffers);
    }
    private void FindSeries(List<Buffer> buffers)
    {
        bool foundSeries = false;
        int seriesCounter = 0;

        List<Buffer> series = new List<Buffer>();
        for (int i = 0; i < buffers.Count; i++)
        {
            if(CheckFutureBuffer(i, buffers))
            {
                foundSeries = true;
                series.Add(buffers[i]);
            }
            else if (foundSeries)
            {
                WrapUpSeries(i, series, buffers, seriesCounter);
                foundSeries = false;
                series = new List<Buffer>();
                seriesCounter++;
            }
        }
    }
    private bool CheckFutureBuffer(int index, List<Buffer> buffers)
    {
        int trueCounter = 0;
        int falseCounter = 0;
        for (int i = index; i < index+25; i++)
        {
            if (buffers.Count > i)
            {
                if (buffers[i].bufferLoudness > threshhold)
                {
                    trueCounter++;
                }
                else
                {
                    falseCounter++;
                }
            }
        }
        
        if(trueCounter > falseCounter)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void WrapUpSeries(int index, List<Buffer> series, List<Buffer> buffers, int seriesCounter)
    {
        List<Buffer> finalBuffering = new List<Buffer>();
        float timeFactor = (float)bufferSize / AudioSettings.outputSampleRate;

        int startingPoint = index - series.Count;

        //FrontWrapper
        List<Buffer> frontWrapper = new List<Buffer>();
        for (int i = 0; i * timeFactor < frontWrapperTimer; i++)
        {
            if (startingPoint - i > 0)
                frontWrapper.Add(buffers[startingPoint - i]);
        }
        frontWrapper.Reverse();
        finalBuffering.AddRange(frontWrapper);

        //Main Series
        finalBuffering.AddRange(series);

        //EndWrapper
        for (int j = 0; j * timeFactor < endWrapperTimer; j++) 
        {
            if (index + j < buffers.Count)
                finalBuffering.Add(buffers[index + j]);
        }

        if (seriesCounter == 0)
        {
            firstWord = DeserializedSeries(finalBuffering);
            if (firstWordUI != null)
                firstWordUI.CreateTexture(firstWord);
        }
        else if(seriesCounter == 1)
        {
            secondWord = DeserializedSeries(finalBuffering);
            if (secondWordUI != null) 
                secondWordUI.CreateTexture(secondWord);
        }
        else
        {
            Debug.Log("Too many Words");
        }  
    }
    private List<float> DeserializedSeries(List<Buffer> series)
    {
        List<float> deSeries = new List<float>();
        for (int i = 0; i < series.Count; i++)
        {
            deSeries.AddRange(series[i].value);
        }
        return deSeries;
    }
    #endregion
}

public class Buffer
{
    public List<float> value;
    public float bufferLoudness;

    public Buffer(List<float> values, float loudness)
    {
        value = values;
        bufferLoudness = loudness;
    }
}