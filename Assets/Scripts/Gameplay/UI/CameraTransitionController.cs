using UnityEngine;
using System.Collections;

public class CameraTransitionController : MonoBehaviour
{
    [SerializeField] private Transform defaultTarget;
    [SerializeField] private Transform inventoryTarget;
    public float transitionDuration = 0.5f;

    private Coroutine currentTransition;

    public void TransitionToInventory()
    {
        defaultTarget.transform.rotation = gameObject.transform.rotation;
        if (currentTransition != null) StopCoroutine(currentTransition);
        currentTransition = StartCoroutine(AnimateCameraTransition(inventoryTarget));
        Debug.Log("Moving the camera to inventory.");
    }

    public void TransitionToDefault()
    {
        if (currentTransition != null) StopCoroutine(currentTransition);
        currentTransition = StartCoroutine(AnimateCameraTransition(defaultTarget));
        Debug.Log("Moving the camera back to the original place.");
    }

    private IEnumerator AnimateCameraTransition(Transform target)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        while (elapsed < transitionDuration)
        {
            float t = elapsed / transitionDuration;
            // Apply ease-in-out using a cubic curve
            float easedT = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(startPos, target.position, easedT);
            transform.rotation = Quaternion.Slerp(startRot, target.rotation, easedT);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target.position;
        transform.rotation = target.rotation;
    }

}
