using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class TTSRequest
{
    public string model;

    public string input;

    public string voice;
}

public class TTS_Manager : MonoBehaviour
{
    [Header("OpenAI")]
    public string apiKey;

    [Header("Audio")]
    public AudioSource audioSource;

    public void Speak(string text)
    {
        StartCoroutine(GenerateSpeech(text));
    }

    IEnumerator GenerateSpeech(string text)
    {
        string url =
            "https://api.openai.com/v1/audio/speech";

        TTSRequest body = new TTSRequest()
        {
            model = "gpt-4o-mini-tts",
            input = text,
            voice = "alloy"
        };

        string json =
            JsonUtility.ToJson(body);

        byte[] bodyRaw =
            System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest request =
            new UnityWebRequest(url, "POST");

        request.uploadHandler =
            new UploadHandlerRaw(bodyRaw);

        request.downloadHandler =
            new DownloadHandlerBuffer();

        request.SetRequestHeader(
            "Content-Type",
            "application/json"
        );

        request.SetRequestHeader(
            "Authorization",
            "Bearer " + apiKey
        );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(
                "TTS Error: " + request.error
            );

            Debug.LogError(
                request.downloadHandler.text
            );

            yield break;
        }

        byte[] audioData =
            request.downloadHandler.data;

        string path = Path.Combine(
            Application.persistentDataPath,
            "tts.mp3"
        );

        File.WriteAllBytes(path, audioData);

        yield return StartCoroutine(
            PlayAudio(path)
        );
    }

    IEnumerator PlayAudio(string path)
    {
        using UnityWebRequest request =
            UnityWebRequestMultimedia.GetAudioClip(
                "file://" + path,
                AudioType.MPEG
            );

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(
                "Audio Error: " + request.error
            );

            yield break;
        }

        AudioClip clip =
            DownloadHandlerAudioClip.GetContent(request);

        audioSource.clip = clip;

        audioSource.Play();
    }
}