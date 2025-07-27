using UnityEngine;
using Mirror;

public class RaycastInteractionHandler : NetworkBehaviour
{
    [SerializeField] private float range = 3f;
    [SerializeField] private LayerMask interactionLayer;
    private IInteractable lastInteractable;
    private InteractableUIManager uiManager;

    void Start()
    {
        if (!isLocalPlayer) return;
        uiManager = GetComponent<InteractableUIManager>();
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * range, Color.green);

        if (Physics.Raycast(ray, out RaycastHit hit, range, interactionLayer))
        {
            var target = hit.collider.gameObject;

            if (target.TryGetComponent(out IInteractable interactable))
            {
                if (lastInteractable != interactable)
                {
                    uiManager?.ShowPrompt(interactable.GetInteractionText(), hit.collider.transform);
                    lastInteractable = interactable;
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.Interact(gameObject);
                    NetworkServer.Destroy(target.gameObject);
                }

                return;
            }
        }

        uiManager?.HidePrompt();
        lastInteractable = null;
    }
}
