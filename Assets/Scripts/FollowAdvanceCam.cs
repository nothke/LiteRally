using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowAdvanceCam : MonoBehaviour
{
    public Rigidbody target;

    Vector3 smoothPos;
    Vector3 smoothVelo;

    private void FixedUpdate()
    {
        if (target)
        {
            Vector3 targetPos = target.position + target.velocity * 1;

            smoothPos = Vector3.SmoothDamp(smoothPos, targetPos, ref smoothVelo, 0.5f);

            Vector3 offset = -transform.forward * 40;

            transform.position = smoothPos + offset;
        }

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
            target = RaceManager.e.players[0].control.rb;
    }
}
