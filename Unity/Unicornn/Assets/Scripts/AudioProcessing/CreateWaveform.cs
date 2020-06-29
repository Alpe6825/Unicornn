using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class CreateWaveform : MonoBehaviour
{
    private int width = 1000;
    private int height = 100;
    private Color waveformColor = Color.green;
    private Texture2D texture;

    public void CreateTexture(List<float> clip)
    {
        texture = new Texture2D(width, height);
        GetComponent<RawImage>().texture = texture;
        SetCurWave(clip);
    }

    private void SetCurWave(List<float> clip)
    {
        texture.SetPixel(0, 0, Color.white);

        for (int i = 0; i < clip.Count; i++)
        {
            texture.SetPixel((int)(width * i / clip.Count), (int)(height * (clip[i] + 1f) / 2f), waveformColor);
        }
        texture.Apply();
    }
}
