using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogicButton : MonoBehaviour
{

    public bool on;
    Circuit me;

    void Awake()
    {
        me = transform.parent.GetComponent<Circuit>();
    }

    public void OnPress() { on = !on; }

    // render
    void Update()
    {
        Vector3 to = new Vector3(0f, 1f, 0f);
        if (on) to = new Vector3(0f, 0.5f, 0f);
        transform.localPosition = Vector3.Lerp(transform.localPosition, to, Time.deltaTime * 3f);
    }

    public void Tick()
    {
        me.inputs[0] = on;
    }
}
