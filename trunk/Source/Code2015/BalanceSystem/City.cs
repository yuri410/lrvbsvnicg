﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apoc3D;
using Apoc3D.Collections;

namespace Code2015.BalanceSystem
{


    /// <summary>
    ///  表示城市的大小
    /// </summary>
    public enum UrbanSize
    {
        Town,
        Normal,
        Large
    }

    /// <summary>
    ///  表示一座城市
    /// </summary>
    public class City : SimulateObject, IUpdatable
    {
        [SLGValueAttribute()]
        const int TownPluginCount = 1;
        [SLGValueAttribute()]
        const int NormalPluginCount = 3;
        [SLGValueAttribute()]
        const int LargePluginCount = 4;

        public City()
        {
            UpdateCity();
        }
        public City(UrbanSize size)
        {
            this.Size = size;
            UpdateCity();
        }
        public City(string name)
        {
            this.Name = name;
            UpdateCity();
        }

        /// <summary>
        ///  表示城市的附加设施
        /// </summary>
        FastList<CityPlugin> plugins = new FastList<CityPlugin>();



        #region  属性
        public string Name
        {
            get;
            set;
        }
        public float Development
        {
            get;
            set;
        }
        public float Population
        {
            get;
            set;
        }

        public float Disease
        {
            get;
            set;
        }



        protected float HPChange
        {
            get;
            set;
        }
        public float SelfHPProductionSpeed
        {
            get;
            protected set;
        }
        public float GetHPChange()
        {
            float r = HPChange;
            HPChange = 0;
            return r;
        }

        protected float LPChange
        {
            get;
            set;
        }
        public float SelfLPProductionSpeed
        {
            get;
            protected set;
        }
        public float GetLPChange()
        {
            float r = LPChange;
            LPChange = 0;
            return r;
        }

        protected float FoodChange
        {
            get;
            set;
        }
        public float SelfFoodCostSpeed
        {
            get;
            protected set;
        }
        public float GetFoodChange()
        {
            float r = FoodChange;
            FoodChange = 0;
            return r;
        }


        /// <summary>
        ///  
        /// </summary>
        public float PluginHPProductionSpeed
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public float PluginLPProductionSpeed
        {
            get;
            set;
        }
        /// <summary>
        ///  
        /// </summary>
        public float PluginFoodCostSpeed
        {
            get;
            set;
        }



        /// <summary>
        ///  城市自身的消耗高能和低能的速度，为负值，表示消耗
        /// </summary>
        public float ProduceHPSpeed
        {
            get { return PluginHPProductionSpeed + SelfHPProductionSpeed; }
        }
        /// <summary>
        /// 
        /// </summary>
        public float ProduceLPSpeed
        {
            get { return PluginLPProductionSpeed + SelfLPProductionSpeed; }
        }
        /// <summary>
        ///  
        /// </summary>
        public float FoodCostSpeed
        {
            get { return SelfFoodCostSpeed + PluginFoodCostSpeed; }
        }
        #endregion

        public UrbanSize Size
        {
            get;
            private set;
        }

        /// <summary>
        ///  获取目前城市最多可以添加的附加设施数量
        /// </summary>
        public int MaxPlugins
        {
            get
            {
                switch (Size)
                {
                    case UrbanSize.Town:
                        return TownPluginCount;
                    case UrbanSize.Normal:
                        return NormalPluginCount;
                    case UrbanSize.Large:
                        return LargePluginCount;
                }
                return 0;
            }
        }

        public void Add(CityPlugin plugin)
        {
            plugins.Add(plugin);
            UpdateCity();
            plugin.NotifyAdded(this);
        }

        public void Remove(CityPlugin plugin)
        {
            plugins.Remove(plugin);
            UpdateCity();
            plugin.NotifyRemoved(this);
        }

        /// <summary>
        ///  更新城市的属性设置
        /// </summary>
        public void UpdateCity()
        {

            switch (Size)
            {
                case UrbanSize.Large:

                    this.SelfHPProductionSpeed = -100;
                    this.SelfLPProductionSpeed = -100;
                    this.Population = 100000;
                    this.SelfFoodCostSpeed = 50;
                    this.CarbonProduceSpeed = 500;
                    break;
                case UrbanSize.Normal:
                    this.SelfHPProductionSpeed = -80;
                    this.SelfLPProductionSpeed = -80;
                    this.Population = 50000;
                    this.SelfFoodCostSpeed = 30;
                    this.CarbonProduceSpeed = 300;
                    break;
                case UrbanSize.Town:
                    this.SelfHPProductionSpeed = -50;
                    this.SelfLPProductionSpeed = -50;
                    this.Population = 20000;
                    this.SelfFoodCostSpeed = 10;
                    this.CarbonProduceSpeed = 100;
                    break;
            }

            PluginFoodCostSpeed = 0;
            PluginHPProductionSpeed = 0;
            PluginLPProductionSpeed = 0;

            for (int i = 0; i < plugins.Count; i++)
            {
                CarbonProduceSpeed += plugins[i].CarbonProduceSpeed;
                PluginFoodCostSpeed += plugins[i].FoodCostSpeed;
                PluginHPProductionSpeed += plugins[i].HPProductionSpeed;
                PluginLPProductionSpeed += plugins[i].LPProductionSpeed;
            }
        }


        public int PluginCount
        {
            get { return plugins.Count; }
        }

        public CityPlugin this[int i]
        {
            get { return plugins[i]; }
        }


        public override void Update(GameTime time)
        {
            float hours = (float)time.ElapsedGameTime.TotalHours;


            this.HPChange += ProduceHPSpeed * hours;
            this.LPChange += ProduceLPSpeed * hours;

            this.CarbonChange += CarbonProduceSpeed * hours;
            this.FoodChange += FoodCostSpeed * hours;



            // 计算发展度




        }
    }
}
