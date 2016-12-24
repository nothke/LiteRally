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
        public Transform[] wheelPoints;

        public bool powered;
        public bool steering;
        public bool invertedSteering;
        public bool brakes = true;
        public bool handbrake = false;
    }

    public Axle[] axles;

    Rigidbody rb;

    void Start()
    {
        rb.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {

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
            foreach (var wheelPoint in axle.wheelPoints)
            {
                Gizmos.DrawSphere(wheelPoint.position, wheelData.wheelRadius);
                Gizmos.DrawLine(wheelPoint.position, wheelPoint.position - wheelPoint.up * wheelData.suspensionLength);
            }
        }
    }

}
