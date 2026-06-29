using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour
{

    public int ID;

    public Circuit from;
    public int input;
    public Circuit to;
    public int output;

    LineRenderer line;

    void OnEnable() { 
        line = GetComponent<LineRenderer>();
    }
    public void SetFromAndTo(Circuit f, Circuit t)
    {
        if (from != null) from.RemoveWireOutput(this, output);
        if (to != null) to.RemoveWireInput(this, input);

        from = f;
        to = t;
        
        from.AddWireOutput(this, output);
        to.AddWireInput(this, input);
    }
    void OnDestroy()
    {
        from.RemoveWireOutput(this, output);
        to.RemoveWireInput(this, input);
    }

    // render
    void Update()
    {
        line.SetPosition(0, from.transform.Find("Outputs").Find(output.ToString()).position);
        line.SetPosition(1, to.transform.Find("Inputs").Find(input.ToString()).position);
        line.material = from.outputs[output] ? LogicManager.Instance.nodeOn : LogicManager.Instance.nodeOff;
    }

    public void Tick()
    {
        bool isOn = false;
        int AMOUNT = 0;
        foreach (Wire w in LogicManager.Instance.ConnectedWiresOnInput(to, input)) {
            if (w.from.outputs[w.output]) {
                isOn = true;
                AMOUNT++;
            }
        }
        //Debug.Log(transform.name + " " + AMOUNT);
        to.inputs[input] = isOn;
    }
}