using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{

    public CharacterController controller;
    public Transform visual;
    public Camera cam;

    public float speed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;
    bool isGrounded;

    MeshRenderer hat;

    void Start()
    {
        if (!IsOwner) return;

        cam = Camera.main;
        cam.GetComponent<MouseLook>().player = transform;

        hat = visual.Find("hat").GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0) 
            velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded) 
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);

        visual.rotation = cam.transform.rotation;
        if (hat.enabled) hat.enabled = false;
    }
}
