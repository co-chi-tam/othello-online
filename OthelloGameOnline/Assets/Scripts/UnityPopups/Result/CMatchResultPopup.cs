using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CMatchResultPopup : CDefaultPopup {

	#region Fields

	protected Text m_TitleText;

	protected GameObject m_BodyPanel;
	protected Button m_QuitButton;
	protected Button m_AdmobButton;

	protected CResultItem m_ResultItemPrefab;
	protected List<CResultItem> m_ResultItems;

	protected Animator m_Animator;
	
	#endregion 

	#region CONDTRUCTOR

	public CMatchResultPopup(): base()
	{
		
	}

	public CMatchResultPopup(string popName, string popObject): base(popName, popObject)
	{
		
	}

	#endregion 

	#region Implementation Default

	public override void OnInitObject()
	{
		base.OnInitObject();
		this.m_TitleText = CRootManager.FindObjectWith(GameObject, "TitleText").GetComponent<Text>();
		// BODY
		this.m_BodyPanel = CRootManager.FindObjectWith(GameObject, "BodyPanel");
		// QUIT
		this.m_QuitButton = CRootManager.FindObjectWith(GameObject, "QuitButton").GetComponent<Button>();
		this.m_AdmobButton = CRootManager.FindObjectWith (GameObject, "AdmobButton").GetComponent<Button>();
		this.m_AdmobButton.onClick.AddListener (this.OnAdmobClick);
		// RESULTS
		this.m_ResultItemPrefab = Resources.Load<CResultItem>("UnityPopups/Result/ResultItem");
		this.m_ResultItems = new List<CResultItem>();
		// ANIMATOR
		this.m_Animator = CRootManager.FindObjectWith(GameObject, "Container").GetComponent<Animator>();
	}

	public override void OnStartObject()
	{
		base.OnStartObject();
		// ADMOB
		this.m_AdmobButton.interactable = CGameSetting.IsTimerToAd(CGameSetting.DELAY_TO_AD);
	}

	public override void OnDestroyObject()
	{
		base.OnDestroyObject();
	}

	#endregion 

	#region Public

	public virtual void Show(string title, CPlayerDisplay[] results, bool showAds, System.Action callback)
	{
		this.m_TitleText.text = title;
		this.m_AdmobButton.gameObject.SetActive (showAds);
		for (int i = 0; i < this.m_ResultItems.Count; i++)
		{
			this.m_ResultItems[i].gameObject.SetActive(false);
		}
		// RESULTS
		for (int i = 0; i < results.Length; i++)
		{
			if (i >= this.m_ResultItems.Count) 
			{
				var item = GameObject.Instantiate(this.m_ResultItemPrefab);
				item.transform.SetParent(this.m_BodyPanel.transform);
				item.transform.localPosition = Vector3.zero;
				item.transform.localRotation = Quaternion.identity;
				item.transform.localScale = Vector3.one;
				this.m_ResultItems.Add (item);
			}
			var avatar = results[i].playerAvatar;
			var playerName = results[i].playerName;
			var count = results[i].chessCount;
			this.m_ResultItems[i].Setup(i, avatar, playerName, count);
			this.m_ResultItems[i].gameObject.SetActive(true);
		}
		// ANIMATOR
		this.SetAnimator ("IsShow");
		// QUIT
		this.m_QuitButton.onClick.RemoveAllListeners();
		this.m_QuitButton.onClick.AddListener (() => {
			if (callback != null)
			{
				callback();
			}
		});
	} 

	public virtual void SetAnimator(string name, object value = null)
	{
		if (this.m_Animator == null)
			return;
		if (value is bool)
		{
			this.m_Animator.SetBool (name, (bool) value);
		}
		else if (value is int)
		{
			this.m_Animator.SetInteger (name, (int) value);
		}
		else if (value is float)
		{
			this.m_Animator.SetFloat (name, (int) value);
		}
		else if (value == null)
		{
			this.m_Animator.SetTrigger(name);
		}
	}

	#endregion 

	#region Private

	private void OnAdmobClick()
	{
		CAdmobManager.OnVideoAdsReward -= this.OnHandleRewardAddGold;
		if (CGameSetting.IsTimerToAd(CGameSetting.DELAY_TO_AD))
		{
			CAdmobManager.OnVideoAdsReward += this.OnHandleRewardAddGold;
			CAdmobManager.ShowRewardedVideo();
			CGameSetting.ResetTimerToAd();
			this.m_AdmobButton.interactable = false;
		}
		CSoundManager.Instance.Play ("sfx_click");
		// ADS SHOWING.
		CGameSetting.IS_ADS_SHOW_ON_SCREEN = true;
	}

	private void OnHandleRewardAddGold()
	{
		CGameSetting.USER_GOLD += CGameSetting.CURRENT_GOLD_MATCH;
		CAdmobManager.OnVideoAdsReward -= this.OnHandleRewardAddGold;
	}

	#endregion 

}
