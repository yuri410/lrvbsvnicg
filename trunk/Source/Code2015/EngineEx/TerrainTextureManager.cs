﻿/*
-----------------------------------------------------------------------------
This source file is part of Zonelink

Copyright (c) 2009+ Tao Games

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  if not, write to the Free Software Foundation, 
Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA, or go to
http://www.gnu.org/copyleft/gpl.txt.

-----------------------------------------------------------------------------
*/
using System;
using System.Collections.Generic;
using System.Text;
using Apoc3D.Core;
using Apoc3D.Graphics;
using Apoc3D.Vfs;

namespace Code2015.EngineEx
{
    class TerrainTextureManager : ResourceManager
    {
        static volatile TerrainTextureManager singleton;
        static volatile object syncHelper = new object();

        public static TerrainTextureManager Instance
        {
            get
            {
                if (singleton == null)
                {
                    lock (syncHelper)
                    {
                        if (singleton == null)
                        {
                            singleton = new TerrainTextureManager(1048576 * 20);
                        }
                    }
                }
                return singleton;
            }
        }


        private TerrainTextureManager(int cacheSize)
            : base(cacheSize)
        {
        }



        public ResourceHandle<TerrainTexture> CreateInstance(RenderSystem rs, FileLocation rl)
        {
            Resource retrived = base.Exists(rl.Name);
            if (retrived == null)
            {
                TerrainTexture mdl = new TerrainTexture(rs, rl);
                retrived = mdl;
                base.NotifyResourceNew(mdl);
            }
            
            //else
            //{
            //    retrived.Use();
            //}
            return new ResourceHandle<TerrainTexture>((TerrainTexture)retrived);
        }
    }
}
