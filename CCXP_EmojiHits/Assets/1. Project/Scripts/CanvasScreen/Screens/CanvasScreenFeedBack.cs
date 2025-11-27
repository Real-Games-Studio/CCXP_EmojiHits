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

    [SerializeField] public TMP_Text musicName; // deve carregar o nome da musica atual Musica - Autor
    [SerializeField] private AudioSource audioSource; // deve carregar a musica atual e tocar ela

    private MusicController.MusicData currentMusicData;


    public override void OnEnable()
    {
        if (MusicController.Instance != null)
        {
            MusicController.Instance.OnMusicChanged += HandleMusicChanged;
        }
        base.OnEnable();
        MusicControllerEvents.OnUserAwnser += HandleUserAnswer;
    }
    public override void OnDisable()
    {
        if (MusicController.Instance != null)
        {
            MusicController.Instance.OnMusicChanged -= HandleMusicChanged;
        }
        base.OnDisable();
        MusicControllerEvents.OnUserAwnser -= HandleUserAnswer;
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

    private void HandleMusicChanged(MusicController.MusicData music)
    {
        currentMusicData = music;
        musicName.text = music != null ? music.musicName + " - " + music.musicAutor : "Letra indisponível.";
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
        if (audioSource == null || currentMusicData?.musicClip == null)
        {
            return;
        }

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
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

}
