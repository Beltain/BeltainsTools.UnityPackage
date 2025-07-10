using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeltainsTools.Experimental
{
    public static class MeshBuilder
    {
        public static Mesh BuildFOVPrism(Vector3 origin, Vector3 direction, float startDistance, float endDistance, float height, float fovAngle)
        {
            float halfAngle = fovAngle / 2f;
            Vector3 directionVectorL = Quaternion.Euler(0f, -halfAngle, 0f) * Vector3.forward;
            Vector3 directionVectorR = Quaternion.Euler(0f, halfAngle, 0f) * Vector3.forward;

            float halfHeight = height / 2f;
            Vector3[] vertices = new Vector3[]
            {
                //Near plane
                /*0*/ (directionVectorL * startDistance) + (Vector3.down * halfHeight),
                /*1*/ (directionVectorL * startDistance) + (Vector3.up * halfHeight),
                /*2*/ (directionVectorR * startDistance) + (Vector3.up * halfHeight),
                /*3*/ (directionVectorR * startDistance) + (Vector3.down * halfHeight),
                //Far plane
                /*4*/ (directionVectorR * endDistance) + (Vector3.down * halfHeight),
                /*5*/ (directionVectorR * endDistance) + (Vector3.up * halfHeight),
                /*6*/ (directionVectorL * endDistance) + (Vector3.up * halfHeight),
                /*7*/ (directionVectorL * endDistance) + (Vector3.down * halfHeight),
            };

            Quaternion orientationRotation = Quaternion.FromToRotation(Vector3.forward, direction);
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = orientationRotation * vertices[i];
                vertices[i] += origin;
            }

            Mesh resPrism = BuildPlane(vertices[0], vertices[1], vertices[2], vertices[3]);
            resPrism.AddPlane(vertices[4], vertices[5], vertices[6], vertices[7]);

            resPrism.AddPlane(vertices[7], vertices[6], vertices[1], vertices[0]);
            resPrism.AddPlane(vertices[3], vertices[2], vertices[5], vertices[4]);
            resPrism.AddPlane(vertices[1], vertices[6], vertices[5], vertices[2]);
            resPrism.AddPlane(vertices[4], vertices[7], vertices[0], vertices[3]);

            resPrism.RecalculateNormals();
            resPrism.RecalculateBounds();
            resPrism.RecalculateTangents();
            return resPrism;
        }

        public static Mesh BuildCube(float scale) => BuildCube(Vector3.one * scale);
        public static Mesh BuildCube(float scale, Vector3 originOffset) => BuildCube(Vector3.one * scale, originOffset);
        public static Mesh BuildCube(Vector3 dimentions) => BuildCube(dimentions, Vector3.zero);
        public static Mesh BuildCube(Vector3 dimentions, Vector3 originOffset)
        {
            Vector3 halfDims = dimentions / 2f;
            Vector2 rightDimentions = new Vector2(dimentions.y, dimentions.z);
            Vector2 forwardDimentions = new Vector2(dimentions.x, dimentions.y);
            Vector2 upDimentions = new Vector2(dimentions.x, dimentions.z);

            Mesh resCube = BuildPlane((Vector3.down * halfDims.y) + (-originOffset), upDimentions, Vector3.down);
            resCube.AddPlane((Vector3.left * halfDims.x) + (-originOffset), rightDimentions, Vector3.left);
            resCube.AddPlane((Vector3.back * halfDims.z) + (-originOffset), forwardDimentions, Vector3.back);
            resCube.AddPlane((Vector3.up * halfDims.y) + (-originOffset), upDimentions, Vector3.up);
            resCube.AddPlane((Vector3.right * halfDims.x) + (-originOffset), rightDimentions, Vector3.right);
            resCube.AddPlane((Vector3.forward * halfDims.z) + (-originOffset), forwardDimentions, Vector3.forward);

            resCube.RecalculateNormals();
            resCube.RecalculateBounds();
            resCube.RecalculateTangents();
            return resCube;
        }

        [System.Obsolete]
        static Mesh BuildCube_Verbose(Vector3 dimentions, Vector3 originOffset)
        {
            Vector3 halfDims = dimentions / 2f;
            Vector3[] vertices = new Vector3[]
            {
                /*0*/ new Vector3(-halfDims.x, -halfDims.y, -halfDims.z),
                /*1*/ new Vector3(-halfDims.x, -halfDims.y, halfDims.z),
                /*2*/ new Vector3(-halfDims.x, halfDims.y, -halfDims.z),
                /*3*/ new Vector3(-halfDims.x, halfDims.y, halfDims.z),
                /*4*/ new Vector3(halfDims.x, -halfDims.y, -halfDims.z),
                /*5*/ new Vector3(halfDims.x, -halfDims.y, halfDims.z),
                /*6*/ new Vector3(halfDims.x, halfDims.y, -halfDims.z),
                /*7*/ new Vector3(halfDims.x, halfDims.y, halfDims.z)
            };

            for (int i = 0; i < vertices.Length; i++)
                vertices[i] += -originOffset;

            int[] triangles = new int[]
            {
                0, 1, 2,    1, 3, 2, //Left face
                3, 7, 2,    2, 7, 6, //Top face
                6, 0, 2,    6, 4, 0, //Front face
                4, 6, 5,    5, 6, 7, //Right face
                7, 3, 5,    5, 3, 1, //Back face
                1, 0, 5,    0, 4, 5 //Bottom face
            };

            Mesh resCube = new Mesh();
            resCube.SetVertices(vertices);
            resCube.SetTriangles(triangles, 0);
            resCube.RecalculateNormals();
            resCube.RecalculateBounds();
            resCube.RecalculateTangents();
            return resCube;
        }

        public static Mesh BuildPlane(Vector3 vertexPosBL, Vector3 vertexPosTL, Vector3 vertexPosTR, Vector3 vertexPosBR)
        {
            Mesh mesh = new Mesh();
            mesh.AddPlane(vertexPosBL, vertexPosTL, vertexPosTR, vertexPosBR);
            return mesh;
        }

        public static Mesh BuildPlane(Vector3 center, Vector2 dimentions, Vector3 normal)
        {
            Mesh mesh = new Mesh();
            mesh.AddPlane(center, dimentions, normal);
            return mesh;
        }


        public static void AddPlane(this Mesh baseMesh, Vector3 center, Vector2 dimentions, Vector3 normal)
        {
            dimentions /= 2f;
            Vector3[] vertices = new Vector3[]
            {
                new Vector3(-dimentions.x, 0, -dimentions.y),
                new Vector3(-dimentions.x, 0, dimentions.y),
                new Vector3(dimentions.x, 0, dimentions.y),
                new Vector3(dimentions.x, 0, -dimentions.y),
            };

            Quaternion normalRot = Quaternion.FromToRotation(Vector3.up, normal);
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = center + (normalRot * vertices[i]);

            baseMesh.AddPlane(vertices[0], vertices[1], vertices[2], vertices[3]);
        }

        static List<Vector3> _s_AddPlane_Vertices = new List<Vector3>();
        static List<int> _s_AddPlane_Triangles = new List<int>();
        public static void AddPlane(this Mesh baseMesh, Vector3 vertexPosBL, Vector3 vertexPosTL, Vector3 vertexPosTR, Vector3 vertexPosBR)
        {
            baseMesh.GetVertices(_s_AddPlane_Vertices);
            baseMesh.GetTriangles(_s_AddPlane_Triangles, 0);

            int firstVertexIndex = _s_AddPlane_Vertices.Count;

            _s_AddPlane_Vertices.Add(vertexPosBL);
            _s_AddPlane_Vertices.Add(vertexPosTL);
            _s_AddPlane_Vertices.Add(vertexPosTR);
            _s_AddPlane_Vertices.Add(vertexPosBR);

            int[] triangles = new int[]
            {
                firstVertexIndex, firstVertexIndex + 1, firstVertexIndex + 2,
                firstVertexIndex, firstVertexIndex + 2, firstVertexIndex + 3
            };
            _s_AddPlane_Triangles.AddRange(triangles);

            baseMesh.SetVertices(_s_AddPlane_Vertices);
            baseMesh.SetTriangles(_s_AddPlane_Triangles, 0);

            _s_AddPlane_Vertices.Clear();
            _s_AddPlane_Triangles.Clear();
        }



        public static Mesh BuildTriangle(Vector3 vertexPos1, Vector3 vertexPos2, Vector3 vertexPos3)
        {
            Mesh mesh = new Mesh();
            mesh.AddTriangle(vertexPos1, vertexPos2, vertexPos3);
            return mesh;
        }

        static List<Vector3> _s_AddTriangle_Vertices = new List<Vector3>();
        static List<int> _s_AddTriangle_Triangle = new List<int>();
        public static void AddTriangle(this Mesh baseMesh, Vector3 vertexPos1, Vector3 vertexPos2, Vector3 vertexPos3)
        {
            baseMesh.GetVertices(_s_AddTriangle_Vertices);
            baseMesh.GetTriangles(_s_AddTriangle_Triangle, 0);

            int firstVertexIndex = _s_AddTriangle_Vertices.Count;

            _s_AddTriangle_Vertices.Add(vertexPos1);
            _s_AddTriangle_Vertices.Add(vertexPos2);
            _s_AddTriangle_Vertices.Add(vertexPos3);

            int[] triangles = new int[]
            {
                firstVertexIndex, firstVertexIndex + 1, firstVertexIndex + 2,
            };
            _s_AddTriangle_Triangle.AddRange(triangles);

            baseMesh.SetVertices(_s_AddTriangle_Vertices);
            baseMesh.SetTriangles(_s_AddTriangle_Triangle, 0);

            _s_AddTriangle_Vertices.Clear();
            _s_AddTriangle_Triangle.Clear();
        }
    }
}
