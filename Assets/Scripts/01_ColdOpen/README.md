# Scene 1 — Cold open

**Build it:** OOP Demo > Build Scene 1 - Cold Open

A knight, a skeleton, a health bar. Click the skeleton and the knight walks over and starts
swinging. The skeleton has nothing to defend itself with, so it just takes the hits until it dies.

That's the whole scene. It works. Nothing here is wrong yet.

## Read these two files, in this order

**`SkeletonEnemy.cs`** — Select the Skeleton in the Hierarchy and look at the Inspector while
you read. Every line in this file is a variable. Max health, current health, defence, a couple of
references. There is no code that *does* anything. It's a bag of numbers.

**`HeroController.cs`** — Most of this is walking and clicking. Skip to the method called
`LandHit` at the bottom. That's where the hit connects. Read it slowly and count how many times
the word `skeleton.` appears.

## What to notice

The hero does the skeleton's bookkeeping *for* it:

- subtracts the skeleton's health
- updates the skeleton's health bar
- plays the skeleton's hurt or death animation
- switches off the skeleton's collider

The skeleton never does anything to itself. Anything that wants to damage a skeleton has to know
all four of those steps and do them in the right order.

## Try this

- While the game is running, select the Skeleton and change **Max Health** or **Defence** in the
  Inspector. Change **Damage** or **Attack Interval** on the Adventurer. Get a feel for it.
- Now imagine adding a poison trap that also damages skeletons. Where does its code go? What does
  it need to know about skeletons to get it right?
- Imagine adding a second kind of enemy, a goblin, with its own script. What happens to `LandHit`?

Keep those answers in mind. Scene 2 is the fix for the first one.
