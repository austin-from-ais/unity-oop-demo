using System.Collections.Generic;
using UnityEngine;

// IMAGE -> STROKE
//
// The recogniser wants an ORDERED list of points: the path a pen took. An image is the opposite,
// a grid of pixels with no order at all. This class bridges the two so we can test the
// recogniser on a picture before the VR controller exists.
//
// How it does it, in three moves:
//
//   1. Shrink the image to a small grid (about 96 cells across) and decide per cell: ink or not.
//      A cell is ink if ANY of the source pixels inside it is darker than the threshold, so thin
//      lines survive the shrink.
//
//   2. Guess how thick the line is, by measuring how far ink cells are from the nearest non-ink
//      cell. We need this for the walk in step 3.
//
//   3. "Walk" the line like an ant: start at the top-most ink cell, and repeatedly jump to the
//      nearest ink cell you have not visited yet, preferring to keep going the way you were
//      already heading. Each visit also marks a small disc around the current cell as visited
//      (a bit wider than the line), so the ant walks DOWN the line instead of zig-zagging
//      across its width. When there is nothing unvisited within reach, the stroke is over. Then we go back to the start and walk the other way, in case we
//      started in the middle of an open shape like a line, and stitch the two halves together.
//
// This is deliberately crude. It knows nothing about curves or corners, and it gives up at gaps
// bigger than a few cells. For clean single-line drawings that is plenty. Note that the point
// where the ant starts is arbitrary (top-most), so the resulting stroke's start point and
// direction mean nothing; see SigilRecognizer.RecognizeAnyStartOrDirection.

namespace Sigils
{
    public static class SigilImageTracer
    {
        /// Numbers about what the tracer saw, for printing to the Console.
        public struct Info
        {
            public int   SourceWidth, SourceHeight;   // the image, in pixels
            public int   GridWidth, GridHeight;       // the shrunk grid, in cells
            public int   InkCells;                    // cells that were dark enough to count
            public int   VisitedCells;                // ink cells the walk actually passed over
            public int   PathPoints;                  // points in the returned stroke
            public float LineThicknessCells;          // estimated, in grid cells

            /// 1.0 means the walk covered every ink cell: one clean connected stroke. Much less
            /// than that means the drawing has separate parts the ant could not reach.
            public float Coverage => InkCells == 0 ? 0f : (float)VisitedCells / InkCells;
        }

        /// Trace a readable (CPU-accessible) texture into an ordered stroke. Coordinates are grid
        /// cells with y UP, matching the rest of the project.
        public static List<Vector2> Trace(Texture2D readable, int maxGridSize, float inkThreshold, out Info info)
        {
            // GetPixels32 returns rows bottom-to-top, so y is already "up".
            return Trace(readable.GetPixels32(), readable.width, readable.height, maxGridSize, inkThreshold, out info);
        }

        /// The same, on raw pixels (row-major, bottom row first). Plain C#: no Unity needed, so it
        /// can be tested outside the editor.
        public static List<Vector2> Trace(Color32[] px, int w, int h, int maxGridSize, float inkThreshold, out Info info)
        {
            info = default;
            info.SourceWidth = w;
            info.SourceHeight = h;

            // ---- 1. shrink to a grid of ink / not-ink -------------------------------------
            int block = Mathf.Max(1, Mathf.CeilToInt(Mathf.Max(w, h) / (float)Mathf.Max(8, maxGridSize)));
            int gw = (w + block - 1) / block, gh = (h + block - 1) / block;
            info.GridWidth = gw;
            info.GridHeight = gh;

            var ink = new bool[gw, gh];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var c = px[y * w + x];
                    float lum = (0.299f * c.r + 0.587f * c.g + 0.114f * c.b) / 255f;   // perceived brightness
                    if (lum < inkThreshold) ink[x / block, y / block] = true;
                }
            }
            for (int y = 0; y < gh; y++)
                for (int x = 0; x < gw; x++)
                    if (ink[x, y]) info.InkCells++;

            var stroke = new List<Vector2>();
            if (info.InkCells == 0) return stroke;

            // ---- 2. estimate line thickness -----------------------------------------------
            // For each ink cell: how many rings out do we go before we hit a non-ink cell? The
            // deepest cell we find is in the middle of the line, so the line is about twice
            // that deep. We take the maximum, not the average, because the walk in step 3
            // must eat the WHOLE width of the line or it will double back along leftovers.
            int deepest = 0;
            for (int y = 0; y < gh; y++)
            {
                for (int x = 0; x < gw; x++)
                {
                    if (!ink[x, y]) continue;
                    int r = 0;
                    while (r < 8 && !RingHasBackground(ink, gw, gh, x, y, r + 1)) r++;
                    if (r > deepest) deepest = r;
                }
            }
            info.LineThicknessCells = 2f * (deepest + 0.5f);

            // ---- 3. walk the line ---------------------------------------------------------
            // The ant may be standing on the EDGE of the line, so to eat the whole width it has to
            // reach a full line-thickness in every direction, plus one cell of slack because a
            // diagonal line is wider than the ring test measures. Capped so a blobby image cannot
            // make the disc swallow the whole shape.
            int eatRadius = Mathf.Clamp(Mathf.CeilToInt(info.LineThicknessCells) + 1, 1, Mathf.Max(1, Mathf.Min(gw, gh) / 8));
            // How far the ant will look for its next cell. Where two lines cross, the disc eaten
            // around the crossing punches a hole in the OTHER line that can be several discs long
            // (lines crossing at a shallow angle), so the reach has to be well past one disc.
            int reach     = eatRadius * 4 + 3;

            // Start at the top-most ink cell (left-most if there is a tie).
            Vector2Int start = default;
            bool found = false;
            for (int y = gh - 1; y >= 0 && !found; y--)
                for (int x = 0; x < gw && !found; x++)
                    if (ink[x, y]) { start = new Vector2Int(x, y); found = true; }

            var visited = new bool[gw, gh];
            int visitedCount = 0;

            var forward  = Walk(ink, visited, gw, gh, start, eatRadius, reach, ref visitedCount);
            var backward = Walk(ink, visited, gw, gh, start, eatRadius, reach, ref visitedCount);

            // If we began in the middle of an open shape, the second walk found the other half.
            // It runs start -> far end, so flip it and put it in front of the first walk.
            // (A handful of leftover cells near the start of a closed shape do not count.)
            if (backward.Count > Mathf.Max(3, forward.Count / 10))
            {
                backward.Reverse();
                backward.RemoveAt(backward.Count - 1);   // drop the duplicate start cell
                foreach (var p in backward) stroke.Add(p);
            }
            foreach (var p in forward) stroke.Add(p);

            info.VisitedCells = visitedCount;
            info.PathPoints = stroke.Count;
            return stroke;
        }

        /// One ant walk. Returns the cells visited, in order, starting with `from`.
        ///
        /// Picking the NEAREST unvisited cell is not quite enough: where two lines cross (the
        /// middle of a star) every branch is equally near. So the ant also has momentum: a cell
        /// that keeps it going in the direction it was already travelling is preferred over one
        /// that turns it sideways, and one that sends it straight back is preferred least.
        static List<Vector2> Walk(bool[,] ink, bool[,] visited, int gw, int gh, Vector2Int from,
                                  int eatRadius, int reach, ref int visitedCount)
        {
            var path = new List<Vector2> { from };
            Eat(ink, visited, gw, gh, from, eatRadius, ref visitedCount);

            var cur = from;
            var heading = Vector2.zero;          // direction of the last step; zero = no momentum yet
            while (true)
            {
                float bestCost = float.MaxValue;
                Vector2Int next = default;
                for (int dy = -reach; dy <= reach; dy++)
                {
                    for (int dx = -reach; dx <= reach; dx++)
                    {
                        int x = cur.x + dx, y = cur.y + dy;
                        if (x < 0 || y < 0 || x >= gw || y >= gh) continue;
                        if (!ink[x, y] || visited[x, y]) continue;

                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        // dot is +1 straight ahead, 0 sideways, -1 straight back. Squaring makes
                        // the preference strong: a cell straight ahead wins even if it is several
                        // times further away than one that would turn the ant onto another line.
                        float dot  = heading == Vector2.zero ? 0f : Vector2.Dot(heading, new Vector2(dx, dy) / dist);
                        float turn = 1.5f - dot;                 // 0.5 straight, 1.5 sideways, 2.5 back
                        float cost = dist * turn * turn;
                        if (cost < bestCost) { bestCost = cost; next = new Vector2Int(x, y); }
                    }
                }
                if (bestCost == float.MaxValue) break;   // nothing left in reach: stroke is over

                heading = ((Vector2)(next - cur)).normalized;
                path.Add(next);
                Eat(ink, visited, gw, gh, next, eatRadius, ref visitedCount);
                cur = next;
            }
            return path;
        }

        /// Mark every ink cell within `radius` of `c` as visited.
        static void Eat(bool[,] ink, bool[,] visited, int gw, int gh, Vector2Int c, int radius, ref int visitedCount)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    int x = c.x + dx, y = c.y + dy;
                    if (x < 0 || y < 0 || x >= gw || y >= gh) continue;
                    if (ink[x, y] && !visited[x, y]) { visited[x, y] = true; visitedCount++; }
                }
            }
        }

        /// Is there any non-ink cell (or the image edge) on the square ring `r` cells out from (cx, cy)?
        static bool RingHasBackground(bool[,] ink, int gw, int gh, int cx, int cy, int r)
        {
            for (int i = -r; i <= r; i++)
            {
                if (IsBackground(ink, gw, gh, cx + i, cy - r)) return true;
                if (IsBackground(ink, gw, gh, cx + i, cy + r)) return true;
                if (IsBackground(ink, gw, gh, cx - r, cy + i)) return true;
                if (IsBackground(ink, gw, gh, cx + r, cy + i)) return true;
            }
            return false;
        }

        static bool IsBackground(bool[,] ink, int gw, int gh, int x, int y)
        {
            if (x < 0 || y < 0 || x >= gw || y >= gh) return true;
            return !ink[x, y];
        }
    }
}
