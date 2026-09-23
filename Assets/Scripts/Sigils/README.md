# Sigils — recognising a drawn shape

The seed of the VR sigil game. One scene, built from the **Sigil Lab** menu, that recognises
a shape from a picture. Later the picture will be replaced by the path of a VR controller; the
recogniser will not change.

| Menu item | Scene | What it shows |
|---|---|---|
| Build Sigil Scene | `Scenes/Sigils` | The $1 recogniser reading a line drawing |

## Try it

1. Build the scene (or open `Scenes/Sigils`). Select **Sigil Recognizer** in the Hierarchy.
2. In the Inspector, the **Sigil Image Test** component has an **Image** slot. It should hold a
   2D line drawing: **one black line on a white background**, something like a circle or a
   triangle, drawn without lifting the pen. There are samples in `Textures/SigilTests`.
3. Press **Recognize image -> Console**. Open the Console (**Window > General > Console**).
4. Read the result. It prints the winning shape, its score, what the tracer saw, and the score
   for every other shape it knows.

Then draw your own in any paint program, drop the PNG into the project, and try that. Try a
scribble too, and watch the score fall below the threshold.

## The scripts, in reading order

| File | What it is |
|---|---|
| `DollarOneRecognizer.cs` | **Read this one.** The $1 recogniser, plain C#, commented step by step. |
| `SigilTemplates.cs` | The shapes it knows out of the box, generated from geometry. |
| `SigilRecognizer.cs` | The recogniser as a component, plus settings in the Inspector. |
| `SigilImageTracer.cs` | Turns an image into an ordered list of points. Test scaffolding. |
| `SigilImageTest.cs` | The Inspector slot and button. |

## The idea in one paragraph

A drawing is a list of points in the order the pen visited them. Clean that list up so that
where, how big and how fast it was drawn no longer matter (resample, rotate, scale, translate),
and two drawings of "a circle" become nearly the same list of numbers. Recognition is then just
"which stored example is this closest to?" That is all $1 does, and it is why it needs no
training data, no library, and about 150 lines.

## Things to try in the code

- In `SigilTemplates.cs`, add a new shape. A single stroke of corner points is enough.
- In the `Sigil Recognizer` component, set **Angle Range** to 180 and test the `V` and a `^`.
- Turn off **Unknown Start And Direction** and see how much the start point matters to $1.
- Lower **Trace Grid Size** to 32 and see when the tracer starts losing shapes.
