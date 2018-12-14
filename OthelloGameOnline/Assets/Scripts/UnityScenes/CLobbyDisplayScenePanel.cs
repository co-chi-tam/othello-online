using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CLobbyDisplayScenePanel : CDefaultScene {

	#region Fields

	protected Button m_JoinRandomButton;
	protected Button m_ExitButton;
	protected Button m_SoundOnOffButton;
	protected GameObject m_SoundOnImage;
	protected GameObject m_SoundOffImage;
	protected Button m_ShowTutorialButton;

	protected Button m_OpenShopButton;
	protected Text m_GoldDisplayText;
	protected Text m_PlayerNameText;

	protected Text m_ChatRoomText;
	protected ScrollRect m_ChatScrollRect;

	protected InputField m_ChatInputField;
	protected Button m_SubmitChatButton;

	protected int m_MaximumChat = 50;
	protected List<string> m_ListChat;
	protected float m_ChatDelay = -1f;

	#endregion
	
	#region Constructor

	public CLobbyDisplayScenePanel() : base()
	{

	}

	public CLobbyDisplayScenePanel(string sceneName, string sceneObject): base(sceneName, sceneObject)
	{
		
	}

	#endregion
	
	#region Implementation Default

	public override void OnInitObject()
	{
		base.OnInitObject();
		// BUTTONS
		this.m_JoinRandomButton = CRootManager.FindObjectWith(GameObject, "JoinRandomButton").GetComponent<Button>();
		this.m_ExitButton = CRootManager.FindObjectWith(GameObject, "ExitButton").GetComponent<Button>();
		this.m_SoundOnOffButton = CRootManager.FindObjectWith(GameObject, "SoundOnOffButton").GetComponent<Button>();
		this.m_SoundOffImage = CRootManager.FindObjectWith(GameObject, "SoundOffImage");
		this.m_SoundOnImage = CRootManager.FindObjectWith(GameObject, "SoundOnImage");
		this.m_SoundOffImage.SetActive (CGameSetting.SETTING_SOUND_MUTE);
		this.m_SoundOnImage.SetActive (!CGameSetting.SETTING_SOUND_MUTE);
		CSoundManager.Instance.MuteAll (CGameSetting.SETTING_SOUND_MUTE);
		this.m_ShowTutorialButton = CRootManager.FindObjectWith(GameObject, "ShowTutorialButton").GetComponent<Button>();
		this.m_OpenShopButton = CRootManager.FindObjectWith(GameObject, "OpenShopButton").GetComponent<Button>();
		// GOLD
		this.m_GoldDisplayText = CRootManager.FindObjectWith (GameObject, "GoldDisplayText").GetComponent<Text>();
		// ON EVENTS
		CSocketManager.Instance.On("joinRoomCompleted", this.OnJoinRoomCompleted);
		CSocketManager.Instance.On("joinRoomFailed", this.OnJoinRoomFailed);

		// BUTTONS
		this.m_JoinRandomButton.onClick.AddListener(this.OnJoinRandomClick);
		this.m_ExitButton.onClick.AddListener(this.OnExitClick);
		this.m_SoundOnOffButton.onClick.AddListener (this.OnSoundOnOffClick);
		this.m_ShowTutorialButton.onClick.AddListener (this.OnShowTutorialClick);
		this.m_OpenShopButton.onClick.AddListener (this.OnOpenShopClick);

		// UI
		this.m_PlayerNameText = CRootManager.FindObjectWith(GameObject, "PlayerNameText").GetComponent<Text>(); 
		this.m_ChatRoomText = CRootManager.FindObjectWith(GameObject, "ChatRoomText").GetComponent<Text>();
		this.m_ChatRoomText.text = "...Chat...";
		this.m_ChatScrollRect = CRootManager.FindObjectWith (GameObject, "ChatScrollRect").GetComponent<ScrollRect>();
		this.m_ChatInputField = CRootManager.FindObjectWith(GameObject, "ChatInputField").GetComponent<InputField>();
		this.m_SubmitChatButton = CRootManager.FindObjectWith (GameObject, "SubmitChatButton").GetComponent<Button>();

		// EVENTS
		CSocketManager.Instance.On ("msgWorldChat", this.OnReceiveWordChat);
		// BUTTONS
		this.m_SubmitChatButton.onClick.AddListener (this.OnSubmitWorldChat);
		// CHAT ULTILITIES
		this.m_ListChat = new List<string>();
	}

	public override void OnStartObject()
	{
		base.OnStartObject();
		// GOLD DISPLAY
		this.m_GoldDisplayText.text = CGameSetting.USER_GOLD.ToString();
		this.m_PlayerNameText.text = CGameSetting.USER_NAME;
		// LEAVE ROOM
		CSocketManager.Instance.Emit("leaveRoom");
	}

	public override void OnUpdateObject(float dt)
	{
		base.OnUpdateObject(dt);
		// DELAY CHAT
		if (this.m_ChatDelay > 0)
		{
			this.m_ChatDelay -= dt;
		}
	}

	public override void OnDestroyObject()
	{
		base.OnDestroyObject();
	}

	public override void OnBackPress()
	{
		// base.OnEscapeObject();
	}

	#endregion

	#region Private

	protected virtual void JoinExistedRoom()
	{
		var sendData = new JSONObject();
		sendData.AddField("playerAvatar", CGameSetting.USER_AVATAR);
		CSocketManager.Instance.Emit("joinExistedRoom", sendData);
		CRootManager.Instance.ShowScene ("MainGamePanel");
	}

	protected virtual void JoinRoom(string name)
	{
		var sendData = new JSONObject();
		sendData.AddField("roomName", name);
		CSocketManager.Instance.Emit("joinOrCreateRoom", sendData);
		CSoundManager.Instance.Play ("sfx_click");
	}

	private void OnJoinRandomClick()
	{
		// JOIN RANDOM
		// var confirm = CRootManager.Instance.ShowPopup("ConfirmPopup") as CConfirmPopup;
		// confirm.Show("JOIN ROOM", "Do you want join existed room ?", "OK", () => {
		// 	confirm.OnBackPress();
		// }, "CANCEL", () => {
		// 	confirm.OnBackPress();
		// });
		this.JoinExistedRoom();
		CSoundManager.Instance.Play ("sfx_click");
	}

	private void OnJoinRoomCompleted(SocketIO.SocketIOEvent ev)
	{
		Debug.Log ("OnJoinRoomCompleted " + ev.data.ToString());
		CRootManager.Instance.ShowScene("MainGamePanel");
		CGameSetting.LAST_ROOM_NAME = ev.data.GetField("roomName").ToString().Replace("\"", "");
	}

	private void OnJoinRoomFailed(SocketIO.SocketIOEvent ev)
	{
		Debug.Log ("OnJoinRoomFailed " + ev.data.ToString());
		CRootManager.Instance.BackTo("LobbyDisplayPanel");
	}

	private void OnExitClick()
	{
		// EXIT
		CRootManager.Instance.Back();
		CSoundManager.Instance.Play ("sfx_click");
	}

	private void OnSoundOnOffClick()
	{
		CGameSetting.SETTING_SOUND_MUTE = !CGameSetting.SETTING_SOUND_MUTE;
		this.m_SoundOffImage.SetActive (CGameSetting.SETTING_SOUND_MUTE);
		this.m_SoundOnImage.SetActive (!CGameSetting.SETTING_SOUND_MUTE);
		CSoundManager.Instance.MuteAll (CGameSetting.SETTING_SOUND_MUTE);
		CSoundManager.Instance.Play ("sfx_click");
	}

	private void OnShowTutorialClick()
	{
		CRootManager.Instance.ShowPopup("TutorialPopup");
	}

	private void OnOpenShopClick()
	{
		var shop = CRootManager.Instance.ShowPopup("ShopPopup") as CShopPopup;
		shop.GetUpdateData(() => {
			// GOLD DISPLAY
			this.m_GoldDisplayText.text = CGameSetting.USER_GOLD.ToString();
		});
		CSoundManager.Instance.Play ("sfx_click");
	}

	#endregion

	#region Public

	#endregion

	#region Private

	private void OnReceiveWordChat(SocketIO.SocketIOEvent ev)
	{
		// Debug.Log ("OnReceiveWordChat " + ev.data.ToString());
		var user = ev.data.GetField("user").ToString().Replace("\"", string.Empty);
		var msg = ev.data.GetField("message").ToString().Replace("\"", string.Empty);
		var display = this.GetFormatChat(user, msg, user.Equals(CGameSetting.USER_NAME) ? "#FF3B3B" : "#00B3FF");
		// ADD DISPLAY
		this.m_ListChat.Add (display);
		this.UpdateChatText();
		this.m_ChatScrollRect.verticalNormalizedPosition = 0;
	}

	private void OnSubmitWorldChat()
	{
		var chatText = this.m_ChatInputField.text;
		if (string.IsNullOrEmpty(chatText))
			return;
		if (chatText.Length < 2)
			return;
		if (this.m_ChatDelay > 0)
			return;
		var data = new JSONObject();
		data.AddField("message", chatText);
		CSocketManager.Instance.Emit("sendWorldChat", data);
		// CHAT BOX
		this.m_ChatInputField.text = string.Empty;
		// SCROLL RECT TO BOTTOM
		this.m_ChatScrollRect.verticalNormalizedPosition = 0f;
		// DELAY
		this.m_ChatDelay = CGameSetting.CHAT_DELAY;
		CSoundManager.Instance.Play ("sfx_click");
	}

	private void UpdateChatText()
	{
		var chatText = "";
		// 0 < 50 < max 
		var min = this.m_ListChat.Count <= this.m_MaximumChat ? 0 : this.m_ListChat.Count - this.m_MaximumChat;
		var max = this.m_ListChat.Count;
		for (int i = min; i < max; i++)
		{
			chatText += this.m_ListChat[i];
		}
		this.m_ChatRoomText.text = chatText;
	}

	private void OnQuitClick()
	{
		CRootManager.Instance.Back();
		CSoundManager.Instance.Play ("sfx_click");
	}
	
	#endregion

	#region Private

	public string GetFormatChat(string user, string msg, string color = "#00B3FF")
	{
		return string.Format("<b><color={0}>[{1}]</color></b>: {2}\n", color, user, msg);
	}

	#endregion

}
