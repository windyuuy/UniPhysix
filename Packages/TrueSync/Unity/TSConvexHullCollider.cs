using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TrueSync.Physics3D;

namespace TrueSync
{

	/**
     *  @brief Collider with a mesh shape. 
     **/
	[AddComponentMenu("TrueSync/Physics/ConvexHullCollider", 0)]
	public class TSConvexHullCollider : TSCollider
	{

		public TriangleMeshShape ShapeSpecific => Shape as TriangleMeshShape;
		protected void updateShape()
		{
			ShapeSpecific.Scale = this.tsTransform.lossyScale;
		}

		public void updateMesh()
		{
			// 缺少
			vertices = GetVertices();
			indices = GetIndices();
			var meshInfo = new TriangleMeshInfo(Vertices, Indices);
			// 避开不必要的重复初始化
			var needUpdate = this._body != null;
			this.ShapeSpecific.SetMeshInfo(meshInfo, needUpdate);
		}

		[SerializeField]
		private Mesh mesh;

		/**
         *  @brief Mesh attached to the same game object. 
         **/
		public Mesh Mesh
		{
			get { return mesh; }
			set
			{
				mesh = value;

				updateMesh();
			}
		}

		private List<TSVector> vertices;

		/**
         *  @brief A list of all mesh's vertices. 
         **/
		public List<TSVector> Vertices
		{
			get
			{
				if (vertices == null)
					vertices = GetVertices();
				return vertices;
			}
		}

		private List<TriangleVertexIndices> indices;

		/**
         *  @brief A list of mess related structs. 
         **/
		public List<TriangleVertexIndices> Indices
		{
			get
			{
				if (indices == null)
					indices = GetIndices();
				return indices;
			}
		}

		/**
         *  @brief Gets (if any) the mesh attached to this game object. 
         **/
		public void Reset()
		{
			if (mesh == null)
			{
				var meshFilter = GetComponent<MeshFilter>();
				mesh = meshFilter.sharedMesh;
			}
		}

		/**
         *  @brief Creates a shape based on attached mesh. 
         **/
		public override Shape CreateShape()
		{
			return new TriangleMeshShape();
		}

		private List<TriangleVertexIndices> GetIndices()
		{
			var triangles = mesh.triangles;
			var result = new List<TriangleVertexIndices>();
			for (int i = 0; i < triangles.Length; i += 3)
				result.Add(new TriangleVertexIndices(triangles[i + 2], triangles[i + 1], triangles[i + 0]));
			return result;
		}

		private List<TSVector> GetVertices()
		{
			var lossyScale = ShapeSpecific.GetShapeScale();
			var result = mesh.vertices.Select(p => new TSVector(p.x * lossyScale.x, p.y * lossyScale.y, p.z * lossyScale.z)).ToList();
			return result;
		}

		protected override Vector3 GetGizmosSize()
		{
			updateShape();
			return ShapeSpecific.GetShapeScale().ToVector();
			// return lossyScale.ToVector();
		}

		protected override void DrawGizmos()
		{
			Gizmos.DrawWireMesh(mesh);
		}

	}

}