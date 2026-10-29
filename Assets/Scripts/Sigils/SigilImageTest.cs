using System.Text;
using UnityEngine;

// TEST THE RECOGNISER ON A PICTURE
//
// Drop an image into the Inspector slot, press the button, read the Console. That is the whole
// component. It exists so you can see the recogniser think before there is a VR controller to
// draw with. The pipeline it runs is the same one the game will run:
//
//     picture  ->  ordered stroke  ->  $1 recogniser  ->  name + score
//     (SigilImageTracer)              (SigilRecognizer)
//
// The image should be a 2D line drawing: ONE black line on a white background. A circle, a
// triangle, a square. Something you could draw without lifting the pen. Anti-aliased or
// slightly grey lines are fine; the tracer thresholds on brightness.

namespace Sigils
{
    [RequireComponent(typeof(SigilRecognizer))]
    public class SigilImageTest : MonoBehaviour
    {
        public const string ImageNote =
            "Image should be a 2D line drawing: a single BLACK line on a WHITE background, " +
            "ideally something like a circle or a triangle, drawn in one continuous stroke " +
            "($1 is a unistroke recognizer). Any size; it is shrunk to a small grid before tracing.";

        [Tooltip(ImageNote)]
        public Texture2D image;

        [Header("Tracing")]
        [Tooltip("The image is shrunk so its long side is about this many cells before tracing. Smaller is faster and more forgiving of gaps; larger keeps more detail.")]
        [Range(32, 256)] public int traceGridSize = 96;

        [Tooltip("Pixels darker than this (0 = black, 1 = white) count as ink.")]
        [Range(0f, 1f)] public float inkThreshold = 0.5f;

        [Header("Recognition")]
        [Tooltip("An image does not tell us where the line was started or which way it was drawn, and $1 cares about both. Leave this on to try every start point and both directions and keep the best. Turn it off to see how much that matters.")]
        public bool unknownStartAndDirection = true;

        [Tooltip("Print the score for every template, not just the winner.")]
        public bool logAllScores = true;

        /// Also reachable from the component's context menu (the three dots / right-click).
        [ContextMenu("Recognize Image")]
        public void RecognizeImage()
        {
            if (image == null)
            {
                Debug.LogWarning($"[Sigil] {name}: no image assigned. {ImageNote}", this);
                return;
            }

            var recognizer = GetComponent<SigilRecognizer>();

            // The image was imported the normal way, so C# cannot read its pixels directly.
            // TraceAny copies it somewhere readable, traces it, and cleans up.
            var stroke = SigilImageTracer.TraceAny(image, traceGridSize, inkThreshold, out var info);

            var sb = new StringBuilder();
            if (stroke.Count < 2)
            {
                sb.AppendLine($"[Sigil] {image.name}: found no ink. Is it a dark line on a light background? " +
                              $"({info.SourceWidth}x{info.SourceHeight}, threshold {inkThreshold:0.00})");
                Debug.LogWarning(sb.ToString(), this);
                return;
            }

            var result = unknownStartAndDirection
                ? recognizer.RecognizeAnyStartOrDirection(stroke)
                : recognizer.Recognize(stroke);

            bool confident = result.IsConfident(recognizer.confidenceThreshold);
            string verdict = confident ? result.Name.ToUpperInvariant() : $"no sigil (closest was {result.Name})";
            sb.AppendLine($"[Sigil] {image.name}  ->  {verdict}   score {result.Score:0.00}, threshold {recognizer.confidenceThreshold:0.00}");

            sb.AppendLine($"        compared against {string.Join(", ", recognizer.KnownNames)}");

            sb.AppendLine($"        traced {info.InkCells} ink cells on a {info.GridWidth}x{info.GridHeight} grid " +
                          $"(from {info.SourceWidth}x{info.SourceHeight} px), line about {info.LineThicknessCells:0.0} cells thick, " +
                          $"stroke of {info.PathPoints} points covering {info.Coverage:P0} of the ink");

            if (info.Coverage < 0.8f)
                sb.AppendLine("        WARNING: the walk could not reach all the ink. The drawing probably has more than one " +
                              "stroke or a gap. $1 is a unistroke recogniser; it only saw the first part.");

            if (logAllScores)
            {
                sb.Append("        ");
                for (int i = 0; i < result.Matches.Count; i++)
                {
                    if (i > 0) sb.Append("  |  ");
                    sb.Append($"{result.Matches[i].Name} {result.Matches[i].Score:0.00}");
                }
                sb.AppendLine();
            }

            if (confident) Debug.Log(sb.ToString(), this);
            else           Debug.LogWarning(sb.ToString(), this);
        }
    }
}
