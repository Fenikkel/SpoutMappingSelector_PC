using extOSC;
using UnityEngine;
using UnityEngine.Events;

public class ConnectionStateController : MonoBehaviour
{
	[Header("OSC Settings")]
	public ConnectionType m_ConnectionType = ConnectionType.Client;
	private OSCReceiver m_Receiver;
	private string m_ReceivingOscAddress = "/ping"; 
	private OSCTransmitter m_Transmitter;
	private string m_SendingOscAddress = "/pong";

	[Header("Ping Settings")]
	public float m_TimeBetweenPings = 2.5f;
	private float m_ConnectionCheckTime;
	private float m_OfflineTimmer = 0.0f;
	private bool m_Connected = false;
	private float m_PingTimmer = 0.0f;

	[Header("Events")]
	public UnityEvent m_OnConnected;
	public UnityEvent m_OnDisconnected;
	public UnityEvent m_OnPingReceived;
	public UnityEvent m_OnPingSended;

	public enum ConnectionType 
	{ 
		Client,
		Server
	}

	protected virtual void Start()
	{
		m_Receiver = FindObjectOfType<OSCReceiver>();
		m_Transmitter = FindObjectOfType<OSCTransmitter>();

        switch (m_ConnectionType)
        {
            case ConnectionType.Client:
				m_ReceivingOscAddress = "/ping";
				m_SendingOscAddress = "/pong";
				break;
            case ConnectionType.Server:
				m_ReceivingOscAddress = "/pong";
				m_SendingOscAddress = "/ping";
				break;
            default:
				Debug.LogWarning("ConnectionType not implemented!");
                break;
        }

        m_Receiver.Bind(m_ReceivingOscAddress, UpdateConnection);
		m_OnDisconnected.Invoke();
	}

	private void Update()
	{
		UpdateOfflineTimmer();
		UpdatePingTimmer();
	}

	private void UpdateConnection(OSCMessage message)
	{
		m_OnPingReceived.Invoke();
		//Debug.LogFormat("Received: {0}", message);

		if (m_Connected)
		{
			m_OfflineTimmer = m_ConnectionCheckTime;
		}
		else
		{
			//CONNECT
			if (message.Values[0] != null)
			{
				m_ConnectionCheckTime = message.Values[0].FloatValue * 2.0f; //Ping sends the time between pings. So we disconnect if we don't receive a ping twice this time
			}

			m_OfflineTimmer = m_ConnectionCheckTime;
			m_Connected = true;
			m_OnConnected.Invoke();
		}

	}
	private void UpdateOfflineTimmer()
	{

		if (m_Connected)
		{
			m_OfflineTimmer -= Time.deltaTime;

			if (m_OfflineTimmer <= 0.0f)
			{
				m_Connected = false; //Disconnected
				m_OnDisconnected.Invoke();
			}
		}
	}

	private void UpdatePingTimmer()
	{

		m_PingTimmer -= Time.deltaTime;

		if (m_PingTimmer <= 0.0f)
		{
			m_PingTimmer = m_TimeBetweenPings;
			Ping();
		}
	}

	private void Ping()
	{
		OSCMessage message = new OSCMessage(m_SendingOscAddress);
		//message.AddValue(OSCValue.String("Hello, world! DEBUG")); // No funciona con Touch si no es float, int o doble y lo indicas bien en touch que tipo es.
		message.AddValue(OSCValue.Float(m_TimeBetweenPings)); //We indicate to the receiver that we gonna pink every m_TimeBetweenPings value;

		m_Transmitter.Send(message);

		m_OnPingSended.Invoke();
		//print("Ping");
	}


}
