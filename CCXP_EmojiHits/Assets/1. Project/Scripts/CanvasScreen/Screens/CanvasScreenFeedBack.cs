using TMPro;
using UnityEngine;

public class CanvasScreenFeedBack : CanvasScreen
{

    [SerializeField] private bool isCorrectFeedback = false;
    [SerializeField] private TMP_Text feedbackText;
    public override void TurnOn()
    {
        MusicControllerEvents.OnUserAwnser += HandleUserAnswer;
        base.TurnOn();
    }

    public override void TurnOff()
    {
        MusicControllerEvents.OnUserAwnser -= HandleUserAnswer;
        base.TurnOff();
    }

    private void HandleUserAnswer(bool isCorrect)
    {
        isCorrectFeedback = isCorrect;
        UpdateFeedbackText();
    }
    private void UpdateFeedbackText()
    {
        if (feedbackText == null)
        {
            Debug.LogWarning("[CanvasScreenFeedBack] feedbackText nao esta atribuido.");
            return;
        }

        feedbackText.text = isCorrectFeedback ? "Resposta Correta!" : "Incorreta!";
    }
}
