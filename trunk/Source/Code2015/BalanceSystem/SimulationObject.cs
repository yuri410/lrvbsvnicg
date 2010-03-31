﻿using System;
using System.Collections.Generic;
using System.Text;
using Apoc3D;
using Apoc3D.Collections;
using Apoc3D.Config;

namespace Code2015.BalanceSystem
{
    public class SimulationObject :IConfigurable, IUpdatable
    {
        /// <summary>
        /// 经度
        /// </summary>
        public float Longitude
        {
            get;
            protected set;
        }
        /// <summary>
        /// 纬度
        /// </summary>
        public float Latitude
        {
            get;
            protected set;
        }

        /// <summary>
        ///  碳元素的排放速度
        /// </summary>
        public float CarbonProduceSpeed
        {
            get;
            protected set;
        }

        public SimulationWorld Region
        {
            get;
            private set;
        }

        public SimulationObject(SimulationWorld region)
        {
            Region = region;
        }
       
        /// <summary>
        ///  派生类先更新
        /// </summary>
        /// <param name="time"></param>
        public virtual void Update(GameTime time)
        {

        }

        #region IConfigurable 成员

    
        public virtual void Parse(ConfigurationSection sect)
        {
            Longitude = sect.GetSingle("Longitude");
            Latitude = sect.GetSingle("Latitude");

            
        }
        #endregion
    }
}
