using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class STT_Manager : MonoBehaviour
{
    private AudioClip clip;

    private string device;

    private bool isRecording = false;

    [Header("OpenAI")]
    public string apiKey = "TU_API_KEY";

    private string micPath;

    public static event Action<string> OnUserSpoke;
    
    [System.Serializable]
    public class WhisperResponse
    {
        public string text;
    }

    void Start()
    {
        device = Microphone.devices[0];

        micPath = Path.Combine(
            Application.persistentDataPath,
            "recording.wav"
        );

        Debug.Log("Mic listo");
    }

    void Update()
    {
        // PRESIONA SPACE
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartRecording();
        }

        // SUELTA SPACE
        if (Input.GetKeyUp(KeyCode.Space))
        {
            StopRecording();
        }
    }

    void StartRecording()
    {
        if (isRecording)
            return;

        clip = Microphone.Start(
            device,
            false,
            30,
            16000
        );

        isRecording = true;

        Debug.Log("Grabando...");
    }

    void StopRecording()
    {
        if (!isRecording)
            return;

        int position = Microphone.GetPosition(device);

        Microphone.End(device);

        isRecording = false;

        Debug.Log("Grabacion detenida");

        if (position > 0)
        {
            float[] samples = new float[position * clip.channels];

            clip.GetData(samples, 0);

            AudioClip newClip = AudioClip.Create(
                clip.name,
                position,
                clip.channels,
                clip.frequency,
                false
            );

            newClip.SetData(samples, 0);

            clip = newClip;
        }

        byte[] wavData = WavUtility.FromAudioClip(clip);

        File.WriteAllBytes(micPath, wavData);

        StartCoroutine(SendAudioToWhisper());
    }

    IEnumerator SendAudioToWhisper()
    {
        byte[] audioData = File.ReadAllBytes(micPath);

        WWWForm form = new WWWForm();

        form.AddField(
            "model",
            "gpt-4o-mini-transcribe"
        );

        form.AddField("language", "es");

        form.AddBinaryData(
            "file",
            audioData,
            "audio.wav",
            "audio/wav"
        );

        UnityWebRequest request =
            UnityWebRequest.Post(
                "https://api.openai.com/v1/audio/transcriptions",
                form
            );

        request.SetRequestHeader(
            "Authorization",
            "Bearer " + apiKey
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        else
        {
            string json = request.downloadHandler.text;

            WhisperResponse response =
                JsonUtility.FromJson<WhisperResponse>(json);

            Debug.Log("Respuesta:");
            Debug.Log(response.text);
            OnUserSpoke?.Invoke(response.text);
        }
    }
}