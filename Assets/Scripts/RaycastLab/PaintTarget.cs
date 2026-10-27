using UnityEngine;
using UnityEngine.UI;

// RAYCAST LAB 4 - WHAT A BUTTON CALLS.
//
// This is the receiving end of a canvas button. There is nothing VR about it. A Button's
// OnClick event calls a public method on some script, and this is that script. The same
// script would work with a mouse in a flat game.
//
// That is the point of Lab 4: once the ray can "click", everything you already know about
// Unity UI works unchanged. The VR part is only three components that turn a ray into a click:
//
//   EventSystem + XRUIInputModule          on the EventSystem object. Replaces the mouse input module.
//   TrackedDeviceGraphicRaycaster          on the Canvas. Lets a ray in the world hit the canvas.
//   XRRayInteractor "UI Interaction" on    on the hand. The ray also raycasts against UI.
//
// A canvas that only has the ordinary GraphicRaycaster is invisible to the ray. The scene
// has one of those too, on the left. Aim at it and press the trigger. Nothing. Now you know
// the first thing to check when a VR button does not click.

namespace RaycastLab
{
    public class PaintTarget : MonoBehaviour
    {
        [Tooltip("The object the buttons paint.")]
        public Renderer target;

        [Tooltip("A UI Text that counts the clicks. Optional.")]
        public Text counter;

        int clicks;

        // Each of these is wired to a Button's OnClick in the Inspector (the builder did it for you).
        public void PaintRed()   { Paint(new Color(0.9f, 0.25f, 0.2f)); }
        public void PaintGreen() { Paint(new Color(0.3f, 0.8f, 0.35f)); }
        public void PaintBlue()  { Paint(new Color(0.25f, 0.5f, 0.95f)); }

        void Paint(Color color)
        {
            clicks++;
            if (target != null) target.material.color = color;
            if (counter != null) counter.text = "Clicks: " + clicks;
            Debug.Log("[Canvas] button clicked, " + clicks + " so far");
        }
    }
}
