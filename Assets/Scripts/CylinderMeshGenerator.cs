using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CylinderMeshGenerator : MonoBehaviour
{

    Mesh mesh;

    [SerializeField]
    protected float radius = 2f;
    [SerializeField]
    protected float height = 4f;
    [SerializeField]
    [Range(1, 100)]
    protected int heightSubdivisions = 3;
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

    void DisplayNormals()
    {
        if (mesh != null)
        {
            Vector3[] normals = mesh.normals;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            // face normals
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 v0 = transform.TransformPoint(vertices[triangles[i]]);
                Vector3 v1 = transform.TransformPoint(vertices[triangles[i + 1]]);
                Vector3 v2 = transform.TransformPoint(vertices[triangles[i + 2]]);
                Vector3 center = (v0 + v1 + v2) / 3;

                Vector3 dir = Vector3.Cross(v1 - v0, v2 - v0);
                dir /= dir.magnitude;

                Debug.DrawRay(center, dir, Color.yellow);
            }
            // vertex normals
            for (int i = 0; i < vertices.Length; i++)
            {
                Debug.DrawRay(transform.TransformPoint(vertices[i]), transform.TransformVector(normals[i]), Color.red);
            }
        }
    }

    // Update is called once per frame
    [SerializeField]
    protected float deltaAngle = 15f;
    void Update()
    {
        transform.Rotate(Vector3.back + Vector3.left, Mathf.Deg2Rad * deltaAngle, Space.Self);
        DisplayNormals();
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
        // k - number of vertices on a parallel
        // total number of vertices would be equal to:
        // k * (heightSubdivisions + 1) + 2 = verticesCount
        // k = (verticesCount - 2) / (heightSubdivisions + 1)
        if (heightSubdivisions > 0)
        {
            int vpp = (verticesCount - 2) / (heightSubdivisions + 1);
            if (vpp > 0)
            {
                Debug.Log("vpp = " + vpp.ToString());
                // vpp is the number of Vertices Per a Parallel
                realVerticesCount = vpp * (heightSubdivisions + 1) + 2;
                Debug.Log("realVerticesCount = " + realVerticesCount.ToString());
                int trianglesCount = heightSubdivisions * vpp * 2 + vpp * 2;
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
                vertices[iv++] = new Vector3(0, height / 2, 0);
                uv[iuv++] = new Vector2(0.5f, 1f);

                // meridian loop
                float deltaAngle = Mathf.PI * 2 / vpp;
                float deltaY = height / heightSubdivisions;

                float uvDeltaY = 1f / (heightSubdivisions + 2);
                float uvDeltaX = 1f / (vpp - 1);
                float uvY = 1f - uvDeltaY;

                float yCoordinate = height / 2;
                float xzRadius = radius;

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
                    //uvX = Mathf.Clamp(uvX, 0, 1);
                    uv[iuv++] = new Vector2(uvX, uvY);
                }

                // meridian loop
                uvY -= uvDeltaY;
                yCoordinate -= deltaY;
                tRangeStart += vpp;
                for (int i = 0; i < heightSubdivisions; uvY -= uvDeltaY, yCoordinate -= deltaY, tRangeStart += vpp, i++)
                {
                    //uvY = Mathf.Clamp(uvY, 0, 1);

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
                        //uvX = Mathf.Clamp(uvX, 0, 1);
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
                vertices[iv++] = new Vector3(0, -height / 2, 0);
                // no triangles, but uv is here
                uv[iuv++] = new Vector2(0.5f, 0f);

                Debug.Log("Vertices filled: " + iv.ToString());
                Debug.Log("UVs filled: " + iuv.ToString());

                // now generate the material texture
                Texture2D cylinderTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
                for (int x = 0; x < cylinderTexture.width; x++)
                {
                    for (int y = 0; y < cylinderTexture.height; y++)
                    {
                        //Color c = new Color((float)x / (float)sphereTexture.width, (float)y / (float)sphereTexture.height, 0.5f, 1f);
                        Color c = new Color(0.5f, (float)y / (float)cylinderTexture.height, 0.5f, 1f);
                        cylinderTexture.SetPixel(x, y, c);
                    }
                }
                cylinderTexture.Apply();
                Material material = new Material(Shader.Find("Standard"));
                if (material != null)
                {
                    material.mainTexture = cylinderTexture;
                }
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.uv = uv;
                mesh.RecalculateNormals();
                meshFilter.mesh = mesh;
                meshRenderer.material = material;
                DisplayNormals();
            }
        }
    }
}
