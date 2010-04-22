﻿using System;
using System.Collections.Generic;
using System.Text;
using Apoc3D;
using Apoc3D.Collections;
using Apoc3D.Graphics;
using Apoc3D.MathLib;
using Apoc3D.Vfs;
using Code2015.BalanceSystem;
using Code2015.EngineEx;
using Code2015.GUI;

namespace Code2015.World.Screen
{
 
    public enum MdgIconType
    {
        Piece,
        Ball
    }

    public static class MdgPhysicsParams
    {
        public const float PieceRadius = 32;
        public const float PieceMass = 1;
        public const float PieceElasity = 0.5f;
        public const float PieceFriction = 0.5f;
        public const float PieceAngularDamp = 0.5f;
        public const float PieceLinearDamp = 0.5f;

        public const float BallRadius = 32;
        public const float BallMass = 2.5f;
        public const float BallElasity = 0.5f;
        public const float BallFriction = 0.5f;
        public const float BallAngularDamp = 0.5f;
        public const float BallLinearDamp = 0.5f;

        public const float InactiveAlpha = 0.2f;
    }



    public interface IMdgSelection
    {
        Vector2 Position { get; set; }
        Vector2 Velocity { get; set; }
        MdgIconType IconType { get; }

    }

    /// <summary>
    ///  表示一个MDG拼图碎片
    /// </summary>
    public class MdgPiece : UIComponent, IMdgSelection
    {
        ScreenRigidBody body;


        MdgType type;

        Texture image;

        ScreenPhysicsWorld physicsWorld;
        MdgResourceManager manager;

        float growup;

        public MdgPiece(MdgResourceManager manager, ScreenPhysicsWorld world, MdgType type, Vector2 pos, float ori)
        {
            this.manager = manager;
            this.physicsWorld = world;

            this.body = new ScreenRigidBody();
            this.body.Orientation = ori;
            this.body.Position = pos;
            this.body.Radius = MdgPhysicsParams.PieceRadius;
            this.body.Mass = MdgPhysicsParams.PieceMass;
            this.body.Elasity = MdgPhysicsParams.PieceElasity;
            this.body.Friction = MdgPhysicsParams.PieceFriction;
            this.body.AngularDamp = MdgPhysicsParams.PieceAngularDamp;
            this.body.LinearDamp = MdgPhysicsParams.PieceLinearDamp;
            this.body.Tag = this;
         
                growup = 0;     
            world.Add(body);

            this.type = type;
            //this.Level = level;

            this.image = MdgResource.LoadImage(type, true);
        }

        public void NotifyRemoved()
        {
            physicsWorld.Remove(body);
        }


        public MdgType Type
        {
            get { return type; }
        }

        public override bool HitTest(int x, int y)
        {
            Vector2 pos = body.Position;

            float d = (float)Math.Sqrt(MathEx.Sqr(x - pos.X) + MathEx.Sqr(y - pos.Y));

            return d <= body.Radius;
        }
        public object Merge(MdgPiece other)
        {
            if (CheckMerge(other))
            {
                //int nextLevel = 0;
                //if (Level == 0 && other.Level == 0)
                //    nextLevel = 2;
                //if (Level == 1 || other.Level == 1)
                //    nextLevel = 2;

                //if (nextLevel == 2)
                //{
                MdgResource res = new MdgResource(manager, physicsWorld, type, body.Position, body.Orientation);

                manager.Remove(this);
                manager.Remove(other);
                manager.Add(res);
                return res;
                //}

                //MdgPiece piece = new MdgPiece(manager, physicsWorld, type, nextLevel, body.Position, body.Orientation);
                //manager.Remove(this);
                //manager.Remove(other);
                //manager.Add(piece);
                //return piece;
            }
            return null;
        }

        public bool CheckMerge(MdgPiece other)
        {
            if (CityGoalSite.CompareCategory(other.type, type))
            {
                return true;
            }
            return false;
        }

        public override void Render(Sprite sprite)
        {
            growup += 0.01f;
            if (growup > 1) growup = 1;

            if (image != null)
            {
                const float scaler = 0.21333f;

                Vector2 rectr = new Vector2(image.Width * scaler * growup, image.Height * scaler * growup);
                body.Radius = rectr.Length() * 0.5f;

                Vector2 pos = body.Position;
                float r = body.Radius;

                sprite.SetTransform(
                    Matrix.Scaling(scaler * growup, scaler * growup, 1) *
                    Matrix.Translation(-rectr.X * 0.5f, -rectr.Y * 0.5f, 0) * Matrix.RotationZ(-body.Orientation) * Matrix.Translation(pos.X, pos.Y, 0));


                sprite.Draw(image, 0, 0, ColorValue.White);

            }

        }

        public override void Update(GameTime time)
        {
            Rectangle rect = physicsWorld.WorldBounds;

            Vector2 dragCentre = new Vector2(rect.Width * 0.5f, rect.Height * 0.5f);

            Vector2 r = (body.Position - dragCentre);
            float rr = r.LengthSquared();
            float r2 = 1 - (rr / (physicsWorld.BoundsRadius * physicsWorld.BoundsRadius));
            r.Normalize();


            if (r2 > float.Epsilon)
            {
                Vector2 f1 = r * (0.1f / r2);
                body.Force += f1;
            }
        }

        #region IMdgSelection 成员


        public MdgIconType IconType
        {
            get { return MdgIconType.Piece; }
        }

        public Vector2 Position
        {
            get { return body.Position; }
            set { body.Position = value; }
        }
        public Vector2 Velocity
        {
            get { return body.Velocity; }
            set { body.Velocity = value; }
        }
        #endregion
    }

    /// <summary>
    ///  表示一个拼好的MDG图标
    /// </summary>
    public class MdgResource : UIComponent, IMdgSelection
    {
        public static Texture LoadImage(MdgType type, bool isPiece)
        {
            string suffix = string.Empty;

            if (isPiece)
            {
                suffix = "half";
            }

            FileLocation fl = FileSystem.Instance.Locate("goal" + ((int)type + 1).ToString() + suffix + ".tex", GameFileLocs.GUI);
            return UITextureManager.Instance.CreateInstance(fl);
        }

        ScreenRigidBody body;

        Texture image;

        MdgType type;
        ScreenPhysicsWorld physicsWorld;
        MdgResourceManager manager;
        public MdgType Type
        {
            get { return type; }
        }

        public override bool HitTest(int x, int y)
        {
            Vector2 pos = body.Position;

            float d = (float)Math.Sqrt(MathEx.Sqr(x - pos.X) + MathEx.Sqr(y - pos.Y));

            return d <= MdgPhysicsParams.BallRadius;
        }



        public MdgResource(MdgResourceManager manager, ScreenPhysicsWorld world, MdgType type, Vector2 pos, float ori)
        {
            this.manager = manager;
            this.physicsWorld = world;

            this.body = new ScreenRigidBody();
            this.body.Orientation = ori;
            this.body.Position = pos;
            this.body.Radius = MdgPhysicsParams.BallRadius;
            this.body.Mass = MdgPhysicsParams.BallMass;
            this.body.Elasity = MdgPhysicsParams.BallElasity;
            this.body.Friction = MdgPhysicsParams.BallFriction;
            this.body.AngularDamp = MdgPhysicsParams.BallAngularDamp;
            this.body.LinearDamp = MdgPhysicsParams.BallLinearDamp;
            this.body.Tag = this;

            world.Add(body);

            this.type = type;
            this.image = LoadImage(type, false);
        }
        public void NotifyRemoved()
        {
            physicsWorld.Remove(body);
        }
        public override void Render(Sprite sprite)
        {
            if (image != null)
            {
                Vector2 pos = body.Position;
                float r = body.Radius;

                sprite.SetTransform(
                    Matrix.Scaling(2 * r / image.Width, 2 * r / image.Height, 1) *
                    Matrix.Translation(-r, -r, 0) * Matrix.RotationZ(-body.Orientation) * Matrix.Translation(pos.X, pos.Y, 0));

                //if (object.ReferenceEquals(manager.GetPrimaryResource(type), this))
                //{
                    sprite.Draw(image, 0, 0, ColorValue.White);
                //}
                //else
                //{
                    //ColorValue opa = new ColorValue(1, 1, 1, MdgPhysicsParams.InactiveAlpha);
                    //sprite.Draw(image, 0, 0, opa);
                //}
            }

        }

        public bool IsInBox 
        {
            get { return !body.CollisionEnabled; }
        }
        public override void Update(GameTime time)
        {
            Vector2 dragCenter = new Vector2();
            switch (type)
            {
                case MdgType.Diseases:
                case MdgType.MaternalHealth:
                case MdgType.ChildMortality:
                    dragCenter = new Vector2(675, 659);
                    break;
                case MdgType.Education:
                case MdgType.GenderEquality:
                    dragCenter = new Vector2(790, 659);
                    break;
                case MdgType.Environment:
                    dragCenter = new Vector2(905, 659);
                    break;
                case MdgType.Hunger:
                    dragCenter = new Vector2(1020, 659);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            Vector2 r = (body.Position - dragCenter);
            float rr = r.LengthSquared();
            float r2 = 1 - (rr / (physicsWorld.BoundsRadius * physicsWorld.BoundsRadius));
            r.Normalize();

            body.CollisionEnabled = rr > (9 * MdgPhysicsParams.BallRadius * MdgPhysicsParams.BallRadius);
            

            if (r2 > float.Epsilon)
            {
                Vector2 f1 = r * (1 / r2);
                body.Force -= f1;
            }

        }
        #region IMdgSelection 成员

        public MdgIconType IconType
        {
            get { return MdgIconType.Ball; }
        }

        public Vector2 Position
        {
            get { return body.Position; }
            set { body.Position = value; }
        }
        public Vector2 Velocity
        {
            get { return body.Velocity; }
            set { body.Velocity = value; }
        }
        #endregion
    }

    /// <summary>
    ///  对拼图游戏中的各种物体的管理器
    ///  记录这种物品的数量，并维护他们在物理引擎的“是否激活”状态
    /// </summary>
    public class MdgResourceManager
    {
        /// <summary>
        ///  第一个索引为MdgType，第二个为list index
        /// </summary>
        FastList<MdgPiece>[] pieces;
        /// <summary>
        ///  第一个索引为MdgType
        /// </summary>
        FastList<MdgResource>[] balls;

        //MdgResource[] primaryBall;
        //MdgPiece[] primaryPiece;

        public MdgResourceManager()
        {
            pieces = new FastList<MdgPiece>[(int)MdgType.Count];
            //primaryBall = new MdgResource[(int)MdgType.Count];
            //primaryPiece = new MdgPiece[(int)MdgType.Count];

            balls = new FastList<MdgResource>[(int)MdgType.Count];

            for (int i = 0; i < pieces.Length; i++)
            {
                pieces[i] = new FastList<MdgPiece>();
                
                balls[i] = new FastList<MdgResource>();
            }

        }

        public void Add(MdgPiece piece)
        {
            //if (pieces[(int)piece.Type][piece.BitMask].Count == 0)
            //{
            //    primaryPiece[(int)piece.Type][piece.BitMask] = piece;
            //}
            pieces[(int)piece.Type].Add(piece);
        }
        public void Add(MdgResource res)
        {
            //if (balls[(int)res.Type].Count == 0)
            //{
            //    primaryBall[(int)res.Type] = res;
            //}
            balls[(int)res.Type].Add(res);
        }
        public void Remove(MdgPiece piece)
        {
            pieces[(int)piece.Type].Remove(piece);
            piece.NotifyRemoved();

        }
        public void Remove(MdgResource res)
        {
            balls[(int)res.Type].Remove(res);
            res.NotifyRemoved();

            //if (object.ReferenceEquals(primaryBall[(int)res.Type], res))
            //{
            //    primaryBall[(int)res.Type] = null;
            //}
        }


        //public MdgResource GetPrimaryResource(MdgType type)
        //{
        //    return primaryBall[(int)type];
        //}

        public int GetPieceCount(MdgType type)
        {
            return pieces[(int)type].Count;
        }
        public MdgPiece GetPiece(MdgType type, int index)
        {
            return pieces[(int)type][index];
        }

        public int GetResourceCount(MdgType type)
        {
            return balls[(int)type].Count;
        }
        public MdgResource GetResource(MdgType type, int index)
        {
            return balls[(int)type][index];
        }

        public void Update(GameTime time)
        {
            for (int i = 0; i < balls.Length; i++)
            {
                for (int j = 0; j < balls[i].Count; j++)
                {
                    balls[i][j].Update(time);
                }
            }

            for (int i = 0; i < pieces.Length; i++)
            {
                for (int j = 0; j < pieces[i].Count; j++)
                {
                    pieces[i][j].Update(time);
                }
            }
        }
    }
}
