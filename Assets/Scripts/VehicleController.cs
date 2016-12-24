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
                    V = transform.InverseTransformVector(V);


                    float forceMult = (1 - hit.distance) * test_suspensionForceMult + (-V.y * test_dampeningForceMult);

                    // Apply suspension force
                    rb.AddForceAtPosition(forceMult * wheelPivot.up, wheelPivot.position);
                }
            }
        }
    }

    void Update()
    {

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

#if UNITY_EDITOR
                UnityEditor.Handles.color = Color.green;
                UnityEditor.Handles.DrawWireDisc(wheelPivot.position, wheelPivot.right, wheelData.wheelRadius);
#endif
                Gizmos.DrawLine(wheelPivot.position, wheelPivot.position - wheelPivot.up * wheelData.suspensionLength);
            }
        }
    }

}
