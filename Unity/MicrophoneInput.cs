using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

    private List<float> firstWord = new List<float>();
    private List<float> secondWord = new List<float>();
    private List<float> thirdWord = new List<float>();

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!isRecording)
                StartRecording();
            else
                StopRecording();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(PlayRecording());
        }
    }

    private void StopRecording()
    {
        isRecording = false;
        Microphone.End(selectedDevice);

        samples = new float[source.clip.samples * source.clip.channels];
        source.clip.GetData(samples, 0);

        ProcessAudioClip();
    }

    void StartRecording()
    {
        isRecording = true;
        source.clip = Microphone.Start(selectedDevice, true, 5, AudioSettings.outputSampleRate);
        Debug.Log(source.clip.channels);
        source.Play();
    }

    private void ProcessAudioClip()
    {
        //Cut off the empty space in front and end of Clip
        int i = 0;
        float clipLoudness = 0;
        List<float> buffer = new List<float>();
        foreach (float f in samples)
        {
            if (i < 512)
            {
                buffer.Add(f);
                clipLoudness += Mathf.Abs(f);
            }
            else
            {
                Debug.Log(clipLoudness);
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


        //initialization for audio waveform drawing
        texture = new Texture2D(width, height);
        GetComponent<RawImage>().texture = texture;
        blank = new Color[width * height];
        
        //create blank screen image
        for(int a = 0; a<size; a++)
        {
            blank[a] = backgroundColor;
        }

        StartCoroutine(UpdateWaveForm());
    }

    IEnumerator UpdateWaveForm()
    {
        //refresh display each 100ms
        while(true)
        {
            GetCurWave();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void GetCurWave()
    {
        texture.SetPixel(blank, 0);
        source.GetOutputData(samples, 0);
        for(int i = 0; i<size; i++)
        {
            texture.SetPixel((int)(width * i / size), (int)(height * (samples[i] + 1f) / 2f), waveformColor);
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

    private IEnumerator PlayRecording()
    {
        int channels = source.clip.channels;
        int freq = source.clip.frequency;
        if (firstWord.Count > 0)
        {
            AudioClip clip = AudioClip.Create("firstWord", firstWord.Count(), channels, freq, false);
            clip.SetData(firstWord.ToArray(), 0);
            source.clip = clip;
            source.Play();
            yield return new WaitForSeconds(clip.length + 1);
            source.Stop();
        }
        if (secondWord.Count > 0)
        {
            AudioClip clip = AudioClip.Create("secondWord", secondWord.Count(), channels, freq, false);
            clip.SetData(secondWord.ToArray(), 0);
            source.clip = clip;
            source.Play();
            yield return new WaitForSeconds(clip.length + 1);
            source.Stop();
        }
        if (thirdWord.Count > 0)
        {
            AudioClip clip = AudioClip.Create("thirdWord", thirdWord.Count(), channels, freq, false);
            clip.SetData(secondWord.ToArray(), 0);
            source.clip = clip;
            source.Play();
            yield return new WaitForSeconds(clip.length + 1);
            source.Stop();
        }
    }
}
