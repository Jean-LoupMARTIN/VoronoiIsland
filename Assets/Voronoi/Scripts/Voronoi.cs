using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class Voronoi<T>
{
    public Dictionary<(int x, int y), (T cell, Vector2 center)> cellTab = new Dictionary<(int x, int y), (T cell, Vector2 center)>();

    public List<T> set;
    public float cellSize = 10;

    [Range(0.01f, 1)]
    public float cellLiberty = 1;



    public (T cell, Vector2 center) CellTab(int x, int y)
    {
        // generate new cell
        if (!cellTab.ContainsKey((x, y)))
            cellTab[(x, y)] = (set[Random.Range(0, set.Count)],
                               cellSize * new Vector2(x + 0.5f + Random.Range(-cellLiberty / 2, cellLiberty / 2),
                                                      y + 0.5f + Random.Range(-cellLiberty / 2, cellLiberty / 2)));
        return cellTab[(x, y)];
    }

    public (T cell, Vector2 center) CellTab(Vector2 pos)
    {
        int xTab = (int)(pos.x / cellSize);
        int yTab = (int)(pos.y / cellSize);
        if (pos.x < 0) xTab--;
        if (pos.y < 0) yTab--;

        (T cell, Vector2 center) crt, closest = CellTab(xTab, yTab);
        float crtDist, closestDist = (pos - closest.center).magnitude;

        for (int y = yTab - 1; y <= yTab + 1; y++) {
            for (int x = xTab - 1; x <= xTab + 1; x++)
            {
                crt = CellTab(x, y);
                crtDist = (pos - crt.center).magnitude;

                if (crtDist < closestDist)
                {
                    closest = crt;
                    closestDist = crtDist;
                }
            }
        }

        return closest;
    }

    public T Cell(Vector2 pos) => CellTab(pos).cell;

    public Dictionary<T, float> SmoothCells(Vector2 pos, float smoothSize, int smoothCount = 3)
    {
        Dictionary<T, float> smoothCells = new Dictionary<T, float>();

        if (smoothCount <= 1)
            smoothCells.Add(Cell(pos), 1);

        else {
            float dWeight = 1f / (smoothCount * smoothCount);
            float dPos = smoothSize / (smoothCount - 1);
            pos -= smoothSize / 2 * Vector2.one;

            for (int x = 0; x < smoothCount; x++) {
                for (int y = 0; y < smoothCount; y++)
                {
                    T cell = Cell(pos + dPos * new Vector2(x, y));
                    if (!smoothCells.ContainsKey(cell))
                         smoothCells[cell] = dWeight;
                    else smoothCells[cell] += dWeight;
                }
            }
        } 
        
        return smoothCells;
    }

    public float DistToBorder(Vector2 pos)
    {
        int xTab = (int)(pos.x / cellSize);
        int yTab = (int)(pos.y / cellSize);
        if (pos.x < 0) xTab--;
        if (pos.y < 0) yTab--;

        (T cell, Vector2 center) crt, closest = CellTab(pos);

        int xClosest = (int)(closest.center.x / cellSize);
        int yClosest = (int)(closest.center.y / cellSize);
        if (closest.center.x < 0) xClosest--;
        if (closest.center.y < 0) yClosest--;

        Vector2 A = closest.center, B;
        float dist = Mathf.Infinity;

        for (int y = yTab - 1; y <= yTab + 1; y++) {
            for (int x = xTab - 1; x <= xTab + 1; x++)
            {
                if (x == xClosest && y == yClosest)
                    continue;

                crt = CellTab(x, y);
                B = crt.center;

                // y = mx + b
                float mAB = (B.y - A.y) / (B.x - A.x);

                Vector2 pBorder = (A + B) / 2;
                float mBorder = -1f / mAB;
                float bBorder = pBorder.y - mBorder * pBorder.x;

                dist = Mathf.Min(dist, Mathf.Abs(mBorder * pos.x - pos.y + bBorder) / Mathf.Sqrt(mBorder * mBorder + 1));
            }
        }

        return dist;
    }

    public float Shape(Vector2 pos)
    {
        return DistToBorder(pos) / cellSize;

        T cell = set[0];
        float max = 0f;

        foreach (KeyValuePair<T, float> e in SmoothCells(pos, 6, 7))
        {
            if (e.Value > max)
            {
                max = e.Value;
                cell = e.Key;
            }
        }

        if (cell.Equals(set[0]))
            return 0;

        return Mathf.Clamp01((max - 0.5f) * 2);

    } 

    public float SmoothShape(Vector2 pos, float smoothSize, int smoothCount = 2)
    {
        if (smoothCount <= 1)
            return Shape(pos);

        float dist = 0;

        pos -= smoothSize / 2 * Vector2.one;
        float dPos = smoothSize / (smoothCount-1);

        for (int y = 0; y < smoothCount; y++)
            for (int x = 0; x < smoothCount; x++)
                dist += DistToBorder(pos + new Vector2(x * dPos, y * dPos));

        return dist / (smoothCount * smoothCount) / cellSize;
    }
}
