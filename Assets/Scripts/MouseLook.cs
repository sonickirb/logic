using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{

    public float sensitivity = 100f;
    public Transform player;

    float xRotation = 0f;

    public LayerMask hitMask;
    public Transform nodeSelector;

    public Transform lookingAtNode;
    public bool makingWire;
    Transform firstNode;

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
        bool hit = Physics.Raycast(transform.position, transform.forward, out result, 10000f, hitMask);

        if (hit && result.collider.tag == "Node")
        {
            lookingAtNode = result.collider.transform;
        } else lookingAtNode = null;

        if (makingWire && lookingAtNode == firstNode) lookingAtNode = null;

        nodeSelector.gameObject.SetActive(lookingAtNode != null);
        if (lookingAtNode) nodeSelector.position = lookingAtNode.position;

        if (Input.GetMouseButtonDown(0))
        {
            if (!makingWire && lookingAtNode && lookingAtNode.parent.name == "Outputs")
            {
                makingWire = true;
                firstNode = lookingAtNode;
                lookingAtNode = null;
            }
            else if (makingWire && lookingAtNode)
            {
                Circuit from = firstNode.parent.parent.GetComponent<Circuit>();
                Circuit to = lookingAtNode.parent.parent.GetComponent<Circuit>();
                LogicManager.Instance.MakeWire(from, int.Parse(firstNode.name), to, int.Parse(lookingAtNode.name));
                firstNode = null;
                makingWire = false;
            }
        }
        
    }
}
