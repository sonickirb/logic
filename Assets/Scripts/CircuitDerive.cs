using System;
using System.Collections.Generic;
using UnityEngine;

public class CircuitDerive : MonoBehaviour
{

    public virtual bool[] GetOutputs(List<bool> inputs)
    {
        bool[] outputs = new bool[inputs.Count];

        for (int i = 0; i < outputs.Length; i++) outputs[i] = inputs[i];

        return outputs;
    }
}