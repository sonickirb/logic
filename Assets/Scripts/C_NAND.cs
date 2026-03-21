using System.Collections.Generic;

public class C_NAND : CircuitDerive
{
    public override bool[] GetOutputs(List<bool> inputs)
    {
        bool[] outputs = new bool[1];

        outputs[0] = !(inputs[0] == inputs[1]);

        return outputs;
    }
}