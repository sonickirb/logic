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

    public void Tick()
    {
        me.inputs[0] = on;
    }
}
