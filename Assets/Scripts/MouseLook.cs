using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{

    public float sensitivity = 100f;
    public Transform player;
    public float eyeOffset = 0.1f;

    float xRotation = 0f;

    

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

        if (player != null)
        {
            player.Rotate(Vector3.up * mouseX);
            transform.rotation = Quaternion.Euler(xRotation, player.rotation.eulerAngles.y, 0f);
            transform.position = player.position + new Vector3(0f,eyeOffset,0f);
        } else
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            transform.Rotate(Vector3.up * mouseX);
            transform.rotation = Quaternion.Euler(xRotation, transform.eulerAngles.y, 0f);

            Vector3 move = transform.right * x + transform.forward * z;

            transform.position += move * 10f * Time.deltaTime;
        }
    }
}
