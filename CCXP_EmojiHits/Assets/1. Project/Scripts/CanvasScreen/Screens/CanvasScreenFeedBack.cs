using System.Collections;
using System;
using TMPro;
using UnityEngine;

public class CanvasScreenFeedBack : CanvasScreen
{
    [SerializeField] private GameObject acertouObject;
    [SerializeField] private GameObject errouObject;
    [SerializeField] private GameObject acabouObject;
    [SerializeField] private bool isCorrectFeedback = false;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private TMP_Text feedbackTextOnStaffScreen;

    [SerializeField] private AudioSource audioSource; // deve carregar a musica atual e tocar ela

    private MusicController.MusicData currentMusicData;
    private Coroutine controllerWatcher;
    private bool isMusicSubscriptionActive;


    public override void OnEnable()
    {
        base.OnEnable();
        MusicControllerEvents.OnUserAwnser += HandleUserAnswer;
        controllerWatcher = StartCoroutine(WatchForMusicController());
    }
    public override void OnDisable()
    {
        base.OnDisable();
        MusicControllerEvents.OnUserAwnser -= HandleUserAnswer;
        if (controllerWatcher != null)
        {
            StopCoroutine(controllerWatcher);
            controllerWatcher = null;
        }

        MusicController.Instance.OnMusicChanged -= HandleMusicChanged;
        isMusicSubscriptionActive = false;
        StopFeedbackMusic();
    }
    public override void TurnOn()
    {
        base.TurnOn();
    }

    public override void TurnOff()
    {
        StopFeedbackMusic();
        base.TurnOff();
    }

    private void HandleUserAnswer(string value)
    {

        Debug.Log($"[CanvasScreenFeedBack] HandleUserAnswer recebido com valor: {value}");
        if (value == "acertou")
        {
            acertouObject.SetActive(true);
            errouObject.SetActive(false);
            acabouObject.SetActive(false);
            isCorrectFeedback = true;
            PlayFeedbackMusic();
        }
        else if (value == "errou")
        {
            acertouObject.SetActive(false);
            errouObject.SetActive(true);
            acabouObject.SetActive(false);
            isCorrectFeedback = false;
            StopFeedbackMusic();
        }
        else if (value == "acabou")
        {
            acertouObject.SetActive(false);
            errouObject.SetActive(false);
            acabouObject.SetActive(true);
            isCorrectFeedback = false;
            StopFeedbackMusic();
        }
        else
        {
            Debug.LogWarning($"[CanvasScreenFeedBack] Valor desconhecido recebido em HandleUserAnswer: {value}");
            return;
        }

        UpdateFeedbackText();
    }
    private void PlayFeedbackMusic()
    {
        if (currentMusicData == null)
        {
            Debug.LogWarning("[CanvasScreenFeedBack] Nenhuma musica atual definida para tocar o feedback.");
            return;
        }

        if (currentMusicData.musicClip == null)
        {
            Debug.LogWarning("[CanvasScreenFeedBack] Clip de audio nao carregado para a musica atual.");
            return;
        }

        audioSource.clip = currentMusicData.musicClip;
        audioSource.Play();
    }

    private void StopFeedbackMusic()
    {
        if (audioSource == null)
        {
            return;
        }

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.clip = null;
    }
    private void UpdateFeedbackText()
    {
        if (feedbackText == null)
        {
            Debug.LogWarning("[CanvasScreenFeedBack] feedbackText nao esta atribuido.");
            return;
        }

        feedbackText.text = isCorrectFeedback ? "Resposta Correta!" : "Resposta Incorreta!";
        feedbackTextOnStaffScreen.text = feedbackText.text;
    }

    private void HandleMusicChanged(MusicController.MusicData music)
    {
        Debug.Log($"[CanvasScreenFeedBack] Musica atual alterada recebida: {music?.musicName ?? "null"}.");
        currentMusicData = music;
    }

    private IEnumerator WatchForMusicController()
    {
        while (MusicController.Instance == null)
        {
            yield return null;
        }

        MusicController.Instance.OnMusicChanged += HandleMusicChanged;
        isMusicSubscriptionActive = true;

        if (MusicController.Instance.TryGetCurrentMusic(out var music))
        {
            currentMusicData = music;
        }
    }

}
