using UnityEngine;
using UnityEngine.UI;

public class RaceUI : MonoBehaviour
{
    public Text playerSpeedText;
    public Text playerLapText;
    public Text playerBestText;
    public Text rankText;

    public CarController playerCar;
    public LapManager lapManager;

    private void Update()
    {
        if (playerCar != null && playerSpeedText != null)
            playerSpeedText.text = "Speed: " + playerCar.CurrentSpeedKmh().ToString("F0") + " km/h";

        if (lapManager != null)
        {
            if (playerLapText != null)
                playerLapText.text = "Lap: " + lapManager.currentLap;

            if (playerBestText != null)
                playerBestText.text = "Best: " + (lapManager.bestLapTime == Mathf.Infinity ? "--" : lapManager.bestLapTime.ToString("F1") + "s");
        }
    }
}
