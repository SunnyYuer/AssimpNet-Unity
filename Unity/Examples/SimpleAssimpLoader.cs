using UnityEngine;
using Assimp;
using System.Collections.Generic;
using System.IO;
using Material = UnityEngine.Material;
using Mesh = UnityEngine.Mesh;

public class SimpleAssimpLoader : MonoBehaviour
{
    void Start()
    {
        // 替换为你的模型路径
        string modelPath = "C:/Users/Yuer/Desktop/cube.fbx";
        LoadModel(modelPath);
    }

    void LoadModel(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            LogUtil.LogToFile($"文件不存在: {filePath}");
            return;
        }
        // 1. 创建导入上下文
        using (var context = new AssimpContext())
        {
            // 2. 导入文件
            // PostProcessSteps 用于预处理模型，例如三角化、生成法线等
            var scene = context.ImportFile(filePath,
                PostProcessSteps.Triangulate |
                PostProcessSteps.GenerateSmoothNormals |
                PostProcessSteps.CalculateTangentSpace);

            if (scene == null || !scene.HasMeshes)
            {
                Debug.LogError("场景为空");
                return;
            }

            // 3. 遍历场景中的每个网格并创建 Unity GameObject
            for (int i = 0; i < scene.MeshCount; i++)
            {
                var assimpMesh = scene.Meshes[i];
                GameObject meshObj = new GameObject(assimpMesh.Name ?? $"Mesh_{i}");

                // 转换 Mesh 数据
                Mesh unityMesh = ConvertToUnityMesh(assimpMesh);

                // 添加组件
                var meshFilter = meshObj.AddComponent<MeshFilter>();
                meshFilter.mesh = unityMesh;

                var meshRenderer = meshObj.AddComponent<MeshRenderer>();
                // 这里简单使用默认材质，实际项目中需要处理 Assimp 的材质和纹理
                meshRenderer.material = new Material(Shader.Find("Standard"));
            }
        }
    }

    /// <summary>
    /// 将 Assimp 网格转换为 Unity 网格
    /// </summary>
    private Mesh ConvertToUnityMesh(Assimp.Mesh assimpMesh)
    {
        Mesh unityMesh = new Mesh();

        // 如果顶点数可能超过 65535，必须设置
        if (assimpMesh.VertexCount > 65535)
        {
            unityMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        // --- 顶点 ---
        Vector3[] vertices = new Vector3[assimpMesh.VertexCount];
        for (int i = 0; i < assimpMesh.VertexCount; i++)
        {
            var v = assimpMesh.Vertices[i];
            // 这里不需要Z取反
            vertices[i] = new Vector3(v.X, v.Y, v.Z);
        }
        unityMesh.vertices = vertices;

        // --- 三角形索引 ---
        if (assimpMesh.HasFaces)
        {
            int[] triangles = new int[assimpMesh.FaceCount * 3];
            for (int i = 0; i < assimpMesh.FaceCount; i++)
            {
                var face = assimpMesh.Faces[i];
                // AssimpNetter 确保三角化后每个面有3个索引
                triangles[i * 3] = face.Indices[0];
                triangles[i * 3 + 1] = face.Indices[1];
                triangles[i * 3 + 2] = face.Indices[2];
            }
            unityMesh.triangles = triangles;
        }

        // --- 法线 ---
        if (assimpMesh.HasNormals)
        {
            Vector3[] normals = new Vector3[assimpMesh.VertexCount];
            for (int i = 0; i < assimpMesh.VertexCount; i++)
            {
                var n = assimpMesh.Normals[i];
                normals[i] = new Vector3(n.X, n.Y, n.Z);
            }
            unityMesh.normals = normals;
        }

        // --- UV (纹理坐标) ---
        if (assimpMesh.HasTextureCoords(0))
        {
            Vector2[] uv = new Vector2[assimpMesh.VertexCount];
            for (int i = 0; i < assimpMesh.VertexCount; i++)
            {
                var t = assimpMesh.TextureCoordinateChannels[0][i];
                uv[i] = new Vector2(t.X, t.Y);
            }
            unityMesh.uv = uv;
        }

        // 重新计算边界盒等
        unityMesh.RecalculateBounds();

        return unityMesh;
    }
}
