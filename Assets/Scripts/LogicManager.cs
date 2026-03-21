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

    public bool autoTick;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) autoTick = !autoTick;
        if (Input.GetKeyDown(KeyCode.P) && !autoTick) Tick();
    }

    // FixedUpdate is called every logic tick
    void FixedUpdate()
    {
        if (autoTick) Tick();
    }

    public void Tick()
    {
        for (int i = 0; i < components.childCount; i++)
        {
            Transform component = components.GetChild(i);
            Circuit circuit = component.GetComponent<Circuit>();

            for (int n = 0; n < circuit.inputs.Count; n++)
            {
                if (!ConnectedWiresOnInput(circuit, n)) circuit.inputs[n] = false;
            }

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

    public bool ConnectedWiresOnInput(Circuit circuit, int input)
    {
        for (int i = 0; i < wires.childCount; i++)
        {
            Transform wire = wires.GetChild(i);
            Wire w = wire.GetComponent<Wire>();
            if (w.to == circuit && w.input == input) return true;
        }
        return false;
    }
    public bool ConnectedWiresOnOutput(Circuit circuit, int output)
    {
        for (int i = 0; i < wires.childCount; i++)
        {
            Transform wire = wires.GetChild(i);
            Wire w = wire.GetComponent<Wire>();
            if (w.from == circuit && w.output == output) return true;
        }
        return false;
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

    public void MakeCircuit(GameObject of, Vector3 at)
    {
        GameObject circuit = Instantiate(of, components);
        circuit.transform.position = at;
    }

    public void RemoveCircuit(GameObject circuit)
    {
        Circuit c = circuit.GetComponent<Circuit>();

        for (int i = 0; i < c.inputs.Count; i++)
        {
            if (ConnectedWiresOnInput(c, i)) return;
        }
        for (int o = 0; o < c.outputs.Count; o++)
        {
            if (ConnectedWiresOnOutput(c, o)) return;
        }

        Destroy(circuit);
    }
}
