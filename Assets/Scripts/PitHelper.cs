using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PitHelper : MonoBehaviour {

    private void OnDrawGizmos()
    {
        Vector3 fl = transform.position + transform.forward * 2.5f - transform.right * 2f;
        Vector3 fr = transform.position + transform.forward * 2.5f + transform.right * 2f;
        Vector3 bl = transform.position - transform.forward * 2.5f - transform.right * 2f;
        Vector3 br = transform.position - transform.forward * 2.5f + transform.right * 2f;

        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(fl, fr);
        Gizmos.DrawLine(bl, br);
        Gizmos.DrawLine(fl, bl);
        Gizmos.DrawLine(fr, br);
    }
}
