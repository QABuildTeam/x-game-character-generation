using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereMeshGenerator : MonoBehaviour
{

    Mesh mesh;

    [SerializeField]
    protected float radius = 2f;
    [SerializeField]
    [Range(4, 30000)]
    protected int verticesCount = 100;
    protected int realVerticesCount;

    private void Awake()
    {
    }
    // Use this for initialization
    void Start()
    {
        GenerateMesh();
    }

    // Update is called once per frame
    [SerializeField]
    protected float deltaAngle = 15f;
    void Update()
    {
        transform.Rotate(Vector3.back + Vector3.left, Mathf.Deg2Rad * deltaAngle, Space.Self);
    }

    void GenerateMesh()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshRenderer == null || meshFilter == null)
        {
            return;
        }
        mesh = new Mesh();
        // calculate the real number of vertices
        // k - number of vertices on a parallel (including the equator) or twice on a meridian
        // then the number of meridians equals to k
        // and the number of parallels equals to (k / 2) - 2 (because the top parallels yield to a single vertice)
        // total number of vertices would be equal to:
        // k * (k - 2)/ 2 + 2 = verticesCount
        // k^2/2 - k + 2 = verticesCount
        // k^2 * (1/2) - k + (2 - verticesCount) = 0
        // k = Ceil((-(-1) +/- sqrt((-1)^2 - 4 * (1/2) * (2 - verticesCount))) / 2 * (1/2))
        float discriminant = 1 - 2 * (2 - verticesCount);
        if (discriminant >= 0)
        {
            float discriminantSqrt = Mathf.Sqrt(discriminant);
            int k1 = Mathf.CeilToInt(1 + discriminantSqrt);
            int k2 = Mathf.CeilToInt(1 - discriminantSqrt);
            int vpp = Mathf.Max(k1, k2);
            if (vpp > 0)
            {
                // make vpp even
                if ((vpp % 2) != 0)
                {
                    vpp++;
                }
                Debug.Log("vpp = " + vpp.ToString());
                // vpp is the number of Vertices Per a Parallel
                realVerticesCount = vpp * (vpp - 2) / 2 + 2;
                Debug.Log("realVerticesCount = " + realVerticesCount.ToString());
                int trianglesCount = vpp + ((vpp - 2) / 2 - 1) * vpp * 2 + vpp;
                Debug.Log("trianglesCount = " + trianglesCount.ToString());

                Vector3[] vertices = new Vector3[realVerticesCount];
                int[] triangles = new int[trianglesCount * 3];
                Vector2[] uv = new Vector2[realVerticesCount];

                // vertex index
                int iv = 0;
                // triangle index
                int it = 0;
                // uv index
                int iuv = 0;

                // top vertex
                vertices[iv++] = new Vector3(0, radius, 0);
                uv[iuv++] = new Vector2(0.5f, 1f);

                // meridian loop
                float deltaAngle = Mathf.PI * 2 / vpp;
                float yAngle = Mathf.PI / 2 - deltaAngle;

                float uvDeltaY = 1f / ((vpp - 2) / 2 + 1);
                float uvDeltaX = 1f / (vpp - 1);
                float uvY = 1f - uvDeltaY;

                float yCoordinate = radius * Mathf.Sin(yAngle);
                float xzRadius = radius * Mathf.Cos(yAngle);

                float xzAngle = 0;
                float uvX = 0;

                int tRangeStart = iv;

                // top parallel
                for (int i = 0; i < vpp; uvX += uvDeltaX, xzAngle += deltaAngle, i++)
                {
                    // just one triangle
                    triangles[it++] = iv;
                    triangles[it++] = 0;
                    triangles[it++] = (i + 1) % vpp + tRangeStart;

                    vertices[iv++] = new Vector3(Mathf.Cos(xzAngle) * xzRadius, yCoordinate, Mathf.Sin(xzAngle) * xzRadius);
                    uvX = Mathf.Clamp(uvX, 0, 1);
                    uv[iuv++] = new Vector2(uvX, uvY);
                }

                // meridian loop
                yAngle -= deltaAngle;
                uvY -= uvDeltaY;
                tRangeStart += vpp;
                for (int i = 0; i < (vpp - 2) / 2 - 1; uvY -= uvDeltaY, yAngle -= deltaAngle, tRangeStart += vpp, i++)
                {
                    yCoordinate = radius * Mathf.Sin(yAngle);
                    xzRadius = radius * Mathf.Cos(yAngle);
                    uvY = Mathf.Clamp(uvY, 0, 1);

                    // parallel loop
                    xzAngle = 0;
                    uvX = 0;

                    for (int j = 0; j < vpp; uvX += uvDeltaX, xzAngle += deltaAngle, j++)
                    {
                        // first triangle
                        triangles[it++] = iv;
                        triangles[it++] = iv - vpp;
                        triangles[it++] = (j + 1) % vpp + tRangeStart;
                        // second triangle
                        triangles[it++] = (j + 1) % vpp + tRangeStart;
                        triangles[it++] = iv - vpp;
                        triangles[it++] = (j + 1) % vpp + tRangeStart - vpp;

                        vertices[iv++] = new Vector3(Mathf.Cos(xzAngle) * xzRadius, yCoordinate, Mathf.Sin(xzAngle) * xzRadius);
                        uvX = Mathf.Clamp(uvX, 0, 1);
                        uv[iuv++] = new Vector2(uvX, uvY);
                    }
                }

                Debug.Log("triangles filled: " + (it / 3).ToString());

                // tRangeStart now points beyond parallels, so move it back to the last one
                tRangeStart -= vpp;
                for (int i = 0; i < vpp; i++)
                {
                    // just one triangle
                    triangles[it++] = tRangeStart + i;
                    triangles[it++] = (i + 1) % vpp + tRangeStart;
                    triangles[it++] = realVerticesCount - 1;
                    // verices and uvs are already filled
                }

                Debug.Log("triangles filled: " + (it / 3).ToString());

                // bottom vertex
                vertices[iv++] = new Vector3(0, -radius, 0);
                // no triangles, but uv is here
                uv[iuv++] = new Vector2(0.5f, 0f);

                Debug.Log("Vertices filled: " + iv.ToString());
                Debug.Log("UVs filled: " + iuv.ToString());

                // now generate the material texture
                Texture2D sphereTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
                for (int x = 0; x < sphereTexture.width; x++)
                {
                    for (int y = 0; y < sphereTexture.height; y++)
                    {
                        //Color c = new Color((float)x / (float)sphereTexture.width, (float)y / (float)sphereTexture.height, 0.5f, 1f);
                        Color c = new Color(0.5f, (float)y / (float)sphereTexture.height, 0.5f, 1f);
                        sphereTexture.SetPixel(x, y, c);
                    }
                }
                sphereTexture.Apply();
                Material material = new Material(Shader.Find("Standard"));
                if (material != null)
                {
                    material.mainTexture = sphereTexture;
                }
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.uv = uv;
                mesh.RecalculateNormals();
                meshFilter.mesh = mesh;
                meshRenderer.material = material;
            }
        }
    }
}
