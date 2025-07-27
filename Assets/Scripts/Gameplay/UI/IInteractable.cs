using UnityEngine;
public interface IInteractable
{
    string GetInteractionText();
    void Interact(GameObject player);
}
