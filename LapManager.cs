using UnityEngine;
using UnityEngine.UI;

public class LapManager : MonoBehaviour
{
    public string playerTag = "Player";
    public Text lapText;
    public Text timerText;
    public Text bestText;
    public Text speedText;
    public CarController carController;

    public int currentLap = 1;
    public float currentLapTime = 0f;
    public float bestLapTime = Mathf.Infinity;

    private bool started = false;
    private bool insideTrigger = false;

    private void Start()
    {
        if (carController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
                carController = player.GetComponent<CarController>();
        }
    }

    private void Update()
    {
        if (started)
            currentLapTime += Time.deltaTime;

        if (lapText != null)
            lapText.text = "Lap: " + currentLap;

        if (timerText != null)
            timerText.text = "Time: " + currentLapTime.ToString("F1") + "s";

        if (bestText != null)
            bestText.text = "Best: " + (bestLapTime == Mathf.Infinity ? "--" : bestLapTime.ToString("F1") + "s");

        if (speedText != null && carController != null)
            speedText.text = "Speed: " + carController.CurrentSpeedKmh().ToString("F0") + " km/h";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        if (insideTrigger)
            return;

        insideTrigger = true;

        if (!started)
        {
            started = true;
            return;
        }

        if (currentLapTime < bestLapTime)
            bestLapTime = currentLapTime;

        currentLap++;
        currentLapTime = 0f;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        insideTrigger = false;
    }
}
