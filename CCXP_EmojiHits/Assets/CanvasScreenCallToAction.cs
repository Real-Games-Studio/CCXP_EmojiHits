using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class CanvasScreenCallToAction : CanvasScreen
{
    [SerializeField] private GameObject timerToStartObject;
    [SerializeField] private List<GameObject>  timerToStartobjects; // telas de 3,2,1

    private bool isCountingDown = false;

    public override void TurnOn()
    {
        foreach (var obj in timerToStartobjects)
        {
            obj.SetActive(false);
        }

        timerToStartObject.SetActive(false);
        base.TurnOn();
    }

    public void ShowTimerToStart()
    {
        if (timerToStartObject != null)
        {
            timerToStartObject.SetActive(true);
        }
    }


    public void StartTimerCountdown()
    {
        if (isCountingDown)
        {
            Debug.LogWarning("Contagem regressiva ja esta em andamento.");
            return;
        }
        Debug.Log("Iniciando contagem regressiva...");
        StartCoroutine(TimerCountdownCoroutine());
    }

    private IEnumerator TimerCountdownCoroutine()
    {
        isCountingDown = true;
        timerToStartObject.SetActive(true);
        foreach (var obj in timerToStartobjects)
        {
            obj.SetActive(true);
            yield return new WaitForSeconds(1f);
            obj.SetActive(false);
        }
    
        CallNextScreen();
        isCountingDown = false;
    }
}
