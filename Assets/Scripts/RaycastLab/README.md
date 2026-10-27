# Raycasts and the XR Interaction Toolkit

Five small VR scenes. Build them from the **Raycast Lab** menu (or **Raycast Lab → Build All**).
Each one adds a single idea on top of the one before.

| Menu item | Scene | What it shows |
|---|---|---|
| 1 - Ray By Hand | `Scenes/Raycast_1_RayByHand` | `Physics.Raycast`, written out and drawn |
| 2 - Ray Interactor | `Scenes/Raycast_2_RayInteractor` | the toolkit doing the same thing: interactor, interactable, manager |
| 3 - Some Things Not Others | `Scenes/Raycast_3_SomeThingsNotOthers` | two different filters: Unity layers and interaction layers |
| 4 - Canvas Buttons | `Scenes/Raycast_4_CanvasButtons` | clicking a world-space canvas with the ray |
| 5 - Grab | `Scenes/Raycast_5_Grab` | picking things up, from a distance and up close |

Open the Console first: **Window → General → Console**.

## Driving the scenes without a headset

Press Play. If no headset is plugged in, the keyboard simulator takes over. **Click inside the
Game view first**, or the keys do nothing.

| Key | What it does |
|---|---|
| `]` (hold) | take hold of the **right** hand (the one with the ray) |
| `[` (hold) | take hold of the **left** hand |
| right mouse button + move | turn whatever you are holding. This is how you **aim** |
| `W` `A` `S` `D` `Q` `E` | move whatever you are holding |
| `G` | **grip** button. Select / grab |
| `T` | **trigger** button. Click a UI button |
| `R` | reset the pose |

So the basic move is: hold `]`, hold the right mouse button, sweep the mouse until the line
lands on something, then tap `G` or `T`. With a real headset, just point and press grip or trigger.

---

## 1. Ray By Hand — what a raycast is

No toolkit. The right controller has a `LineRenderer`, a marker sphere, and `RayProbe.cs`.
Read the script. It is one question, asked every frame:

```csharp
bool hitSomething = Physics.Raycast(origin, direction, out hit, maxDistance, whatTheRaySees);
```

- **origin**: where the controller is.
- **direction**: `transform.forward`, the way the controller points.
- **maxDistance**: give up after this many metres.
- **hit**: filled in only if it returns `true`. What (`hit.collider`), where (`hit.point`), how far
  (`hit.distance`), which way the surface faces (`hit.normal`).

1. Press Play. The line is green and ends on the centre cube. The label over the hand says
   what you are hitting and how far away it is.
2. Aim at the purple cube on the left. The line goes straight through it and lands on the
   wall behind. Select the cube in the Hierarchy: **it has no Collider**. A ray only sees colliders.
   Not meshes. Not what you can see.
3. Aim up at the sky. Red line, "(nothing within 10 m)". Change **Max Distance** to 2 in the
   Inspector while playing. Now the centre cube at 3 m is out of reach too.
4. Watch the Console. The script only prints when the answer changes. That is a habit worth
   copying: logging every frame is useless.

## 2. Ray Interactor — the toolkit does it for you

Same room. `RayProbe` is gone. In its place on the right controller:

- **XRRayInteractor**: does the raycast every frame. The thing that points.
- **XRInteractorLineVisual** (+ a `LineRenderer`): draws it. Green = hovering something it can
  use. Red = hitting something it can't, or nothing.
- **XRInteractionManager** (its own object): every frame, asks each interactor what it hit, and
  tells the object on the other end.

The cubes each have **XRSimpleInteractable**: the thing that can be pointed at. And `HoverHighlight.cs`,
which only *listens*. Read it: there is no raycast in it. It gets told.

1. Aim at the centre cube. Yellow = **hover** (`hoverEntered`). Press `G`. Cyan = **select**
   (`selectEntered`). Release. Back to yellow. Aim away. Back to blue (`hoverExited`).
2. Aim at the left cube. The line stops on it but turns **red**. The label says "no interaction".
   It has a collider, so the ray hits it. It has no interactable, so the toolkit has nothing to tell.
3. In the Inspector, untick **XR Interaction Manager** while playing. Everything still draws,
   nothing highlights. The manager is the one calling the events.

Words to keep: **Interactor** points. **Interactable** gets pointed at. **Manager** connects them.
**Hover** = aimed at. **Select** = aimed at + button held.

## 3. Some Things Not Others — two filters

Two cubes won't play along, for two different reasons. Select **Right Controller** and look at
the XR Ray Interactor:

- **Raycast Mask** is a *physics* filter made of **Unity layers** (the dropdown at the top right
  of every object). It is set to everything except `Ghost`. The ray does not see Ghost objects at all.
- **Interaction Layers** is a *toolkit* filter made of **interaction layers** (Project Settings →
  XR Interaction Toolkit). It is set to `Default` only. An interactable on another layer is
  hit, but never hovered or selected.

1. Aim at the grey cube on the left. The ray goes **through** it and lights up the cube
   behind. Select it: its Layer is `Ghost`. Change it back to `Default` while playing. Now it blocks.
2. Aim at the dark red cube on the right. The line **stops** on it, red. Label: "no interaction".
   Select it: its XR Simple Interactable has **Interaction Layer Mask** = `Locked`. Add `Locked`
   to the ray interactor's Interaction Layers. Now it works.

Which filter do you want? "The ray should ignore this wall": Unity layer, raycast mask.
"The ray should stop on this but not pick it up": interaction layer. Doors, locked chests,
"this hand grabs, that hand doesn't": all interaction layers.

## 4. Canvas Buttons — clicking UI with a ray

Two canvases that look the same. Only the right one works. The ray needs three things:

| Where | Component | Job |
|---|---|---|
| EventSystem | **XRUIInputModule** | the mouse input module, but for tracked devices |
| Canvas | **TrackedDeviceGraphicRaycaster** | lets a ray in the world hit this canvas |
| Right Controller | XR Ray Interactor → **UI Interaction** ticked | the ray also raycasts against UI |

1. Aim at a button on the right canvas. It highlights. Press `T` (trigger). The cube
   changes colour, the counter goes up. The label says `UI: Red`.
2. Aim at the left canvas. Nothing, and the label does not say `UI:`. Select the canvas: it has
   only the ordinary **GraphicRaycaster**, the one for mouse and touch. Add a
   `TrackedDeviceGraphicRaycaster` to it while playing. Now it works.
3. Select a button on the right canvas and look at **On Click**. It calls `PaintTarget.PaintRed`.
   Open `PaintTarget.cs`. There is nothing VR in it. Once the ray can click, every bit of Unity
   UI you already know works unchanged.

Note the split: **grip** (`G`) selects objects, **trigger** (`T`) presses UI. Both are set on the
ray interactor (Select Input, UI Press Input).

## 5. Grab — picking things up

Two ways, one per hand.

**Right hand, from a distance.** The same ray interactor. The objects on the table have
**XRGrabInteractable** (plus a Rigidbody; it insists on one). Aim, hold `G`: the object jumps to
your hand, because **Force Grab** is ticked on the interactor. Untick it: the object stays out
at the end of the ray instead. Let go while moving and it keeps the velocity (**Throw On Detach**).

**Left hand, up close.** No ray. The left controller has an **XRDirectInteractor** and a
*trigger* Sphere Collider around it. Hold `[`, walk the hand into an object with `W`, hold `G`.
It grabs what it is touching. Every grab in VR is one of these two.

The white cube on the right has a collider and nothing else. The ray stops on it, red. Not
everything that blocks a ray is something you can take.

Select a grabbed cube in the Hierarchy while holding it: watch **Movement Type**. `Kinematic`
means it follows the hand but still shoves other physics objects out of the way.

---

## Recap

```
    origin + direction + max distance  ->  Physics.Raycast  ->  hit (what, where, how far)
                  |
       XRRayInteractor does this every frame, then XRInteractionManager tells the
       XR...Interactable on the far end: hoverEntered, selectEntered, ...
                  |
       Filter with Raycast Mask (Unity layers: ray passes through)
           or Interaction Layers (toolkit: ray stops, no interaction)
                  |
       UI:   XRUIInputModule + TrackedDeviceGraphicRaycaster + UI Interaction on
       Grab: XRGrabInteractable + Rigidbody.  Ray (far) or XRDirectInteractor (near)
```

**Exercise.** In scene 5, make the ball grabbable only by the left (direct) hand, and the cubes
only by the right (ray) hand. You need one new interaction layer and no new code.
