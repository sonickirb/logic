using UnityEngine;

public class Wire : MonoBehaviour
{

    public Circuit from;
    public int input;
    public Circuit to;
    public int output;

    public void Tick()
    {
        to.inputs[input] = from.outputs[output];
    }
}