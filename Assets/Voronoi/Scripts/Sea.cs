using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sea : MonoBehaviour
{
    public float height = 10, dHeight = 0.1f, speed = 1;


    void Update()
    {
        transform.position = new Vector3(0,height + dHeight * Mathf.Cos(Time.time * speed),0);
    }
}
