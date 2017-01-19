using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VehicleController))]
public class VehicleInput : MonoBehaviour
{
    public ControlScheme controlScheme;

    private VehicleController _control;

    public VehicleController control
    {
        get
        {
            if (!_control)
                _control = GetComponent<VehicleController>();

            return _control;
        }
    }

    public string steerAxis;
    public string accelAxis;
    public string handbrakeButton;

    void Update()
    {
        control.accelInput = Input.GetAxis(controlScheme.accelAxisName);
        control.steerInput = Input.GetAxis(controlScheme.steeringAxisName);
        control.handbrakeInput = Input.GetButton(controlScheme.handbrakeButtonName) ? 1 : 0;
    }
}
