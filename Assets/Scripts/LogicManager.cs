using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct ComponentData
{
    public string name;
    public GameObject prefab;
}

public class LogicManager : NetworkBehaviour
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
        if (!IsServer)
        {
            Debug.Log("nuh uh uh");
            return;
        }
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
        } else
        {
            Debug.LogWarning("no World data to load");
        }
    }

    void Update()
    {
        if (!IsServer) return;
        if (Input.GetKeyDown(KeyCode.P) && !autoTick) Tick();
        if (Input.GetKeyDown(KeyCode.I)) autoTick = !autoTick;
        if (Input.GetKeyDown(KeyCode.O)) SaveSystem.SaveWorldData();
    }

    // FixedUpdate is called every logic tick
    void FixedUpdate()
    {
        if (!IsServer) return;
        if (autoTick) Tick();
    }

    public void Tick()
    {
        if (!IsServer) return;
        for (int i = 0; i < components.childCount; i++)
        {
            Transform component = components.GetChild(i);
            Circuit circuit = component.GetComponent<Circuit>();

            for (int n = 0; n < circuit.inputs.Count; n++)
            {
                if (!ConnectedWiresOnInput(circuit, n)) circuit.inputs[n] = false;
            }

            circuit.Tick();
            
            UpdateCircuitClientRpc(circuit.ID, circuit.inputs.ToArray(), circuit.outputs.ToArray());
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
        if (!IsServer)
        {
            MakeWireServerRpc(from.ID, output, to.ID, input);
            return null;
        }
        GameObject wire = Instantiate(wirePrefab, wires);
        Wire w = wire.AddComponent<Wire>();
        w.ID = Random.Range(1000000, 9999999);
        w.from = from;
        w.to = to;
        w.output = output;
        w.input = input;

        wire.GetComponent<LineRenderer>().SetPosition(0, from.transform.Find("Outputs").Find(output.ToString()).position);
        wire.GetComponent<LineRenderer>().SetPosition(1, to.transform.Find("Inputs").Find(input.ToString()).position);

        MakeWireClientRpc(w.ID, from.ID, output, to.ID, input);

        return w;
    }

    public void RemoveWire(GameObject wire)
    {
        int c = wire.GetComponent<Wire>().ID;
        if (!IsServer)
        {
            DeleteWireServerRpc(c);
            return;
        }
        Destroy(wire);
        DeleteWireClientRpc(c);
    }

    public Circuit MakeCircuit(GameObject of, Vector3 at)
    {
        if (!IsServer)
        {
            MakeCircuitServerRpc(GetCircuitIDFromName(of.name), at);
            return null;
        }
        GameObject circuit = Instantiate(of, components);
        circuit.GetComponent<Circuit>().ID = Random.Range(1000000, 9999999);
        circuit.transform.position = at;
        circuit.name = of.name;
        MakeCircuitClientRpc(circuit.GetComponent<Circuit>().ID, GetCircuitIDFromName(circuit.name), at);
        return circuit.GetComponent<Circuit>();
    }

    public void RemoveCircuit(GameObject circuit)
    {
        if (!IsServer)
        {
            DeleteCircuitServerRpc(circuit.GetComponent<Circuit>().ID);
            return;
        }
        Circuit c = circuit.GetComponent<Circuit>();

        for (int i = 0; i < c.inputs.Count; i++)
        {
            if (ConnectedWiresOnInput(c, i)) return;
        }
        for (int o = 0; o < c.outputs.Count; o++)
        {
            if (ConnectedWiresOnOutput(c, o)) return;
        }

        int id = circuit.GetComponent<Circuit>().ID;

        Destroy(circuit);
        
        DeleteCircuitClientRpc(id);
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
    public Transform GetCircuitFromInstanceID(int ID)
    {
        for (int i = 0; i < components.childCount; i++)
        {
            if (components.GetChild(i).GetComponent<Circuit>().ID == ID) return components.GetChild(i);
        }
        Debug.LogError("could not find circuit of instance id " + ID);
        return null;
    }
    public Transform GetWireFromInstanceID(int ID)
    {
        for (int i = 0; i < wires.childCount; i++)
        {
            if (wires.GetChild(i).GetComponent<Wire>().ID == ID) return wires.GetChild(i);
        }
        Debug.LogError("could not find wire of instance id " + ID);
        return null;
    }

    [ServerRpc(RequireOwnership = false)]
    private void MakeCircuitServerRpc(int ID, Vector3 at)
    {
        MakeCircuit(circuitPrefabs[ID].prefab, at);
    }
    [ServerRpc(RequireOwnership = false)]
    private void DeleteCircuitServerRpc(int c)
    {
        RemoveCircuit(GetCircuitFromInstanceID(c).gameObject);
    }
    [ServerRpc(RequireOwnership = false)]
    private void MakeWireServerRpc(int fromID, int output, int toID, int input)
    {
        Circuit from = GetCircuitFromInstanceID(fromID).GetComponent<Circuit>();
        Circuit to = GetCircuitFromInstanceID(toID).GetComponent<Circuit>();
        MakeWire(from, output, to, input);
    }
    [ServerRpc(RequireOwnership = false)]
    private void DeleteWireServerRpc(int w)
    {
        RemoveWire(GetWireFromInstanceID(w).gameObject);
    }
    

    [ClientRpc(RequireOwnership = false)]
    private void MakeCircuitClientRpc(int myID, int ID, Vector3 at)
    {
        if (IsHost) return;
        GameObject of = circuitPrefabs[ID].prefab;
        GameObject circuit = Instantiate(of, components);
        circuit.GetComponent<Circuit>().ID = myID;
        circuit.transform.position = at;
        circuit.name = of.name;
    }
    [ClientRpc(RequireOwnership = false)]
    private void DeleteCircuitClientRpc(int c)
    {
        if (IsHost) return;
        Destroy(GetCircuitFromInstanceID(c).gameObject);
    }
    [ClientRpc(RequireOwnership = false)]
    private void UpdateCircuitClientRpc(int c, bool[] inputs, bool[] outputs)
    {
        if (IsHost) return;
        Circuit circuit = GetCircuitFromInstanceID(c).GetComponent<Circuit>();

        circuit.inputs = inputs.ToList<bool>();
        circuit.outputs = outputs.ToList<bool>();
    }
    [ClientRpc(RequireOwnership = false)]
    private void MakeWireClientRpc(int myID, int fromID, int output, int toID, int input)
    {
        if (IsHost) return;
        Circuit from = GetCircuitFromInstanceID(fromID).GetComponent<Circuit>();
        Circuit to = GetCircuitFromInstanceID(toID).GetComponent<Circuit>();

        GameObject wire = Instantiate(wirePrefab, wires);
        Wire w = wire.AddComponent<Wire>();
        w.ID = myID;
        w.from = from;
        w.to = to;
        w.output = output;
        w.input = input;

        wire.GetComponent<LineRenderer>().SetPosition(0, from.transform.Find("Outputs").Find(output.ToString()).position);
        wire.GetComponent<LineRenderer>().SetPosition(1, to.transform.Find("Inputs").Find(input.ToString()).position);
    }
    [ClientRpc(RequireOwnership = false)]
    private void DeleteWireClientRpc(int w)
    {
        if (IsHost) return;
        Destroy(GetWireFromInstanceID(w).gameObject);
    }

    [ClientRpc]
    private void TestClientRpc() {
        Debug.Log("TestClientRpc");
    }
}
