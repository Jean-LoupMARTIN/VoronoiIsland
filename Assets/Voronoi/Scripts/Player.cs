using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player inst;
    public float speed = 10;

    private void Awake()
    {
        inst = this;
    }

    private void Update()
    {
        transform.Translate(speed * Time.deltaTime * Vector3.forward);
        Vector3 pos = transform.position;
        Vector2 pos2D = new Vector2(pos.x, pos.z);
        pos.y = Mathf.Max(10, VoronoiBiomes.inst.voronoi.Cell(pos2D).Evaluate(pos2D, VoronoiBiomes.inst.Shape(pos2D)).height) / 2 + 50;
        transform.position = pos;
    }
}
