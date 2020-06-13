using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(RawImage))]
public class MicrophoneInput : MonoBehaviour
{
    private enum Word { none = 0, first = 1, second = 2, third = 3 }
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
    [HideInInspector] public List<float> thirdWord = new List<float>();
    [HideInInspector] public List<float> wholeClip = new List<float>();

    //variables for audio waveform drawing
    private int width = 500;
    private int height = 100;
    private Color backgroundColor = Color.black;
    private Color waveformColor = Color.green;
    private int size = 2048;
    private Color[] blank;
    private Texture2D texture;

    private void Start()
    {
        selectedDevice = Microphone.devices[0].ToString();
        source = GetComponent<AudioSource>();
    }

    public void StopRecording()
    {
        if (isRecording)
        {
            isRecording = false;
            Microphone.End(selectedDevice);

            samples = new float[source.clip.samples * source.clip.channels];
            source.clip.GetData(samples, 0);

            ProcessAudioClip();
        }
    }

    public void StartRecording()
    {
        HandleNewRecording();
        isRecording = true;
        source.clip = Microphone.Start(selectedDevice, true, 5, AudioSettings.outputSampleRate);
        Debug.Log(source.clip.channels);
        source.Play();
        Invoke("StopRecording", 4);
    }
    
    private void ProcessAudioClip()
    {
        //Cut off the empty space in front and end of Clip
        int i = 0;
        float clipLoudness = 0;
        List<float> buffer = new List<float>();
        foreach (float f in samples)
        {
            wholeClip.Add(f);
            if (i < 512)
            {
                buffer.Add(f);
                clipLoudness += Mathf.Abs(f);
            }
            else
            {
                WriteToWordList(clipLoudness, buffer);
                i = 0;
                clipLoudness = 0;
                buffer.Clear();
            }
            i++;

            if (f != 0)
                processedSamples.Add(f);
        }
        Debug.Log("Recorded Clip lenght: " + samples.Length.ToString() + "\n");

        Debug.Log("first word lenght: " + firstWord.Count().ToString() + "\n");
        Debug.Log("second word lenght: " + secondWord.Count().ToString() + "\n");
        Debug.Log("third word lenght: " + thirdWord.Count().ToString() + "\n");
        CreateTexture();
        StartCoroutine(UpdateWaveForm());
    }
    
    //initialization for audio waveform drawing
    private void CreateTexture()
    {
        texture = new Texture2D(width, height);
        GetComponent<RawImage>().texture = texture;
        blank = new Color[width * height];

        //create blank screen image
        for (int a = 0; a < size; a++)
        {
            blank[a] = backgroundColor;
        }
    }

    IEnumerator UpdateWaveForm()
    {
        //refresh display each 100ms
        while (true)
        {
            GetCurWave();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void GetCurWave()
    {
        //texture.SetPixel(blank, 0);
        texture.SetPixel(0, 0, Color.white);

        for (int i = 0; i < wholeClip.Count(); i++)
        {
            texture.SetPixel((int)(width * i / wholeClip.Count()), (int)(height * (wholeClip[i] + 1f) / 2f), waveformColor);
        }
        texture.Apply();
    }

    private void WriteToWordList(float loudness, List<float> buffer)
    {
        if (loudness > 15 && word == Word.none)
        {
            wordCount++;
            word = (Word)wordCount;
        }
        else if (word != Word.none && loudness < 15)
        {
            word = Word.none;
        }
        if (loudness > 15)
        {
            switch (word)
            {
                case Word.first:
                    firstWord.AddRange(buffer);
                    break;
                case Word.second:
                    secondWord.AddRange(buffer);
                    break;
                case Word.third:
                    thirdWord.AddRange(buffer);
                    break;
            }
        }
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
        thirdWord.Clear();
        processedSamples.Clear();
    }
}