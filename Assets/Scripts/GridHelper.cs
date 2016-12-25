using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridHelper : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Vector3 fl = transform.position + transform.forward * 2 - transform.right * 1;
        Vector3 fr = transform.position + transform.forward * 2 + transform.right * 1;
        Vector3 bl = transform.position - transform.forward * 2 - transform.right * 1;
        Vector3 br = transform.position - transform.forward * 2 + transform.right * 1;

        Gizmos.DrawLine(fl, fr);
        //Gizmos.DrawLine(bl, br);
        Gizmos.DrawLine(fl, bl);
        Gizmos.DrawLine(fr, br);
    }
}
