using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Grass : MonoBehaviour {
  struct SurfaceSample {
    public Vector3 position;
    public Vector3 normal;
    public Vector3 tangent;
  }

  public int numGrass = 10000;
  public MeshFilter surfaceMeshFilter;
  public ComputeShader shader;
  public Material material;
  public Mesh grassMesh;

  // Per-surface data
  private ComputeBuffer m_samplesBuffer;
  private List<Vector3> m_vertices;
  private List<Vector3> m_normals;
  private List<Vector4> m_tangents;
  private List<int> m_indices;

  private RenderParams renderParams;

  void Awake() {
    m_samplesBuffer = new ComputeBuffer(numGrass, Marshal.SizeOf<SurfaceSample>());
    
    // TODO figure out what to do with bounds. Surface mesh +- margins?
    int extent = 100;
    
    renderParams = new(material);
    // renderParams.receiveShadows = true;
    // renderParams.shadowCastingMode = ShadowCastingMode.On;
    renderParams.matProps = new();
    renderParams.worldBounds = new Bounds(new(), new(extent, extent, extent));
    renderParams.matProps.SetBuffer("samples", m_samplesBuffer);
  }
  
  void Start() {
    // Collect data
    m_vertices = new();
    m_normals = new();
    m_tangents = new();
    m_indices = new();

    var surfaceMesh = surfaceMeshFilter.mesh;
    var surfaceTransform = surfaceMeshFilter.transform;
    
    surfaceMesh.GetVertices(m_vertices);
    surfaceMesh.GetNormals(m_normals);
    surfaceMesh.GetTriangles(m_indices, 0);
    surfaceMesh.GetTangents(m_tangents);

    // Calculate areas
    int numTriangles = m_indices.Count / 3;
    var areas = new float[numTriangles];
    float totalArea = 0.0f;
    
    for (int i = 0; i < numTriangles; ++i) {
      var A = m_vertices[m_indices[i * 3]];
      var B = m_vertices[m_indices[i * 3 + 1]];
      var C = m_vertices[m_indices[i * 3 + 2]];

      var area = 0.5f * Vector3.Cross(B - A, C - A).magnitude;
      areas[i] = area;
      totalArea += area;
    }

    // Calculate CDF
    var cdf = new float[numTriangles];
    for (int i = 0; i < numTriangles; ++i) {
      cdf[i] = areas[i] / totalArea;
      if (i != 0) {
        cdf[i] += cdf[i - 1];
      }
    }

    // Create buffers
    var vertexBuffer = new ComputeBuffer(m_vertices.Count, Marshal.SizeOf<Vector3>());
    var normalBuffer = new ComputeBuffer(m_normals.Count, Marshal.SizeOf<Vector3>());
    var tangentsBuffer = new ComputeBuffer(m_tangents.Count, Marshal.SizeOf<Vector4>());
    var indexBuffer = new ComputeBuffer(m_indices.Count, sizeof(int));
    var cdfBuffer = new ComputeBuffer(cdf.Length, sizeof(float));
    
    vertexBuffer.SetData(m_vertices);
    normalBuffer.SetData(m_normals);
    tangentsBuffer.SetData(m_tangents);
    indexBuffer.SetData(m_indices);
    cdfBuffer.SetData(cdf);
    
    // Link
    int kernel = shader.FindKernel("GrassInit");
    shader.SetBuffer(kernel, "vertices", vertexBuffer);
    shader.SetBuffer(kernel, "normals", normalBuffer);
    shader.SetBuffer(kernel, "tangents", tangentsBuffer);
    shader.SetBuffer(kernel, "indices", indexBuffer);
    shader.SetBuffer(kernel, "cdf", cdfBuffer);
    shader.SetInt("numInstances", numGrass);
    shader.SetInt("numTriangles", numTriangles);
    shader.SetInt("seed", 0);
    shader.SetMatrix("transform", surfaceTransform.localToWorldMatrix);
    
    shader.SetBuffer(kernel, "samples", m_samplesBuffer);
    
    // Dispatch
    shader.GetKernelThreadGroupSizes(kernel, out uint batchX, out uint _, out uint _);
    shader.Dispatch(kernel, Mathf.CeilToInt(numGrass / (float)batchX), 1, 1);
    
    // Dispose
    vertexBuffer.Dispose();
    normalBuffer.Dispose();
    tangentsBuffer.Dispose();
    indexBuffer.Dispose();
    cdfBuffer.Dispose();
    
    // Get Data TODO debug
    // var samples = new SurfaceSample[numGrass];
    // m_samplesBuffer.GetData(samples);
    //
    // foreach (var sample in samples) {
    //   Debug.DrawRay(sample.position, sample.normal, Color.green, float.MaxValue);
    // }
    //
    // Debug.Log("Haha");
  }

  private void OnDestroy() {
    m_samplesBuffer.Dispose();
  }

  void Update() {
    Graphics.RenderMeshPrimitives(renderParams, grassMesh, 0, numGrass);
  }
}