using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VehicleController))]
public class VehicleInput : MonoBehaviour
{

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
        control.accelInput = Input.GetAxis(accelAxis);
        control.steerInput = Input.GetAxis(steerAxis);
        control.handbrakeInput = Input.GetButton(handbrakeButton) ? 1 : 0;
    }
}
