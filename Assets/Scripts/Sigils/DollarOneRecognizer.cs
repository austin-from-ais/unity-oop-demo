using System;
using System.Collections.Generic;
using UnityEngine;

// THE $1 UNISTROKE RECOGNIZER
//
// This file is the whole "brain" of the sigil system. Read it top to bottom; it is meant to be
// read, not just run. There is no machine learning in here, no training step, and no external
// library. It is about 150 lines of arithmetic that you could do by hand with graph paper.
//
// The idea, in one paragraph:
//
//     A drawn shape is just a list of 2D points in the order the pen visited them. If we clean
//     that list up so that WHERE it was drawn, HOW BIG it was drawn, and HOW FAST it was drawn
//     no longer matter, then two drawings of "a circle" end up looking almost identical as lists
//     of numbers. To recognise a new drawing, we clean it up the same way and ask: "which of my
//     stored example drawings (templates) is it closest to?" The closest one wins.
//
// It comes from a 2007 paper by Wobbrock, Wilson and Li, "Gestures without libraries, toolkits
// or training: a $1 recognizer for user interface prototypes". The "$1" is a joke: it costs
// about a dollar's worth of effort to implement. That is the point of it. If you can follow this
// file, you can build gesture recognition into anything.
//
// The pipeline for EVERY stroke, template or candidate, is the same four steps:
//
//     1. Resample     -> N evenly spaced points, so drawing speed stops mattering
//     2. Rotate       -> so the first point sits at "3 o'clock" from the centre (rotation)
//     3. Scale        -> to a fixed-size square, so drawing size stops mattering
//     4. Translate    -> so the centre sits at (0,0), so drawing position stops mattering
//
// After those four steps, comparing two strokes is just: for each i, how far is point i of one
// stroke from point i of the other? Average that. Small average = similar shape.
//
// Coordinate convention in this project: x to the right, y UP (Unity style). Where the points
// come from does not matter to this class. In this lab they come from tracing an image; in the
// VR game they will come from projecting the controller's path onto a plane in front of the player.

namespace Sigils
{
    public class DollarOneRecognizer
    {
        // ------------------------------------------------------------------ data types

        /// A stored example of a shape, already run through the four normalisation steps.
        /// You can store several templates with the same Name (e.g. a circle drawn clockwise and
        /// one drawn anticlockwise). Recognition reports the best-scoring one per name.
        public class Template
        {
            public string Name;
            public List<Vector2> Points;
        }

        /// One template's result against the candidate stroke.
        public struct Match
        {
            public string Name;
            public float Score;     // 1 = identical, 0 = as different as the maths allows
            public float Distance;  // the raw average point-to-point distance, in "square units"
        }

        /// Everything Recognize() found out. Name/Score are the winner; Matches is the whole
        /// leaderboard (best per template name, sorted best first) so you can see runners-up.
        public class Result
        {
            public string Name = "(nothing)";
            public float Score = 0f;
            public List<Match> Matches = new List<Match>();

            /// Did the winner clear the bar? A scribble will still have SOME nearest template, so
            /// always check this before casting a spell.
            public bool IsConfident(float threshold) => Score >= threshold;
        }

        // ------------------------------------------------------------------ settings

        /// How many points every stroke is resampled to. 64 is the paper's number. More is not
        /// better past ~64; the average distance just gets computed over more, similar, points.
        public int NumPoints = 64;

        /// The side length of the square every stroke is scaled into. Its actual value does not
        /// matter (distances scale with it, and Score divides it back out). 250 is the paper's.
        public float SquareSize = 250f;

        /// How far we are willing to rotate a candidate, either way, hunting for a better fit.
        /// Step 2 already lines strokes up roughly; this is the fine adjustment. 45 degrees means
        /// a "V" and a "^" are still different shapes, but a slightly tilted circle is fine.
        public float AngleRangeDegrees = 45f;

        /// When the rotation search has narrowed the best angle down to within this many
        /// degrees, stop. Smaller = a few more iterations for a slightly better answer.
        public float AnglePrecisionDegrees = 2f;

        /// The golden ratio conjugate, 0.618... Used by the rotation search in step 5. See below.
        static readonly float Phi = 0.5f * (-1f + Mathf.Sqrt(5f));

        /// Half the diagonal of the square. The worst plausible average distance, used to turn a
        /// raw distance into a 0..1 score.
        float HalfDiagonal => 0.5f * Mathf.Sqrt(2f * SquareSize * SquareSize);

        readonly List<Template> templates = new List<Template>();
        public IReadOnlyList<Template> Templates => templates;

        // ------------------------------------------------------------------ public API

        /// Store an example of a shape. rawPoints can be as sparse as the corners of a triangle
        /// or as dense as one point per pixel; step 1 (resample) evens that out.
        public void AddTemplate(string name, IList<Vector2> rawPoints)
        {
            templates.Add(new Template { Name = name, Points = Normalize(rawPoints) });
        }

        public void ClearTemplates() => templates.Clear();

        /// The main event. Give it a raw stroke, get back the closest template and a score.
        public Result Recognize(IList<Vector2> rawPoints)
        {
            var result = new Result();
            if (rawPoints == null || rawPoints.Count < 2 || PathLength(rawPoints) < 1e-4f)
                return result;                               // nothing drawn, nothing to say

            // Put the candidate through exactly the same four steps as every template. This is
            // the whole trick: "compare like with like".
            var candidate = Normalize(rawPoints);

            // Score every template. Keep the best per name (there may be several "Circle"s).
            var bestByName = new Dictionary<string, Match>();
            foreach (var t in templates)
            {
                // Step 5: how far apart are they, at the best small rotation we can find?
                float d = DistanceAtBestAngle(candidate, t.Points,
                                              -AngleRangeDegrees * Mathf.Deg2Rad,
                                               AngleRangeDegrees * Mathf.Deg2Rad,
                                               AnglePrecisionDegrees * Mathf.Deg2Rad);

                // Turn "average distance in square units" into "0 to 1, higher is better".
                // If every point were half a diagonal away the score would be 0.
                float score = 1f - d / HalfDiagonal;

                if (!bestByName.TryGetValue(t.Name, out var existing) || score > existing.Score)
                    bestByName[t.Name] = new Match { Name = t.Name, Score = score, Distance = d };
            }

            result.Matches.AddRange(bestByName.Values);
            result.Matches.Sort((a, b) => b.Score.CompareTo(a.Score));   // best first
            if (result.Matches.Count > 0)
            {
                result.Name  = result.Matches[0].Name;
                result.Score = result.Matches[0].Score;
            }
            return result;
        }

        // ------------------------------------------------------------------ normalisation

        /// Steps 1-4, in order. Every stroke that enters this class goes through here.
        public List<Vector2> Normalize(IList<Vector2> rawPoints)
        {
            var p = Resample(rawPoints, NumPoints);        // 1. speed no longer matters
            float angle = IndicativeAngle(p);              // 2. (find how it is tilted...)
            p = RotateBy(p, -angle);                       //    ...and un-tilt it)
            p = ScaleTo(p, SquareSize);                    // 3. size no longer matters
            p = TranslateTo(p, Vector2.zero);              // 4. position no longer matters
            return p;
        }

        // STEP 1 - RESAMPLE
        //
        // A VR controller reports its position ~90 times a second. Draw a circle slowly and you
        // get 400 points; draw it fast and you get 40. Draw it with a pause halfway and you get
        // a big clump of points in one spot. None of that is "shape", it is all "speed".
        //
        // So we walk along the stroke and drop a new point every (total length / (n-1)) units,
        // like putting fence posts at equal spacing along a winding path. Afterwards every stroke
        // has exactly n points and they are evenly spread along it, no matter how it was drawn.
        public static List<Vector2> Resample(IList<Vector2> points, int n)
        {
            float interval = PathLength(points) / (n - 1);   // desired gap between fence posts
            float accumulated = 0f;                           // distance walked since last post

            var src = new List<Vector2>(points);              // copy: we insert into it below
            var dst = new List<Vector2>(n) { src[0] };        // the first post is the first point

            for (int i = 1; i < src.Count; i++)
            {
                float d = Vector2.Distance(src[i - 1], src[i]);
                if (accumulated + d >= interval && d > 0f)
                {
                    // The next post lands somewhere ON this segment. Find exactly where by
                    // linear interpolation: t is how far along the segment (0..1) it falls.
                    float t = (interval - accumulated) / d;
                    var q = Vector2.LerpUnclamped(src[i - 1], src[i], t);
                    dst.Add(q);

                    // Insert q into the source so the NEXT walk starts from the post we just
                    // placed, not from the original point before it. Then reset the odometer.
                    src.Insert(i, q);
                    accumulated = 0f;
                }
                else
                {
                    accumulated += d;
                }
            }

            // Floating point rounding can leave us one short. If so, the last original point is
            // the missing post.
            while (dst.Count < n) dst.Add(src[src.Count - 1]);
            return dst;
        }

        // STEP 2 - ROTATE TO THE "INDICATIVE ANGLE"
        //
        // People do not draw a triangle at exactly the same tilt twice. We need SOME rule to line
        // strokes up, and $1's rule is simple: take the angle from the centre of the shape to its
        // FIRST point, and rotate the whole thing so that angle is zero (first point is due east
        // of the centre). Two circles that both start "at the top" now both start at 3 o'clock.
        //
        // Notice this depends on where the stroke STARTS. A circle started at the top and a
        // circle started at the bottom end up rotated 180 degrees apart. $1 accepts that and fixes
        // it with the small rotation search in step 5 plus, if you need it, extra templates.
        public static float IndicativeAngle(IList<Vector2> points)
        {
            var c = Centroid(points);
            return Mathf.Atan2(c.y - points[0].y, c.x - points[0].x);
        }

        /// Rotate every point about the centroid by the given angle (radians). Plain 2D rotation:
        ///     x' = (x - cx) cos(a) - (y - cy) sin(a) + cx
        ///     y' = (x - cx) sin(a) + (y - cy) cos(a) + cy
        public static List<Vector2> RotateBy(IList<Vector2> points, float radians)
        {
            var c = Centroid(points);
            float cos = Mathf.Cos(radians), sin = Mathf.Sin(radians);
            var outPts = new List<Vector2>(points.Count);
            foreach (var p in points)
            {
                float dx = p.x - c.x, dy = p.y - c.y;
                outPts.Add(new Vector2(dx * cos - dy * sin + c.x,
                                       dx * sin + dy * cos + c.y));
            }
            return outPts;
        }

        // STEP 3 - SCALE TO A SQUARE
        //
        // Stretch the stroke's bounding box to a fixed square, so a tiny circle and a huge circle
        // become the same circle. Note the stretch is NON-uniform: width and height are scaled
        // separately. That means a squashed oval also becomes a circle, which is usually what you
        // want from a shape recogniser ("that was meant to be a circle").
        //
        // One trap: a straight line has zero height, and you cannot stretch zero to 250. So if the
        // box is very thin in one direction we scale uniformly instead, using the long side. The
        // "very thin" cut-off (1:4) is borrowed from $1's big brother, the $N recognizer.
        public static List<Vector2> ScaleTo(IList<Vector2> points, float size)
        {
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;
            foreach (var p in points)
            {
                minX = Mathf.Min(minX, p.x); maxX = Mathf.Max(maxX, p.x);
                minY = Mathf.Min(minY, p.y); maxY = Mathf.Max(maxY, p.y);
            }
            float w = maxX - minX, h = maxY - minY;
            float longest = Mathf.Max(w, h);
            if (longest < 1e-6f) longest = 1f;                      // all points identical

            bool oneDimensional = Mathf.Min(w, h) / longest < 0.25f;
            float sx = oneDimensional ? size / longest : size / w;
            float sy = oneDimensional ? size / longest : size / h;

            var outPts = new List<Vector2>(points.Count);
            foreach (var p in points) outPts.Add(new Vector2(p.x * sx, p.y * sy));
            return outPts;
        }

        // STEP 4 - TRANSLATE TO THE ORIGIN
        //
        // Slide the whole stroke so its centroid (average of all points) sits at the target,
        // normally (0,0). Now a circle drawn top-left and one drawn bottom-right are identical.
        public static List<Vector2> TranslateTo(IList<Vector2> points, Vector2 target)
        {
            var c = Centroid(points);
            var outPts = new List<Vector2>(points.Count);
            foreach (var p in points) outPts.Add(p - c + target);
            return outPts;
        }

        // ------------------------------------------------------------------ comparison

        // STEP 5 - FIND THE BEST SMALL ROTATION (golden section search)
        //
        // Step 2 lined the strokes up roughly. Now we try rotating the candidate a little either
        // way (within +/- AngleRange) to see if the fit improves. We could just try every angle
        // in 1 degree steps (90 comparisons), but there is a cleverer way.
        //
        // The distance-vs-angle curve has one dip near the answer. Golden section search finds
        // the bottom of such a dip by keeping a bracket [a, b] and two probes inside it placed at
        // the golden ratio. Whichever probe is worse, the answer cannot be beyond it, so that side
        // of the bracket moves in. The magic of the golden ratio is that ONE of the old probes is
        // already in the right place for the new bracket, so each iteration costs just one new
        // distance computation. It converges to 2 degrees in about 10 steps instead of 90.
        //
        // You do not need to understand the golden ratio to use this. You do need to understand
        // that all it does is: "wiggle the candidate a bit, keep the best fit".
        static float DistanceAtBestAngle(List<Vector2> candidate, List<Vector2> template,
                                         float thetaA, float thetaB, float thetaDelta)
        {
            float x1 = Phi * thetaA + (1f - Phi) * thetaB;
            float f1 = DistanceAtAngle(candidate, template, x1);
            float x2 = (1f - Phi) * thetaA + Phi * thetaB;
            float f2 = DistanceAtAngle(candidate, template, x2);

            while (Mathf.Abs(thetaB - thetaA) > thetaDelta)
            {
                if (f1 < f2)
                {
                    // The dip is on the left. Shrink the bracket from the right.
                    thetaB = x2;
                    x2 = x1; f2 = f1;
                    x1 = Phi * thetaA + (1f - Phi) * thetaB;
                    f1 = DistanceAtAngle(candidate, template, x1);
                }
                else
                {
                    // The dip is on the right. Shrink the bracket from the left.
                    thetaA = x1;
                    x1 = x2; f1 = f2;
                    x2 = (1f - Phi) * thetaA + Phi * thetaB;
                    f2 = DistanceAtAngle(candidate, template, x2);
                }
            }
            return Mathf.Min(f1, f2);
        }

        /// Rotate the candidate by a trial angle, then measure how far it is from the template.
        static float DistanceAtAngle(List<Vector2> candidate, List<Vector2> template, float radians)
        {
            var rotated = RotateBy(candidate, radians);
            return PathDistance(rotated, template);
        }

        // THE ACTUAL COMPARISON
        //
        // After all that preparation, this is embarrassingly simple: point 0 of A to point 0 of B,
        // point 1 to point 1, ... and average the distances. Both lists have exactly n points
        // (step 1 guaranteed it), both are the same size (step 3), both are centred (step 4), and
        // both start at 3 o'clock (step 2). So if they are the same shape, corresponding points
        // sit nearly on top of each other and the average is tiny.
        //
        // This is also why $1 cares about DIRECTION: a circle drawn clockwise pairs point 10 with
        // a point going the other way round on an anticlockwise template. If you want both
        // directions to count as "Circle", store both as templates. (SigilTemplates.cs does.)
        static float PathDistance(List<Vector2> a, List<Vector2> b)
        {
            float sum = 0f;
            int n = Mathf.Min(a.Count, b.Count);
            for (int i = 0; i < n; i++) sum += Vector2.Distance(a[i], b[i]);
            return sum / n;
        }

        // ------------------------------------------------------------------ small helpers

        /// Total length of the stroke: the sum of the gaps between consecutive points.
        public static float PathLength(IList<Vector2> points)
        {
            float len = 0f;
            for (int i = 1; i < points.Count; i++) len += Vector2.Distance(points[i - 1], points[i]);
            return len;
        }

        /// The average of all the points. "The middle" of the stroke.
        public static Vector2 Centroid(IList<Vector2> points)
        {
            var sum = Vector2.zero;
            foreach (var p in points) sum += p;
            return sum / points.Count;
        }
    }
}
