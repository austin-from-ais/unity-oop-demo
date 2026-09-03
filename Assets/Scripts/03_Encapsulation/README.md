# Scene 3 — Encapsulation

**Build it:** OOP Demo > Build Scene 3 - Encapsulation

This scene is **broken on purpose**. Your job is to find out why, then fix it.

The scripts in this folder are the broken starting point. `Solution/` has the fixed versions.
Try not to look at those until you've had a go.

## What's in the scene

- **Skeleton A** is standing in a green poison puddle (`PoisonCloud.cs`).
- **Skeleton B** is standing at a gold healing shrine (`HealingShrine.cs`).
- **Skeleton C** is out in the open.
- **Debug Cheats** (`DebugCheats.cs`): press **K** to kill everything, **H** to heal everything.
- The hero is the same as scene 2.

## See the bug

1. Press Play. Don't click anything. Select **Skeleton A** and watch **Health** in the Inspector.
   It goes 95, 90, 85... and the bar above its head doesn't move. It reaches 0 and keeps going.
   -5, -10, -50. The skeleton is standing there, idle, at negative health.
2. Click Skeleton A. The hero walks over and then does nothing. Look at `Hero.cs`: it refuses to
   attack anything where `IsDead` is true. The skeleton is dead *and* standing up.
3. Select **Skeleton B**. Health: 120, 140, 160. Max health is 100. What does 160/100 mean?
4. Press **K**. Every skeleton is now at -50. None of them fell over. Press **H**. All at 999.

## Find the cause

`Enemy.cs` has a perfectly good `TakeDamage` method with a clamp in it. Read it. It can't
produce -50. So something else is writing to `health` without going through `TakeDamage`.

There are four scripts in this folder. Which ones? You could search for `.health`. Or you could
let the compiler do it for you:

Open `Enemy.cs` and change

```csharp
public int health;
```

to

```csharp
private int health;
```

Save, go back to Unity, and look at the Console. Every red line is a script that was reaching in.

## Fix it

Each error is a one-line change to go through the gate instead of around it:

| Script | Was | Should be |
|---|---|---|
| `PoisonCloud` | `enemy.health -= x` | `enemy.TakeDamage(x)` |
| `HealingShrine` | `enemy.health += x` | `enemy.Heal(x)` — you'll need to write `Heal`, with a cap at `maxHealth` inside it |
| `DebugCheats` | `enemy.health = -50` | `enemy.Kill()` — write this one too |
| `DebugCheats` | `enemy.health = 999` | `enemy.Heal(enemy.MaxHealth)` — needs a read-only `MaxHealth` property |

Play again. Bars move. Skeletons in poison die. The shrine tops out at 100. The cheats can't put
an enemy into a state that isn't real.

## Why every Unity tutorial writes `[SerializeField] private`

Now look at the other fields. Should `maxHealth` be public? No script should ever set it. But the
designer needs to tune it in the Inspector, and `private` hides it from the Inspector too.

| | other scripts can write it | Inspector can edit it |
|---|---|---|
| `public int x` | yes | yes |
| `private int x` | no | no |
| `[SerializeField] private int x` | **no** | **yes** |

That third row is the one you want for almost every tunable number in a game. You've probably typed
it a hundred times following tutorials. This is why.

Finish by making every field in `Enemy.cs` private, and adding read-only properties like
`public int Health => health;` so other scripts can still *look* without being able to *touch*.
`Solution/Enemy.cs` shows the finished version.

## Think about

- After the fix, can any future script, written by anyone, make health negative? Not "unlikely". Can it?
- You want to play a sound whenever an enemy takes damage from anything. Where's the one line to add?
- What did making `health` private cost? Count the edits.
