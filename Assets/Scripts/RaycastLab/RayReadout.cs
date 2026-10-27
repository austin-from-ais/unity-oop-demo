using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// RAYCAST LAB 2 to 5 - ASKING THE INTERACTOR WHAT IT SEES.
//
// The XRRayInteractor does the raycast for you, but you can still ask it what it found.
// This script asks every frame and writes the answer on a label floating over the hand.
//
// Why bother: in the labs you will aim at things that do NOT light up, and the first question
// is always "did the ray even reach it?" This label answers that. Three cases:
//
//   hit a collider, and the toolkit is hovering it     -> it is an interactable on a matching layer
//   hit a collider, but no hover                       -> a plain collider, or wrong interaction layer
//   hit nothing                                        -> out of range, or the object is on a layer
//                                                         the raycast mask skips (Lab 3)
//
// There are two separate "what did I hit" calls, because the toolkit runs two raycasts:
//   TryGetCurrent3DRaycastHit    the physics world: colliders
//   TryGetCurrentUIRaycastResult the UI world: buttons on a canvas (Lab 4)

namespace RaycastLab
{
    public class RayReadout : MonoBehaviour
    {
        [Tooltip("The interactor to ask. Usually the one on the same hand.")]
        public XRRayInteractor interactor;

        [Tooltip("Where to write the answer.")]
        public TextMesh readout;

        void Update()
        {
            if (interactor == null || readout == null) return;

            string text;

            if (interactor.TryGetCurrentUIRaycastResult(out var ui))
            {
                // A UI hit is reported first because XRI lets UI win over the 3D world behind it.
                text = "UI: " + ui.gameObject.name + "\n" + ui.distance.ToString("0.00") + " m";
            }
            else if (interactor.TryGetCurrent3DRaycastHit(out var hit))
            {
                text = hit.collider.gameObject.name + "\n" + hit.distance.ToString("0.00") + " m";
            }
            else
            {
                text = "(nothing)";
            }

            // hasHover / hasSelection are the toolkit's own view: did the hit turn into an interaction?
            if (interactor.hasSelection)  text += "\nSELECTED";
            else if (interactor.hasHover) text += "\nhover";
            else                          text += "\nno interaction";

            readout.text = text;
        }
    }
}
