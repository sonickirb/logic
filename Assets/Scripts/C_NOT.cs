using System.Collections.Generic;

public class C_NOT : CircuitDerive
{
    public override bool[] GetOutputs(List<bool> inputs)
    {
        bool[] outputs = new bool[inputs.Count];

        for (int i = 0; i < outputs.Length; i++) outputs[i] = !inputs[i];

        return outputs;
    }
}