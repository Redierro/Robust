using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class RaycastPickupLogic : NetworkBehaviour
{
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private LayerMask itemLayer;

    void Update()
    {
        if (!isLocalPlayer) return;

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.green);

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, itemLayer))
            {
                if (hit.collider.gameObject.TryGetComponent(out ItemPickup pickupItem))
                {
                    if (pickupItem == null) { Debug.LogError("Couldnt find pickupitem script on object."); return; }

                    pickupItem.CmdPickupItem(gameObject);
                }
            }
        }
    }
}
