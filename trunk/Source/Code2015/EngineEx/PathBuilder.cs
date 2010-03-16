﻿using System;
using System.Collections.Generic;
using System.Text;
using Apoc3D.Graphics;
using Apoc3D.MathLib;
using Code2015.Logic;
using Code2015.World;

namespace Code2015.EngineEx
{
    struct PathVertex
    {
        public Vector3 Position;
        public Vector3 N;
        public Vector3 Right;
        public Vector2 Tex1;

        public static int Size 
        {
            get { return Vector3.SizeInBytes + Vector3.SizeInBytes + Vector3.SizeInBytes + Vector2.SizeInBytes; }
        }

        static PathVertex()
        {
            Elements = new VertexElement[4];
            Elements[0] = new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position);
            Elements[1] = new VertexElement(Elements[0].Size, VertexElementFormat.Vector3, VertexElementUsage.Normal);
            Elements[2] = new VertexElement(Elements[1].Size + Elements[1].Offset, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0);
            Elements[3] = new VertexElement(Elements[2].Size + Elements[2].Offset, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1);
        }


        public static VertexElement[] Elements
        {
            get;
            private set;
        }
    }

    unsafe static class PathBuilder
    {
        static Vector3[] positionBuffer = new Vector3[1000];

        static Vector3[] positionBuffer2 = new Vector3[1000];
        static Vector3[] nrmBuffer = new Vector3[1000];
        static Vector3[] rightBuffer = new Vector3[1000];


        public static ModelData BuildModel(RenderSystem rs, Map map, Point[] points)
        {
            MeshData surface = new MeshData(rs);

            int vertexLen = points.Length;
            int vertexCount = vertexLen * 4;

            // 计算世界坐标
            for (int i = 0; i < vertexLen; i++)
            {
                float lng;
                float lat;
                Map.GetCoord(points[i].X, points[i].Y, out lng, out lat);

                positionBuffer[i] = PlanetEarth.GetPosition(lng, lat);
            }

            // 插值
            for (int i = 1; i < vertexLen - 3; i++)
            {
                positionBuffer2[i * 2] =
                    MathEx.CatmullRom(positionBuffer[i - 1], positionBuffer[i], positionBuffer[i + 1], positionBuffer[i + 2], 0);
                positionBuffer2[i * 2 + 1] =
                    MathEx.CatmullRom(positionBuffer[i - 1], positionBuffer[i], positionBuffer[i + 1], positionBuffer[i + 2], 0.5f);
            }

            vertexLen *= 2;

            positionBuffer2[0] = positionBuffer[0];
            positionBuffer2[vertexLen - 1] = positionBuffer2[vertexLen - 1];

            int index = vertexLen - 2;
            if (index > 0)
                positionBuffer2[index] = MathEx.CatmullRom(positionBuffer2[index - 1],
                    positionBuffer2[index], positionBuffer2[index + 1], positionBuffer2[index + 1], 0.5f);


            // 计算切线空间向量
            for (int i = 0; i < vertexLen - 1; i++)
            {
                Vector3 dir = positionBuffer2[i + 1] - positionBuffer2[i];
                dir.Normalize();

                Vector3 up = positionBuffer2[i];
                up.Normalize();

                // Slop tangent matrix calculate


                nrmBuffer[i] = up;
                Vector3.Cross(ref dir, ref up, out rightBuffer[i]);
            }

            // 计算网格顶点
            PathVertex[] vertices = new PathVertex[vertexCount];
            for (int i = 0; i < vertexLen; i++)
            {


            }

            fixed (PathVertex* src = &vertices[0])
            {
                surface.SetData(src, PathVertex.Size * vertexCount);
            }
            surface.VertexCount = vertexCount;
            surface.VertexElements = PathVertex.Elements;
            surface.VertexSize = PathVertex.Size;

            MeshFace[] faces = new MeshFace[vertexCount - 2];
            for (int i = 0; i < faces.Length; i+=2) 
            {
                faces[i].MaterialIndex = 0;
                faces[i].IndexA = i;
                faces[i].IndexB = i + 1;
                faces[i].IndexC = i + 2;

                faces[i + 1].MaterialIndex = 0;
                faces[i + 1].IndexA = i + 1;
                faces[i + 1].IndexB = i + 2;
                faces[i + 1].IndexC = i + 3;
            }
            surface.Faces = faces;

            Mesh surfaceMesh = new Mesh(rs, surface);
            ModelData result = new ModelData(rs, new Mesh[] { surfaceMesh });
            return result;
        }
    }
}