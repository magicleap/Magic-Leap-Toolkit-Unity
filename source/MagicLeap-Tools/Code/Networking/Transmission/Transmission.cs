// ---------------------------------------------------------------------
//
// Copyright (c) 2019 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/creator-terms
//
// ---------------------------------------------------------------------

using UnityEngine;
using UnityEngine.Events;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace MagicLeapTools
{
    //Enums:
    public enum TransmissionAudience { SinglePeer, KnownPeers, NetworkBroadcast };

    /// <summary>
    /// Note: Ensure that Transmission is slotted before Default Time in Script Execution Order to avoid errors with startup execution.
    /// </summary>
    public class Transmission : MonoBehaviour
    {
        //Public Variables:
        public int port = 23000;
        public int bufferSize = 1024;
        [Tooltip("On component addition a randomized ID will be generated.  All applications running on your network must have the same appKey and privateKey to recognize eachother - empty keys are accepted.")]
        public string appKey;
        [Tooltip("All applications running on your network must have the same appKey and privateKey to recognize eachother - empty keys are accepted.")]
        public string privateKey;
        [Tooltip("All GameObjects in this list (in addition to the Transmission GameObject) will receive SendMessages when RPC messages are sent.")]
        public GameObject[] rpcTargets;
        public bool debugOutgoing;
        public bool debugIncoming;

        //Events:
        [Space]
        /// <summary>
        /// Fired when a peer with matching appKey and privateKey is found on the network. String value contains the IP address of this host.
        /// </summary>
        public StringEvent OnPeerFound;
        /// <summary>
        /// Fired when a previously known peer disappears. String value contains the IP address of this peer.
        /// </summary>
        public StringEvent OnPeerLost;
        /// <summary>
        /// Fired when a reliable message send was impossible. String value contains the guid of the original message.
        /// </summary>
        public StringEvent OnSendMessageFailure;
        /// <summary>
        /// Fired when a reliable was successful. String value contains the guid of the original message.
        /// </summary>
        public StringEvent OnSendMessageSuccess;
        /// <summary>
        /// When a global value changes this will provide the key to it.
        /// </summary>
        public StringEvent OnGlobalStringChanged;
        /// <summary>
        /// When a global value changes this will provide the key to it.
        /// </summary>
        public StringEvent OnGlobalBoolChanged;
        /// <summary>
        /// When a global value changes this will provide the key to it.
        /// </summary>
        public StringEvent OnGlobalFloatChanged;
        /// <summary>
        /// When a global value changes this will provide the key to it.
        /// </summary>
        public StringEvent OnGlobalVector2Changed;
        /// <summary>
        /// When a global value changes this will provide the key to it.
        /// </summary>
        public StringEvent OnGlobalVector3Changed;
        /// <summary>
        /// When a global value changes this will provide the key to it.
        /// </summary>
        public StringEvent OnGlobalVector4Changed;
                
        public TransmissionObjectMsgEvent OnOwnershipLost;
        public TransmissionObjectMsgEvent OnOwnershipGained;
        public TransmissionObjectMsgEvent OnOwnershipTransferDenied;
        public BoolMsgEvent OnBoolMessage;
        public BoolArrayMsgEvent OnBoolArrayMessage;
        public ByteArrayMsgEvent OnByteArrayMessage;
        public ColorMsgEvent OnColorMessage;
        public ColorArrayMsgEvent OnColorArrayMessage;
        public FloatMsgEvent OnFloatMessage;
        public FloatArrayMsgEvent OnFloatArrayMessage;
        public PoseMsgEvent OnPoseMessage;
        public PoseArrayMsgEvent OnPoseArrayMessage;
        public QuaternionMsgEvent OnQuaternionMessage;
        public QuaternionArrayMsgEvent OnQuaternionArrayMessage;
        public StringMsgEvent OnStringMessage;
        public StringArrayMsgEvent OnStringArrayMessage;
        public Vector2MsgEvent OnVector2Message;
        public Vector2ArrayMsgEvent OnVector2ArrayMessage;
        public Vector3MsgEvent OnVector3Message;
        public Vector3ArrayMsgEvent OnVector3ArrayMessage;
        public Vector4MsgEvent OnVector4Message;
        public Vector4ArrayMsgEvent OnVector4ArrayMessage;
        public PCFEvent OnPCF;
        /// <summary>
        /// Consumed internally by TransmissionObject to synchronize a transform to peers.
        /// </summary>
        public TransformSyncMsgEvent OnTransformSync;
        /// <summary>
        /// Consumed internally by SpatialAlignment to alert of a new peer we are aligned with.
        /// </summary>
        public SpatialAlignmentMsgEvent OnSpatialAlignment;

        //Public Properties:
        public static Transmission Instance
        {
            get
            {
                if (_quitting)
                {
                    return null;
                }

                //find:
                if (_instance == null)
                {
                    _instance = FindObjectOfType<Transmission>();
                }

                //missing:
                if (_instance == null)
                {
                    Debug.Log("No instance of Transmission found in scene.");
                }

                //initialize:
                Initialize();

                return _instance;
            }
        }

        public static string[] Peers
        {
            get
            {
                return _peers.Keys.ToArray();
            }
        }

        //Private Variables:
        private const float HeartbeatInterval = 2;
        private const float ReliableResendInterval = .5f;
        private const float MaxResendDuration = 7;
        private const float StalePeerTimeout = 8; 
        private static bool _receiveThreadAlive;
        private static ConcurrentBag<string> _receivedMessages = new ConcurrentBag<string>(); //do we need to be concerned about the constant growth of this?
        private List<string> _confirmedReliableMessages = new List<string>();
        private static Dictionary<string, TransmissionMessage> _unconfirmedReliableMessages = new Dictionary<string, TransmissionMessage>();
        private static Dictionary<string, float> _peers = new Dictionary<string, float>();
        private static Transmission _instance;
        private static UdpClient _udpClient;
        private static Thread _receiveThread;
        private static IPEndPoint _receiveEndPoint = new IPEndPoint(IPAddress.Any, 0);
        private static bool _initialized;
        private static Coroutine _alignmentCoroutine;
        private static Dictionary<string, string> _globalStrings = new Dictionary<string, string>();
        private static Dictionary<string, float> _globalFloats = new Dictionary<string, float>();
        private static Dictionary<string, bool> _globalBools = new Dictionary<string, bool>();
        private static Dictionary<string, Vector2> _globalVector2 = new Dictionary<string, Vector2>();
        private static Dictionary<string, Vector3> _globalVector3 = new Dictionary<string, Vector3>();
        private static Dictionary<string, Vector4> _globalVector4 = new Dictionary<string, Vector4>();
        private static bool _quitting;

        //Init:
        private void Awake()
        {
            Initialize();
        }

        private void Reset()
        {
            appKey = MathUtilities.UniqueID();
        }

        //Deinit:
        private void OnApplicationQuit()
        {
            _quitting = true;
        }

        private void OnDestroy()
        {
            //stop receive thread:
            if (_receiveThread != null)
            {
                _receiveThread.Abort();
            }
            _receiveThreadAlive = false;

            //close socket:
            if (_udpClient != null)
            {
                _udpClient.Close();
            }

            StopAllCoroutines();
        }

        //Loops:
        private void Update()
        {
            ReceiveMessages();
        }

        //Public Methods:
        public static void SetGlobalString(string key, string value)
        {
            //set:
            SetLocalGlobalStrings(key, value);

            //share:
            Send(new GlobalStringChangedMessage(key, value));
        }

        public static bool HasGlobalString(string key)
        {
            return _globalStrings.ContainsKey(key);
        }

        public static string GetGlobalString(string key)
        {
            if (HasGlobalString(key))
            {
                return _globalStrings[key];
            }
            else
            {
                Debug.LogWarning("Global string does not exist - returning default value.");
                return "";
            }
        }

        public static void SetGlobalBool(string key, bool value)
        {
            //set:
            SetLocalGlobalBools(key, value);

            //share:
            Send(new GlobalBoolChangedMessage(key, value));
        }

        public static bool HasGlobalBool(string key)
        {
            return _globalBools.ContainsKey(key);
        }

        public static bool GetGlobalBool(string key)
        {
            if (HasGlobalBool(key))
            {
                return _globalBools[key];
            }
            else
            {
                Debug.LogWarning("Global bool does not exist - returning default value.");
                return false;
            }
        }

        public static void SetGlobalFloat (string key, float value)
        {
            //set:
            SetLocalGlobalFloats(key, value);

            //share:
            Send(new GlobalFloatChangedMessage(key, value));
        }

        public static bool HasGlobalFloat(string key)
        {
            return _globalFloats.ContainsKey(key);
        }

        public static float GetGlobalFloat(string key)
        {
            if (HasGlobalFloat(key))
            {
                return _globalFloats[key];
            }
            else
            {
                Debug.LogWarning("Global float does not exist - returning default value.");
                return 0;
            }
        }

        public static void SetGlobalVector2(string key, Vector2 value)
        {
            //set:
            SetLocalGlobalVector2(key, value);

            //share:
            Send(new GlobalVector2ChangedMessage(key, value));
        }

        public static bool HasGlobalVector2(string key)
        {
            return _globalVector2.ContainsKey(key);
        }

        public static Vector2 GetGlobalVector2(string key)
        {
            if (HasGlobalVector2(key))
            {
                return _globalVector2[key];
            }
            else
            {
                Debug.LogWarning("Global Vector2 does not exist - returning default value.");
                return Vector2.zero;
            }
        }
        public static void SetGlobalVector3(string key, Vector3 value)
        {
            //set:
            SetLocalGlobalVector3(key, value);

            //share:
            Send(new GlobalVector3ChangedMessage(key, value));
        }

        public static bool HasGlobalVector3(string key)
        {
            return _globalVector3.ContainsKey(key);
        }

        public static Vector3 GetGlobalVector3(string key)
        {
            if (HasGlobalVector3(key))
            {
                return _globalVector3[key];
            }
            else
            {
                Debug.LogWarning("Global Vector3 does not exist - returning default value.");
                return Vector3.zero;
            }
        }

        public static void SetGlobalVector4(string key, Vector4 value)
        {
            //set:
            SetLocalGlobalVector4(key, value);

            //share:
            Send(new GlobalVector4ChangedMessage(key, value));
        }

        public static bool HasGlobalVector4(string key)
        {
            return _globalVector4.ContainsKey(key);
        }

        public static Vector4 GetGlobalVector4(string key)
        {
            if (HasGlobalVector4(key))
            {
                return _globalVector4[key];
            }
            else
            {
                Debug.LogWarning("Global Vector4 does not exist - returning default value.");
                return Vector4.zero;
            }
        }

        /// <summary>
        /// Checks if a known peer still has an active heartbeat.
        /// </summary>
        public static bool PeerAlive(string address)
        {
            return _peers.ContainsKey(address);
        }

        /// <summary>
        /// Used internally to update all the currently spawned objects to a new known peer.
        /// </summary>
        public void SpawnRecap(TransmissionObject transmissionObject, string destination)
        {
            SpawnRecapMessage spawnRecapMessage = new SpawnRecapMessage(transmissionObject, destination);
            Send(spawnRecapMessage);
        }

        /// <summary>
        /// Spawns an object from the Resources folder onto every known peer on the network.  ResourceFileName must be the unique name of a prefab in a Resource folder without the file extension.
        /// </summary>
        public static TransmissionObject Spawn(string resourceFileName, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            //spawn local:
            TransmissionObject spawned = PerformSpawn(resourceFileName, true, NetworkUtilities.MyAddress, Guid.NewGuid().ToString(), position, rotation, scale);

            //share if the spawn was successful:
            if (spawned != null)
            {
                //different remote prefab requested?
                if (spawned.remotePrefab != null)
                {
                    resourceFileName = spawned.remotePrefab.name;
                }

                //if we have peers then let them know we spawned something:
                if (_peers.Count != 0)
                {
                    SpawnMessage spawnMessage = new SpawnMessage(resourceFileName, spawned.guid, position, rotation, scale);
                    Send(spawnMessage);
                }
            }

            return spawned;
        }

        /// <summary>
        /// Transmits a TransmissionMessage to the network.
        /// </summary>
        public static void Send(TransmissionMessage message)
        {
            //reliable logging:
            if (message.r == 1)
            {
                if (!_unconfirmedReliableMessages.ContainsKey(message.g))
                {
                    //set target counts:
                    if (string.IsNullOrEmpty(message.t))
                    {
                        message.ts = _peers.Count;
                    }
                    else
                    {
                        message.ts = 1;
                    }

                    _unconfirmedReliableMessages.Add(message.g, message);
                }
            }

            //generate transmission:
            string serialized = JsonUtility.ToJson(message);
            byte[] bytes = Encoding.UTF8.GetBytes(serialized);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, Instance.port);

            //size check:
            if (bytes.Length > _udpClient.Client.SendBufferSize)
            {
                Debug.Log($"Message too large to send! Buffer is currently {Instance.bufferSize} bytes and you are tring to send {bytes.Length} bytes. Try increasing the buffer size.");
                return;
            }

            //send:
            if (string.IsNullOrEmpty(message.t))
            {
                //send to all peers:
                foreach (var item in Peers)
                {
                    endPoint.Address = IPAddress.Parse(item);
                    _udpClient.Send(bytes, bytes.Length, endPoint);

                    //debug:
                    if (Instance.debugOutgoing)
                    {
                        Debug.Log($"Sent {serialized} to {endPoint.Address.ToString()}");
                    }
                }
            }
            else
            {
                endPoint.Address = IPAddress.Parse(message.t);
                _udpClient.Send(bytes, bytes.Length, endPoint);

                //debug:
                if (Instance.debugOutgoing)
                {
                    Debug.Log($"Sent {serialized} to {endPoint}");
                }
            }
        }

        //Event Handlers:
        private void OnApplicationPause(bool pause)
        {
            //to make sure we get recaps if we pause it is best to clear out our peers:
            if (pause)
            {
                _peers.Clear();
            }
        }

        //Private Methods:
        private static void SetLocalGlobalStrings(string key, string value)
        {
            //not set:
            if (!_globalStrings.ContainsKey(key))
            {
                _globalStrings.Add(key, value);
            }

            _globalStrings[key] = value;
        }

        private static void SetLocalGlobalBools(string key, bool value)
        {
            //not set:
            if (!_globalBools.ContainsKey(key))
            {
                _globalBools.Add(key, value);
            }

            _globalBools[key] = value;
        }

        private static void SetLocalGlobalFloats(string key, float value)
        {
            //not set:
            if (!_globalFloats.ContainsKey(key))
            {
                _globalFloats.Add(key, value);
            }

            _globalFloats[key] = value;
        }

        private static void SetLocalGlobalVector2(string key, Vector2 value)
        {
            //not set:
            if (!_globalVector2.ContainsKey(key))
            {
                _globalVector2.Add(key, value);
            }

            _globalVector2[key] = value;
        }

        private static void SetLocalGlobalVector3(string key, Vector3 value)
        {
            //not set:
            if (!_globalVector3.ContainsKey(key))
            {
                _globalVector3.Add(key, value);
            }

            _globalVector3[key] = value;
        }

        private static void SetLocalGlobalVector4(string key, Vector4 value)
        {
            //not set:
            if (!_globalVector4.ContainsKey(key))
            {
                _globalVector4.Add(key, value);
            }

            _globalVector4[key] = value;
        }

        private static void Remove(string ip)
        {
            Destroy(TransmissionRoot.Get(ip).gameObject);
        }

        private static void Initialize()
        {
            //flag initializtion complete:
            if (_initialized)
            {
                return;
            }
            _initialized = true;

            //add local root:
            TransmissionRoot.Add(NetworkUtilities.MyAddress);

            //establish socket:
            bool socketOpen = false;
            while (!socketOpen)
            {
                try
                {
                    _udpClient = new UdpClient(Instance.port);
                    _udpClient.Client.SendBufferSize = Instance.bufferSize;
                    _udpClient.Client.ReceiveBufferSize = Instance.bufferSize;
                    socketOpen = true;
                }
                catch (Exception)
                {
                }
            }

            //establish receive thread:
            _receiveThreadAlive = true;
            _receiveThread = new Thread(new ThreadStart(Receive));
            _receiveThread.IsBackground = true;
            _receiveThread.Start();

            Instance.StartCoroutine(Heartbeat());
            Instance.StartCoroutine(ReliableRetry());

            //fire off an awake event:
            Send(new TransmissionMessage(TransmissionMessageType.AwakeMessage, TransmissionAudience.NetworkBroadcast, "", true));

            //TransmissionObjects can only be spawned for proper use:
            TransmissionObject[] unspawnedTransmissionObjects = FindObjectsOfType<TransmissionObject>();
            if (unspawnedTransmissionObjects.Length != 0)
            {
                Debug.LogError("You can not manually place TransmissionObjects in your scene. They can only be spawned with the Spawn method for proper use. Disabling all found TransmissionObjects.");
                foreach (var item in unspawnedTransmissionObjects)
                {
                    item.gameObject.SetActive(false);
                }
            }
        }

        private static TransmissionObject PerformSpawn(string resourceFileName, bool mine, string creator, string guid, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            //already exists;
            if (TransmissionObject.Exists(guid))
            {
                return null;
            }

            //find:
            TransmissionObject template = Resources.Load<TransmissionObject>(resourceFileName);

            //found?
            if (template == null)
            {
                Debug.Log($"Prefab '{resourceFileName}' not found for spawning in a Resources folder.");
                return null;
            }

            //props pre-set before spawning:
            if (mine)
            {
                template.guid = Guid.NewGuid().ToString();
            }
            else
            {
                template.guid = guid;
            }

            //create:
            TransmissionObject spawned = Instantiate<TransmissionObject>(template);

            //props post-set after spawning:
            if (mine)
            {
                spawned.IsMine = true;
            }
            else
            {
                spawned.IsMine = false;
            }

            //properties:
            spawned.resourceFileName = resourceFileName;
            spawned.creator = creator;

            //parenting:
            spawned.transform.parent = TransmissionRoot.Get(creator).transform;
            spawned.targetPosition = position;
            spawned.targetRotation = rotation;
            spawned.targetScale = scale;
            spawned.transform.localPosition = position;
            spawned.transform.localRotation = rotation;
            spawned.transform.localScale = scale;
            return spawned;
        }

        private void ReceiveMessages()
        {
            while (_receivedMessages.Count > 0)
            {
                string rawMessage;
                if (_receivedMessages.TryTake(out rawMessage))
                {
                    //get message:
                    TransmissionMessage currentMessage = JsonUtility.FromJson<TransmissionMessage>(rawMessage);

                    //debug:
                    if (debugIncoming)
                    {
                        Debug.Log($"Received {rawMessage} from {currentMessage.f}");
                    }

                    //parse status:
                    bool needToParse = true;

                    //reliable message?
                    if (currentMessage.r == 1)
                    {
                        if (_confirmedReliableMessages.Contains(currentMessage.g))
                        {
                            //we have previously consumed this message but the confirmation failed so we only
                            //need to focus on sending another confirmation:
                            needToParse = false;
                            continue;
                        }
                        else
                        {
                            //mark this reliable message as confirmed:
                            _confirmedReliableMessages.Add(currentMessage.g);
                        }

                        //send back confirmation message with same guid:
                        TransmissionMessage confirmationMessage = new TransmissionMessage(
                            TransmissionMessageType.ConfirmedMessage,
                            TransmissionAudience.SinglePeer,
                            currentMessage.f,
                            false,
                            "",
                            currentMessage.g);

                        Send(confirmationMessage);
                    }

                    //parsing needed?
                    if (!needToParse)
                    {
                        continue;
                    }

                    switch ((TransmissionMessageType)currentMessage.ty)
                    {
                        case TransmissionMessageType.GlobalStringsRequestMessage:
                            Send(new GlobalStringsRecapMessage(currentMessage.f, _globalStrings.Keys.ToArray<string>(), _globalStrings.Values.ToArray<string>()));
                            break;

                        case TransmissionMessageType.GlobalFloatsRequestMessage:
                            Send(new GlobalFloatsRecapMessage(currentMessage.f, _globalFloats.Keys.ToArray<string>(), _globalFloats.Values.ToArray<float>()));
                            break;

                        case TransmissionMessageType.GlobalBoolsRequestMessage:
                            string boolsSerialized = JsonUtility.ToJson(_globalBools);
                            Send(new GlobalBoolsRecapMessage(currentMessage.f, _globalBools.Keys.ToArray<string>(), _globalBools.Values.ToArray<bool>()));
                            break;

                        case TransmissionMessageType.GlobalVector2RequestMessage:
                            string vec2Serialized = JsonUtility.ToJson(_globalVector2);
                            Send(new GlobalVector2RecapMessage(currentMessage.f, _globalVector2.Keys.ToArray<string>(), _globalVector2.Values.ToArray<Vector2>()));
                            break;

                        case TransmissionMessageType.GlobalVector3RequestMessage:
                            string vec3Serialized = JsonUtility.ToJson(_globalVector3);
                            Send(new GlobalVector3RecapMessage(currentMessage.f, _globalVector3.Keys.ToArray<string>(), _globalVector3.Values.ToArray<Vector3>()));
                            break;

                        case TransmissionMessageType.GlobalVector4RequestMessage:
                            string vec4Serialized = JsonUtility.ToJson(_globalVector4);
                            Send(new GlobalVector4RecapMessage(currentMessage.f, _globalVector4.Keys.ToArray<string>(), _globalVector4.Values.ToArray<Vector4>()));
                            break;

                        case TransmissionMessageType.HeartbeatMessage:
                            //new peer:
                            if (!_peers.ContainsKey(currentMessage.f))
                            {
                                TransmissionRoot.Add(currentMessage.f);
                                _peers.Add(currentMessage.f, 0);

                                //if I have no global values then ask this new peer for theirs:
                                if (_peers.Count == 1)
                                {
                                    if (_globalStrings.Count == 0)
                                    {
                                        Send(new TransmissionMessage(TransmissionMessageType.GlobalStringsRequestMessage, TransmissionAudience.SinglePeer, currentMessage.f, true));
                                    }

                                    if (_globalBools.Count == 0)
                                    {
                                        Send(new TransmissionMessage(TransmissionMessageType.GlobalBoolsRequestMessage, TransmissionAudience.SinglePeer, currentMessage.f, true));
                                    }

                                    if (_globalFloats.Count == 0)
                                    {
                                        Send(new TransmissionMessage(TransmissionMessageType.GlobalFloatsRequestMessage, TransmissionAudience.SinglePeer, currentMessage.f, true));
                                    }

                                    if (_globalVector2.Count == 0)
                                    {
                                        Send(new TransmissionMessage(TransmissionMessageType.GlobalVector2RequestMessage, TransmissionAudience.SinglePeer, currentMessage.f, true));
                                    }

                                    if (_globalVector3.Count == 0)
                                    {
                                        Send(new TransmissionMessage(TransmissionMessageType.GlobalVector3RequestMessage, TransmissionAudience.SinglePeer, currentMessage.f, true));
                                    }

                                    if (_globalVector4.Count == 0)
                                    {
                                        Send(new TransmissionMessage(TransmissionMessageType.GlobalVector4RequestMessage, TransmissionAudience.SinglePeer, currentMessage.f, true));
                                    }
                                }

                                OnPeerFound?.Invoke(currentMessage.f);
                            }
                            //catalog heartbeat time:
                            _peers[currentMessage.f] = Time.realtimeSinceStartup;
                            break;

                        case TransmissionMessageType.ConfirmedMessage:
                            //update confirmed targets (this allows for KnownPeers confirmation from all
                            //peers):
                            _unconfirmedReliableMessages[currentMessage.g].ts -= 1;

                            //all done?
                            if (_unconfirmedReliableMessages[currentMessage.g].ts == 0)
                            {
                                //confirmed!
                                _unconfirmedReliableMessages.Remove(currentMessage.g);
                                OnSendMessageSuccess?.Invoke(currentMessage.g);
                            }
                            break;

                        case TransmissionMessageType.AwakeMessage:
                            //if this peer hasn't been gone long then fire a recap:
                            if (_peers.ContainsKey(currentMessage.f))
                            {
                                OnPeerFound?.Invoke(currentMessage.f);
                                _peers[currentMessage.f] = Time.realtimeSinceStartup;
                            }
                            break;

                        case TransmissionMessageType.RPCMessage:
                            RPCMessage rpcMessage = UnpackMessage<RPCMessage>(rawMessage);

                            gameObject.SendMessage(rpcMessage.m, rpcMessage.pa, SendMessageOptions.DontRequireReceiver);
                            foreach (var item in rpcTargets)
                            {
                                item.SendMessage(rpcMessage.m, rpcMessage.pa, SendMessageOptions.DontRequireReceiver);
                            }
                            break;

                        case TransmissionMessageType.OnEnabledMessage:
                            OnEnabledMessage onEnabledMessage = UnpackMessage<OnEnabledMessage>(rawMessage);

                            TransmissionObject enableTarget = TransmissionObject.Get(onEnabledMessage.ig);
                            if (enableTarget != null)
                            {
                                enableTarget.gameObject.SetActive(true);
                            }

                            break;

                        case TransmissionMessageType.OnDisabledMessage:
                            OnDisabledMessage onDisabledMessage = UnpackMessage<OnDisabledMessage>(rawMessage);

                            TransmissionObject disableTarget = TransmissionObject.Get(onDisabledMessage.ig);
                            if (disableTarget != null)
                            {
                                disableTarget.gameObject.SetActive(false);
                            }
                            break;

                        case TransmissionMessageType.GlobalFloatChangedMessage:
                            GlobalFloatChangedMessage globalFloatChangedMessage = UnpackMessage<GlobalFloatChangedMessage>(rawMessage);
                            SetLocalGlobalFloats(globalFloatChangedMessage.k, globalFloatChangedMessage.v);
                            OnGlobalFloatChanged?.Invoke(globalFloatChangedMessage.k);
                            break;

                        case TransmissionMessageType.GlobalFloatsRecapMessage:
                            GlobalFloatsRecapMessage globalFloatsRecapMessage = UnpackMessage<GlobalFloatsRecapMessage>(rawMessage);
                            _globalFloats = globalFloatsRecapMessage.k.Zip(globalFloatsRecapMessage.v, (s, i) => new { s, i }).ToDictionary(item => item.s, item => item.i);
                            foreach (var item in _globalFloats)
                            {
                                OnGlobalFloatChanged?.Invoke(item.Key);
                            }
                            break;

                        case TransmissionMessageType.GlobalBoolChangedMessage:
                            GlobalBoolChangedMessage globalBoolChangedMessage = UnpackMessage<GlobalBoolChangedMessage>(rawMessage);
                            SetLocalGlobalBools(globalBoolChangedMessage.k, globalBoolChangedMessage.v);
                            OnGlobalBoolChanged?.Invoke(globalBoolChangedMessage.k);
                            break;

                        case TransmissionMessageType.GlobalBoolsRecapMessage:
                            GlobalBoolsRecapMessage globalBoolsRecapMessage = UnpackMessage<GlobalBoolsRecapMessage>(rawMessage);
                            _globalBools = globalBoolsRecapMessage.k.Zip(globalBoolsRecapMessage.v, (s, i) => new { s, i }).ToDictionary(item => item.s, item => item.i);
                            foreach (var item in _globalBools)
                            {
                                OnGlobalBoolChanged?.Invoke(item.Key);
                            }
                            break;

                        case TransmissionMessageType.GlobalStringChangedMessage:
                            GlobalStringChangedMessage globalStringChangedMessage = UnpackMessage<GlobalStringChangedMessage>(rawMessage);
                            SetLocalGlobalStrings(globalStringChangedMessage.k, globalStringChangedMessage.v);
                            OnGlobalStringChanged?.Invoke(globalStringChangedMessage.k);
                            break;

                        case TransmissionMessageType.GlobalStringsRecapMessage:
                            GlobalStringsRecapMessage globalStringsRecapMessage = UnpackMessage<GlobalStringsRecapMessage>(rawMessage);
                            _globalStrings = globalStringsRecapMessage.k.Zip(globalStringsRecapMessage.v, (s, i) => new { s, i }).ToDictionary(item => item.s, item => item.i);
                            foreach (var item in _globalStrings)
                            {
                                OnGlobalStringChanged?.Invoke(item.Key);
                            }
                            break;

                        case TransmissionMessageType.GlobalVector2ChangedMessage:
                            GlobalVector2ChangedMessage globalVector2ChangedMessage = UnpackMessage<GlobalVector2ChangedMessage>(rawMessage);
                            SetLocalGlobalVector2(globalVector2ChangedMessage.k, globalVector2ChangedMessage.v);
                            OnGlobalVector2Changed?.Invoke(globalVector2ChangedMessage.k);
                            break;

                        case TransmissionMessageType.GlobalVector2RecapMessage:
                            GlobalVector2RecapMessage globalVector2RecapMessage = UnpackMessage<GlobalVector2RecapMessage>(rawMessage);
                            _globalVector2 = globalVector2RecapMessage.k.Zip(globalVector2RecapMessage.v, (s, i) => new { s, i }).ToDictionary(item => item.s, item => item.i);
                            foreach (var key in _globalVector2.Keys) { OnGlobalVector2Changed?.Invoke(key); }
                            break;

                        case TransmissionMessageType.GlobalVector3ChangedMessage:
                            GlobalVector3ChangedMessage globalVector3ChangedMessage = UnpackMessage<GlobalVector3ChangedMessage>(rawMessage);
                            SetLocalGlobalVector3(globalVector3ChangedMessage.k, globalVector3ChangedMessage.v);
                            OnGlobalVector3Changed?.Invoke(globalVector3ChangedMessage.k);
                            break;

                        case TransmissionMessageType.GlobalVector3RecapMessage:
                            GlobalVector3RecapMessage globalVector3RecapMessage = UnpackMessage<GlobalVector3RecapMessage>(rawMessage);
                            _globalVector3 = globalVector3RecapMessage.k.Zip(globalVector3RecapMessage.v, (s, i) => new { s, i }).ToDictionary(item => item.s, item => item.i);
                            foreach (var key in _globalVector3.Keys) { OnGlobalVector3Changed?.Invoke(key); }
                            break;

                        case TransmissionMessageType.GlobalVector4ChangedMessage:
                            GlobalVector4ChangedMessage globalVector4ChangedMessage = UnpackMessage<GlobalVector4ChangedMessage>(rawMessage);
                            SetLocalGlobalVector4(globalVector4ChangedMessage.k, globalVector4ChangedMessage.v);
                            OnGlobalVector4Changed?.Invoke(globalVector4ChangedMessage.k);
                            break;

                        case TransmissionMessageType.GlobalVector4RecapMessage:
                            GlobalVector4RecapMessage globalVector4RecapMessage = UnpackMessage<GlobalVector4RecapMessage>(rawMessage);
                            _globalVector4 = globalVector4RecapMessage.k.Zip(globalVector4RecapMessage.v, (s, i) => new { s, i }).ToDictionary(item => item.s, item => item.i);
                            foreach (var key in _globalVector4.Keys) { OnGlobalVector4Changed?.Invoke(key); }
                            break;


                        case TransmissionMessageType.StringMessage:
                            StringMessage stringMessage = UnpackMessage<StringMessage>(rawMessage);
                            OnStringMessage?.Invoke(stringMessage);
                            break;

                        case TransmissionMessageType.StringArrayMessage:
                            StringArrayMessage stringArrayMessage = UnpackMessage<StringArrayMessage>(rawMessage);
                            OnStringArrayMessage?.Invoke(stringArrayMessage);
                            break;

                        case TransmissionMessageType.PoseMessage:
                            PoseMessage poseMessage = UnpackMessage<PoseMessage>(rawMessage);
                            OnPoseMessage?.Invoke(poseMessage);
                            break;

                        case TransmissionMessageType.PoseArrayMessage:
                            PoseArrayMessage poseArrayMessage = UnpackMessage<PoseArrayMessage>(rawMessage);
                            OnPoseArrayMessage?.Invoke(poseArrayMessage);
                            break;

                        case TransmissionMessageType.BoolMessage:
                            BoolMessage boolMessage = UnpackMessage<BoolMessage>(rawMessage);
                            OnBoolMessage?.Invoke(boolMessage);
                            break;

                        case TransmissionMessageType.BoolArrayMessage:
                            BoolArrayMessage booArraylMessage = UnpackMessage<BoolArrayMessage>(rawMessage);
                            OnBoolArrayMessage?.Invoke(booArraylMessage);
                            break;

                        case TransmissionMessageType.QuaternionMessage:
                            QuaternionMessage quaternionMessage = UnpackMessage<QuaternionMessage>(rawMessage);
                            OnQuaternionMessage?.Invoke(quaternionMessage);
                            break;

                        case TransmissionMessageType.QuaternionArrayMessage:
                            QuaternionArrayMessage quaternionArrayMessage = UnpackMessage<QuaternionArrayMessage>(rawMessage);
                            OnQuaternionArrayMessage?.Invoke(quaternionArrayMessage);
                            break;

                        case TransmissionMessageType.Vector2Message:
                            Vector2Message vector2Message = UnpackMessage<Vector2Message>(rawMessage);
                            OnVector2Message?.Invoke(vector2Message);
                            break;

                        case TransmissionMessageType.Vector2ArrayMessage:
                            Vector2ArrayMessage vector2ArrayMessage = UnpackMessage<Vector2ArrayMessage>(rawMessage);
                            OnVector2ArrayMessage?.Invoke(vector2ArrayMessage);
                            break;

                        case TransmissionMessageType.Vector3Message:
                            Vector3Message vector3Message = UnpackMessage<Vector3Message>(rawMessage);
                            OnVector3Message?.Invoke(vector3Message);
                            break;

                        case TransmissionMessageType.Vector3ArrayMessage:
                            Vector3ArrayMessage vector3ArrayMessage = UnpackMessage<Vector3ArrayMessage>(rawMessage);
                            OnVector3ArrayMessage?.Invoke(vector3ArrayMessage);
                            break;

                        case TransmissionMessageType.Vector4Message:
                            Vector4Message vector4Message = UnpackMessage<Vector4Message>(rawMessage);
                            OnVector4Message?.Invoke(vector4Message);
                            break;

                        case TransmissionMessageType.Vector4ArrayMessage:
                            Vector4ArrayMessage vector4ArrayMessage = UnpackMessage<Vector4ArrayMessage>(rawMessage);
                            OnVector4ArrayMessage?.Invoke(vector4ArrayMessage);
                            break;

                        case TransmissionMessageType.ColorMessage:
                            ColorMessage colorMessage = UnpackMessage<ColorMessage>(rawMessage);
                            OnColorMessage?.Invoke(colorMessage);
                            break;

                        case TransmissionMessageType.ColorArrayMessage:
                            ColorArrayMessage colorArrayMessage = UnpackMessage<ColorArrayMessage>(rawMessage);
                            OnColorArrayMessage?.Invoke(colorArrayMessage);
                            break;

                        case TransmissionMessageType.FloatMessage:
                            FloatMessage floatMessage = UnpackMessage<FloatMessage>(rawMessage);
                            OnFloatMessage?.Invoke(floatMessage);
                            break;

                        case TransmissionMessageType.FloatArrayMessage:
                            FloatArrayMessage floatArrayMessage = UnpackMessage<FloatArrayMessage>(rawMessage);
                            OnFloatArrayMessage?.Invoke(floatArrayMessage);
                            break;

                        case TransmissionMessageType.ByteArrayMessage:
                            ByteArrayMessage byteArrayMessage = UnpackMessage<ByteArrayMessage>(rawMessage);
                            OnByteArrayMessage?.Invoke(byteArrayMessage);
                            break;

                        case TransmissionMessageType.SpawnMessage:
                            SpawnMessage spawnMessage = UnpackMessage<SpawnMessage>(rawMessage);

                            Vector3 spawnPosition = new Vector3((float)spawnMessage.px, (float)spawnMessage.py, (float)spawnMessage.pz);
                            Quaternion spawnRotation  = new Quaternion((float)spawnMessage.rx, (float)spawnMessage.ry, (float)spawnMessage.rz, (float)spawnMessage.rw);
                            Vector3 spawnScale = new Vector3((float)spawnMessage.sx, (float)spawnMessage.sy, (float)spawnMessage.sz);

                            TransmissionObject spawnObject = PerformSpawn(spawnMessage.rf, false, spawnMessage.f, spawnMessage.i, spawnPosition, spawnRotation, spawnScale);
                            break;

                        case TransmissionMessageType.DespawnMessage:
                            DespawnMessage despawnMessage = UnpackMessage<DespawnMessage>(rawMessage);

                            TransmissionObject despawnTarget = TransmissionObject.Get(despawnMessage.ig);
                            if (despawnTarget != null)
                            {
                                Destroy(despawnTarget.gameObject);
                            }

                            break;

                        case TransmissionMessageType.SpawnRecapMessage:
                            SpawnRecapMessage spawnRecapMessage = UnpackMessage<SpawnRecapMessage>(rawMessage);

                            Vector3 spawnRecapPosition = new Vector3((float)spawnRecapMessage.px, (float)spawnRecapMessage.py, (float)spawnRecapMessage.pz);
                            Quaternion spawnRecapRotation = new Quaternion((float)spawnRecapMessage.rx, (float)spawnRecapMessage.ry, (float)spawnRecapMessage.rz, (float)spawnRecapMessage.rw);
                            Vector3 spawnRecapScale = new Vector3((float)spawnRecapMessage.sx, (float)spawnRecapMessage.sy, (float)spawnRecapMessage.sz);

                            //make sure root is ready:
                            TransmissionRoot.Add(spawnRecapMessage.f);

                            TransmissionObject spawnRecapObject = PerformSpawn(spawnRecapMessage.rf, false, spawnRecapMessage.f, spawnRecapMessage.i, spawnRecapPosition, spawnRecapRotation, spawnRecapScale);
                            break;

                        case TransmissionMessageType.TransformSyncMessage:
                            TransformSyncMessage transformSyncMessage = UnpackMessage<TransformSyncMessage>(rawMessage);
                            OnTransformSync?.Invoke(transformSyncMessage);
                            break;

                        case TransmissionMessageType.SpatialAlignmentMessage:
                            SpatialAlignmentMessage spatialAlignmentMessage = UnpackMessage<SpatialAlignmentMessage>(rawMessage);
                            OnSpatialAlignment?.Invoke(spatialAlignmentMessage.f);
                            break;

                        case TransmissionMessageType.PCFMessage:
                            PCFMessage pcfMessage = UnpackMessage<PCFMessage>(rawMessage);
                            OnPCF?.Invoke(pcfMessage.f, pcfMessage.c, pcfMessage.o, pcfMessage.i);
                            break;

                        case TransmissionMessageType.OwnershipTransferenceRequestMessage:
                            OwnershipTransferenceRequestMessage ownershipTransferenceRequestMessage = UnpackMessage<OwnershipTransferenceRequestMessage>(rawMessage);
                            TransmissionObject target = TransmissionObject.Get(ownershipTransferenceRequestMessage.ig);
                            if (!target.ownershipLocked)
                            {
                                target.IsMine = false;
                                OnOwnershipLost?.Invoke(target);
                                Send(new OwnershipTransferenceGrantedMessage(target.guid, ownershipTransferenceRequestMessage.f));
                            }
                            else
                            {
                                Send(new OwnershipTransferenceDeniedMessage(target.guid, ownershipTransferenceRequestMessage.f));
                            }
                            break;

                        case TransmissionMessageType.OwnershipTransferenceDeniedMessage:
                            OwnershipTransferenceDeniedMessage ownershipTransferenceDeniedMessage = UnpackMessage<OwnershipTransferenceDeniedMessage>(rawMessage);
                            TransmissionObject denied = TransmissionObject.Get(ownershipTransferenceDeniedMessage.ig);
                            OnOwnershipTransferDenied?.Invoke(denied);
                            break;

                        case TransmissionMessageType.OwnershipTransferenceGrantedMessage:
                            OwnershipTransferenceGrantedMessage ownershipTransferenceGrantedMessage = UnpackMessage<OwnershipTransferenceGrantedMessage>(rawMessage);
                            TransmissionObject gained = TransmissionObject.Get(ownershipTransferenceGrantedMessage.ig);
                            OnOwnershipGained?.Invoke(gained);
                            break;
                    }
                }
            }
        }

        private T UnpackMessage<T>(string rawMessage)
        {
            return JsonUtility.FromJson<T>(rawMessage);
        }

        //Coroutines:
        private static IEnumerator ReliableRetry()
        {
            while (true)
            {
                //iterate a copy so we don't have issues with inbound confirmations:
                foreach (var item in _unconfirmedReliableMessages.Values.ToArray())
                {
                    if (Time.realtimeSinceStartup - item.ti < MaxResendDuration)
                    {
                        //resend:
                        Send(item);
                    }
                    else
                    {
                        //TODO: add explict list of who didn't get it for KnownPeers intended messages:
                        //reliable message send failed - only if we have some targets left, otherwise there 
                        //were no recipients to begin with which easily happens if someone attempted a KnownPeers
                        //send when no one was around:
                        if (item.ts != 0)
                        {
                            Instance.OnSendMessageFailure?.Invoke(item.g);
                        }
                        _unconfirmedReliableMessages.Remove(item.g);
                    }
                }

                //loop:
                yield return new WaitForSeconds(ReliableResendInterval);
                yield return null;
            }
        }

        private static IEnumerator Heartbeat()
        {
            while (true)
            {
                //transmit message:
                Send(new TransmissionMessage(TransmissionMessageType.HeartbeatMessage, TransmissionAudience.NetworkBroadcast, "", false));

                //stale peer identification:
                List<string> stalePeers = new List<string>();
                foreach (var item in _peers)
                {
                    if (Time.realtimeSinceStartup - item.Value > StalePeerTimeout)
                    {
                        stalePeers.Add(item.Key);
                    }
                }

                //stale peer removal:
                foreach (var item in stalePeers)
                {
                    Remove(item);
                    _peers.Remove(item);
                    Instance.OnPeerLost?.Invoke(item);
                }

                //loop:
                yield return new WaitForSeconds(HeartbeatInterval);
                yield return null;
            }
        }

        //Threads:
        private static void Receive()
        {
            while (_receiveThreadAlive)
            {
                //catalog message:
                byte[] bytes = _udpClient.Receive(ref _receiveEndPoint);

                //get raw message for key evaluation:
                string serialized = Encoding.UTF8.GetString(bytes);
                TransmissionMessage rawMessage = JsonUtility.FromJson<TransmissionMessage>(serialized);

                //address evaluation:
                if (rawMessage.f != NetworkUtilities.MyAddress)
                {
                    //keys evaluations:
                    if (rawMessage.a == Instance.appKey && rawMessage.p == Instance.privateKey)
                    {
                        //we send the serialized string for easier debug messages:
                        _receivedMessages.Add(serialized);
                    }
                }
            }
        }
    }
}