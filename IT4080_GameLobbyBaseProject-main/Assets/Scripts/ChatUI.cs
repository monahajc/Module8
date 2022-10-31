using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : NetworkBehaviour
{
    public TMPro.TMP_Text txtChatLog;
    public Button btnSend;
    public TMPro.TMP_InputField inputMessage;

    ulong[] singleClientId = new ulong[1];

    public void Start()
    {
        btnSend.onClick.AddListener(ClientOnSendClicked);
        inputMessage.onSubmit.AddListener(ClientOnInputSubmit);
    }

    public override void OnNetworkSpawn()
    {
        txtChatLog.text = "-- Start Chat Log --";
        if(IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HostOnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HostOnClientDisconnected;
            DisplayMessageLocally("You are the host!");
        }
        else
        {
            DisplayMessageLocally($"You are Player #{NetworkManager.Singleton.LocalClientId}!");
        }
    }

    private void SendUIMessage()
    {
        string msg = inputMessage.text;
        inputMessage.text = "";
        SendChatMessageServerRpc(msg);
    }
    
    private void SendDirectMessage(string message, ulong from, ulong to)
    {
        ClientRpcParams rpcParams = default;
        rpcParams.Send.TargetClientIds = singleClientId;

        singleClientId[0] = from;
        SendChatMessageClientRpc($"[you] {message}", rpcParams);

        singleClientId[0] = to;
        SendChatMessageClientRpc($"<whisper> {message}", rpcParams);
    }
    //------------------------------------
    // Events
    //------------------------------------
    
    private void HostOnClientConnected(ulong clientId)
    {
        SendChatMessageClientRpc($"Client {clientId} connected");
    }

    private void HostOnClientDisconnected(ulong clientId)
    {
        SendChatMessageClientRpc($"Client {clientId} disconnected");
    }
    
    public void ClientOnSendClicked()
    {
        SendUIMessage();
    }

    public void ClientOnInputSubmit(string text)
    {
        SendUIMessage();
    }

    //-------------------------------------
    // RPC
    //-------------------------------------
    
    [ClientRpc]
    public void SendChatMessageClientRpc(string message, ClientRpcParams clientRpcParams = default)
    {
        DisplayMessageLocally(message);
    }
    [ServerRpc(RequireOwnership = false)]
    public void SendChatMessageServerRpc(string message, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log($"Host got message: {message}");
        string newMessage = $"[Player #{serverRpcParams.Receive.SenderClientId}]: {message}";

        if(message.StartsWith("@"))
        {
            string[] parts = message.Split(" ");
            string clientIdStr = parts[0].Replace("@", "");
            ulong toClientId = ulong.Parse(clientIdStr);

            SendDirectMessage(message, serverRpcParams.Receive.SenderClientId, toClientId);
        }
        else
        {
            SendChatMessageClientRpc(newMessage);
        }
    }

    public void DisplayMessageLocally(string message)
    {
        Debug.Log(message);
        txtChatLog.text += $"\n{message}";
    }
}
