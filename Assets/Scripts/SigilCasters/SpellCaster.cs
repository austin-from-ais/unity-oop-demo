using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// SIGIL CASTERS, PART 3 - AIMING AND FIRING
//
// In one line: hold a spell, point the controller, press grip, and the first block the ray hits
// gets the spell.
//
// This is Raycast Lab 1 again. The question is the same one: "start at the hand, go the way it
// points, what is the first collider within N meters?" The only new part is what we do with the
// answer: look for a SpellTarget component on it and hand it the spell.
//
// Two buttons, two jobs, the same split as the Raycast Lab:
//   trigger   draws   (Part 2, SigilDrawer)
//   grip      casts   (this script)
// If both lived on the trigger, letting go after a stroke would also fire the spell at wherever
// the hand happened to be pointing at that instant.
//
// WHERE SPELLS COME FROM. The drawer only knows shape NAMES ("Circle"). This script owns a small
// table: shape name -> Spell object. That table is the ONLY place in the game that says "a circle
// means levitate". To add a spell: a shape in SigilTemplates.cs, a Spell class in this folder,
// and one line in the table. Nothing else changes. That is the payoff of Part 4.
//
// SIMPLE vs ROBUST:
//   - The table is hard-coded in C#. The robust version is a ScriptableObject edited in the
//     Inspector, so a designer can rebind shapes to spells without touching code.
//   - Physics.Raycast by hand, not XRRayInteractor. One line instead of three components, and
//     the whole data flow stays readable in this file. The toolkit version listens for
//     selectEntered on an XRSimpleInteractable instead (Raycast Lab 2).
//   - GetComponent<SpellTarget>() on the collider's own object. If a block kept its collider on
//     a child object this would miss; GetComponentInParent is the robust call.
//   - A spell is used up when cast, hit or miss. No cooldowns, no mana, no "repeat last spell".

namespace SigilCasters
{
    [RequireComponent(typeof(LineRenderer))]
    public class SpellCaster : MonoBehaviour
    {
        [Tooltip("The grip button, as an Input System action. The scene builder binds it to <XRController>{RightHand}/{GripButton}.")]
        public InputActionProperty castButton;

        [Tooltip("How far a spell can reach, in meters.")]
        public float maxDistance = 10f;

        [Tooltip("Read-only, so you can watch it in the Inspector: the spell currently loaded, or blank.")]
        public string loadedSpellName = "";

        [Header("Aim line colors")]
        public Color unloadedColor = new Color(1f, 1f, 1f, 0.2f);   // faint: nothing to cast yet
        public Color missColor     = new Color(1f, 0.3f, 0.3f);     // loaded, but not pointing at a target
        public Color hitColor      = new Color(0.3f, 1f, 0.4f);     // loaded and pointing at a target

        // THE TABLE. Shape name, spelled exactly as SigilTemplates.cs spells it, -> the spell.
        // One Spell object per kind is enough, because a spell keeps no per-cast state.
        readonly Dictionary<string, Spell> spellsByShape = new Dictionary<string, Spell>
        {
            { "Circle", new LevitateSpell() },
            { "Zigzag", new ElectricSpell()  },
        };

        Spell loaded;          // the spell in the hand right now. null = nothing loaded.
        LineRenderer aimLine;  // a ray has no visual of its own, so we draw one

        void Awake()
        {
            aimLine = GetComponent<LineRenderer>();
            aimLine.positionCount = 2;      // a straight line: start and end
            aimLine.useWorldSpace = true;
        }

        void OnEnable()  => castButton.action.Enable();
        void OnDisable() => castButton.action.Disable();

        /// Called by SigilDrawer when it is sure what shape it saw. Look the name up in the table.
        public void Load(string shapeName)
        {
            if (spellsByShape.TryGetValue(shapeName, out Spell spell))
            {
                loaded = spell;
                loadedSpellName = spell.Name;
                Debug.Log("[Spell Caster] " + shapeName + " -> " + spell.Name + " loaded. Aim and press grip.");
            }
            else
            {
                Debug.Log("[Spell Caster] " + shapeName + " is a shape I know, but no spell is bound to it.");
            }
        }

        void Update()
        {
            // 1. The question, every frame, exactly as in Raycast Lab 1.
            Vector3 origin    = transform.position;
            Vector3 direction = transform.forward;
            RaycastHit hit;
            bool hitSomething = Physics.Raycast(origin, direction, out hit, maxDistance);

            // 2. Is what we hit something a spell can land on? Only read 'hit' if the raycast
            //    returned true: otherwise its fields are leftovers from some earlier frame.
            SpellTarget target = hitSomething ? hit.collider.GetComponent<SpellTarget>() : null;

            // 3. Draw the aim line, colored by what would happen if you pressed grip right now.
            Vector3 end = hitSomething ? hit.point : origin + direction * maxDistance;
            aimLine.SetPosition(0, origin);
            aimLine.SetPosition(1, end);
            Color color = loaded == null ? unloadedColor : (target != null ? hitColor : missColor);
            aimLine.startColor = aimLine.endColor = color;

            // 4. Grip pressed this frame? If not, we are done.
            if (!castButton.action.WasPressedThisFrame()) return;

            if (loaded == null)
            {
                Debug.Log("[Spell Caster] Nothing loaded. Draw a sigil first.");
                return;
            }

            if (target == null)
            {
                Debug.Log("[Spell Caster] " + loaded.Name + " fizzles: hit "
                          + (hitSomething ? hit.collider.gameObject.name + ", which is not a SpellTarget" : "nothing") + ".");
            }
            else
            {
                // 5. THE CALL. One line, and we do not know or care which spell this is.
                //    LevitateSpell does one thing here, ElectricSpell another. Scene 5, polymorphism.
                loaded.Apply(target);
                Debug.Log("[Spell Caster] " + loaded.Name + " hits " + target.gameObject.name + ".");
            }

            // Used up either way.
            loaded = null;
            loadedSpellName = "";
        }
    }
}
