# Assignment 3 — Flying

One VR scene, ready to go. The rig is built, the script is attached, the slots are filled in.
The only thing missing is the inside of `Update()` in `Flying.cs`. That part is yours.

| Menu item | Scene | What it is |
|---|---|---|
| Assignments > Build Assignment 3 - Flying | `Scenes/Assignment3_Flying` | A VR player, a forest, three hoops |

You don't need to build it. The scene is already in the project: open `Scenes/Assignment3_Flying`.
If you break the scene while experimenting, the menu item makes a fresh one. It does not touch
your script.

The assignment itself (what to hand in, the rubric) is in `Assignments/Assignment3.txt` at the top
of the project folder.

## What's in the scene

```
XR Origin (XR Rig)        the player's body. Flying.cs is on this object.
  Camera Offset           lifts the head to standing height
    Main Camera           the head. Follows the headset.
    Left Controller       the left hand. Blue block.
    Right Controller      the right hand. Red block.
Headset Or Simulator      picks a real headset or the keyboard simulator when you press Play
XR Interaction Simulator  the keyboard simulator. Greyed out until it's needed.
World                     ground, trees, hoops, towers, clouds
```

Select **XR Origin (XR Rig)** and look at the **Flying** component in the Inspector. Three slots,
already filled: the two controllers and the camera. Those are the only three things your script
needs to know about.

The red-and-white towers are rulers. Each band is 5 m tall, so you can read how high you are.

## Running it with a headset

Any PC headset that speaks OpenXR works. For a Meta Quest:

1. Connect the Quest to the PC with Link (cable) or Air Link, and start Link from inside the headset.
2. In the Meta Quest Link app on the PC: **Settings > General > OpenXR Runtime**, set Meta Quest
   Link as the active runtime. You only do this once.
3. Press **Play** in Unity. The Console says `[Assignment 3] Headset found`. Put the headset on.

You see a blue block where your left hand is and a red block where your right hand is. Nothing
moves yet, because `Update()` is empty.

## Running it without a headset

Just press **Play**. The Console says `[Assignment 3] No headset found. Using the simulator`,
and you drive a pretend headset and two pretend controllers from the keyboard.
Any OpenXR lines the Console prints before that message are Unity looking for a headset and not
finding one. Ignore them.

**Click inside the Game view first**, or the keys do nothing.

| Key | What it does |
|---|---|
| `[` | take hold of the **left** hand |
| `]` | take hold of the **right** hand |
| `[` and `]` together | take hold of **both** hands |
| `W` `S` | move what you're holding forward / back |
| `A` `D` | move it left / right |
| `E` `Q` | move it up / down |
| hold right mouse button + move the mouse | turn what you're holding |
| `R` | put it back where it started |
| `Tab` | switch between "hands" and "whole body" |

When you press Play you are holding the whole body: W A S D walks you around the room, hands and
all. That is not flying, and it does not use your script. Press `[` or `]` to take hold of a hand.

To test your script: take hold of both hands, hold `E` for a second so they rise above your head,
and let go. If your `Update()` works you are now going up. Hold `Q` until they are below your
head and you come back down.

The simulator draws its own small panel in the Game view. It shows what you are holding right now.

## The scripts

- **`Flying.cs`** is the assignment. Read the comment at the top before you write anything. It
  explains the one idea that makes this work: you read the head and hands, and you move their parent.
- **`HeadsetOrSimulator.cs`** is plumbing. It starts the headset when you press Play, or switches
  the simulator on if there isn't one. You don't need to change it. It is short, and it is the
  Lifecycle Lab's `Awake` and `OnDestroy` doing a real job, so it's worth a read.

## Things to try once it works

None of these are graded.

- Fly through all three hoops in order: yellow, orange, pink.
- Change `speed` in the Inspector while the game is running. What feels good? What makes you queasy?
- Rest your arms at your sides. What happens, and why? What would you change so that resting
  means hovering?
- You can fly through the ground. How would you stop that?
