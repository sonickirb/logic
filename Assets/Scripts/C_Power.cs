using System.Collections.Generic;

public class C_Power : CircuitDerive
{
    public override bool[] GetOutputs(List<bool> inputs)
    {
        bool[] outputs = new bool[1];

        outputs[0] = true;

        return outputs;
    }
}