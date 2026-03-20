using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicManager : MonoBehaviour
{

    public static LogicManager Instance;

    public Transform components;
    public Transform wires;
    
    public GameObject nodePrefab;
    public GameObject wirePrefab;

    public Material nodeOn;
    public Material wireNoNo;
    public Material select;
    public Material nodeOff;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    // FixedUpdate is called every game tick
    void FixedUpdate()
    {
        for (int i = 0; i < components.childCount; i++)
        {
            Transform component = components.GetChild(i);
            Circuit circuit = component.GetComponent<Circuit>();
            circuit.Tick();
        } 
        for (int i = 0; i < wires.childCount; i++)
        {
            Transform wire = wires.GetChild(i);
            Wire w = wire.GetComponent<Wire>();
            w.Tick();
            LineRenderer line = wire.GetComponent<LineRenderer>();
            line.material = w.from.outputs[w.output] ? nodeOn : nodeOff;
        }
    }

    public void MakeWire(Circuit from, int output, Circuit to, int input)
    {
        GameObject wire = Instantiate(wirePrefab, wires);
        Wire w = wire.AddComponent<Wire>();
        w.from = from;
        w.to = to;
        w.output = output;
        w.input = input;

        wire.GetComponent<LineRenderer>().SetPosition(0, from.transform.Find("Outputs").Find(output.ToString()).position);
        wire.GetComponent<LineRenderer>().SetPosition(1, to.transform.Find("Inputs").Find(input.ToString()).position);
    }
}
