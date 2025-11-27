using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CanvasScreenGameplay : CanvasScreen
{
    [SerializeField] private float gameplayTime = 60f;
    private float timer;
    private bool isCountingDown;

    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Image progressBarFill;

    [SerializeField] private Image emojiImage;
    [SerializeField] private TMP_Text musicAndAutorText; // Nome da Musica\ Autor
    [SerializeField] private TMP_Text musicAndAutorText2; // Nome da Musica\ Autor
    [SerializeField] private TMP_Text musicAndAutorTextOnFeedbackScreen; // Nome da Musica\ Autor
    [SerializeField] private TMP_Text musicTrecho;


    public override void OnEnable()
    {
        base.OnEnable();
        if (MusicController.Instance != null)
        {
            MusicController.Instance.OnMusicChanged += HandleMusicChanged;
        }

        SyncWithCurrentMusic();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        StopCountdown();
        if (MusicController.Instance != null)
        {
            MusicController.Instance.OnMusicChanged -= HandleMusicChanged;
        }
    }

    public override void TurnOn()
    {
        base.TurnOn();

        var controller = MusicController.Instance;
        if (controller == null)
        {
            Debug.LogWarning("[CanvasScreenGameplay] Nenhum MusicController encontrado ao abrir a tela.");
            ShowLoadingState();
            return;
        }

        ApplyMusicToUI(controller.GetRandomMusic());
        StartCountdown();
    }

    private void HandleMusicChanged(MusicController.MusicData music)
    {
        Debug.Log($"[CanvasScreenGameplay] OnMusicChanged recebido: {music?.musicName ?? "null"}.");
        ApplyMusicToUI(music);
        StartCountdown();
    }

    private void SyncWithCurrentMusic()
    {
        var controller = MusicController.Instance;
        if (controller != null && controller.TryGetCurrentMusic(out var music))
        {
            Debug.Log($"[CanvasScreenGameplay] Sincronizando com musica atual: {music.musicName}.");
            ApplyMusicToUI(music);
            StartCountdown();
        }
        else
        {
            Debug.LogWarning("[CanvasScreenGameplay] Nao foi possivel sincronizar musica atual.");
            StopCountdown();
            ShowLoadingState();
        }
    }

    private void StartCountdown()
    {
        timer = gameplayTime;
        isCountingDown = true;
        UpdateTimerUI();
    }

    private void StopCountdown()
    {
        isCountingDown = false;
    }

    private void Update()
    {
        if (!isCountingDown)
        {
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = 0f;
            UpdateTimerUI();
            Debug.Log("[CanvasScreenGameplay] Tempo esgotado. Considerando resposta errada.");
            StopCountdown();
            HandleTimeout();
            return;
        }

        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        float normalized = gameplayTime > 0f ? Mathf.Clamp01(timer / gameplayTime) : 0f;

        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(timer).ToString();
        }

        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = normalized;
        }
    }

    private void HandleTimeout()
    {
        MusicControllerEvents.OnUserAwnser?.Invoke("acabou");

        CallNextScreen();
    }

    private void ApplyMusicToUI(MusicController.MusicData music)
    {
        if (music == null)
        {
            Debug.LogWarning("[CanvasScreenGameplay] Dados de musica nulos recebidos, exibindo estado de carregamento.");
            ShowLoadingState();
            return;
        }

        Debug.Log($"[CanvasScreenGameplay] Aplicando musica no UI: {music.musicName}.");

        if (emojiImage != null)
        {
            emojiImage.sprite = music.musicEmoji;
            emojiImage.enabled = music.musicEmoji != null;
            Debug.Log($"[CanvasScreenGameplay] Emoji {(music.musicEmoji != null ? "aplicado" : "ausente")}.");
        }

        if (musicAndAutorText != null)
        {
            if (!string.IsNullOrEmpty(music.musicAutor))
            {
                musicAndAutorText.text = $"{music.musicName} - {music.musicAutor}";
                Debug.Log($"[CanvasScreenGameplay] Texto atualizado: {music.musicName} - {music.musicAutor}");
            }
            else
            {
                musicAndAutorText.text = music.musicName;
                Debug.Log($"[CanvasScreenGameplay] Texto atualizado: {music.musicName}");
            }

            musicAndAutorText2.text = musicAndAutorText.text;
            musicAndAutorTextOnFeedbackScreen.text = musicAndAutorText.text;

            musicTrecho.SetText(music.musicLyric);
        }
    }

    private void ShowLoadingState()
    {
        Debug.Log("[CanvasScreenGameplay] Exibindo estado de carregamento.");
        if (emojiImage != null)
        {
            emojiImage.enabled = false;
            emojiImage.sprite = null;
        }

        if (musicAndAutorText != null)
        {
            musicAndAutorText.text = "Carregando musica...";
        }
    }

    // Chamada para declarar resposta correta
    public void Button_ClickCorrect()
    {
        Debug.Log("[CanvasScreenGameplay] Botao de resposta correta clicado.");
        StopCountdown();
        MusicControllerEvents.OnUserAwnser?.Invoke("acertou");
        CallNextScreen();
    }

    // Chamada para declarar resposta errada
    public void Button_ClickWrong()
    {
        Debug.Log("[CanvasScreenGameplay] Botao de resposta errada clicado.");
        StopCountdown();
        MusicControllerEvents.OnUserAwnser?.Invoke("errou");
        CallNextScreen();
    }
}
