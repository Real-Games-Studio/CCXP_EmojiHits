using TMPro;
using UnityEngine;

public class CanvasScreenVitoria : CanvasScreen
{
    public AudioSource audioSource; // deve carregar a musica atual e tocar ela
    public TMP_Text musicLyricText; // deve carregar a letra da musica atual

    private void OnEnable()
    {
        if (MusicController.Instance != null)
        {
            MusicController.Instance.OnMusicChanged += HandleMusicChanged;
        }
    }

    private void OnDisable()
    {
        if (MusicController.Instance != null)
        {
            MusicController.Instance.OnMusicChanged -= HandleMusicChanged;
        }

        StopAudioPlayback();
    }

    public override void TurnOn()
    {
        base.TurnOn();
        SyncWithCurrentMusic();
    }

    public override void TurnOff()
    {
        StopAudioPlayback();
        base.TurnOff();
    }

    private void HandleMusicChanged(MusicController.MusicData music)
    {
        if (IsOn())
        {
            ApplyMusicData(music);
        }
    }

    private void SyncWithCurrentMusic()
    {
        var controller = MusicController.Instance;
        if (controller != null && controller.TryGetCurrentMusic(out var music))
        {
            ApplyMusicData(music);
        }
        else
        {
            ResetUI();
        }
    }

    private void ApplyMusicData(MusicController.MusicData music)
    {
        if (musicLyricText != null)
        {
            musicLyricText.text = string.IsNullOrEmpty(music.musicLyric)
                ? "Letra indisponivel."
                : music.musicLyric;
        }

        if (audioSource != null)
        {
            audioSource.clip = music.musicClip;
            if (audioSource.clip != null)
            {
                audioSource.Play();
            }
        }
    }

    private void ResetUI()
    {
        if (musicLyricText != null)
        {
            musicLyricText.text = "Nenhuma musica selecionada.";
        }

        StopAudioPlayback();
    }

    private void StopAudioPlayback()
    {
        if (audioSource != null)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioSource.clip = null;
        }
    }
}
