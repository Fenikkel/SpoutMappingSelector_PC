using extOSC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResponseTransmitter : MonoBehaviour
{   
    [Header("OSC Settings")]
    
    public string m_SyncAddress = "/sync";

    VideoManager _VideoManager;
    OSCTransmitter _Transmitter;

    void Start()
    {
        _Transmitter = FindObjectOfType<OSCTransmitter>();
        _VideoManager = FindObjectOfType<VideoManager>();
    }
    public void Sync()
    {
        OSCMessage message = new OSCMessage(m_SyncAddress);

        List<OSCValue> oscList = new List<OSCValue>();

        int[] infoArray = _VideoManager.GetInfoArray();

        for (int i = 0 ; i < infoArray.Length ; i++)
        {
            oscList.Add(OSCValue.Int(infoArray[i]));
        }

        OSCValue[] oscArray = oscList.ToArray();

        message.AddValue(OSCValue.Array(oscArray));

        _Transmitter.Send(message);
        Debug.Log("Osc sended: Array of length -> " + oscArray.Length);
    }
}
