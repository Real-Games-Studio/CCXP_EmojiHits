using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioEffect : MonoBehaviour
{
    [System.Serializable]
    public class AudioBarEffect
    {
        public RawImage visualizer;
        [Range(0f, 10f)] public float intensity = 1f;
        [HideInInspector] public RectTransform rectTransform;
        [HideInInspector] public float currentValue;
    }

    public AudioSource audioSource;
    [Range(32, 4096)] public int spectrumSampleSize = 64;
    public FFTWindow fftWindow = FFTWindow.Blackman;
    [Range(0f, 1f)] public float interpolation = 0.25f;
    [Range(0.01f, 2f)] public float minScale = 0.2f;
    [Range(0.01f, 5f)] public float maxScale = 1.4f;
    [Range(0f, 10f)] public float minIntensity = 0.1f;
    [Range(0f, 10f)] public float maxIntensity = 3f;
    public Color baseColor = Color.white;
        public AnimationCurve intensityCurve = new AnimationCurve(
        new Keyframe(0f, 0.25f),
        new Keyframe(0.5f, 1f),
        new Keyframe(1f, 0.25f));

    public List<AudioBarEffect> audioBarEffects = new List<AudioBarEffect>();

    private float[] spectrumData;


    public bool onValidateConfigAudioBars;

    private void ConfigureBars()
    {
        var barCount = audioBarEffects.Count;
        if (barCount == 0)
        {
            return;
        }

        var centerIndex = (barCount - 1f) * 0.5f;
        var maxDistance = Mathf.Max(centerIndex, 1f);

        for (var i = 0; i < barCount; i++)
        {
            var bar = audioBarEffects[i];
            if (bar == null || bar.visualizer == null)
            {
                continue;
            }

            bar.rectTransform = bar.visualizer.rectTransform;
            bar.currentValue = minScale;
            var distanceFromCenter = Mathf.Abs(i - centerIndex);
            var normalizedDistance = distanceFromCenter / maxDistance;
            var curveValue = Mathf.Clamp01(intensityCurve.Evaluate(normalizedDistance));
            var targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, curveValue);
            bar.intensity = Mathf.Clamp(targetIntensity, minIntensity, maxIntensity);
            bar.visualizer.color = baseColor;
        }
    }

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        spectrumData = new float[spectrumSampleSize];

        ConfigureBars();
    }

    private void OnEnable()
    {
        if (spectrumData == null || spectrumData.Length != spectrumSampleSize)
        {
            spectrumData = new float[spectrumSampleSize];
        }
    }

    private void OnValidate()
    {
        spectrumSampleSize = Mathf.Clamp(Mathf.NextPowerOfTwo(Mathf.Max(32, spectrumSampleSize)), 32, 4096);
        minScale = Mathf.Max(0.01f, minScale);
        maxScale = Mathf.Max(minScale, maxScale);
        minIntensity = Mathf.Max(0f, minIntensity);
        maxIntensity = Mathf.Max(minIntensity, maxIntensity);

        if (!onValidateConfigAudioBars && Application.isPlaying)
        {
            return;
        }

        onValidateConfigAudioBars = false;
        ConfigureBars();
    }

    private void Update()
    {
        if (audioSource == null || audioBarEffects.Count == 0)
        {
            return;
        }

        if (spectrumData == null || spectrumData.Length != spectrumSampleSize)
        {
            spectrumData = new float[spectrumSampleSize];
        }

        audioSource.GetSpectrumData(spectrumData, 0, fftWindow);

        var barCount = audioBarEffects.Count;
        var step = Mathf.Max(1, spectrumSampleSize / barCount);

        // Smoothly update each bar using the sampled spectrum magnitude.
        for (var i = 0; i < barCount; i++)
        {
            var bar = audioBarEffects[i];
            if (bar == null || bar.rectTransform == null)
            {
                continue;
            }

            var sampleIndex = Mathf.Clamp(i * step, 0, spectrumData.Length - 1);
            var magnitude = spectrumData[sampleIndex] * bar.intensity * 100f;
            magnitude = Mathf.Log10(Mathf.Max(1e-5f, magnitude));
            var target = Mathf.Clamp(minScale + magnitude, minScale, maxScale);

            var lerpFactor = 1f - Mathf.Pow(1f - interpolation, Time.deltaTime * 60f);
            bar.currentValue = Mathf.Lerp(bar.currentValue, target, lerpFactor);

            var localScale = bar.rectTransform.localScale;
            localScale.y = bar.currentValue;
            bar.rectTransform.localScale = localScale;

            var intensityFactor = Mathf.InverseLerp(minScale, maxScale, bar.currentValue);
            var color = Color.Lerp(baseColor, Color.white, intensityFactor);
            color.a = baseColor.a;
            bar.visualizer.color = color;
        }
    }
}
