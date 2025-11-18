using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;

public class CanvasScreenGameplay : CanvasScreen
{
    [SerializeField] private float gameplayTime = 60f;
    private float timer;

    [SerializeField] private TMPro.TMP_Text timerText;
    [SerializeField] private UnityEngine.UI.Image progressBarFill;

    [SerializeField] private Image emojiImage;
    [SerializeField] private TMP_Text musicAndAutorText; // Nome da Musica\ Autor

    // Chamada para declarar resposta correta
    public void Button_ClickCorrect()
    {
        MusicControllerEvents.OnUserAwnser?.Invoke(true);
        CallNextScreen();
    }

    // Chamada para declarar resposta errada
    public void Button_ClickWrong()
    {
        MusicControllerEvents.OnUserAwnser?.Invoke(true);
        CallNextScreen();
    }
}
