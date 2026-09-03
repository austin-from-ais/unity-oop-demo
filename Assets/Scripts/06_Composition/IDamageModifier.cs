// SCENE 6 - INTERFACES AND COMPOSITION
//
// A second, smaller promise: "give me an amount of damage, I'll give you back what's left."
// Shield and Armour both keep it. Health asks every modifier on the same GameObject, in order,
// without knowing what any of them are. Add a third kind and Health doesn't change.

namespace Lecture06
{
    public interface IDamageModifier
    {
        int Modify(int amount);
    }
}
