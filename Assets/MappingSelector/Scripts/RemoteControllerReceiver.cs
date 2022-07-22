using extOSC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RemoteControllerReceiver : MonoBehaviour
{
	[Header("OSC Address")]
	public string m_SyncAddress = "/sync";
	public string m_PlayAddress = "/play";
	public string m_StopAddress = "/stop";
	public string m_PlayAllAddress = "/playAll";

	[Header("Events")]
	public UnityEvent OnSyncReceived;
	public UnityEvent OnPlayVideoReceived;
	public UnityEvent OnStopProjectorReceived;
	public UnityEvent OnPlayAllReceived;
	public UnityEvent OnStopAllReceived;

	/* Private */
	OSCReceiver _Receiver;
	VideoManager _VideoManager;

	void Start()
	{
		_Receiver = FindObjectOfType<OSCReceiver>();
		_VideoManager = FindObjectOfType<VideoManager>();

		_Receiver.Bind(m_SyncAddress, Sync);
		_Receiver.Bind(m_PlayAllAddress, PlayAll);
		_Receiver.Bind(m_PlayAddress, PlayVideo);
		_Receiver.Bind(m_StopAddress, StopProjector);
	}

	private void Sync(OSCMessage message)
	{
		print("Osc received: Sync -> " + message.Values[0].IsImpulse);

		if (message.HasImpulse())
		{
			
			OnSyncReceived.Invoke();
		}
		else 
		{
			Debug.Log("NO IMPULSE!!");
		}
	}

	private void PlayVideo(OSCMessage message) 
	{
		List<OSCValue> oscList;
		OSCValue[] oscArray;

		if (message.ToArray(out oscList))
        {
			if (oscList.Count == 2) 
			{
				oscArray = oscList.ToArray();
			}
            else
            {
				Debug.LogWarning("Incorrect information");
				return;
            }
        }
        else
        {
			Debug.LogWarning("Cant convert to a list");
			return;
        }
		

		print("Osc received: \nOsc index -> " + oscArray[0].IntValue + "\nVideo index -> " + oscArray[1].IntValue);

		_VideoManager.PlayVideo(oscArray[0].IntValue, oscArray[1].IntValue);
		OnPlayVideoReceived.Invoke();
	}

	private void StopProjector(OSCMessage message)
	{
		print("Osc received: StopVideo -> " + message.Values[0].IntValue);

		_VideoManager.StopProjector(message.Values[0].IntValue);
		OnStopProjectorReceived.Invoke();
	}

	private void PlayAll(OSCMessage message)
	{
		print("Osc received: PlayAll -> " + message.Values[0].BoolValue);

		if (message.Values[0].BoolValue)
		{
			OnPlayAllReceived.Invoke();
		}
		else
		{
			OnStopAllReceived.Invoke();
		}
	}
}
