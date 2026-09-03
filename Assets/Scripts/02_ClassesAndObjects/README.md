# Scene 2 — Classes and objects

**Build it:** OOP Demo > Build Scene 2 - Classes and Objects

Same arena, same controls. Two skeletons this time, and an Enemy Spawner that's switched off.

## What changed from scene 1

Open `Enemy.cs`. The fields at the top are the same as `SkeletonEnemy` had. What's new is
underneath them: a method called `TakeDamage`. Everything the hero used to do to the skeleton in
scene 1 now lives *inside the skeleton*, in that one method.

Now open `Hero.cs` and find `LandHit`. It's one line: `enemy.TakeDamage(damage)`.

The hero no longer knows how a skeleton dies. It asks the skeleton to take damage and the skeleton
handles the rest. That's the whole idea of a class: the data and the code that works on that data,
kept together in one place.

## Blueprint and house

Select **Skeleton A** in the Hierarchy, then **Skeleton B**. Same component, `Enemy`. Different
numbers: A has 100 health and no defence, B has 300 health and 5 defence.

`Enemy` (the file) is a blueprint. Skeleton A is a house built from it. Skeleton B is another
house. Changing B's health doesn't touch A, and neither of them *is* the blueprint.

The word for a house is an **object**, or an **instance**. Unity makes this unusually visible:
one script, dragged onto two GameObjects, shows up in two Inspectors with two sets of values.

## Try this

1. Press Play. Click Skeleton A, kill it. Click Skeleton B and notice it's slower to kill.
   Its defence is subtracting from every hit. Find where in `TakeDamage`.
2. Stop. Select Skeleton A and press **Ctrl+D**. You've added an enemy. No code.
3. Tick the checkbox on the **Enemy Spawner** component and press Play. Ten skeletons appear in a
   ring, each with its own random health. Open `EnemySpawner.cs` and find the loop. The line with
   `Instantiate` is the one that builds an enemy. Compare that with what "add an enemy" would have
   meant in scene 1.
4. Change **Count** to 30 and play again.

## Think about

- Where does Skeleton B's current health live? In the `Enemy` file? In Skeleton B? In the hero?
- If you change the default `maxHealth = 100` in `Enemy.cs`, does Skeleton B's 300 change?
- What does the hero know about skeletons now? Try to list it. It should be a short list.
