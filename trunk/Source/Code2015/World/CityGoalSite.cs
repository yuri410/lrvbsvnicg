﻿using System;
using System.Collections.Generic;
using System.Text;
using Apoc3D.Collections;
using Apoc3D.Graphics;
using Apoc3D.MathLib;
using Apoc3D.Scene;
using Code2015.BalanceSystem;
using Code2015.World.Screen;

namespace Code2015.World
{
    public class CityGoalSite : IRenderable
    {
        public const int SiteCount = 4;

        struct GoalSite
        {
            public bool HasPiece;           
            public MdgType Type;

            public bool IsTyped;
            public MdgType Desired;
        }

        CityObject parent;
        CityStyle style;
        bool isRotating;
        float rotation;
        float actuallRotation;

        GoalSite[] sites = new GoalSite[SiteCount];

        FastList<RenderOperation> opBuffer = new FastList<RenderOperation>();

        public CityGoalSite(CityObject obj, CityStyle style)
        {
            this.parent = obj;
            this.style = style;
        }

        #region IRenderable 成员

        public RenderOperation[] GetRenderOperation()
        {
            opBuffer.FastClear();

            if (parent.IsCaptured)
            {
                for (int i = 0; i < SiteCount; i++)
                {
                    RenderOperation[] ops = null;

                    if (sites[i].IsTyped)
                    {
                        ops = style.MdgSiteEmpty[(int)sites[i].Desired].GetRenderOperation();
                    }
                    else
                    {
                        ops = style.MdgSiteInactive.GetRenderOperation();
                    }
                    if (ops != null)
                    {
                        for (int j = 0; j < ops.Length; j++)
                        {
                            ops[j].Transformation *= CityStyleTable.SiteTransform[i];
                        }
                        opBuffer.Add(ops);
                    }

                    if (sites[i].HasPiece)
                    {
                        ops = style.MdgSite[(int)sites[i].Type].GetRenderOperation();

                        if (ops != null)
                        {
                            for (int j = 0; j < ops.Length; j++)
                            {
                                ops[j].Transformation *= CityStyleTable.SiteTransform[i];
                            }
                            opBuffer.Add(ops);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < SiteCount; i++)
                {
                    RenderOperation[] ops = style.MdgSiteEmpty[(int)parent.MajorProblem].GetRenderOperation();
                    if (ops != null)
                    {
                        for (int j = 0; j < ops.Length; j++)
                        {
                            ops[j].Transformation *= CityStyleTable.SiteTransform[i];
                        }
                        opBuffer.Add(ops);
                    }
                }

            }
            opBuffer.TrimClear();
            return opBuffer.Elements;
        }

        public RenderOperation[] GetRenderOperation(int level)
        {
            return GetRenderOperation();
        }

        #endregion


        public void Clear()
        {
            for (int i = 0; i < SiteCount; i++)
            {
                sites[i].HasPiece = false;
            }
        }

        public unsafe void Rotate(int span)
        {
            GoalSite* newSites = stackalloc GoalSite[SiteCount];

            for (int i = 0; i < SiteCount; i++)
            {
                int n = (i + span) % SiteCount;

                newSites[n] = sites[i];
            }

            for (int i = 0; i < SiteCount; i++)
            {
                sites[i] = newSites[i];
            }
        }
        public void BeginRotate()
        {
            isRotating = true;
        }


        public void Rotating(float amount)
        {
            const float GlueThreshold = 0.1f;

            rotation = amount;

            if (parent.Size == UrbanSize.Large)
            {
                float s = Math.Sign(rotation);
                float rem = Math.Abs(rotation) % MathEx.PiOver4;

                if (rem < GlueThreshold)
                {
                    actuallRotation = rotation - s * rem;
                }
            }
            else
            {
                float s = Math.Sign(rotation);
                float rem = Math.Abs(rotation) % MathEx.PiOver2;

                if (rem < GlueThreshold)
                {
                    actuallRotation = rotation - s * rem;
                }
            }
        }
        public void EndRotate()
        {
            isRotating = false;

            if (parent.Size == UrbanSize.Large)
            {
                const float Pi8 = MathEx.PiOver4 * 0.5f;

                int sp = (int)(actuallRotation / Pi8);
                Rotate(sp);
            }
            else
            {
                const float Pi4 = MathEx.PiOver4;

                int sp = (int)(actuallRotation / Pi4);
                Rotate(sp);
            }
        }


        public void SetDesired(int i, MdgType type) 
        {
            sites[i].IsTyped = true;
            sites[i].Desired = type;
        }

        public bool HasPiece(int i)
        {
            return sites[i].HasPiece;
        }
        public MdgType GetPieceType(int i)
        {
            return sites[i].Type;
        }
        public static MdgType GetDesired(CityPluginTypeId pltype) 
        {
            switch (pltype)
            {
                case CityPluginTypeId.EducationOrg:
                    return MdgType.Education;
                case CityPluginTypeId.Hospital:
                    return MdgType.Diseases;
                case CityPluginTypeId.BiofuelFactory:
                case CityPluginTypeId.OilRefinary:
                case CityPluginTypeId.WoodFactory:
                    return MdgType.Environment;
            }
            return MdgType.Hunger;
        }
        public bool Match(int i, CityPluginTypeId cpltype)
        {
            if (!sites[i].HasPiece)
                return false;

            MdgType type = sites[i].Type;
            switch (cpltype)
            {
                case CityPluginTypeId.EducationOrg:
                    switch (type)
                    {
                        case MdgType.GenderEquality:
                        case MdgType.Education:
                            return true;
                    }
                    return false;
                case CityPluginTypeId.Hospital:
                    switch (type)
                    {
                        case MdgType.ChildMortality:
                        case MdgType.Diseases:
                            return true;
                    }
                    return false;
                case CityPluginTypeId.BiofuelFactory:
                case CityPluginTypeId.OilRefinary:
                case CityPluginTypeId.WoodFactory:
                    return type == MdgType.Environment;
            }
            return false;
        }
        public bool MatchPiece(int i, MdgType type)
        {
            if (sites[i].HasPiece)
                return false;

            //switch (sites[i].plugin.TypeId)
            //{
            //    case CityPluginTypeId.EducationOrg:
            //        switch (type)
            //        {
            //            case MdgType.GenderEquality:
            //            case MdgType.Education:
            //                return true;
            //        }
            //        return false;
            //    case CityPluginTypeId.Hospital:
            //        switch (type)
            //        {
            //            case MdgType.ChildMortality:
            //            case MdgType.Diseases:
            //                return true;
            //        }
            //        return false;
            //    case CityPluginTypeId.BiofuelFactory:
            //    case CityPluginTypeId.OilRefinary:
            //    case CityPluginTypeId.WoodFactory:
            //        return type == MdgType.Environment;
            //}
            return true;
        }
        public void SetPiece(int i, MdgType res)
        {
            sites[i].HasPiece = true;
            sites[i].Type = res;

            if (!parent.TryLink())
            {
                parent.TryUpgrade();
            }
        }
        public void ClearAt(int i) 
        {
            sites[i].HasPiece = false;
        }

        public MdgType? GetPiece(int i)
        {
            if (!sites[i].HasPiece)
                return null;
            return sites[i].Type;
        }
    }
}
