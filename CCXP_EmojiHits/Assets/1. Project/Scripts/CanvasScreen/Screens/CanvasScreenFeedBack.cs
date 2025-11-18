using UnityEngine;

public class CanvasScreenFeedBack : CanvasScreen
{

    [SerializeField] private bool isCorrectFeedback = false;
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
    }
}
