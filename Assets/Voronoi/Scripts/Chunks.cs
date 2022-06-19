using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunks : MonoBehaviour
{
    public static Chunks inst;

    public float chunkSize = 10;
    public int chunkRes = 10;
    public int chunkRadius = 3;
    public Material chunkMaterial;

    (int x, int y) playerCoor = (0, 0);
    Dictionary<(int, int), GameObject> chunkTab = new Dictionary<(int, int), GameObject>();
    Queue<(int, int)> createChunkQueue = new Queue<(int, int)>();






    private void OnDrawGizmos()
    {
        Player player = FindObjectOfType<Player>();
        (int x, int y) playerCoor = ((int)(player.transform.position.x / chunkSize) - (player.transform.position.x < 0 ? 1 : 0),
                                     (int)(player.transform.position.z / chunkSize) - (player.transform.position.z < 0 ? 1 : 0));

        for (int x = -chunkRadius; x <= chunkRadius; x++)
            for (int y = -chunkRadius; y <= chunkRadius; y++)
                if (x * x + y * y <= chunkRadius * chunkRadius)
                    Gizmos.DrawWireCube(new Vector3((playerCoor.x + x + 0.5f) * chunkSize, 0, (playerCoor.y + y + 0.5f) * chunkSize), new Vector3(1, 0, 1) * chunkSize);

    }


    private void Awake()
    {
        inst = this;
    }

    private void Start()
    {
        UpdateChunks(true);
        StartCoroutine(UpdateChunksIEnum());
    }



    IEnumerator UpdateChunksIEnum()
    {
        while (true)
        {
            UpdateChunks();
            yield return new WaitForSeconds(1);
        }
    }


    IEnumerator CreateChunkIEnum()
    {
        while (createChunkQueue.Count > 0)
        {
            int x, y;
            (x, y) = createChunkQueue.Dequeue();
            CreateChunk(x, y);

            yield return new WaitForEndOfFrame();
        }
    }


    void UpdateChunks(bool force = false)
    {
        (int, int) newPlayerCoor = ((int)(Player.inst.transform.position.x / chunkSize) - (Player.inst.transform.position.x < 0 ? 1 : 0),
                                    (int)(Player.inst.transform.position.z / chunkSize) - (Player.inst.transform.position.z < 0 ? 1 : 0));

        if (!force && playerCoor == newPlayerCoor)
            return;

        for (int x = -chunkRadius; x <= chunkRadius; x++) {
            for (int y = -chunkRadius; y <= chunkRadius; y++)
            {
                if (x * x + y * y <= chunkRadius * chunkRadius)
                {
                    int xt = playerCoor.x + x;
                    int yt = playerCoor.y + y;

                    if (chunkTab.ContainsKey((xt, yt)))
                      chunkTab[(xt, yt)].SetActive(false);
                }
            }
        }


        playerCoor = newPlayerCoor;
        createChunkQueue.Clear();

        for (int x = -chunkRadius; x <= chunkRadius; x++) {
            for (int y = -chunkRadius; y <= chunkRadius; y++)
            {
                if (x * x + y * y <= chunkRadius * chunkRadius)
                {
                    int xt = playerCoor.x + x;
                    int yt = playerCoor.y + y;

                    if (chunkTab.ContainsKey((xt, yt)))
                        chunkTab[(xt, yt)].SetActive(true);

                    else createChunkQueue.Enqueue((xt, yt));
                }
            }
        }

        StopCoroutine("CreateChunkIEnum");
        StartCoroutine("CreateChunkIEnum");
    }


    void CreateChunk(int x, int y)
    {
        GameObject chunk = new GameObject(x + " : " + y);
        chunk.transform.SetParent(transform);
        chunk.transform.position = new Vector3(x, 0, y) * chunkSize;

        Mesh mesh = new Mesh();

        // vertices
        Vector3[] vertices = new Vector3[chunkRes * chunkRes];
        Color[] colors = new Color[chunkRes * chunkRes];
        float dpos = chunkSize / (chunkRes - 1);

        for (int x2 = 0; x2 < chunkRes; x2++) {
            for (int y2 = 0; y2 < chunkRes; y2++)
            {
                Vector2 pos = new Vector2(x * chunkSize + x2 * dpos, y * chunkSize + y2 * dpos);
                Biome biome = VoronoiBiomes.inst.voronoi.Cell(pos);
                float height;
                Color color;
                (height, color) = biome.Evaluate(pos, VoronoiBiomes.inst.Shape(pos));
                vertices[y2 * chunkRes + x2] = new Vector3(x2 * dpos, height, y2 * dpos);
                colors[y2 * chunkRes + x2] = color;
            }
        }

        // triangles
        int[] triangles = new int[(chunkRes - 1) * (chunkRes - 1) * 6];

        for (int x2 = 0; x2 < chunkRes - 1; x2++) {
            for (int y2 = 0; y2 < chunkRes - 1; y2++)
            {
                triangles[(y2 * (chunkRes - 1) + x2) * 6 + 0] =  y2      * chunkRes + x2;
                triangles[(y2 * (chunkRes - 1) + x2) * 6 + 1] = (y2 + 1) * chunkRes + x2;
                triangles[(y2 * (chunkRes - 1) + x2) * 6 + 2] = (y2 + 1) * chunkRes + x2 + 1;

                triangles[(y2 * (chunkRes - 1) + x2) * 6 + 3] =  y2      * chunkRes + x2;
                triangles[(y2 * (chunkRes - 1) + x2) * 6 + 4] = (y2 + 1) * chunkRes + x2 + 1;
                triangles[(y2 * (chunkRes - 1) + x2) * 6 + 5] =  y2      * chunkRes + x2 + 1;
            }
        }

        // mesh
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        chunk.AddComponent<MeshFilter>().mesh = mesh;
        chunk.AddComponent<MeshRenderer>().material = chunkMaterial;

        chunkTab[(x, y)] = chunk;
    }



}
