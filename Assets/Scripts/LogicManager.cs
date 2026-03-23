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
    public Material pixelOff;
    public Material pixelOn;

    public bool autoTick;

    public List<ComponentData> circuitPrefabs;

    public bool loading = true;

    void Awake()
    {
        Instance = this;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientJoined;
        NetworkManager.Singleton.OnServerStarted += LoadWorld;
    }

    public void LoadWorld()
    {
        WorldData data = SaveSystem.LoadWorldData();
        if (data != null)
        {
            Circuit[] circuitIndex = new Circuit[data.components.Length];
            int inputIndex = 0;
            int outputIndex = 0;
            for (int i = 0; i < data.components.Length; i++)
            {
                // to give you a POSITION at THIS JOB youd have to SLAVE AWAY for ONE DOLLAR AN HOUR
                Vector3 position = new Vector3(data.positionX[i], data.positionY[i], data.positionZ[i]);
                circuitIndex[i] = MakeCircuit(circuitPrefabs[data.components[i]].prefab, position);
                circuitIndex[i].Tick();
                circuitIndex[i].UpdateNodes();
                
                for (int j = 0; j < data.compInputCount[i]; j++)
                {
                    while (circuitIndex[i].inputs.Count < data.compInputCount[i]) circuitIndex[i].inputs.Add(false);
                    circuitIndex[i].inputs[j] = data.inputStates[inputIndex];
                    inputIndex += 1;
                }
                for (int j = 0; j < data.compOutputCount[i]; j++)
                {
                    while (circuitIndex[i].outputs.Count < data.compOutputCount[i]) circuitIndex[i].outputs.Add(false);
                    circuitIndex[i].outputs[j] = data.outputStates[outputIndex];
                    outputIndex += 1;
                }
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

    public void OnClientJoined(ulong client)
    {
        if (IsHost) return;

        List<int> circuitIDs = new List<int>();
        List<int> circuitInstanceIDs = new List<int>();
        List<Vector3> circuitPositions = new List<Vector3>();
        List<int> wireIDs = new List<int>();
        List<int> wireFromIDs = new List<int>();
        List<int> wireOutputs = new List<int>();
        List<int> wireToIDs = new List<int>();
        List<int> wireInputs = new List<int>();
        List<int> inputCounts = new List<int>();
        List<bool> inputs = new List<bool>();
        List<int> outputCounts = new List<int>();
        List<bool> outputs = new List<bool>();

        for (int i = 0; i < components.childCount; i++)
        {
            Circuit circuit = components.GetChild(i).GetComponent<Circuit>();

            circuitIDs.Add(GetCircuitIDFromName(circuit.transform.name));
            circuitInstanceIDs.Add(circuit.ID);
            circuitPositions.Add(circuit.transform.position);
            inputCounts.Add(circuit.inputs.Count);
            for (int j = 0; j < circuit.inputs.Count; j++) inputs.Add(circuit.inputs[j]);
            outputCounts.Add(circuit.outputs.Count);
            for (int j = 0; j < circuit.outputs.Count; j++) outputs.Add(circuit.outputs[j]);
        }
        for (int i = 0; i < wires.childCount; i++)
        {
            Wire wire = wires.GetChild(i).GetComponent<Wire>();

            wireIDs.Add(wire.ID);
            wireFromIDs.Add(wire.from.ID);
            wireOutputs.Add(wire.output);
            wireToIDs.Add(wire.to.ID);
            wireInputs.Add(wire.input);
        }

        UpdateWorldClientRpc(circuitIDs.ToArray(), circuitInstanceIDs.ToArray(), circuitPositions.ToArray(), wireIDs.ToArray(), wireFromIDs.ToArray(), wireOutputs.ToArray(), 
            wireToIDs.ToArray(), wireInputs.ToArray(), inputCounts.ToArray(), inputs.ToArray(), outputCounts.ToArray(), outputs.ToArray());
    }

    void Update()
    {
        if (!IsServer) return;
        loading = false;
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

        int[] networkIDs = new int[components.childCount];
        bool[][] networkInputs = new bool[components.childCount][];
        bool[][] networkOutputs = new bool[components.childCount][];

        for (int i = 0; i < components.childCount; i++)
        {
            Transform component = components.GetChild(i);
            Circuit circuit = component.GetComponent<Circuit>();

            for (int n = 0; n < circuit.inputs.Count; n++)
            {
                if (ConnectedWiresOnInput(circuit, n).Count < 1) circuit.inputs[n] = false;
            }

            circuit.Tick();
            
            //UpdateCircuitClientRpc(circuit.ID, circuit.inputs.ToArray(), circuit.outputs.ToArray());
            networkIDs[i] = circuit.ID;
            networkInputs[i] = circuit.inputs.ToArray();
            networkOutputs[i] = circuit.outputs.ToArray();

            circuit.Extra();
        }
        for (int i = 0; i < wires.childCount; i++)
        {
            Transform wire = wires.GetChild(i);
            Wire w = wire.GetComponent<Wire>();
            w.Tick();
            LineRenderer line = wire.GetComponent<LineRenderer>();
            line.material = w.from.outputs[w.output] ? nodeOn : nodeOff;
        }

        UpdateCircuitsClientRpc(networkIDs, networkInputs, networkOutputs);
    }

    public List<Wire> ConnectedWiresOnInput(Circuit circuit, int input)
    {
        List<Wire> connectedWires = new List<Wire>();
        for (int i = 0; i < wires.childCount; i++)
        {
            Transform wire = wires.GetChild(i);
            Wire w = wire.GetComponent<Wire>();
            if (w.to == circuit && w.input == input) connectedWires.Add(w);
        }
        return connectedWires;
    }
    public List<Wire> ConnectedWiresOnOutput(Circuit circuit, int output)
    {
        List<Wire> connectedWires = new List<Wire>();
        for (int i = 0; i < wires.childCount; i++)
        {
            Transform wire = wires.GetChild(i);
            Wire w = wire.GetComponent<Wire>();
            if (w.from == circuit && w.output == output) connectedWires.Add(w);
        }
        return connectedWires;
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
            if (ConnectedWiresOnInput(c, i).Count > 0) return;
        }
        for (int o = 0; o < c.outputs.Count; o++)
        {
            if (ConnectedWiresOnOutput(c, o).Count > 0) return;
        }

        int id = circuit.GetComponent<Circuit>().ID;

        Destroy(circuit);
        
        DeleteCircuitClientRpc(id);
    }

    public void PressButton(LogicButton button)
    {
        if (!IsServer)
        {
            PressButtonServerRpc(button.transform.parent.GetComponent<Circuit>().ID);
            return;
        }
        button.OnPress();
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
    [ServerRpc(RequireOwnership = false)]
    private void PressButtonServerRpc(int c)
    {
        PressButton(GetCircuitFromInstanceID(c).Find("Button").GetComponent<LogicButton>());
    }
    

    [ClientRpc(RequireOwnership = false)]
    private void MakeCircuitClientRpc(int myID, int ID, Vector3 at)
    {
        if (IsHost || loading) return;
        GameObject of = circuitPrefabs[ID].prefab;
        GameObject circuit = Instantiate(of, components);
        circuit.GetComponent<Circuit>().ID = myID;
        circuit.transform.position = at;
        circuit.name = of.name;
    }
    [ClientRpc(RequireOwnership = false)]
    private void DeleteCircuitClientRpc(int c)
    {
        if (IsHost || loading) return;
        Destroy(GetCircuitFromInstanceID(c).gameObject);
    }
    [ClientRpc(RequireOwnership = false)]
    private void UpdateCircuitClientRpc(int c, bool[] inputs, bool[] outputs)
    {
        if (IsHost || loading) return;
        Circuit circuit = GetCircuitFromInstanceID(c).GetComponent<Circuit>();

        circuit.inputs = inputs.ToList<bool>();
        circuit.outputs = outputs.ToList<bool>();

        circuit.Extra();

        for (int i = 0; i < circuit.inputs.Count; i++)
        {
            List<Wire> connected = ConnectedWiresOnInput(circuit, i);
            for (int w = 0; w < connected.Count; w++)
            {
                Wire wire = connected[w];
                LineRenderer line = wire.GetComponent<LineRenderer>();
                line.material = circuit.inputs[i] ? nodeOn : nodeOff;
            }
        }
        for (int o = 0; o < circuit.outputs.Count; o++)
        {
            List<Wire> connected = ConnectedWiresOnInput(circuit, o);
            for (int w = 0; w < connected.Count; w++)
            {
                Wire wire = connected[w];
                LineRenderer line = wire.GetComponent<LineRenderer>();
                line.material = circuit.outputs[o] ? nodeOn : nodeOff;
            }
        }
    }
    [ClientRpc(RequireOwnership = false)]
    private void UpdateCircuitsClientRpc(int[] IDs, bool[][] cInputs, bool[][] cOutputs)
    {
        if (IsHost || loading) return;
        for (int j = 0; j < IDs.Length; j++)
        {
            int c = IDs[j];
            bool[] inputs = cInputs[j];
            bool[] outputs = cOutputs[j];
            Circuit circuit = GetCircuitFromInstanceID(c).GetComponent<Circuit>();

            circuit.inputs = inputs.ToList();
            circuit.outputs = outputs.ToList();

            circuit.Extra();

            for (int i = 0; i < circuit.inputs.Count; i++)
            {
                List<Wire> connected = ConnectedWiresOnInput(circuit, i);
                for (int w = 0; w < connected.Count; w++)
                {
                    Wire wire = connected[w];
                    LineRenderer line = wire.GetComponent<LineRenderer>();
                    line.material = circuit.inputs[i] ? nodeOn : nodeOff;
                }
            }
            for (int o = 0; o < circuit.outputs.Count; o++)
            {
                List<Wire> connected = ConnectedWiresOnInput(circuit, o);
                for (int w = 0; w < connected.Count; w++)
                {
                    Wire wire = connected[w];
                    LineRenderer line = wire.GetComponent<LineRenderer>();
                    line.material = circuit.outputs[o] ? nodeOn : nodeOff;
                }
            }
        }
    }
    [ClientRpc(RequireOwnership = false)]
    private void MakeWireClientRpc(int myID, int fromID, int output, int toID, int input)
    {
        if (IsHost || loading) return;
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
        if (IsHost || loading) return;
        Destroy(GetWireFromInstanceID(w).gameObject);
    }
    [ClientRpc(RequireOwnership = false)]
    private void UpdateWorldClientRpc(int[] circuitIDs, int[] circuitInstanceIDs, Vector3[] circuitPositions, int[] wireIDs, 
        int[] wireFromIDs, int[] wireOutputs, int[] wireToIDs, int[] wireInputs, int[] inputCounts, bool[] inputs, int[] outputCounts, bool[] outputs)
    {
        if (IsHost) return;
        int inputIndex = 0;
        int outputIndex = 0;
        for (int i = 0; i < circuitIDs.Length; i++)
        {
            int ID = circuitIDs[i];
            int myID = circuitInstanceIDs[i];
            Vector3 at = circuitPositions[i];
            GameObject of = circuitPrefabs[ID].prefab;
            GameObject circuit = Instantiate(of, components);
            circuit.GetComponent<Circuit>().ID = myID;
            circuit.transform.position = at;
            circuit.name = of.name;

            Circuit c = circuit.GetComponent<Circuit>();

            for (int j = 0; j < inputCounts[i]; j++)
            {
                while (c.inputs.Count < inputCounts[i]) c.inputs.Add(false);
                c.inputs[j] = inputs[inputIndex];
                inputIndex++;
            }
            for (int j = 0; j < outputCounts[i]; j++)
            {
                while (c.outputs.Count < outputCounts[i]) c.outputs.Add(false);
                c.outputs[j] = outputs[outputIndex];
                outputIndex++;
            }

            c.UpdateNodes();
        }
        for (int i = 0; i < wireIDs.Length; i++)
        {
            int myID = wireIDs[i];
            int fromID = wireFromIDs[i];
            int toID = wireToIDs[i];
            int input = wireInputs[i];
            int output = wireOutputs[i];

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
        loading = false;
    }

    [ClientRpc]
    private void TestClientRpc() {
        Debug.Log("TestClientRpc");
    }
}
