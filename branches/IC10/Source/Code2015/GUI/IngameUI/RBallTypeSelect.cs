﻿using System;
using System.Collections.Generic;
using System.Text;
using Code2015.World;
using Apoc3D;
using Apoc3D.Graphics;
using Code2015.Logic;
using Code2015.EngineEx;
using Apoc3D.MathLib;
using Apoc3D.Vfs;
using Apoc3D.GUI.Controls;


namespace Code2015.GUI.IngameUI
{
    class RBallTypeSelect : UIComponent
    {
        static readonly Vector2[] ItemTL = { new Vector2(16, 16), new Vector2(20, 89), new Vector2(20, 150) };
        static readonly Vector2[] ItemBR = { new Vector2(68, 72), new Vector2(72, 125), new Vector2(72, 196) };

        enum State
        {
            Close,
            Opened,
            Opening,
            Closing
        }

        GameScene scene;
        GameState gameLogic;
        RenderSystem renderSys;
        Code2015 game;
        Game parent;
        Player player;

        Texture background;

        Texture greenBallBtn;
        Texture healthBallBtn;
        Texture educationBallBtn;

        State state;

        float animProgress;


        Vector2[] transformedItemTL = new Vector2[3];
        Vector2[] transformedItemBR = new Vector2[3];


        struct BallRecord
        {
            public RBallType Type;
            public int count;
        }
        private static int BallRecordCompare(BallRecord a, BallRecord b)
        {
            return b.count.CompareTo(a.count);
        }

        /// <summary>
        /// 0 代表Green
        /// 1 代表Education
        /// 2 代表Health
        /// </summary>
        BallRecord[] resBallsCount = new BallRecord[3];


        /// <summary>
        /// 0 代表第1个框
        /// 1 代表第2个框
        /// 2 代表第3个框
        /// </summary>
        RBallType[] resBallsItemType = new RBallType[3];
        int[] resBallItemCount = new int[3];

        RBallType selectedBallType;

        City targetCity;
        City sourceCity;

        bool isCancelled;

        public bool IsCancelled
        {
            get { return isCancelled; }
        }
        public City SourceCity
        {
            get { return sourceCity; }
        }
        public City TargetCity
        {
            get { return targetCity; }
        }
        public RBallType SelectedBallType
        {
            get { return selectedBallType; }
        }
        public override int Order
        {
            get
            {
                return 80;
            }
        }


        void ChangeState(State ns)
        {
            state = ns;
        }

        public RBallTypeSelect(Code2015 game, Game parent, GameScene scene, GameState logic)
        {
            this.game = game;
            this.parent = parent;
            this.scene = scene;
            this.gameLogic = logic;
            this.player = parent.HumanPlayer;
            this.renderSys = game.RenderSystem;

            FileLocation fl = FileSystem.Instance.Locate("nig_icon_green.tex", GameFileLocs.GUI);
            greenBallBtn = UITextureManager.Instance.CreateInstance(fl);


            fl = FileSystem.Instance.Locate("nig_icon_edu.tex", GameFileLocs.GUI);
            educationBallBtn = UITextureManager.Instance.CreateInstance(fl);


            fl = FileSystem.Instance.Locate("nig_icon_hospital.tex", GameFileLocs.GUI);
            healthBallBtn = UITextureManager.Instance.CreateInstance(fl);



            fl = FileSystem.Instance.Locate("nig_send_ball.tex", GameFileLocs.GUI);
            background = UITextureManager.Instance.CreateInstance(fl);


            resBallsCount[0].Type = RBallType.Green;
            resBallsCount[0].count = 0;

            resBallsCount[1].Type = RBallType.Education;
            resBallsCount[1].count = 0;

            resBallsCount[2].Type = RBallType.Health;
            resBallsCount[2].count = 0;

            startY[0] = 50;
            startY[1] = 111;
            startY[2] = 170;
        }


        public void Open(City souceCity, City targetCity)
        {
            isCancelled = true;
            this.sourceCity = souceCity;
            this.targetCity = targetCity;

            resBallsCount[0].count = 0;
            resBallsCount[1].count = 0;
            resBallsCount[2].count = 0;

            ChangeState(State.Opening);
        }
        public void Close()
        {
            ChangeState(State.Closing);
        }
        public bool IsOpened
        {
            get { return state == State.Opened || state == State.Opening; }
        }

        public override bool HitTest(int x, int y)
        {
            if (state == State.Opened)
            {
                return true;
            }
            return false;
        }


        public override void Render(Sprite sprite)
        {

            RenderSendBall(sprite);


        }



        public override void Update(Apoc3D.GameTime time)
        {

            if (state == State.Opened)
            {
                StatisticRBall();
            }
            if (state == State.Opening)
            {
                animProgress += time.ElapsedGameTimeSeconds * 4;
                if (animProgress > 1)
                {
                    animProgress = 1;
                    ChangeState(State.Opened);
                }
            }
            else if (state == State.Closing)
            {
                animProgress -= time.ElapsedGameTimeSeconds * 4;
                if (animProgress < 0)
                {
                    animProgress = 0;
                    ChangeState(State.Close);
                }
            }
        }

        public override void UpdateInteract(Apoc3D.GameTime time)
        {
            if (state == State.Opened)
            {
                if (MouseInput.IsMouseUpLeft)
                {
                    int selectedIndex = -1;
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 size = transformedItemBR[i] - transformedItemTL[i];

                        Rectangle rect = new Rectangle((int)transformedItemTL[i].X, (int)transformedItemTL[i].Y, (int)size.X + 1, (int)size.Y + 1);

                        if (Control.IsInBounds(MouseInput.X, MouseInput.Y, ref rect))
                        {
                            selectedIndex = i;
                        }
                    }

                    if (selectedIndex != -1)
                    {
                        if (resBallItemCount[selectedIndex] != 0)
                        {
                            selectedBallType = resBallsItemType[selectedIndex];
                        }

                        isCancelled = false;
                    }

                    Close();
                }
            }

        }

        private void StatisticRBall()
        {
            if (targetCity != null)
            {
                resBallsCount[0].count = 0;
                resBallsCount[1].count = 0;
                resBallsCount[2].count = 0;

                resBallItemCount[0] = 0;
                resBallItemCount[1] = 0;
                resBallItemCount[2] = 0;

                for (int i = 0; i < sourceCity.NearbyOwnedBallCount; i++)
                {
                    if (sourceCity.GetNearbyOwnedBall(i).Type == RBallType.Green)
                        resBallsCount[0].count++;


                    if (sourceCity.GetNearbyOwnedBall(i).Type == RBallType.Education)
                        resBallsCount[1].count++;

                    if (sourceCity.GetNearbyOwnedBall(i).Type == RBallType.Health)
                        resBallsCount[2].count++;
                }
            }


        }

        void UpdateHotarea(ref Matrix trans)
        {
            for (int i = 0; i < 3; i++)
            {
                transformedItemTL[i] = Vector2.TransformSimple(ItemTL[i], ref trans);
                transformedItemBR[i] = Vector2.TransformSimple(ItemBR[i], ref trans);
            }
        }

        private void RenderSendBall(Sprite sprite)
        {
            if (targetCity != null)
            {
                float radLng = MathEx.Degree2Radian(targetCity.Longitude);
                float radLat = MathEx.Degree2Radian(targetCity.Latitude);

                float latSign = Math.Sign(radLat);

                Vector3 tangy = PlanetEarth.GetTangentY(radLng, radLat);
                Vector3 tangx = PlanetEarth.GetTangentX(radLng, radLat);


                Vector3 cityNormal = PlanetEarth.GetNormal(radLng, radLat);
                cityNormal.Normalize();

                Vector3 hpPos = targetCity.Position + cityNormal * 100 + tangx * (latSign * City.CityRadius * 0.55f);

                Viewport vp = renderSys.Viewport;
                Vector3 screenPos = vp.Project(hpPos, scene.Camera.ProjectionMatrix,
                                               scene.Camera.ViewMatrix, Matrix.Identity);

                Vector3 lp = vp.Project(hpPos + tangx, scene.Camera.ProjectionMatrix,
                                                       scene.Camera.ViewMatrix, Matrix.Identity);

                Vector3 rp = vp.Project(hpPos - tangx, scene.Camera.ProjectionMatrix,
                                                       scene.Camera.ViewMatrix, Matrix.Identity);


                float scale0 = MathEx.Saturate(1.5f * Vector3.Distance(lp, rp));
                float scale = (animProgress * animProgress) * scale0;

                Matrix trans = Matrix.Translation(0, -background.Height / 2, 0) *
                           Matrix.Scaling(scale, scale, 1) * Matrix.Translation(screenPos.X, screenPos.Y, 0);




                sprite.SetTransform(trans);

                sprite.Draw(background, 0, 0, ColorValue.White);

                UpdateHotarea(ref trans);

                if (state == State.Opened)
                {
                    Matrix trans2 = Matrix.Translation(-background.Width / 2, -background.Height / 2, 0) *
                        Matrix.Scaling(scale0, scale0, 1) * Matrix.Translation(screenPos.X, screenPos.Y, 0);

                    sprite.SetTransform(trans2);

                    RenderBallIcon(sprite);
                }

                sprite.SetTransform(Matrix.Identity);

            }
        }




        int[] startY = new int[3];

        private void RenderBallIcon(Sprite sprite)
        {
            if (targetCity != null)
            {
                //Array.Sort(resBallsCount, BallRecordCompare);

                int left = 0;
                for (int i = 0; i < resBallsCount.Length; i++)
                {
                    switch (resBallsCount[i].Type)
                    {
                        case RBallType.Green:
                            {
                                if (resBallsCount[i].count != 0)
                                {
                                    resBallsItemType[left] = RBallType.Green;
                                    resBallItemCount[left] = resBallsCount[i].count;
                                    int x = 82 - greenBallBtn.Width / 2;
                                    int y = startY[left] - greenBallBtn.Height / 2;

                                    sprite.Draw(greenBallBtn, x, y, ColorValue.White);
                                    left++;
                                }

                            }
                            break;
                        case RBallType.Education:
                            {
                                if (resBallsCount[i].count != 0)
                                {
                                    resBallsItemType[left] = RBallType.Education;
                                    resBallItemCount[left] = resBallsCount[i].count;
                                    int x = 82 - educationBallBtn.Width / 2;
                                    int y = startY[left] - educationBallBtn.Height / 2;
                                    sprite.Draw(educationBallBtn, x, y, ColorValue.White);
                                    left++;
                                }

                            }
                            break;
                        case RBallType.Health:
                            {
                                if (resBallsCount[i].count != 0)
                                {
                                    resBallsItemType[left] = RBallType.Health;
                                    resBallItemCount[left] = resBallsCount[i].count;
                                    int x = 82 - healthBallBtn.Width / 2;
                                    int y = startY[left] - healthBallBtn.Height / 2;
                                    sprite.Draw(healthBallBtn, x, y, ColorValue.White);
                                    left++;
                                }
                            }
                            break;
                    }
                }
            }
        }

    }



}