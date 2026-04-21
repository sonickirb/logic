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

    void Update()
    {
        UpdateNodes();
    }

    public void UpdateNodes()
    {
        if (nodeInputsParent == null)  nodeInputsParent  = transform.Find("Inputs");
        if (nodeOutputsParent == null) nodeOutputsParent = transform.Find("Outputs");
        while (nodeInputs.Count < inputs.Count)
        {
            GameObject node = Instantiate(LogicManager.Instance.nodePrefab, nodeInputsParent);
            nodeInputs.Add(node);
        }
        while (nodeOutputs.Count < outputs.Count)
        {
            GameObject node = Instantiate(LogicManager.Instance.nodePrefab, nodeOutputsParent);
            nodeOutputs.Add(node);
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

    public void Tick()
    {
        if (script == null)
        {
            script = LogicManager.Instance.transform.Find(scriptID).GetComponent<CircuitDerive>();
        }
        if (transform.Find("Button")) transform.Find("Button").GetComponent<LogicButton>().Tick();
        bool[] got = script.GetOutputs(inputs);
        for (int i = 0; i < got.Length; i++)
        {
            if (outputs.Count < got.Length) outputs.Add(false);
            outputs[i] = got[i];
        }
        while (outputs.Count > got.Length) outputs.RemoveAt(outputs.Count-1);
    }

    public void Extra()
    {
        if (transform.Find("Pixel")) transform.Find("Pixel").GetComponent<MeshRenderer>().material = inputs[0] ? LogicManager.Instance.pixelOn : LogicManager.Instance.pixelOff;
    }
}
