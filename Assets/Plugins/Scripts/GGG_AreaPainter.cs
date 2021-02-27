#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using Unity.Collections;

namespace GrandGeoGrass
{

#if UNITY_2019_3_OR_NEWER
    [ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif

    [RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
    public class GGG_AreaPainter : MonoBehaviour
    {
        [SerializeField] Vector2 area = new Vector2(10, 10);
        [SerializeField] int amount = 10;

        [SerializeField] int randomSeed = 0;
        [SerializeField] bool useSeed = true;
        [SerializeField] float rayStartHeight = 100;
        public bool uniform = true;
        [SerializeField] Vector2 grassHeightRange = Vector2.one;
        [SerializeField] Vector2 grassWidthRange = Vector2.one;

        [Space]

        [SerializeField] bool useDensityMap = false;
        public Texture2D densityMap = null;
        [SerializeField] Vector2 tiling = Vector2.one; //min 0
        [SerializeField] float densityMultiplier = 1f; //Range 0-1
        [SerializeField] float densityThreshold = 1f; //Range 0-1
        [SerializeField] bool worldSpace = false;

        [Space]
        public Material material = null;
        [HideInInspector] public MeshFilter meshFilter = null;
        [SerializeField] MeshRenderer rend;
        Mesh mesh;

        //---------------------
        [HideInInspector] public string saveFolder = "GrassMesh";
        [HideInInspector] public Texture logo;


        private void OnValidate()
        {
            if (this.gameObject.scene.IsValid())
            {
                if (meshFilter == null)
                    meshFilter = GetComponent<MeshFilter>();
                if (rend == null)
                {
                    rend = GetComponent<MeshRenderer>();
                    rend.material = material;
                }
                else
                    rend.material = material;
            }
        }

        public void Clear()
        {
            meshFilter.sharedMesh.Clear();
        }

        Vector3[] GeneratePositions()
        {
            List<Vector3> positions = new List<Vector3>();
            if (useDensityMap && densityMap != null)
            {
                int count = Mathf.FloorToInt(Mathf.Clamp(densityMap.width * densityMap.height, 1, 60000) * densityMultiplier);

                int index = 0;
                while (index < count)
                {
                    Vector3 origin = transform.position;
                    origin.y += rayStartHeight;
                    origin.x += area.x * Random.Range(-0.5f, 0.5f);
                    origin.z += area.y * Random.Range(-0.5f, 0.5f);

                    Vector2 uv;
                    if (worldSpace)
                    {
                        uv.x = 1 - (origin.z) / area.y;
                        uv.y = (origin.x) / area.x;
                    }
                    else
                    {
                        uv.x = 1 - (origin.z - transform.position.z + (area.x / 2)) / area.y;
                        uv.y = (origin.x - transform.position.x + (area.x / 2)) / area.x;
                    }

                    //Replace that with something efficient
                    Color density = densityMap.GetPixel(System.Convert.ToInt32(uv.x * densityMap.width * tiling.x), System.Convert.ToInt32(uv.y * densityMap.height * tiling.y));

                    if (density.grayscale < densityThreshold)
                    {
                        positions.Add(origin);
                        index++;
                    }
                }
            }
            else
            {
                for (int i = 0; i < amount; i++)
                {
                    Vector3 origin = transform.position;
                    origin.y += rayStartHeight;
                    origin.x += area.x * Random.Range(-0.5f, 0.5f);
                    origin.z += area.y * Random.Range(-0.5f, 0.5f);

                    positions.Add(origin);
                }
            }

            return positions.ToArray();
        }

        public void Generate()
        {
            if (mesh == null)
                mesh = new Mesh();

            if (useSeed)
                Random.InitState(randomSeed);

            Vector3[] TEMPpositions = GeneratePositions();
            amount = TEMPpositions.Length;

            NativeArray<Vector3> rayPositions = new NativeArray<Vector3>(amount, Allocator.TempJob);
            NativeArray<Vector3> vertices = new NativeArray<Vector3>(amount, Allocator.TempJob);
            NativeArray<int> indices = new NativeArray<int>(amount, Allocator.TempJob);
            NativeArray<Vector3> normals = new NativeArray<Vector3>(amount, Allocator.TempJob);
            NativeArray<Vector4> tangents = new NativeArray<Vector4>(amount, Allocator.TempJob);
            NativeArray<Color> colors = new NativeArray<Color>(amount, Allocator.TempJob);

            //Copy generated positions
            rayPositions.CopyFrom(TEMPpositions);

            for (int i = 0; i < amount; i++)
            {
                colors[i] = new Color(
                    Random.Range(grassHeightRange.x, grassHeightRange.y), //Height
                    1, //This is used by the wind, but could be saved and used before the wind is applied
                    Random.Range(grassWidthRange.x, grassWidthRange.y)); //Width;
            }

            var raycastCommands = new NativeArray<RaycastCommand>(amount, Allocator.TempJob);
            NativeArray<RaycastHit> raycastHits = new NativeArray<RaycastHit>(amount, Allocator.TempJob);

            var setupRaycastsJob = new PrepareRaycastCommands()
            {
                Raycasts = raycastCommands,
                Positions = rayPositions
            };

            var setupDependency = setupRaycastsJob.Schedule(amount, 32, default(JobHandle));

            var raycastDependency = RaycastCommand.ScheduleBatch(raycastCommands, raycastHits, 32, setupDependency);

            raycastDependency.Complete();

            var setupMeshDataJob = new PrepareMeshData()
            {
                Vertices = vertices,
                Indices = indices,
                Normals = normals,
                Tangents = tangents,
                Colors = colors,
                hit = raycastHits,
                ObjPosition = transform.position
            };

            var setupMeshDependency = setupMeshDataJob.Schedule(amount, 32, raycastDependency);

            setupMeshDependency.Complete();

            mesh.Clear();
#if UNITY_2019_3_OR_NEWER
            mesh.SetVertices(vertices);
            mesh.SetIndices(indices, MeshTopology.Points, 0);
            mesh.SetNormals(normals);
            mesh.SetTangents(tangents);
            mesh.SetColors(colors);
#else
        mesh.vertices = vertices.ToArray();
        mesh.SetIndices(indices.ToArray(), MeshTopology.Points, 0);
        mesh.normals = normals.ToArray();
        mesh.tangents = tangents.ToArray();
        mesh.colors = colors.ToArray();
#endif

            meshFilter.sharedMesh = mesh;

            raycastCommands.Dispose();
            raycastHits.Dispose();
            rayPositions.Dispose();
            vertices.Dispose();
            indices.Dispose();
            normals.Dispose();
            tangents.Dispose();
            colors.Dispose();
        }

        struct PrepareRaycastCommands : IJobParallelFor
        {
            public NativeArray<RaycastCommand> Raycasts;
            [ReadOnly]
            public NativeArray<Vector3> Positions;


            public void Execute(int i)
            {
                Raycasts[i] = new RaycastCommand(Positions[i], Vector3.down);
            }
        }

        struct PrepareMeshData : IJobParallelFor
        {
            public NativeArray<Vector3> Vertices;
            public NativeArray<int> Indices;
            public NativeArray<Vector3> Normals;
            public NativeArray<Vector4> Tangents;
            public NativeArray<Color> Colors;
            [ReadOnly]
            public NativeArray<RaycastHit> hit;
            [ReadOnly]
            public Vector3 ObjPosition;


            public void Execute(int i)
            {
                Vertices[i] = (hit[i].point - ObjPosition);
                Indices[i] = i;
                Normals[i] = hit[i].normal;
                Vector4 tangent = Vector3.Cross(hit[i].normal, Vector3.forward).normalized;
                tangent.w = -1;
                Tangents[i] = tangent;
                Colors[i] = Colors[i];
            }
        }
    }

}

#endif