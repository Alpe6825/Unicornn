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
    private enum Word { none = 0, first = 1, second = 2}
    private Word word;
    private int wordCount;
    private float[] samples;

    private List<float> processedSamples = new List<float>();

    [SerializeField]
    private string selectedDevice;
    private bool isRecording;
    private AudioSource source;

    [HideInInspector] public List<float> firstWord = new List<float>();
    [HideInInspector] public List<float> secondWord = new List<float>();
    [HideInInspector] public List<float> wholeClip = new List<float>();

    private void Start()
    {
        selectedDevice = Microphone.devices[0].ToString();
        source = GetComponent<AudioSource>();
    }

    #region Recording Buttons
    public void StopRecording()
    {
        if (isRecording)
        {
            isRecording = false;
            Microphone.End(selectedDevice);

            samples = new float[source.clip.samples * source.clip.channels];
            source.clip.GetData(samples, 0);

            HandleAudioBuffer();
        }
    }
    public void StartRecording()
    {
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
        wordCount = 0;
        firstWord.Clear();
        secondWord.Clear();
        processedSamples.Clear();
    }
    #endregion


    public int bufferSize = 1024;
    public float threshhold = 15;
    public float wrapperTimer = 0.1f;
    //Create List of Buffers
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

    //Iterate through and see if there are following buffers over threshhold
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
        for (int i = index; i < index+10; i++)
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
        for (int i = 0; i * timeFactor < wrapperTimer; i++)
        {
            if (startingPoint - i > 0)
                frontWrapper.Add(buffers[startingPoint - i]);
        }
        frontWrapper.Reverse();
        finalBuffering.AddRange(frontWrapper);

        //Main Series
        finalBuffering.AddRange(series);

        //EndWrapper
        for (int j = 0; j * timeFactor < wrapperTimer; j++) 
        {
            if (index + j < buffers.Count)
                finalBuffering.Add(buffers[index + j]);
        }

        if (seriesCounter == 0)
        {
            firstWord = DeserializedSeries(finalBuffering);
        }
        else if(seriesCounter == 1)
        {
            secondWord = DeserializedSeries(finalBuffering);
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


    /*
    private void CutAudioFile()
    {
        float clipLoudness = 0;
        int bufferCounter = 0;
        List<float> buffer = new List<float>();
        List<float> preBuffer = new List<float>();

        for (int i = 0; i < samples.Length; i++)
        {
            wholeClip.Add(samples[i]);

            if (bufferCounter < 1024)
            {
                buffer.Add(samples[i]);
                clipLoudness += Mathf.Abs(samples[i]);
                bufferCounter++;
            }

            if (bufferCounter >= 1024)
            {
                WriteToWordList(clipLoudness, buffer, preBuffer);
                bufferCounter = 0;
                clipLoudness = 0;
                preBuffer = buffer;
                buffer.Clear();
            }

            if (samples[i] != 0)
                processedSamples.Add(samples[i]);
        }
    }

    private void WriteToWordList(float loudness, List<float> buffer, List<float> preBuffer)
    {
        if (loudness > 10 && word == Word.none)
        {
            wordCount++;
            word = (Word)wordCount;
            AddToWord(word, preBuffer);
        }
        else if (word != Word.none && loudness < 10)
        {
            AddToWord(word, buffer);
            word = Word.none;
        }
        if (loudness > 10)
        {
            AddToWord(word, buffer);
        }
    }

    private void AddToWord(Word word, List<float> buffer)
    {
        switch (word)
        {
            case Word.first:
                firstWord.AddRange(buffer);
                break;
            case Word.second:
                secondWord.AddRange(buffer);
                break;
        }
    }
    */
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