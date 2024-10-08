using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatBar : MonoBehaviour
{
    public Image healthImage;
    public Image healthDelayImage;
    public Image powerImage;

    private bool isRecoveringPower;
    private Character currentCharacter;

    private void Update()
    {
        if(healthDelayImage.fillAmount > healthImage.fillAmount)
        {
            healthDelayImage.fillAmount -= Time.deltaTime * 0.7f;
        }

        if (isRecoveringPower)
        {
            float percentage = currentCharacter.currentPower / currentCharacter.maxPower;
            powerImage.fillAmount = percentage;

            if (percentage >= 1)
            {
                isRecoveringPower = false;
                return;
            }
        }
    }

    /// <summary>
    /// receive the percentage of health
    /// </summary>
    /// <param name="percentage">health percentage = current/max </param>
    public void OnHealthChange(float percentage)
    {
        healthImage.fillAmount = percentage;
    }

    public void OnPowerChange(Character character)
    {
        isRecoveringPower = true;
        currentCharacter = character;

    }
}
