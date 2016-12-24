using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour
{

    [System.Serializable]
    public class WheelData
    {
        public float gripGain = 1;
        public float gripScale = 10;
        public AnimationCurve sidewaysFriction;
        public AnimationCurve longitudialFriction;

        public float suspensionLength = 1;
        public float wheelRadius = 0.3f;
    }

    public WheelData wheelData;

    [System.Serializable]
    public class Axle
    {
        public Wheel[] wheels;

        public bool powered;
        public bool steering;
        public bool invertedSteering;
        public bool brakes = true;
        public bool handbrake = false;


    }

    public Axle[] axles;

    [System.Serializable]
    public class Wheel
    {
        public Transform pivot;

        [HideInInspector]
        public float distance;
    }

    Rigidbody rb;

    public float accelMult;
    public AnimationCurve accelCurve;
    public float test_suspensionForceMult = 10;
    public float test_dampeningForceMult = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        foreach (var axle in axles)
        {
            foreach (var wheel in axle.wheels)
            {
                Transform wheelPivot = wheel.pivot;

                RaycastHit hit;
                if (Physics.Raycast(wheelPivot.position, -wheelPivot.up, out hit, wheelData.suspensionLength))
                {
                    wheel.distance = hit.distance;

                    Vector3 V = rb.GetPointVelocity(wheelPivot.position);
                    V = wheelPivot.InverseTransformVector(V);

                    // STEERING
                    if (axle.steering)
                    {
                        wheel.pivot.localRotation = Quaternion.AngleAxis(steerInput * 60, Vector3.up);
                    }

                    // SUSPENSION

                    float spring = (1 - hit.distance) * test_suspensionForceMult;
                    float dampening = -V.y * test_dampeningForceMult;
                    float suspensionForce = spring + dampening;

                    // Apply suspension force
                    rb.AddForceAtPosition(suspensionForce * wheelPivot.up, wheelPivot.position);

                    // WHEEL FRICTION

                    // Sideways
                    float sidewaysForce = -wheelData.sidewaysFriction.Evaluate(V.x / wheelData.gripScale) * wheelData.gripGain;

                    // longitudial (when wheels are still)
                    float longitudialForce = -wheelData.longitudialFriction.Evaluate(V.z / wheelData.gripScale) * wheelData.gripGain;

                    // Traction force (from using engine)
                    float accel = accelInput * accelCurve.Evaluate(V.z) * accelMult;

                    Vector3 frictionForce = new Vector3(sidewaysForce, 0, +accel); // longitudialForce

                    frictionForce = wheelPivot.TransformVector(frictionForce);

                    // apply friction forces
                    rb.AddForceAtPosition(frictionForce, wheelPivot.position);


                }
                else wheel.distance = wheelData.suspensionLength;
            }
        }
    }

    float steerInput;
    float accelInput;

    private void Update()
    {
        steerInput = Input.GetAxis("Horizontal");
        accelInput = Input.GetAxis("Vertical");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        if (axles == null) return;

        foreach (var axle in axles)
        {
            foreach (var wheel in axle.wheels)
            {
                Transform wheelPivot = wheel.pivot;

                if (!wheelPivot) return;

                Vector3 wheelPos = wheelPivot.position - wheelPivot.up * (wheel.distance - wheelData.wheelRadius);

#if UNITY_EDITOR
                UnityEditor.Handles.color = Color.green;
                UnityEditor.Handles.DrawWireDisc(wheelPos, wheelPivot.right, wheelData.wheelRadius);
#endif
                Gizmos.DrawLine(wheelPivot.position, wheelPivot.position - wheelPivot.up * wheelData.suspensionLength);
            }
        }
    }

}
