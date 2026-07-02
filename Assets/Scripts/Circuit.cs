using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circuit : MonoBehaviour
{

    public int ID;

    public List<bool> inputs;
    public List<bool> outputs;
    public string scriptID;
    CircuitDerive script;

    Transform nodeInputsParent;
    Transform nodeOutputsParent;

    List<GameObject> nodeInputs = new List<GameObject>();
    List<GameObject> nodeOutputs = new List<GameObject>();

    public Dictionary<int, List<Wire>> inputWires;
    public Dictionary<int, List<Wire>> outputWires;

    void OnEnable()
    {
        inputWires = new();
        outputWires = new();
    }

    public void AddWireInput(Wire wire, int input)
    {
        if (inputWires == null) inputWires = new();
        if (!inputWires.ContainsKey(input)) inputWires[input] = new();

        inputWires[input].Add(wire);
    }
    public void AddWireOutput(Wire wire, int output)
    {
        if (outputWires == null) outputWires = new();
        if (!outputWires.ContainsKey(output)) outputWires[output] = new();

        outputWires[output].Add(wire);
    }
    public void RemoveWireInput(Wire wire, int input)
    {
        if (inputWires == null) inputWires = new();
        if (!inputWires.ContainsKey(input)) inputWires[input] = new();

        inputWires[input].Remove(wire);
    }
    public void RemoveWireOutput(Wire wire, int output)
    {
        if (outputWires == null) outputWires = new();
        if (!outputWires.ContainsKey(output)) outputWires[output] = new();

        outputWires[output].Remove(wire);
    }

    void Update()
    {
        UpdateNodes();
    }

    public void UpdateNodes()
    {
        if (nodeInputsParent == null)  nodeInputsParent  = transform.Find("Inputs");
        if (nodeOutputsParent == null) nodeOutputsParent = transform.Find("Outputs");
        int inputthing = nodeInputs.Count-1;
        while (nodeInputs.Count < inputs.Count)
        {
            GameObject node = Instantiate(LogicManager.Instance.nodePrefab, nodeInputsParent);
            nodeInputs.Add(node);
            node.layer = LogicManager.Instance.nodeLayer;
            inputWires.Add(inputthing, new());
            inputthing++;
        }
        inputthing = nodeOutputs.Count-1;
        while (nodeOutputs.Count < outputs.Count)
        {
            GameObject node = Instantiate(LogicManager.Instance.nodePrefab, nodeOutputsParent);
            nodeOutputs.Add(node);
            node.layer = LogicManager.Instance.nodeLayer;
            outputWires.Add(inputthing, new());
            inputthing++;
        }

        for (int i = 0; i < nodeInputs.Count; i++)
        {
            GameObject node = nodeInputs[i];

            float x = 0.2f * i;
            float y = 0;

            node.transform.localPosition = new Vector3(x,y,0);
            node.transform.name = i.ToString();
            node.GetComponent<MeshRenderer>().material = inputs[i] ? LogicManager.Instance.nodeOn : LogicManager.Instance.nodeOff;
        }
        for (int i = 0; i < nodeOutputs.Count; i++)
        {
            GameObject node = nodeOutputs[i];

            float x = 0.2f * i;
            float y = 0;

            node.transform.localPosition = new Vector3(x,y,0);
            node.transform.name = i.ToString();
            node.GetComponent<MeshRenderer>().material = outputs[i] ? LogicManager.Instance.nodeOn : LogicManager.Instance.nodeOff;
        }
    }

    public bool Tick()
    {
        bool changed = false;

        if (script == null)
        {
            script = LogicManager.Instance.transform.Find(scriptID).GetComponent<CircuitDerive>();
        }
        if (transform.Find("Button")) transform.Find("Button").GetComponent<LogicButton>().Tick();
        bool[] got = script.GetOutputs(inputs);
        for (int i = 0; i < got.Length; i++)
        {
            if (outputs.Count < got.Length) outputs.Add(false);
            bool old = outputs[i];
            outputs[i] = got[i];
            if (outputs[i] != old) changed = true;
        }
        while (outputs.Count > got.Length) outputs.RemoveAt(outputs.Count-1);

        if (transform.Find("Pixel"))
        {
            Material a = inputs[0] ? LogicManager.Instance.pixelOn : LogicManager.Instance.pixelOff;
            if (transform.Find("Pixel").GetComponent<MeshRenderer>().material != a)
                changed = true;
        }

        return changed;
    }

    public void Extra()
    {
        if (transform.Find("Pixel")) transform.Find("Pixel").GetComponent<MeshRenderer>().material = inputs[0] ? LogicManager.Instance.pixelOn : LogicManager.Instance.pixelOff;
    }
}
