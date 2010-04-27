﻿using System;
using System.Collections.Generic;
using System.Text;
using Apoc3D;
using Apoc3D.Graphics;
using Apoc3D.MathLib;

namespace Code2015.GUI
{
    class Popup : UIComponent
    {
        const float YShift = 100;
        int x;
        int y;


        float time;
        float duration;
        Texture texture;

        public Popup(RenderSystem rs, Texture tex, int x, int y, float duration)
        {
            this.x = x;
            this.y = y;
            this.texture = tex;
            this.duration = duration;
        }

        public bool IsFinished
        {
            get { return time > duration; }
        }


        public override void Render(Sprite sprite)
        {
            float alpha = 1 - MathEx.Saturate(time / duration);
            int ny = (int)(y - (1 - alpha) * YShift);

            ColorValue modClr = new ColorValue(1, 1, 1, alpha);
            ColorValue modClr2 = new ColorValue(0, 0, 0, alpha);
            sprite.Draw(texture, x, ny, ColorValue.White);
            //font.DrawString(sprite, text, x + 1, ny + 1, 15, DrawTextFormat.Center | DrawTextFormat.VerticalCenter, (int)modClr2.PackedValue);
            //font.DrawString(sprite, text, x, ny, 15, DrawTextFormat.Center | DrawTextFormat.VerticalCenter, (int)modClr.PackedValue);
        }
        public override void Update(GameTime time)
        {
            this.time += time.ElapsedGameTimeSeconds;
        }

    }
}
