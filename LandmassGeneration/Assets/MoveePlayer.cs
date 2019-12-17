using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveePlayer : MonoBehaviour
{
    public float speed;
    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.forward * speed * Time.deltaTime;
    }
}
