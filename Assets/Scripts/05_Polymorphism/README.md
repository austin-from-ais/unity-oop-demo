# Scene 5 — Polymorphism

**Build it:** OOP Demo > Build Scene 5 - Polymorphism

A ring of enemies around you: three ground skeletons, three flying mages, one big boss. Walk
toward them. The skeletons walk. The mages float. The boss stands still, then charges, then
rests. A counter in the corner says how many are alive.

## Start with Horde.cs

Skip the spawning. Find `Update`. It's four lines:

```csharp
foreach (Enemy e in enemies)
{
    if (!e.IsDead)
        e.Move();
}
```

That's every enemy in the scene moving, and there are three different ways to move. The loop
doesn't know that. It has a `List<Enemy>` and calls `Move` on each. A ground skeleton runs
`GroundEnemy.Move`. A mage runs `FlyingEnemy.Move`. The boss runs `BossEnemy.Move`. Same line
of code, three behaviours.

That's **polymorphism**: many shapes behind one name. You write the call once, against the parent
type, and each object supplies its own answer.

## Why this is the payoff

Scenes 2 and 3 were about organisation: put things in the right box, guard the box. Useful, but you
could imagine living without it.

This scene is different. To add a fourth kind of enemy, you write one new class with its own
`Move`, add it to the list, and *nothing else changes*. Not the loop, not the hero, not the
counter. Without polymorphism the loop would be a growing pile of `if (e is FlyingEnemy) ...
else if (e is BossEnemy) ...` that you'd edit every time. Scene 1's copy-paste problem was
coming back through a side door. This closes it.

## Notice in the files

- `Enemy.cs` is now `abstract`. You can't make a plain Enemy. `Move` is `abstract` too: no default
  body, every child *must* write one. The compiler enforces it.
- `Enemy.cs` has **no `Update`**. It doesn't move itself any more. The Horde moves it. Compare with
  scene 4, where each enemy called its own `Move`. Both work. This version makes the loop visible.
- `BossEnemy` inherits from `GroundEnemy`, not `Enemy`. When it isn't charging it calls
  `base.Move()`, which is the ground skeleton's walk. Grandchild reusing the parent's parent.
- Select the **Horde** object while playing and expand **Enemies** in the Inspector. One list.
  Mixed types. You can see it.

## The Unity tie-in

You've been on the receiving end of this loop since your first script. Unity keeps a list of every
`MonoBehaviour` in the scene and calls `Update` on each one, every frame. It doesn't know your
class exists. It knows "MonoBehaviour with an Update", and your object supplies its own.

And `GetComponent<T>()` is the same trick from the other side. Look at `Hero.cs`:

```csharp
var enemy = hit.collider.GetComponentInParent<Enemy>();
```

`Enemy` is abstract. Nothing in the scene is exactly an Enemy. This still finds every skeleton,
mage and boss, because each of them *is an* Enemy. Ask for the parent type, get whichever child is
actually there, call `TakeDamage` on it. The hero doesn't have three attack methods. It has one.

## Try this

1. Change the counts on the Horde: 0 ground, 6 flying, 2 bosses. Play. The loop didn't change.
2. Write `class ZigZagEnemy : GroundEnemy` that overrides `Move` to call `base.Move()` and then
   shuffles sideways. Make a prefab from SkeletonMinion with it and a HealthBar, add a
   `zigzagPrefab` field to Horde, spawn some. Count how many lines you touched outside your new class.
3. In `Horde.Update`, temporarily replace the loop body with
   `Debug.Log(e.name + " is a " + e.GetType().Name)`. Play for one frame. That's the list telling
   you what each thing really is, even though the variable type is just `Enemy`.

## Think about

- The list is `List<Enemy>`. Could it be `List<GroundEnemy>`? What would break?
- Could the Horde call `e.FallToGround()`? Why not, and what would you have to change to allow it?
- Where else in Unity does "ask for the parent type, get the child" happen? (`Collider`, `Renderer`...)
