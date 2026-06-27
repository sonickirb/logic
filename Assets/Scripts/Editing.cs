using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Editing : MonoBehaviour
{

    public Transform cam;

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

    Transform lookingAtButton;

    public AudioSource circuitSFX;
    public AudioSource wireSFX;

    public bool gridSnap;

    public TMP_Text controlsText;
    public GameObject gridView;

    void OnEnable()
    {
        LogicManager.Instance.gameUI.SetActive(true);
    }
    void OnDisable()
    {
        LogicManager.Instance.gameUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        List<string> controls = new List<string>();

        if (LogicManager.Instance.IsServer)
        {
            controls.Add("Save World - O");
            controls.Add((LogicManager.Instance.autoTick ? "Disable" : "Enable") + " Ticking - P");
            if (!LogicManager.Instance.autoTick) controls.Add("Tick - I");
        }

        RaycastHit result;
        bool hit = Physics.Raycast(cam.position, cam.forward, out result, 300f, hitMask);
        
        bool IWannaDeleteACircuit = Input.GetKey(KeyCode.F);

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

            wireHit = Physics.Raycast(cam.position, cam.forward, out wireResult, 300f, wireMask);
            if (wireHit) lookingAtWire = wire;
            if (wireHit) break;
        }




        if (hit && result.collider.tag == "Node")
        {
            lookingAtNode = result.collider.transform;
        } else lookingAtNode = null;

        if (makingWire && lookingAtNode == firstNode) lookingAtNode = null;
        if (placing) lookingAtNode = null;

        if (((hit && result.collider.transform.GetComponent<Circuit>()) || lookingAtWire) && !lookingAtNode && !makingWire && !placing)
            controls.Add((IWannaDeleteACircuit ? "Stop" : "Start") + " Delete - Hold F");

        if (hit && result.collider.transform.GetComponent<Circuit>() && !lookingAtNode && !makingWire && !placing && IWannaDeleteACircuit)
        {
            lookingAtCircuit = result.collider.transform;
        } else lookingAtCircuit = null;

        if (!IWannaDeleteACircuit)
            lookingAtWire = null;

        if (hit && result.collider.tag == "Button" && !lookingAtNode && !lookingAtCircuit && !makingWire && !placing)
        {
            lookingAtButton = result.collider.transform;
        } else lookingAtButton = null;

        nodeSelector.gameObject.SetActive(lookingAtNode != null);
        if (lookingAtNode) nodeSelector.position = lookingAtNode.position;

        if (!makingWire && lookingAtNode && lookingAtNode.parent.name == "Outputs")
            controls.Add("Make Wire - LMB");
        else if (makingWire && lookingAtNode && lookingAtNode.parent.name != "Outputs" && lookingAtNode.parent.parent.name != "Button")
            controls.Add("Finish Wire - LMB");
        else if (makingWire && !lookingAtNode)
            controls.Add("Cancel Wire - LMB");
        else if (placing)
            controls.Add("Place Component - LMB");
        else if (lookingAtButton)
            controls.Add("Press - LMB");

        if (Input.GetMouseButtonDown(0))
        {
            if (!makingWire && lookingAtNode && lookingAtNode.parent.name == "Outputs")
            {
                makingWire = true;
                firstNode = lookingAtNode;
                lookingAtNode = null;
                wireMaking.SetPosition(0, firstNode.position);
                wireSFX.Play();
            }
            else if (makingWire && lookingAtNode && lookingAtNode.parent.name != "Outputs" && lookingAtNode.parent.parent.name != "Button")
            {
                Circuit from = firstNode.parent.parent.GetComponent<Circuit>();
                Circuit to = lookingAtNode.parent.parent.GetComponent<Circuit>();
                LogicManager.Instance.MakeWire(from, int.Parse(firstNode.name), to, int.Parse(lookingAtNode.name));
                firstNode = null;
                makingWire = false;
                wireSFX.Play();
            } else if (makingWire && !lookingAtNode)
            {
                makingWire = false;
            } else if (placing)
            {
                LogicManager.Instance.MakeCircuit(placing, place.position);
                placing = null;
                circuitSFX.Play();
            } else if (lookingAtButton)
            {
                LogicManager.Instance.PressButton(lookingAtButton.GetComponent<LogicButton>());
            }
        }
        if (lookingAtWire)
            controls.Add("Delete Wire - RMB");
        else if (lookingAtCircuit)
            controls.Add("Delete Component - RMB");
        if (Input.GetMouseButtonDown(1))
        {
            if (lookingAtWire)
            {
                LogicManager.Instance.RemoveWire(lookingAtWire.gameObject);
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
            if (Input.GetKeyDown(KeyCode.Alpha5)) placing = inventory[4];
            if (Input.GetKeyDown(KeyCode.Alpha6)) placing = inventory[5];
            if (Input.GetKeyDown(KeyCode.Alpha7)) placing = inventory[6];
            if (Input.GetKeyDown(KeyCode.Alpha8)) placing = inventory[7];
            if (Input.GetKeyDown(KeyCode.Alpha9)) placing = inventory[8];
            if (Input.GetKeyDown(KeyCode.Alpha0)) placing = inventory[9];
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            gridSnap = !gridSnap;
            gridView.SetActive(gridSnap);
        }
        controls.Add((gridSnap ? "Disable" : "Enable") + " Snapping - G");

        place.gameObject.SetActive(placing != null);
        if (placing && hit)
        {
            Vector3 p = result.point;
            if (gridSnap)
                p = new Vector3((Mathf.Floor(p.x / 0.5f) * 0.5f) + 0.25f, p.y, (Mathf.Floor(p.z / 0.5f) * 0.5f) + 0.25f);
            place.position = p + (result.normal * (place.localScale.y / 2));
        }

        deleteCircuit.gameObject.SetActive(lookingAtCircuit != null);
        if (lookingAtCircuit)
        {
            deleteCircuit.position = lookingAtCircuit.position;
        }
        
        controlsText.text = "";
        foreach (string control in controls)
            controlsText.text += control + "\n";
    }
}