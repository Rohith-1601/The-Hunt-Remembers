using UnityEngine;

public class EmergencySignalController : MonoBehaviour
{
    [Header("Signal Feedback")]
    [SerializeField] private GameObject[] objectsToActivate;
    [SerializeField] private GameObject[] objectsToDeactivate;

    [Header("Audio")]
    [SerializeField] private AudioSource signalAudio;

    public void SendSignal()
    {
        if (objectsToActivate != null)
        {
            foreach (GameObject target in objectsToActivate)
            {
                if (target != null)
                {
                    target.SetActive(true);
                }
            }
        }

        if (objectsToDeactivate != null)
        {
            foreach (GameObject target in objectsToDeactivate)
            {
                if (target != null)
                {
                    target.SetActive(false);
                }
            }
        }

        if (signalAudio != null)
        {
            signalAudio.Play();
        }

        Debug.Log("EMERGENCY SIGNAL SENT");
    }
}