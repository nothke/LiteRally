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
        [HideInInspector]
        public float sidewaysForce;
        [HideInInspector]
        public float accelForce;

        [HideInInspector]
        public float surfaceGrip;
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

                    float spring = (1 - hit.distance / wheelData.suspensionLength) * test_suspensionForceMult;
                    float dampening = -V.y * test_dampeningForceMult;
                    float suspensionForce = spring + dampening;

                    // Apply suspension force
                    rb.AddForceAtPosition(suspensionForce * wheelPivot.up, wheelPivot.position);

                    // WHEEL FRICTION

                    wheel.surfaceGrip = GetGripFromGround(hit);

                    // Sideways
                    int sign = V.x == 0 ? 0 : (V.x < 0 ? -1 : 1);
                    float sidewaysForce = -sign * wheelData.sidewaysFriction.Evaluate(Mathf.Abs(V.x) * wheelData.gripScale) * wheelData.gripGain;
                    wheel.sidewaysForce = sidewaysForce;

                    bool handbrake = axle.handbrake && handbrakeInput == 1;
                    float brakes = 1 - Mathf.Clamp01(1 + accelInput);

                    if (handbrake) brakes = 1;

                    // longitudial (when wheels are still)
                    float longitudialForce = -wheelData.longitudialFriction.Evaluate(V.z / wheelData.gripScale) * wheelData.gripGain;

                    // Traction force (from using engine)
                    wheel.accelForce = Mathf.Clamp01(accelInput) * accelCurve.Evaluate(V.z) * accelMult;
                    float brakeForce = longitudialForce * brakes;

                    if (handbrake) wheel.surfaceGrip = 0;

                    Vector3 frictionForce = new Vector3(sidewaysForce, 0, +wheel.accelForce + brakeForce); // longitudialForce
                    frictionForce *= wheel.surfaceGrip;

                    frictionForce = wheelPivot.TransformVector(frictionForce);

                    // apply friction forces
                    rb.AddForceAtPosition(frictionForce, wheelPivot.position);

                    Color lightGray = new Color(0.95f, 0.95f, 0.95f);
                    RaceManager.MultPixel(lightGray, hit.textureCoord.x, hit.textureCoord.y);
                }
                else wheel.distance = wheelData.suspensionLength;
            }
        }
    }

    public float GetGripFromGround(RaycastHit hit)
    {
        Color c = GetColorFromTexture(hit);

        if (c.g == c.r && c.r == c.b) // no color > is tarmac
        {
            //Debug.Log("TARMAC!");
            return 1;
        }
        else
        {
            //Debug.Log("Grass");
            return 0.5f;
        }
    }

    public Color GetColorFromTexture(RaycastHit hit)
    {
        Texture2D tex = hit.collider.GetComponent<Renderer>().sharedMaterial.mainTexture as Texture2D;

        Color c = tex.GetPixelBilinear(hit.textureCoord.x, hit.textureCoord.y);

        return c;
    }

    float steerInput;
    float accelInput;
    float handbrakeInput;

    private void Update()
    {
        steerInput = Input.GetAxis("Horizontal");
        accelInput = Input.GetAxis("Vertical");
        handbrakeInput = Input.GetButton("Jump") ? 1 : 0;
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

                Vector3 contactPos = wheelPivot.position - wheelPivot.up * (wheel.distance);
                Vector3 wheelPos = wheelPivot.position - wheelPivot.up * (wheel.distance - wheelData.wheelRadius);

#if UNITY_EDITOR
                UnityEditor.Handles.color = Color.green;
                UnityEditor.Handles.DrawWireDisc(wheelPos, wheelPivot.right, wheelData.wheelRadius);
#endif
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(wheelPivot.position, wheelPivot.position - wheelPivot.up * wheelData.suspensionLength);

                Gizmos.color = Color.red;
                Gizmos.DrawRay(contactPos, wheelPivot.right * wheel.sidewaysForce / 4000);
                Gizmos.color = Color.green;
                Gizmos.DrawRay(contactPos, wheelPivot.forward * wheel.accelForce / 4000);

                if (wheel.surfaceGrip == 1)
                    Gizmos.color = Color.gray;
                else
                    Gizmos.color = Color.green;

                Gizmos.DrawSphere(contactPos + Vector3.up * 2.5f, 0.6f);
            }
        }
    }
}
