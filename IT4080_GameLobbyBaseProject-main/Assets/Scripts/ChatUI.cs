using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChatUI : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if(IsHost)
        {
            SendChatMessageClientRpc("I am the host");
        }
        else
        {
            SendChatMessageServerRpc("I am the host");
        }
    }
    
    [ClientRpc]
    public void SendChatMessageClientRpc(string message, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log(message);
    }
    [ServerRpc(RequireOwnership = false)]
    public void SendChatMessageServerRpc(string message, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log($"Host got message: {message}");
    }
}
