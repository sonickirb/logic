using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{

    public float sensitivity = 100f;
    public Transform player;

    float xRotation = 0f;

    public LayerMask hitMask;
    public LayerMask wireMask;
    public Transform nodeSelector;
    public LineRenderer wireMaking;
    public CapsuleCollider lineCollider;

    public Transform lookingAtNode;
    public bool makingWire;
    Transform firstNode;
    
    RaycastHit wireResult;
    Transform lookingAtWire;

    public Transform place;
    public Transform deleteCircuit;
    public GameObject placing;
    Transform lookingAtCircuit;

    public List<GameObject> inventory;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        player.Rotate(Vector3.up * mouseX);
        
        RaycastHit result;
        bool hit = Physics.Raycast(transform.position, transform.forward, out result, 300f, hitMask);

        bool wireHit;
        lookingAtWire = null;
        for (int i = 0; i < LogicManager.Instance.wires.childCount; i++)
        {
            Transform wire = LogicManager.Instance.wires.GetChild(i);
            LineRenderer line = wire.GetComponent<LineRenderer>();
            
            lineCollider.transform.position = line.GetPosition(0) + (line.GetPosition(1) - line.GetPosition(0)) / 2;
            lineCollider.transform.LookAt(line.GetPosition(0));
            lineCollider.height = (line.GetPosition(1) - line.GetPosition(0)).magnitude;
            lineCollider.radius = line.startWidth / 2;
            lineCollider.center = Vector3.zero;

            wireHit = Physics.Raycast(transform.position, transform.forward, out wireResult, 300f, wireMask);
            if (wireHit) lookingAtWire = wire;
            if (wireHit) break;
        }

        bool IWannaDeleteACircuit = Input.GetKey(KeyCode.F);

        if (hit && result.collider.tag == "Node")
        {
            lookingAtNode = result.collider.transform;
        } else lookingAtNode = null;

        if (makingWire && lookingAtNode == firstNode) lookingAtNode = null;
        if (placing) lookingAtNode = null;

        if (hit && result.collider.transform.GetComponent<Circuit>() && !lookingAtNode && !makingWire && !placing && IWannaDeleteACircuit)
        {
            lookingAtCircuit = result.collider.transform;
        } else lookingAtCircuit = null;

        nodeSelector.gameObject.SetActive(lookingAtNode != null);
        if (lookingAtNode) nodeSelector.position = lookingAtNode.position;

        if (Input.GetMouseButtonDown(0))
        {
            if (!makingWire && lookingAtNode && lookingAtNode.parent.name == "Outputs")
            {
                makingWire = true;
                firstNode = lookingAtNode;
                lookingAtNode = null;
                wireMaking.SetPosition(0, firstNode.position);
            }
            else if (makingWire && lookingAtNode)
            {
                Circuit from = firstNode.parent.parent.GetComponent<Circuit>();
                Circuit to = lookingAtNode.parent.parent.GetComponent<Circuit>();
                LogicManager.Instance.MakeWire(from, int.Parse(firstNode.name), to, int.Parse(lookingAtNode.name));
                firstNode = null;
                makingWire = false;
            } else if (makingWire && !lookingAtNode)
            {
                makingWire = false;
            } else if (placing)
            {
                LogicManager.Instance.MakeCircuit(placing, place.position);
                placing = null;
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            if (lookingAtWire)
            {
                Destroy(lookingAtWire.gameObject);
            } else if (lookingAtCircuit)
            {
                LogicManager.Instance.RemoveCircuit(lookingAtCircuit.gameObject);
            }
        }
        
        wireMaking.gameObject.SetActive(makingWire);
        if (makingWire)
        {
            wireMaking.SetPosition(1, result.point);
            wireMaking.material = lookingAtNode ? LogicManager.Instance.select : LogicManager.Instance.wireNoNo;
        } else
        {
            if (lookingAtWire)
            {
                lookingAtWire.GetComponent<LineRenderer>().material = LogicManager.Instance.wireNoNo;
            }
        }

        if (!makingWire)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) placing = inventory[0];
            if (Input.GetKeyDown(KeyCode.Alpha2)) placing = inventory[1];
            if (Input.GetKeyDown(KeyCode.Alpha3)) placing = inventory[2];
            if (Input.GetKeyDown(KeyCode.Alpha4)) placing = inventory[3];
        }

        place.gameObject.SetActive(placing != null);
        if (placing && hit)
        {
            place.position = result.point + (result.normal * (place.localScale.y / 2));
        }

        deleteCircuit.gameObject.SetActive(lookingAtCircuit != null);
        if (lookingAtCircuit)
        {
            deleteCircuit.position = lookingAtCircuit.position;
        }
        
    }
}
