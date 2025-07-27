using UnityEngine;
using TMPro;

public class InteractableUIManager : MonoBehaviour
{
    public GameObject promptPrefab;
    private GameObject currentPrompt;
    private TMP_Text promptText;

    public void ShowPrompt(string text, Transform target)
    {
        if (currentPrompt == null)
        {
            currentPrompt = Instantiate(promptPrefab);
            promptText = currentPrompt.GetComponentInChildren<TMP_Text>();
        }

        promptText.text = text;
        currentPrompt.transform.position = target.position + Vector3.up * 1.5f;
        currentPrompt.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        currentPrompt.SetActive(true);
    }

    public void HidePrompt()
    {
        if (currentPrompt != null)
            currentPrompt.SetActive(false);
    }
}
