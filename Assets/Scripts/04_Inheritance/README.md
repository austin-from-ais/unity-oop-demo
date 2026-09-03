# Scene 4 — Inheritance

**Build it:** OOP Demo > Build Scene 4 - Inheritance

Two skeletons on the ground, two skeleton mages floating in the air. Walk toward them and they
come to you. Kill a flying one and it falls.

## Two files

**`Enemy.cs`** is scene 3's finished `Enemy` with one addition: it moves. Every frame `Update`
calls a method named `Move`, and `Move` walks toward you.

**`FlyingEnemy.cs`** is tiny. Read the first line of the class:

```csharp
public class FlyingEnemy : Enemy
```

That colon is the whole lesson. It says: a `FlyingEnemy` *is an* `Enemy`. It has everything an
Enemy has, without repeating any of it. Health, the bar, `TakeDamage`, all inherited. The file
only contains what's *different* about a flying one: its own `Move`, and what to do when it dies.

## The words

- `Enemy` is the **parent** (or base class). `FlyingEnemy` is the **child** (or derived class).
- In the parent, `Move` is marked `virtual`: "children may replace this."
- In the child, `Move` is marked `override`: "I am replacing it."
- `protected` is a third option next to `public` and `private`: hidden from outsiders, visible to
  children. The movement numbers in `Enemy` are `protected` so `FlyingEnemy` can use them.

Look at `Enemy.Update()`. It calls `Move()`. It doesn't check what kind of enemy it is. On a
ground skeleton, `Enemy.Move` runs. On a mage, `FlyingEnemy.Move` runs. The parent calls the
hook; whichever child you are answers.

## The rug pull

Open any script you've ever written for Unity. The first line of the class says:

```csharp
public class Something : MonoBehaviour
```

Same colon. `MonoBehaviour` is the parent. Your script is the child. And `Start`, `Update`,
`OnTriggerEnter`, all of those, are exactly what `Move` is here: hooks that the parent calls and
you fill in. You never call `Update` yourself. Unity does, the same way `Enemy.Update` calls `Move`.

You've been inheriting since your first script. Now you know what the word means.

## Try this

1. Select a flying mage while playing. Its component says `Flying Enemy`, but the Inspector shows
   Stats, Movement and Wiring headers that live in `Enemy.cs`. Inherited fields show up too.
2. In `FlyingEnemy.cs`, delete the `OnDied` method. Kill a mage. It now dies in mid-air, because
   the parent's `OnDied` does nothing. Put it back.
3. Make a third enemy type. `class FastEnemy : Enemy` that overrides `Move` to call `base.Move()`
   after doubling `moveSpeed`... or anything you like. Add it to a SkeletonMinion prefab. You
   should not need to write a single line about health.

## One gotcha worth knowing

`Enemy` has a `Start()`. If you also write `void Start()` in `FlyingEnemy`, Unity calls only the
child's, and the parent's never runs. That's because `Start` isn't `virtual`, so you didn't
override it, you *hid* it. If you need both, mark the parent's `protected virtual void Start()`,
override it in the child, and call `base.Start()` inside. Or do what this scene does: give children
their own hook (`OnDied`) and keep `Start` in one place.

## Think about

- What does `Hero.cs` know about flying enemies? Search the file for "Flying". Then explain how
  the hero manages to attack one anyway. Scene 5 is about that.
