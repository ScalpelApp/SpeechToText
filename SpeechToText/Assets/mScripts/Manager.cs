using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using ApiAiSDK;
using ApiAiSDK.Model;
using ApiAiSDK.Unity;
using Newtonsoft.Json;
using System.Net;
using System;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.DataTypes;

public class Manager : MonoBehaviour {

    public AudioRecorder recorder;
    public AudioPlayer player;
    public Text labelRecordingStateValue;
    public Text textAnswerField;
    public Button mainButton;

    private AudioClip _currentClip;

    private ApiAiUnity apiAiUnity;

    private string
        username = "25cd6fcb-aa09-4ed7-a97b-a7e991fe8564",
        password = "Hf4XEkydMnNJ",
        url = "https://stream.watsonplatform.net/speech-to-text/api";


    // BACK-UP CREDENTIALS - COMMENT OUT ABOVE LINES AND REMOVE "//" FROM THESE LINES
    //private string
    //    username = "095d7fdd-a751-4783-bf30-44f601861ab5",
    //    password = "7VLquzgEWdIi",
    //    url = "https://stream.watsonplatform.net/speech-to-text/api";

    private SpeechToText _speechToText;

    private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
    };

    public void Start()
    {
        Credentials credentials = new Credentials(username, password, url);
        _speechToText = new SpeechToText(credentials);
        GetWatsonModels();

        mainButton.onClick.AddListener(StartProcess);

        //DEV ACCESS: 3afa7e0be30741fd95dd5151267ad48e
        //client access: 7c1c7c9580f54762a5d42d82f6cac1ab

        //const string ACCESS_TOKEN = "bff7a013ad8b4297b9b04602ded6b89a";

        //var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.English);

        //apiAiUnity = new ApiAiUnity();
        //apiAiUnity.Initialize(config);

        //apiAiUnity.OnError += HandleOnError;
        //apiAiUnity.OnResult += HandleOnResult;
    }

    //private void HandleOnResult(object sender, AIResponseEventArgs e)
    //{
    //    print("RESULT");

    //    if (_aiClip != null)
    //        Destroy(_aiClip);

    //    var aiResponse = e.Response;
    //    if (aiResponse != null)
    //    {
    //        var outText = JsonConvert.SerializeObject(aiResponse, jsonSettings);
    //        print(outText);
    //        answerTextField.text = outText;
    //    }
    //    else
    //    {
    //        Debug.LogError("Response is null");
    //    }
    //}

    //private void HandleOnError(object sender, AIErrorEventArgs e)
    //{
    //    throw new NotImplementedException();
    //}

    public void Update()
    {
        labelRecordingStateValue.text = recorder.State.ToString();

        //if (apiAiUnity != null)
        //{
        //    apiAiUnity.Update();
        //}
    }


    void StartProcess()
    {
        if (mainButton.GetComponentInChildren<Text>().text == "START")
        {
            print("START PROCESS");
            mainButton.GetComponentInChildren<Text>().text = "STOP";

            StartRecording();
        }
        else
        {
            mainButton.GetComponentInChildren<Text>().text = "START";

            //_speechToText.StopListening();
        }
    }

    bool init = true;
    public void StartRecording()
    {
        if (recorder != null)
        {
            recorder.StartRecording();
            print("RECORDING");

            if (init)
            {
                init = false;
                recorder.OnRecordingEnd.AddListener(OnRecordingEnd);
            }
        }
    }

    private void OnRecordingEnd(AudioClip clip)
    {        
        if (player != null)
            print("AUDIO PLAYBACK");
        //player.StartPlaying(clip);
        else OnPlayingEnd(false);

        AudioClip _audioClip = WaveFile.ParseWAV("testClip", AudioClipToByteArray(clip));

        _speechToText.Recognize(_audioClip, OnRecognize);


        //apiAiUnity.StartVoiceRequestThread(_aiClip);
    }

    private void OnPlayingEnd(bool b)
    {
        if (_currentClip != null)
            Destroy(_currentClip);
        if (b)
            if (recorder != null)
                StartRecording();
    }
    private void GetWatsonModels()
    {
        if (!_speechToText.GetModels(HandleGetModels))
            Debug.Log("***FAILED TO GET MODELS***");
    }

    private void HandleGetModels(ModelSet result, string customData)
    {
        Debug.Log("ExampleSpeechToText" + " Speech to Text - Get models response: {0} " + customData);
    }

    private void OnRecognize(SpeechRecognitionEvent result)
    {
        print("REGOGNIZED");

        if (result != null && result.results.Length > 0)
        {
            foreach (var res in result.results)
            {
                //foreach (var alt in res.alternatives)
                //{
                //    string text = alt.transcript;

                //    print("RESULT: " + text);
                //}

                foreach (var alt in res.alternatives)
                {
                    string text = alt.transcript;
                    Debug.Log("ExampleStreaming " + string.Format("{0} ({1}, {2:0.00})\n", text, res.final ? "Final" : "Interim", alt.confidence));

                    if (res.final)
                        textAnswerField.text = text;
                }
            }
        }

        StartRecording();
    }

    // Returns a wav file from any AudioClip (doesn't saves):
    private static byte[] AudioClipToByteArray(AudioClip clip)
    {
        // Clip content:
        float[] samples = new float[clip.samples];
        clip.GetData(samples, 0);                                                       // The audio data in samples.

        // Write all data to byte array:
        List<byte> wavFile = new List<byte>();

        // RIFF header:
        wavFile.AddRange(new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' });    // "RIFF"
        wavFile.AddRange(System.BitConverter.GetBytes(samples.Length * 2 + 44 - 8));    // ChunkSize
        wavFile.AddRange(new byte[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' });    // "WAVE"
        wavFile.AddRange(new byte[] { (byte)'f', (byte)'m', (byte)'t', (byte)' ' });    // "fmt"
        wavFile.AddRange(System.BitConverter.GetBytes(16));                             // Subchunk1Size (16bit for PCM)
        wavFile.AddRange(System.BitConverter.GetBytes((ushort)1));                      // AudioFormat (1 for PCM)
        wavFile.AddRange(System.BitConverter.GetBytes((ushort)clip.channels));          // NumChannels
        wavFile.AddRange(System.BitConverter.GetBytes(clip.frequency));                 // SampleRate
        wavFile.AddRange(System.BitConverter.GetBytes(clip.frequency * clip.channels * (16 / 8)));    // ByteRate
        wavFile.AddRange(System.BitConverter.GetBytes((ushort)(clip.channels * (16 / 8))));         // BlockAlign
        wavFile.AddRange(System.BitConverter.GetBytes((ushort)16));                                 // BitsPerSample
        wavFile.AddRange(new byte[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' });    // "data"
        wavFile.AddRange(System.BitConverter.GetBytes(samples.Length * 2));             // Subchunk2Size
        // Add the audio data in bytes:
        for (int i = 0; i < samples.Length; i++)
        {
            short sample = (short)(samples[i] * 32768.0F);
            wavFile.AddRange(System.BitConverter.GetBytes(sample));                     // The audio data in bytes.
        }
        // Return the byte array to be saved:
        return wavFile.ToArray();
    }
}