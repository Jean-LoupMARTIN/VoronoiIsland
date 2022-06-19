using System;
using UnityEngine;


public class Biome : MonoBehaviour
{
    public AnimationCurve heightCurve;
    public Gradient gradient;
    public PerlinNoise[] noises;
    public PerlinNoise heightCoefNoise;

    public (float height, Color color) Evaluate(Vector2 pos, float shape)
    {
        float height = 0;
        float heightMax = 0;

        foreach (PerlinNoise n in noises)
        {
            height += n.Height(pos);
            heightMax += n.height;
        }

        shape = Mathf.Clamp01(shape * 4);
        height *= shape;
        float progress = height / heightMax;

        return (heightCurve.Evaluate(progress) * heightMax * (1 + heightCoefNoise.Height(pos)), gradient.Evaluate(progress));
    }
}

[Serializable]
public class PerlinNoise
{
    public float scale = 10;
    public float height = 1;
    public float Height(Vector2 pos) => Mathf.PerlinNoise(pos.x / scale, pos.y / scale) * height;
}