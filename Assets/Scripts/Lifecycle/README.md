# C# in Unity — why there is no `main()`

Three tiny scenes for the lifecycle lecture. Build them from the **Lifecycle Lab** menu.
Each one is a cube or three and a script you can read in under a minute.

| Menu item | Scene | What it shows |
|---|---|---|
| 1 - Probe | `Scenes/Lifecycle_1_Probe` | Unity calling your methods, in its order |
| 2 - Call Chain | `Scenes/Lifecycle_2_CallChain` | A MonoBehaviour calling plain C# |
| 3 - Frames vs Time | `Scenes/Lifecycle_3_FramesVsTime` | Why `Time.deltaTime` exists |

Open the Console first: **Window → General → Console**. Clear it before each step.

---

## 1. Probe — watch Unity call your code

One cube. One script, `LifecycleProbe.cs`. Every method in it just prints its own name.
You never call any of them. Read the file: there is no `main`, nothing calls `Awake`, nothing
loops. Unity owns the loop. You fill in the hooks.

1. **Press Play.** Read the first ten lines. Write down the order. Which methods fired once?
   Which keep firing?
2. **Pause.** Untick the checkbox at the top of the cube's Inspector. What printed? Tick it
   again. What printed this time? Before you look: did `Awake` or `Start` fire again? Why not?
3. Still playing, **delete the cube** from the Hierarchy. What are the last two lines?
4. Stop. **Duplicate the cube** (Ctrl+D). Name them `A` and `B`. Clear the Console, Play.
   Look at the first six lines. Does A finish its whole startup before B begins? What does that
   tell you about when it's safe for one object to talk to another?

The mental model, one line each:

- **Awake** — "I exist." Set yourself up. Don't touch other objects yet.
- **OnEnable** — "I'm on." Fires every time the object is activated.
- **Start** — "Everyone exists." Now it's safe to find and talk to other objects.
- **Update** — "A frame is happening." Input, non-physics movement.
- **FixedUpdate** — "A physics step is happening." May run 0, 1 or several times per frame.
- **LateUpdate** — "Everyone's Update is done." Cameras, anything that needs final positions.
- **OnDisable / OnDestroy** — mirrors of OnEnable / Awake. Clean up.

## 2. Call Chain — plain C# still works, if something calls it

Three scripts:

- `Greeter.cs` is plain C#. No `MonoBehaviour`, no `using UnityEngine`. Unity has no idea it exists.
- `Forgotten.cs` is also plain C#. It even has a method called `Start()`. It never runs.
- `CallChainDemo.cs` is a MonoBehaviour. Unity calls its `Start` and `Update`, and from inside
  those it calls `greeter.Greet(...)`.

Press Play. `Greeter` prints, once at start and then every 120 frames. `Forgotten` never prints.

The chain is always: **Unity → a MonoBehaviour on an active object → whatever that calls.**
Naming a method `Start` does nothing on its own. It only matters on a MonoBehaviour, because
that's the only kind of object Unity looks at.

Try it: in `CallChainDemo.Start`, add `forgotten.Start();`. Now it prints. Nothing about
`Forgotten` changed. Something in the chain called it.

## 3. Frames vs Time — why `Time.deltaTime` exists

Three cubes race from the left line to the right line. They wrap around when they finish.

| Cube | Script | Moves |
|---|---|---|
| Red | `FrameMover` | 0.1 units **every Update** |
| Green | `TimeMover` | 6 units **per second**, using `Time.deltaTime` |
| Blue | `FixedMover` | 6 units per second, but in **FixedUpdate** |

Press Play. At 60 fps the red and green cubes move at the same speed (0.1 × 60 = 6).

Now press **2** to cap the game at 30 fps. Red slows to half speed. Green doesn't change.
Press **1** for 15 fps. Red crawls. Green still doesn't change. Press **R** to line them up again.

Red is counting frames. Green is counting seconds. A player on a slow laptop gets fewer frames
per second, so anything that moves per frame moves slower for them. `Time.deltaTime` is how long
the last frame took, so speed × deltaTime gives the right distance no matter how long the frame was.

The blue cube keeps pace with green at any frame rate, but watch it at 15 fps: it moves in little
jumps. `FixedUpdate` runs on its own fixed clock (every 0.02 s by default), several times per slow
frame, so its motion is correct but only *shown* once per frame.

One more script on each cube: `WrapAround.cs`. It runs in `LateUpdate`, after every mover has
finished, and checks whether the cube ran off the edge. That's what LateUpdate is for: looking at
where things ended up this frame.
