using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class WorldData
{
    public int[] components;
    public float[] positionX;
    public float[] positionY;
    public float[] positionZ;
    public int[] connectionsFrom;
    public int[] connectionsTo;
    public int[] output;
    public int[] input;

    public WorldData()
    {


        components = new int[LogicManager.Instance.components.childCount];
        positionX = new float[LogicManager.Instance.components.childCount];
        positionY = new float[LogicManager.Instance.components.childCount];
        positionZ = new float[LogicManager.Instance.components.childCount];

        Dictionary<Transform, int> circuitIndex = new Dictionary<Transform, int>();

        for (int c = 0; c < LogicManager.Instance.components.childCount; c++)
        {
            Transform circuit = LogicManager.Instance.components.GetChild(c);
            int id = LogicManager.Instance.GetCircuitIDFromName(circuit.name);
            components[c] = id;
            positionX[c] = circuit.position.x;
            positionY[c] = circuit.position.y;
            positionZ[c] = circuit.position.z;
            
            circuitIndex.Add(circuit, c);
        }

        connectionsFrom = new int[LogicManager.Instance.wires.childCount];
        connectionsTo   = new int[LogicManager.Instance.wires.childCount];
        input           = new int[LogicManager.Instance.wires.childCount];
        output          = new int[LogicManager.Instance.wires.childCount];
        for (int w = 0; w < LogicManager.Instance.wires.childCount; w++)
        {
            Wire wire = LogicManager.Instance.wires.GetChild(w).GetComponent<Wire>();

            connectionsFrom[w] = circuitIndex[wire.from.transform];
            connectionsTo[w]   = circuitIndex[wire.to.transform];
            input[w]           = wire.input;
            output[w]          = wire.output;
        }
    }
}