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
        public Transform[] wheelPivots;

        public bool powered;
        public bool steering;
        public bool invertedSteering;
        public bool brakes = true;
        public bool handbrake = false;
    }

    public Axle[] axles;

    Rigidbody rb;

    public float test_suspensionForceMult = 10;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        foreach (var axle in axles)
        {
            foreach (var wheelPivot in axle.wheelPivots)
            {
                RaycastHit hit;
                if (Physics.Raycast(wheelPivot.position, -wheelPivot.up, out hit, wheelData.suspensionLength))
                {
                    float forceMult = (1 - hit.distance) * test_suspensionForceMult;

                    Debug.Log(hit.collider);

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
            foreach (var wheelPivot in axle.wheelPivots)
            {
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
