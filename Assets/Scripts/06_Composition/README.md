# Scene 6 — Interfaces, and composition over inheritance

**Build it:** OOP Demo > Build Scene 6 - Composition

This time the enemies fight back. You have a health bar now. Skeletons swing at you, mages throw
fireballs, there's a poison puddle that hurts anyone standing in it, and there are wooden crates
you can smash. Some skeletons have shields that soak up damage before it reaches their health.

## First, the problem with more inheritance

Open `Explosion/TheHierarchyExplosion.cs`. Don't use it, just read it.

After scene 5 the obvious next move is more subclasses: `FlyingShootingEnemy`,
`GroundShieldedEnemy`, `FlyingShootingShieldedEnemy`... Two ways to move, two ways to attack, shield
or not: eight classes. Add bosses: sixteen. And because a class can only have *one* parent,
the shooting code gets pasted into every branch that shoots, and the shield code into every
branch that has a shield. Scene 1's copy-paste problem is back.

Inheritance is great for one axis of variation. Real games have five.

## Two answers, used together

### 1. Interfaces: a promise with no code in it

`IDamageable.cs` is four lines. It says: anything that claims to be `IDamageable` has a
`TakeDamage` method and an `IsDead` property. Nothing about health or bars or animations.

Three completely unrelated things keep that promise here:

| Class | What it is | What "damage" means to it |
|---|---|---|
| `Health` | a component you can put on anything | subtract hit points |
| `BreakableCrate` | a cube | count hits, break on the third, ignore the amount |
| *(the player)* | the Adventurer, via its `Health` | same as any skeleton |

They share no parent. Yet `Hero.cs` attacks all of them with one line, because it asks for
`IDamageable` instead of `Enemy`:

```csharp
var damageable = hit.collider.GetComponentInParent<IDamageable>();
```

Yes, `GetComponent` works with interfaces. So does the poison puddle (`DamageZone.cs`) and the
mages' fireballs (`Projectile.cs`). None of them know what a crate is.

There's a second, smaller interface, `IDamageModifier`. `Shield` and `Armour` both implement it.
`Health.TakeDamage` asks every modifier on the same GameObject to adjust the number before
applying it. Add a third modifier tomorrow and `Health` doesn't change.

### 2. Composition: a GameObject is a bag of behaviours

Now look at the Hierarchy. Select **Flying Shooting Shielded Skeleton**. Look at the Inspector.
Its components are:

```
Health
Shield
FlyingMover
Shooter
```

There is no `FlyingShootingShieldedEnemy` class. There's no `Enemy` class at all in this folder.
The thing is *assembled* from small parts that each do one job and know nothing about each other.
`GroundMover` walks. `Shooter` shoots. `Shield` soaks. `Health` dies. Swap a part, get a
different enemy. That's **composition**: build things out of parts, don't describe them with a
family tree.

Now the punchline. This is not a clever trick from this course. This is **Unity**. A GameObject
has always been a bag of components. `Transform`, `Rigidbody`, `Collider`, your scripts: no
taxonomy, just parts on an object. Unity chose composition as its whole architecture, and you've
been using it since your first `AddComponent`. This scene just does it on purpose.

## Which one when

- **Inheritance** when things really are one kind of a thing with a small variation, and the
  hierarchy is shallow. `FlyingEnemy : Enemy` in scene 4 was fine.
- **Interfaces** when unrelated things need to be *treated* the same. Damageable. Interactable.
  Saveable.
- **Composition** for building actual game objects. Almost always. This is the one you'll use.

The mature version of "is a" vs "has a": prefer *has a*.

## Try this

1. Build a new enemy without writing a class. Duplicate **Ground Skeleton**, remove `GroundMover`,
   add `FlyingMover`. Play. You made a flying melee skeleton in ten seconds.
2. Put a `Shield` on a **crate**. Nothing happens to it, and there's no error. Why? (Read
   `BreakableCrate.TakeDamage`. Then read `Health.TakeDamage`. Who asks the modifiers?)
3. Put `Health` and `Shooter` on a crate. Congratulations, it's a turret.
4. Put a `Shield` on the **Adventurer**. You now have one, and neither `Hero.cs` nor `Health.cs`
   was touched.
5. Write a new `IDamageModifier` called `Dodge` that returns 0 for the whole hit 25% of the time.
   Drop it on anything. Count how many existing files you edited: it should be zero.

## Think about

- `Health` calls `GetComponents<IDamageModifier>()`. What would this look like with
  inheritance instead? Where would `Shield` have to live?
- The crate ignores the damage amount. Is that a bug? Who decides what damage *means*?
- Look at `MeleeAttacker.cs`. It finds the player with `FindAnyObjectByType<Hero>()` and then
  asks for `IDamageable`. Could it attack crates instead? What one line would change?
