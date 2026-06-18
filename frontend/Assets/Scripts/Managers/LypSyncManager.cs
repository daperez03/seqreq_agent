using UnityEngine;

public class LipSync_Manager : MonoBehaviour
{
    public AudioSource audioSource;

    public SkinnedMeshRenderer faceMesh;

    [Header("BlendShape")]
    public int mouthOpenIndex = 0;

    [Header("Settings")]
    public float sensitivity = 100f;

    public float smoothSpeed = 10f;

    private float currentWeight;

    void Update()
    {
        if (
            audioSource == null ||
            !audioSource.isPlaying
        )
        {
            faceMesh.SetBlendShapeWeight(
                mouthOpenIndex,
                0
            );

            return;
        }

        float[] samples = new float[256];

        audioSource.GetOutputData(samples, 0);

        float volume = 0f;

        foreach (float sample in samples)
        {
            volume += Mathf.Abs(sample);
        }

        volume /= samples.Length;

        float targetWeight =
            Mathf.Clamp01(volume * sensitivity) * 100f;

        currentWeight = Mathf.Lerp(
            currentWeight,
            targetWeight,
            Time.deltaTime * smoothSpeed
        );

        faceMesh.SetBlendShapeWeight(
            mouthOpenIndex,
            currentWeight
        );
    }
}