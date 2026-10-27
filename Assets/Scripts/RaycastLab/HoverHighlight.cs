using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// RAYCAST LAB 2 - WHAT THE TOOLKIT TELLS AN OBJECT.
//
// In Lab 1 our own script did the raycast and knew what it hit. The object being hit knew
// nothing. With the XR Interaction Toolkit it is the other way round: the toolkit does the
// raycast, and then it TELLS the object. This script is the object listening.
//
// The three words you need:
//   Interactor     the thing doing the pointing. XRRayInteractor on the controller.
//   Interactable   the thing being pointed at. XRSimpleInteractable (or XRGrabInteractable) on the cube.
//   Manager        XRInteractionManager. Every frame it asks each interactor what it hits, checks
//                  the layers match, and fires the events below on both sides.
//
// An Interactable raises events as the ray comes and goes:
//   hoverEntered   the ray is on me                 (nothing pressed)
//   hoverExited    the ray left me
//   selectEntered  the ray is on me AND the button went down
//   selectExited   the button went up (or the ray left while held)
//
// We colour the object for each state. That is all. No raycast code in here at all.
// Hover is "aiming", select is "holding the button". Grab, click and pull all build on select.

namespace RaycastLab
{
    [RequireComponent(typeof(Renderer))]
    public class HoverHighlight : MonoBehaviour
    {
        public Color hoverColor  = new Color(1f, 0.85f, 0.2f);   // yellow: aimed at
        public Color selectColor = new Color(0.3f, 0.9f, 1f);    // cyan: button held

        Renderer rend;
        Color normalColor;
        XRBaseInteractable interactable;

        void Awake()
        {
            rend = GetComponent<Renderer>();

            // .material (not .sharedMaterial) gives THIS object its own copy, so colouring one cube
            // does not colour every cube that shares the material.
            normalColor = rend.material.color;

            // Any interactable will do: simple, grab, whatever. They all raise the same events.
            interactable = GetComponent<XRBaseInteractable>();
        }

        // Subscribe when switched on, unsubscribe when switched off. (Lifecycle Lab: OnEnable / OnDisable.)
        void OnEnable()
        {
            if (interactable == null) return;
            interactable.hoverEntered.AddListener(OnHoverEntered);
            interactable.hoverExited.AddListener(OnHoverExited);
            interactable.selectEntered.AddListener(OnSelectEntered);
            interactable.selectExited.AddListener(OnSelectExited);
        }

        void OnDisable()
        {
            if (interactable == null) return;
            interactable.hoverEntered.RemoveListener(OnHoverEntered);
            interactable.hoverExited.RemoveListener(OnHoverExited);
            interactable.selectEntered.RemoveListener(OnSelectEntered);
            interactable.selectExited.RemoveListener(OnSelectExited);
        }

        // Each event hands you 'args'. args.interactorObject is WHO did it: which hand, which ray.
        // We only need to know that it happened, but it is there if you want it.

        void OnHoverEntered(HoverEnterEventArgs args)
        {
            // Do not overwrite the select colour if a second ray starts hovering while we are held.
            if (!interactable.isSelected) rend.material.color = hoverColor;
            Debug.Log("[" + name + "] hover entered by " + args.interactorObject.transform.name);
        }

        void OnHoverExited(HoverExitEventArgs args)
        {
            if (!interactable.isSelected && !interactable.isHovered) rend.material.color = normalColor;
        }

        void OnSelectEntered(SelectEnterEventArgs args)
        {
            rend.material.color = selectColor;
            Debug.Log("[" + name + "] SELECTED by " + args.interactorObject.transform.name);
        }

        void OnSelectExited(SelectExitEventArgs args)
        {
            // Button released. Are we still being aimed at? Then back to hover, else back to normal.
            rend.material.color = interactable.isHovered ? hoverColor : normalColor;
        }
    }
}
