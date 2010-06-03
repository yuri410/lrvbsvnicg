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
using Apoc3D.Graphics;
using Code2015.GUI;

namespace Apoc3D.GUI.Controls
{
    public class ImageControl : Control
    {
        Texture image;


        public ImageControl()
        {
        }

        public Texture Image
        {
            get { return image; }
            set { image = value; }
        }

        public override void Update(GameTime dt)
        {                     
            base.Update(dt);
        }

        public override void Render(Sprite sprite)
        {
            //Sprite.Transform = Matrix.Translation(X, Y, 0);
            base.Render(sprite);

            if (image != null)
            {
                sprite.Draw(image, X, Y, modColor);
            }
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                //UITextureManager.Instance.DestoryInstance(image);

            }

            image = null;
        }

    }
}
