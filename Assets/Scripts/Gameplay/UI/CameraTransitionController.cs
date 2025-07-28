using UnityEngine;
using System.Collections;

public class CameraTransitionController : MonoBehaviour
{
    public Transform defaultTarget;
    public Transform inventoryTarget;
    public float transitionDuration = 0.5f;

    private Coroutine currentTransition;

    public void TransitionToInventory()
    {
        if (currentTransition != null) StopCoroutine(currentTransition);
        currentTransition = StartCoroutine(AnimateCameraTransition(inventoryTarget));
    }

    public void TransitionToDefault()
    {
        if (currentTransition != null) StopCoroutine(currentTransition);
        currentTransition = StartCoroutine(AnimateCameraTransition(defaultTarget));
    }

    private IEnumerator AnimateCameraTransition(Transform target)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        while (elapsed < transitionDuration)
        {
            float t = elapsed / transitionDuration;
            transform.position = Vector3.Lerp(startPos, target.position, t);
            transform.rotation = Quaternion.Slerp(startRot, target.rotation, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}
