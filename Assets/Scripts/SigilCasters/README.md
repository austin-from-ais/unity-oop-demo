# Sigil Casters — putting the pieces together

One scene that uses everything from the labs at once. You walk up to a block, draw a shape in
the air with the right controller, the $1 recogniser names it, you point at a block and press
grip, and the block reacts. Power the generator and the door opens.

| Menu item | Scene |
|---|---|
| Sigil Lab → Build Sigil Casters Scene | `Scenes/SigilCasters` |

## Play it

Put on the headset (a Quest 3 over Link) and press Play. No headset? The keyboard simulator
takes over; click the Game view first.

| Do this | With the headset | With the simulator |
|---|---|---|
| Walk | left thumbstick | hold `[`, then the thumbstick keys (see the simulator's on-screen help) |
| Draw a sigil | hold the **right trigger**, move the hand, let go | hold `]`, hold `T`, move the mouse, release `T` |
| Cast | point the right hand, press **grip** | hold `]`, aim with right mouse, press `G` |

1. Draw a **circle**. The Console prints the leaderboard and "Levitate loaded". The aim line
   turns from faint to red (loaded, not aimed) or green (aimed at a block).
2. Point at the **crate**, press grip. It floats up and hangs there.
3. Draw a **zigzag**. Point at the **generator**, press grip. It turns yellow, and the door slides up.
4. Draw a **triangle**. "A shape I know, but no spell is bound to it." Point at a wall with a spell
   loaded: it fizzles. Scribble: "not confident enough".

If your circles keep scoring 0.6, select **Sigil Recognizer** and lower **Confidence Threshold**
while playing. Then draw a scribble and see what it takes to get a false positive.

## The four building blocks, in the order they happen

| Part | File | Lab it comes from | What it does |
|---|---|---|---|
| 1 | `StickWalk.cs` | Assignment 3 (Flying) | Left stick moves the XR Origin along the head's flattened forward. |
| 2 | `SigilDrawer.cs` | Sigil Lab | Trigger held = record hand positions. Trigger up = flatten onto a page facing the head, call the recogniser, tell the caster the shape's name. |
| 3 | `SpellCaster.cs` | Raycast Lab 1 | Holds the loaded spell. Grip = `Physics.Raycast` from the hand; if the hit has a `SpellTarget`, `spell.Apply(target)`. |
| 4 | `Spell.cs`, `LevitateSpell.cs`, `ElectricSpell.cs`, `SpellTarget.cs`, `PoweredDoor.cs` | Scenes 3 to 6 | What a spell is (abstract class), two spells (polymorphism), what a block hands over (composition, encapsulation), and a door that watches a block. |

Unchanged and reused as-is: `Scripts/Sigils/DollarOneRecognizer.cs`, `SigilRecognizer.cs`,
`SigilTemplates.cs`, and `Scripts/Assignment3/HeadsetOrSimulator.cs`.

## How the parts talk to each other

```
 left stick ──> StickWalk ──> moves XR Origin                       (nobody else cares)

 right trigger ──> SigilDrawer ──(List<Vector2>)──> SigilRecognizer.Recognize
                       │                                   │
                       └───────(shape NAME, if confident)<─┘
                                         │
                                         v
 right grip ──> SpellCaster:  name ──> table ──> Spell ──> Physics.Raycast ──> SpellTarget
                                                            │
                                                            └──> spell.Apply(target)
                                                                   LevitateSpell: Rigidbody
                                                                   ElectricSpell: Renderer + Power()
                                                                                        │
                                                            PoweredDoor watches IsPowered <┘
```

Each arrow is one method call or one field. The drawer knows nothing about spells. The spells
know nothing about VR. The door knows nothing about either. That is the whole design.

## Simple vs robust

Every script here makes the simple, readable choice on purpose and says so in a
`SIMPLE vs ROBUST` block at the top. Read those blocks. For your own project, each one is a
place you may want the robust version instead: collision for the walker, an event instead of a
direct reference from the drawer, a ScriptableObject spell table, a spell that wears off.

## Things to try

- Bind `Triangle` to a new `ShrinkSpell` (scale the block to half). Three edits: a class, one
  line in the table in `SpellCaster.cs`, and nothing else.
- Make `LevitateSpell` wear off after 3 seconds. Where does the timer live, the spell or the block?
- Add a second generator and make the door need both. What does `PoweredDoor` look like now?
- Replace the direct `caster.Load(...)` call in `SigilDrawer` with a `UnityEvent<string>` and
  wire it up in the Inspector. Then add a sound that listens to the same event.
