using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class Agent : MonoBehaviour
{

    public NNModel modelSource;
    Model model;
    IWorker worker;

    void Start()
    {
        model = ModelLoader.Load(modelSource);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);

        Texture2D texture = new Texture2D(32, 32, TextureFormat.Alpha8, false);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, Color.black);
            }
        }
        texture.Apply();
        //Debug.Log(texture.GetPixel(0, 0));

        int predictedClass = Unicornn(texture);
        Debug.Log("PredictedClass für leere Textur: " + predictedClass);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    int Unicornn(Texture2D texture)
    {
        var tensor = new Tensor(texture, 1);
        //print("T:" + tensor[0, 0, 0, 0]);
        var shape = tensor.shape;
        //Debug.Log(shape + " or " + shape.batch + shape.height + shape.width + shape.channels);

        worker.Execute(tensor);
        tensor.Dispose();

        tensor = worker.PeekOutput();
        shape = tensor.shape;

        float highestValue = -1;
        int predictedClass = 0;
        for (int i = 0; i < 28; i++)
        {
            if (tensor[0, 0, 0, i] > highestValue)
            {
                highestValue = tensor[0, 0, 0, i];
                predictedClass = i;
            }
        }
        //Debug.Log("PredictedClass: " + predictedClass);

        tensor.Dispose();
        worker.Dispose();

        return predictedClass;
    }


}
