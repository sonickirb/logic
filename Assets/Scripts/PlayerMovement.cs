using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{

    public CharacterController controller;
    public Transform visual;
    public Camera cam;

    public float moveSpeed = 200f;
    public float flySpeed = 100f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 1f;
    public LayerMask groundMask;

    public float flyTimer = 0f;
    public float flyBounce = 0.3f;
    public bool flying;

    public Vector3 groundDamp = Vector3.one;
    public Vector3 airDamp = Vector3.one;
    public Vector3 flyDamp = Vector3.one;

    Vector3 velocity;
    bool isGrounded;

    MeshRenderer hat;
    MeshRenderer visualRenderer;

    void OnEnable()
    {
        if (!IsOwner) return;
        LogicManager.Instance.editing.enabled = true;
    }
    void OnDisable()
    {
        if (!IsOwner) return;
        LogicManager.Instance.editing.enabled = false;
    }

    void Start()
    {
        if (!IsOwner) return;
        LogicManager.Instance.editing.enabled = true;

        cam = Camera.main;
        cam.GetComponent<MouseLook>().player = transform;

        hat = visual.Find("hat").GetComponent<MeshRenderer>();
        visualRenderer = visual.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        bool wasGrounded = isGrounded;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && !wasGrounded) // land
        {
            flying = false;
            transform.position = groundCheck.position + Vector3.up;
        }

        if (isGrounded && velocity.y < 0) 
            velocity.y = -2f;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        float speed = flying ? flySpeed : moveSpeed;

        Vector3 move = transform.right * x + transform.forward * z;

        velocity += move.normalized * speed * Time.deltaTime;

        if (Input.GetButtonDown("Jump")) {
            if (isGrounded)
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            else if (flyTimer < flyBounce)
                flying = !flying;
            else flyTimer = 0f;
        }
        if (Input.GetButton("Jump") && flying) velocity.y += speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftShift) && flying) velocity.y -= speed * Time.deltaTime;

        flyTimer += Time.deltaTime;

        if (!flying)
            velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        visual.rotation = cam.transform.rotation;
        if (hat.enabled) hat.enabled = false;
        if (visualRenderer.enabled) visualRenderer.enabled = false;
    }

    void FixedUpdate()
    {
        Vector3 damp = flying ? flyDamp : isGrounded ? groundDamp : airDamp;
        velocity = new Vector3(velocity.x * damp.x, velocity.y * damp.y, velocity.z * damp.z);
    }
}
