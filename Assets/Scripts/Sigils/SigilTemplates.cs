using System.Collections.Generic;
using UnityEngine;

// THE BUILT-IN SIGILS
//
// $1 needs at least one example ("template") of every shape it should know. Normally you would
// record those by drawing them. For the lab we generate them from geometry instead, so the
// recogniser works the moment you press Play and everyone gets the same templates.
//
// Two things to notice:
//
//   * A template can be as sparse as its CORNERS. Step 1 of the recogniser (resample) walks along
//     the straight lines between them and drops 64 evenly spaced points, so "Triangle" is
//     literally four Vector2s here.
//
//   * Every shape is added twice: as given, and reversed. $1 compares point i with point i, so a
//     clockwise circle and an anticlockwise circle look different to it. Storing both under the
//     same name makes direction not matter. (Where the stroke STARTS still matters; see
//     SigilRecognizer.RecognizeAnyStartOrDirection for how the image test deals with that.)
//
// Coordinates are in a unit-ish box, x right, y up. Size does not matter (step 3 rescales).

namespace Sigils
{
    public static class SigilTemplates
    {
        public static void AddDefaults(DollarOneRecognizer r)
        {
            AddBothDirections(r, "Circle",   Circle(32));

            AddBothDirections(r, "Triangle", new Vector2(0f, 1f), new Vector2(1f, -1f),
                                             new Vector2(-1f, -1f), new Vector2(0f, 1f));

            AddBothDirections(r, "Square",   new Vector2(-1f, 1f), new Vector2(1f, 1f),
                                             new Vector2(1f, -1f), new Vector2(-1f, -1f),
                                             new Vector2(-1f, 1f));

            AddBothDirections(r, "Line",     new Vector2(-1f, 0f), new Vector2(1f, 0f));

            AddBothDirections(r, "V",        new Vector2(-1f, 1f), new Vector2(0f, -1f),
                                             new Vector2(1f, 1f));

            AddBothDirections(r, "Zigzag",   new Vector2(-1f, 1f), new Vector2(-0.33f, -1f),
                                             new Vector2(0.33f, 1f), new Vector2(1f, -1f));

            AddBothDirections(r, "Star",     Star());
        }

        /// Add the stroke as drawn, then again backwards, under the same name.
        public static void AddBothDirections(DollarOneRecognizer r, string name, params Vector2[] pts)
        {
            r.AddTemplate(name, pts);
            var reversed = new List<Vector2>(pts);
            reversed.Reverse();
            r.AddTemplate(name, reversed);
        }

        /// A circle starting at the top (12 o'clock) and going clockwise.
        static Vector2[] Circle(int segments)
        {
            var pts = new Vector2[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float a = Mathf.PI / 2f - i * (2f * Mathf.PI / segments);   // start at top, go clockwise
                pts[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            }
            return pts;
        }

        /// A five-point star drawn in one stroke: top point, then every second point around.
        static Vector2[] Star()
        {
            var pts = new Vector2[6];
            for (int i = 0; i <= 5; i++)
            {
                float a = Mathf.PI / 2f - i * (4f * Mathf.PI / 5f);           // jump 144 degrees each time
                pts[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            }
            return pts;
        }
    }
}
