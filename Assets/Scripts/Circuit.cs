using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circuit : MonoBehaviour
{

    public List<bool> inputs;
    public List<bool> outputs;
    public CircuitDerive script;

    Transform nodeInputsParent;
    Transform nodeOutputsParent;

    List<GameObject> nodeInputs = new List<GameObject>();
    List<GameObject> nodeOutputs = new List<GameObject>();

    void Awake()
    {
        nodeInputsParent = transform.Find("Inputs");
        nodeOutputsParent = transform.Find("Outputs");
    }

    void Update()
    {
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
        bool[] got = script.GetOutputs(inputs);
        for (int i = 0; i < got.Length; i++)
        {
            if (outputs.Count < got.Length) outputs.Add(false);
            outputs[i] = got[i];
        }
        while (outputs.Count > got.Length) outputs.RemoveAt(outputs.Count-1);
    }
}
