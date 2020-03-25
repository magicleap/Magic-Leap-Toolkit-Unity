// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

//NOTE: You can test with one headset by running this example on device and hitting play on the scene in the Unity editor - you must build to device to test this way.

using UnityEngine;
using UnityEngine.UI;
using MagicLeapTools;

public class TransmissionExample : MonoBehaviour
{
#if PLATFORM_LUMIN
    //Public Variables:
    public ControlInput controlLocator;
    public Text info;

    //Private Variables:
    private const string GlobalScoreKey = "Score";
    private Renderer _headRenderer;
    private Renderer _controlRenderer;
    private string _initialInfo;

    //Init:
    private void Start()
    {
        //hooks:
        controlLocator.OnTriggerDown.AddListener(HandleTriggerDown);
        controlLocator.OnBumperDown.AddListener(HandleBumperDown);
        controlLocator.OnTouchDown.AddListener(HandleTouchDown);
        Transmission.Instance.OnStringMessage.AddListener(HandleStringMessage);

        //shared head locator:
        TransmissionObject headTransmissionObject = Transmission.Spawn("SampleTransmissionObject", Vector3.zero, Quaternion.identity, Vector3.one);
        headTransmissionObject.motionSource = Camera.main.transform;
        _headRenderer = headTransmissionObject.GetComponentInChildren<Renderer>();

        //shared control locator:
        TransmissionObject controlTransmissionObject = Transmission.Spawn("SampleTransmissionObject", Vector3.zero, Quaternion.identity, Vector3.one);
        controlTransmissionObject.motionSource = controlLocator.transform;
        _controlRenderer = controlTransmissionObject.GetComponentInChildren<Renderer>();

        //sets:
        _initialInfo = info.text;
    }

    //Event Handlers:
    private void HandleTriggerDown()
    {
        SendRPC();
    }

    private void HandleBumperDown()
    {
        SendHelloWorld();
    }

    private void HandleTouchDown(Vector4 touchInfo)
    {
        UpdateGlobalFloat();
    }

    private void HandleStringMessage(StringMessage stringMessage)
    {
        Debug.Log($"{stringMessage.v} was sent from {stringMessage.f}");
    }

    //Loops:
    private void Update()
    {
        string output = _initialInfo + System.Environment.NewLine;
        output += "Peers Available: " + Transmission.Instance.Peers.Length + System.Environment.NewLine;
        
        //only show the score if we have a value for it:
        if (Transmission.HasGlobalFloat(GlobalScoreKey))
        {
            output += "Global Float: " + Transmission.GetGlobalFloat(GlobalScoreKey);
        }

        info.text = output;
    }

    //GUI:
    private void OnGUI()
    {
        if (!Application.isEditor)
        {
            return;
        }

        //only used when testing with a connection between the editor and a headset:
        if (GUILayout.Button("Send Hello World"))
        {
            SendHelloWorld();
        }

        if (GUILayout.Button("Send RPC"))
        {
            SendRPC();
        }

        if (GUILayout.Button("Update Global Float"))
        {
            UpdateGlobalFloat();
        }
    }

    //Private Methods:
    private void UpdateGlobalFloat()
    {
        //get and increment score:
        float currentScore = Transmission.GetGlobalFloat(GlobalScoreKey);
        currentScore++;

        //set score:
        Transmission.SetGlobalFloat(GlobalScoreKey, currentScore);
    }

    private void SendHelloWorld()
    {
        StringMessage stringMessage = new StringMessage($"Hello World at: {Time.realtimeSinceStartup}");
        Transmission.Send(stringMessage);
    }

    private void SendRPC()
    {
        //RPCs use SendMessage under the hood and are sent to the Transmission GameObject and any GameObject in its RPC Targets
        RPCMessage rpcMessage = new RPCMessage("ChangeColor");
        Transmission.Send(rpcMessage);
    }

    //Public Methods(RPCs):
    public void ChangeColor()
    {
        Color randomColor = new Color(Random.value, Random.value, Random.value);
        _headRenderer.material.color = randomColor;
        _controlRenderer.material.color = randomColor;
    }
#endif
}