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

public class Manager : MonoBehaviour {

    public AudioRecorder recorder;
    public AudioPlayer player;
    public Text labelRecordingStateValue;
    public Text inputTextField;
    public Text textAnswerField;
    public Text answerTextField;

    private Text _aiText;
    private AudioClip _currentClip;
    private AudioClip _aiClip;

    private ApiAiUnity apiAiUnity;

    private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
    };

    public void Start()
    {
        //DEV ACCESS: 3afa7e0be30741fd95dd5151267ad48e
        //client access: 7c1c7c9580f54762a5d42d82f6cac1ab

        const string ACCESS_TOKEN = "bff7a013ad8b4297b9b04602ded6b89a";

        var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.English);

        apiAiUnity = new ApiAiUnity();
        apiAiUnity.Initialize(config);

        apiAiUnity.OnError += HandleOnError;
        apiAiUnity.OnResult += HandleOnResult;
    }

    private void HandleOnResult(object sender, AIResponseEventArgs e)
    {
        print("RESULT");

        if (_aiClip != null)
            Destroy(_aiClip);

        var aiResponse = e.Response;
        if (aiResponse != null)
        {
            var outText = JsonConvert.SerializeObject(aiResponse, jsonSettings);
            print(outText);
            answerTextField.text = outText;
        }
        else
        {
            Debug.LogError("Response is null");
        }
    }

    private void HandleOnError(object sender, AIErrorEventArgs e)
    {
        throw new NotImplementedException();
    }

    public void Update()
    {
        labelRecordingStateValue.text = recorder.State.ToString();

        if (apiAiUnity != null)
        {
            apiAiUnity.Update();
        }
    }

    public void onClick()
    {
        if (recorder != null)
        {
            recorder.StartRecording();
            recorder.OnRecordingEnd.AddListener(OnRecordingEnd);
            print("RECORDING");
        }
    }

    public void TextQuery()
    {
        var mAiText = inputTextField.text;

        var response = apiAiUnity.TextRequest(mAiText);
        if (response != null)
        {
            var outText = JsonConvert.SerializeObject(response, jsonSettings);
            Debug.Log("Result: " + outText);
            textAnswerField.text = outText;
        }
        else
        {
            Debug.LogError("Response is null");
        }
    }

    private void OnRecordingEnd(AudioClip clip)
    {
        _currentClip = _aiClip = clip;
        
        if (player != null)
            print("AUDIO PLAYBACK");
        //player.StartPlaying(clip);
        else OnPlayingEnd(false);

        apiAiUnity.StartVoiceRequestThread(_aiClip);
    }

    private void OnPlayingEnd(bool b)
    {
        if (_currentClip != null)
            Destroy(_currentClip);
        if (b)
            if (recorder != null)
                onClick();
    }
}