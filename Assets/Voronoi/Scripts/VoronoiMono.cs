
using System.Collections.Generic;
using UnityEngine;

public class VoronoiMono<T> : MonoBehaviour
{
    public bool gizmoAwake = false;

    public enum TextureDisplay { Cell, SmoothCells, Shape, SmoothShape }
    public TextureDisplay textureDisplay;
    public float textureSize = 100;
    public int textureRes = 100;
    public List<Color> setColor;
    public Voronoi<T> voronoi;
    public float smoothCellsSize = 2;
    public int smoothCellsCount = 3;
    public float smoothShapeSize = 1;
    public int smoothShapeCount = 2;


    private void OnDrawGizmos()
    {
        if (gizmoAwake)
        {
            gizmoAwake = false;
            Awake();
        }
    }

    public virtual void Awake()
    {
        voronoi.cellTab.Clear();
        UpdateTexture();
    }


    void UpdateTexture()
    {
        transform.rotation = Quaternion.Euler(90, 0, 0);
        transform.localScale = Vector3.one * textureSize;

        Texture2D texture = new Texture2D(textureRes, textureRes);

        float dpos = textureSize / (textureRes - 1);
        Vector2 pos = new Vector2(transform.position.x, transform.position.z) - textureSize / 2 * Vector2.one, crtPos;

        for (int x = 0; x < textureRes; x++)
        {
            for (int y = 0; y < textureRes; y++)
            {
                crtPos = pos + new Vector2(x, y) * dpos;
                Color color = Color.black;

                if (textureDisplay == TextureDisplay.Cell)
                    color = setColor[voronoi.set.IndexOf(voronoi.Cell(crtPos))];

                else if (textureDisplay == TextureDisplay.Shape)
                {
                    float rgb = voronoi.Shape(crtPos);
                    color = new Color(rgb, rgb, rgb);
                }

                else if (textureDisplay == TextureDisplay.SmoothCells)
                {
                    Vector3 colorsVect = Vector3.zero;
                    foreach (KeyValuePair<T, float> e in voronoi.SmoothCells(crtPos, smoothCellsSize, smoothCellsCount))
                        colorsVect += ColorToVector(setColor[voronoi.set.IndexOf(e.Key)]) * e.Value;

                    color = VectorToColor(colorsVect);
                }

                else if (textureDisplay == TextureDisplay.SmoothShape)
                {
                    float rgb = voronoi.SmoothShape(crtPos, smoothShapeSize, smoothShapeCount);
                    color = new Color(rgb, rgb, rgb);
                }

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        GetComponent<MeshRenderer>().material.mainTexture = texture;
    }

    Vector3 ColorToVector(Color c) => new Vector3(c.r, c.g, c.b);
    Color VectorToColor(Vector3 v) => new Color(v.x, v.y, v.z);


    public T Cell(Vector2 pos) => voronoi.Cell(pos);
    public Dictionary<T, float> SmoothCells(Vector2 pos) => voronoi.SmoothCells(pos, smoothCellsSize, smoothCellsCount);
    public float Shape(Vector2 pos) => voronoi.Shape(pos);
    public float SmoothShape(Vector2 pos) => voronoi.SmoothShape(pos, smoothShapeSize, smoothShapeCount);
}

