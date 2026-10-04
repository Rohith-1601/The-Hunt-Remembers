using System.Collections;
using UnityEngine;
using TMPro;

public class HunterAlertUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text alertText;

    [Header("Display")]
    [SerializeField] private float displayDuration = 1f;

    private Coroutine displayCoroutine;

    private void Awake()
    {
        if (alertText == null)
        {
            Debug.LogError(
                "HunterAlertUI: Alert Text is not assigned.",
                this
            );

            enabled = false;
            return;
        }

        alertText.text = "HEARD";
        alertText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        HunterAI.OnHunterHeardSomething += HandleHunterHeard;
    }

    private void OnDisable()
    {
        HunterAI.OnHunterHeardSomething -= HandleHunterHeard;
    }

    private void HandleHunterHeard(
        Vector3 soundPosition,
        float loudness)
    {
        Debug.Log(
            $"UI: Hunter heard player. Loudness = {loudness:F1}"
        );

        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }

        displayCoroutine =
            StartCoroutine(ShowAlert());
    }

    private IEnumerator ShowAlert()
    {
        alertText.gameObject.SetActive(true);

        yield return new WaitForSeconds(
            displayDuration
        );

        alertText.gameObject.SetActive(false);

        displayCoroutine = null;
    }
}