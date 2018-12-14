using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CChess : MonoBehaviour {

	[SerializeField]	protected Animator m_Animator;
	[SerializeField]	protected Image m_WhiteTeam;
	[SerializeField]	protected Image m_BlackTeam;

	protected WaitForSeconds m_WaitEndAnimation = new WaitForSeconds(0.25f);

	public virtual void Init()
	{
		// ANIMATOR
		this.m_Animator = this.gameObject.GetComponent<Animator>();
		// UI
		this.m_WhiteTeam = this.transform.Find("WhiteTeam").GetComponent<Image>();
		this.m_BlackTeam = this.transform.Find("BlackTeam").GetComponent<Image>();
	}

	public virtual void Set(int team)
	{
		this.Set ((ETeam) team);
	}

	public virtual void Set(ETeam team)
	{
		this.Set (team, "white_chess", "black_chess");
	}

	public virtual void Set(int team, string whiteTeam, string blackTeam)
	{
		this.Set ((ETeam) team, whiteTeam, blackTeam);
	}

	public virtual void Set(ETeam team,  string whiteTeam, string blackTeam)
	{
		// UI
		this.m_WhiteTeam.sprite = CGameSetting.GetChessSprite (whiteTeam);
		this.m_BlackTeam.sprite = CGameSetting.GetChessSprite (blackTeam);
		// CHANGE
		this.m_WhiteTeam.gameObject.SetActive (team == ETeam.WHITE_TEAM);
		this.m_BlackTeam.gameObject.SetActive (team == ETeam.BLACK_TEAM);
	}

	public virtual void Clear()
	{
		
	}

	public void Change (ETeam team)
	{
		// ANIMATION
		this.SetAnimator ("IsChange");
		// CHANGE
		StopCoroutine (this.HandleChangeFace(team));
		StartCoroutine (this.HandleChangeFace(team));
	}

	protected virtual IEnumerator HandleChangeFace(ETeam team)
	{
		yield return this.m_WaitEndAnimation;
		// CHANGE
		this.m_WhiteTeam.gameObject.SetActive (team == ETeam.WHITE_TEAM);
		this.m_BlackTeam.gameObject.SetActive (team == ETeam.BLACK_TEAM);
	}

	public virtual void SetAnimator(string name, object value = null)
	{
		if (this.m_Animator == null)
			return;
		if (this.m_Animator.isInitialized == false)
			return;
		if (value is int)
		{
			this.m_Animator.SetInteger (name, (int) value);
		}
		else if (value is float)
		{
			this.m_Animator.SetFloat (name, (float) value);
		}
		else if (value is bool)
		{
			this.m_Animator.SetBool (name, (bool) value);
		}
		else if (value == null)
		{
			this.m_Animator.SetTrigger (name);
		}
	}

}
