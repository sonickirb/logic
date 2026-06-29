using UnityEngine;

public class Wire : MonoBehaviour
{

    public int ID;

    public Circuit from;
    public int input;
    public Circuit to;
    public int output;

    LineRenderer line;

    void Start() { line = GetComponent<LineRenderer>(); }

    void Update()
    {
        line.SetPosition(0, from.transform.Find("Outputs").Find(output.ToString()).position);
        line.SetPosition(1, to.transform.Find("Inputs").Find(input.ToString()).position);
        line.material = from.inputs[output] ? LogicManager.Instance.nodeOn : LogicManager.Instance.nodeOff;
    }

    public void Tick()
    {
        to.inputs[input] = from.outputs[output];
    }
}