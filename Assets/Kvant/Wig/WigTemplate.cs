using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Kvant
{
    public class WigTemplate : ScriptableObject
    {
        #region Public properties

        /// Number of segments (editable)
        public int segmentCount {
            get { return _segmentCount; }
        }

        [SerializeField] int _segmentCount = 8;

        /// Number of filaments (read only)
        public int filamentCount {
            get { return _foundation.width; }
        }

        // Foundation texture (read only)
        public Texture2D foundation {
            get { return _foundation; }
        }

        [SerializeField] Texture2D _foundation;

        // Tmplate mesh (read only)
        public Mesh mesh {
            get { return _mesh; }
        }

        [SerializeField] Mesh _mesh;

        #endregion

        #region Public methods

        #if UNITY_EDITOR

        // Asset initialization method
        public void Initialize(Mesh source)
        {
            if (_foundation != null)
            {
                Debug.LogError("Already initialized");
                return;
            }

            // Input vertices
            var inVertices = source.vertices;
            var inNormals = source.normals;

            // Output vertices
            var outVertices = new List<Vector3>();
            var outNormals = new List<Vector3>();

            // Enumerate unique vertices
            for (var i = 0; i < inVertices.Length; i++)
            {
                if (!outVertices.Any(_ => _ == inVertices[i]))
                {
                    outVertices.Add(inVertices[i]);
                    outNormals.Add(inNormals[i]);
                }
            }

            // Create a texture to store the foundation.
            var tex = new Texture2D(outVertices.Count, 2, TextureFormat.RGBAFloat, false);
            tex.name = "Wig Foundation";
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            // Store the vertices into the texture.
            for (var i = 0; i < outVertices.Count; i++)
            {
                var v = outVertices[i];
                var n = outNormals[i];
                tex.SetPixel(i, 0, new Color(v.x, v.y, v.z, 1));
                tex.SetPixel(i, 1, new Color(n.x, n.y, n.z, 0));
            }

            // Finish up the texture.
            tex.Apply(false, true);
            _foundation = tex;

            // Build the initial template mesh.
            RebuildMesh();
        }

        #endif

        // Template mesh rebuild method
        public void RebuildMesh()
        {
            _mesh.Clear();

            // The number of vertices in the foundation == texture width
            var vcount = _foundation.width;
            var length = Mathf.Clamp(_segmentCount, 3, 64);

            // Create vertex array for the template.
            var vertices = new List<Vector3>(vcount * length);
            var indices = new List<int>(vcount * (length - 1) * 2);

            for (var i1 = 0; i1 < vcount; i1++)
            {
                var u = (float)i1 / vcount;

                for (var i2 = 0; i2 < length; i2++)
                {
                    var v = (float)i2 / length;
                    vertices.Add(new Vector3(u, v, 0));
                }

                for (var i2 = 0; i2 < length - 1; i2++)
                {
                    var i = i1 * length + i2;
                    indices.Add(i);
                    indices.Add(i + 1);
                }
            }

            // Reset the mesh asset.
            _mesh.SetVertices(vertices);
            _mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
            _mesh.Optimize();
            _mesh.UploadMeshData(true);
        }

        #endregion

        #region ScriptableObject functions

        void OnEnable()
        {
            if (_mesh == null)
            {
                _mesh = new Mesh();
                _mesh.name = "Wig Template";
            }
        }

        #endregion
    }
}
