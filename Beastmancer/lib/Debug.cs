﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RDR2;
using RDR2.Native;
using RDR2.UI;
using RDR2.Math;

namespace Beastmancer
{
    class Debug : Script
    {
        private static TextElement debug_one = new TextElement("", new System.Drawing.PointF(300f, 300f), 0.3f);
        private static TextElement debug_two = new TextElement("", new System.Drawing.PointF(300f, 400f), 0.3f);
        private static TextElement debug_three = new TextElement("", new System.Drawing.PointF(300f, 500f), 0.3f);
        private static int debug_one_length = 0;
        private static int debug_two_length = 0;
        private static int debug_three_length = 0;

        public Debug()
        {
            Tick += OnTick;
            Interval = 1;
        }
        private void OnTick(object sender, EventArgs evt)
        {
            debug_one.Draw();
            debug_two.Draw();
            debug_three.Draw();
            Clear();
        }

        public static void Subtitle(string text)
        {
            RDR2.UI.Screen.ShowSubtitle(text);
        }

        public static void DebugOne(string text)
        {
            debug_one_length = 0;
            SetDebugString(debug_one, text);
        }
        public static void DebugTwo(string text)
        {
            debug_two_length = 0;
            SetDebugString(debug_two, text);
        }
        public static void DebugThree(string text)
        {
            debug_three_length = 0;
            SetDebugString(debug_three, text);
        }

        private static void SetDebugString(TextElement te, string text)
        {
            te.Caption = text;
        }

        private static void Clear()
        {
            debug_one_length += 1;
            debug_two_length += 1;
            debug_three_length += 1;

            if (debug_one_length > 180)
            {
                SetDebugString(debug_one, "");
            }
            if (debug_two_length > 180)
            {
                SetDebugString(debug_two, "");
            }
            if (debug_three_length > 180)
            {
                SetDebugString(debug_three, "");
            }
        }
    }
}
