using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class Agent : MonoBehaviour
{

    public NNModel modelSource;

    // Start is called before the first frame update
    void Start()
    {
        var model = ModelLoader.Load(modelSource);
        var worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);

        var tensor = new Tensor(1, 2, 3, 1);
        tensor[0, 0, 0, 0] = 1.0f;
        tensor[0, 0, 1, 0] = 2.0f;
        tensor[0, 0, 2, 0] = 3.0f;
        tensor[0, 1, 0, 0] = 4.0f;
        tensor[0, 1, 1, 0] = 5.0f;
        tensor[0, 1, 2, 0] = 6.0f;

        var shape = tensor.shape;
        Debug.Log(shape + " or " + shape.batch + shape.height + shape.width + shape.channels);

        //string[] outputNames = model.outputs; // query model outputs

        worker.Execute(tensor);
        tensor.Dispose();

        tensor = worker.PeekOutput();
        Debug.Log(tensor);
        shape = tensor.shape;
        Debug.Log(shape + " or " + shape.batch + shape.height + shape.width + shape.channels);
        Debug.Log(tensor[0, 0, 0, 0]);
        Debug.Log(tensor[0, 0, 1, 0]);
        Debug.Log(tensor[0, 0, 2, 0]);
        Debug.Log(tensor[0, 1, 0, 0]);
        Debug.Log(tensor[0, 1, 1, 0]);
        Debug.Log(tensor[0, 1, 2, 0]);


        tensor.Dispose();
        worker.Dispose();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
