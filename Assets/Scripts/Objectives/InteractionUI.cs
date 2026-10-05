using UnityEngine;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text interactionText;

    private ObjectiveInteractable[] interactables;

    private void Awake()
    {
        if (interactionText == null)
        {
            Debug.LogError(
                "InteractionUI: Interaction Text is not assigned.",
                this
            );

            enabled = false;
            return;
        }

        interactionText.gameObject.SetActive(false);
    }

    private void Start()
    {
        interactables =
            FindObjectsOfType<ObjectiveInteractable>();
    }

    private void Update()
    {
        if (interactables == null ||
            interactables.Length == 0)
        {
            interactionText.gameObject.SetActive(false);
            return;
        }

        ObjectiveInteractable bestInteractable = null;

        foreach (ObjectiveInteractable interactable in interactables)
        {
            if (interactable == null)
                continue;

            if (!interactable.gameObject.activeInHierarchy)
                continue;

            if (!interactable.PlayerInRange)
                continue;

            bestInteractable = interactable;
            break;
        }

        if (bestInteractable != null)
        {
            interactionText.text =
                bestInteractable.GetPrompt();

            interactionText.gameObject.SetActive(true);
        }
        else
        {
            interactionText.gameObject.SetActive(false);
        }
    }
}