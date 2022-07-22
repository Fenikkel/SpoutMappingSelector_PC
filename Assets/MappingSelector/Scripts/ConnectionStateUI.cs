using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionStateUI : MonoBehaviour
{
	[Header("UI Elements")]
	public Image m_ConnectionImage;
	public TextMeshProUGUI m_ConnectionText;
	public Color32 m_ConnectedColor = Color.green;
	public Color32 m_DisconnectedColor = Color.red;

	public void SetConnected()
	{
		m_ConnectionText.text = "Connected";
		m_ConnectionImage.color = m_ConnectedColor;
	}

	public void SetDisconnected()
	{
		m_ConnectionText.text = "Disconnected";
		m_ConnectionImage.color = m_DisconnectedColor;
	}

	public void SwitchView()
	{
		m_ConnectionText.gameObject.SetActive(!m_ConnectionText.gameObject.activeSelf);
		m_ConnectionImage.gameObject.SetActive(!m_ConnectionImage.gameObject.activeSelf);
	}
}
