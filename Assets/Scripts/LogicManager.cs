using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ComponentData
{
    public string name;
    public GameObject prefab;
}

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

    public List<ComponentData> circuitPrefabs;

    void Awake()
    {
        Instance = this;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        // eventually we should do this in some kind of main-menu instead
        WorldData data = SaveSystem.LoadWorldData();
        if (data != null)
        {
            Circuit[] circuitIndex = new Circuit[data.components.Length];
            for (int i = 0; i < data.components.Length; i++)
            {
                // to give you a POSITION at THIS JOB youd have to SLAVE AWAY for ONE DOLLAR AN HOUR
                Vector3 position = new Vector3(data.positionX[i], data.positionY[i], data.positionZ[i]);
                circuitIndex[i] = MakeCircuit(circuitPrefabs[data.components[i]].prefab, position);
                circuitIndex[i].Tick();
                circuitIndex[i].UpdateNodes();
            }
            foreach (Circuit c in circuitIndex) {
                c.Tick();
                c.UpdateNodes();
            }
            for (int i = 0; i < data.connectionsFrom.Length; i++)
            {
                Circuit from = circuitIndex[data.connectionsFrom[i]];
                Circuit to   = circuitIndex[data.connectionsTo[i]];
                int input    = data.input[i];
                int output   = data.output[i];

                Wire wire = MakeWire(from, output, to, input);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) autoTick = !autoTick;
        if (Input.GetKeyDown(KeyCode.P) && !autoTick) Tick();
        if (Input.GetKeyDown(KeyCode.O)) SaveSystem.SaveWorldData();
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

    public Wire MakeWire(Circuit from, int output, Circuit to, int input)
    {
        GameObject wire = Instantiate(wirePrefab, wires);
        Wire w = wire.AddComponent<Wire>();
        w.from = from;
        w.to = to;
        w.output = output;
        w.input = input;

        wire.GetComponent<LineRenderer>().SetPosition(0, from.transform.Find("Outputs").Find(output.ToString()).position);
        wire.GetComponent<LineRenderer>().SetPosition(1, to.transform.Find("Inputs").Find(input.ToString()).position);

        return w;
    }

    public Circuit MakeCircuit(GameObject of, Vector3 at)
    {
        GameObject circuit = Instantiate(of, components);
        circuit.transform.position = at;
        circuit.name = of.name;
        return circuit.GetComponent<Circuit>();
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

    public int GetCircuitIDFromName(string name)
    {
        for (int i = 0; i < circuitPrefabs.Count; i++)
        {
            ComponentData c = circuitPrefabs[i];
            if (c.name == name) return i;
        }
        Debug.LogError("Circuit named \"" + name + "\" does not exist");
        return -1;
    }
}
