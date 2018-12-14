using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CCell : MonoBehaviour {

	[SerializeField]	protected CChess m_CurrentChess;
	public CChess currentChess 
	{ 
		get { return this.m_CurrentChess; }
		set 
		{ 
			this.m_CurrentChess = value; 
			if (value != null)
			{
				this.m_CurrentChess.transform.SetParent (this.transform);
				this.m_CurrentChess.transform.localPosition = Vector3.zero;
				this.m_CurrentChess.transform.localRotation = Quaternion.identity;
				this.m_CurrentChess.transform.localScale = Vector3.one;
				this.m_CurrentChess.gameObject.SetActive (true);
			}
		}
	}
	[SerializeField]	protected ETeam m_CurrentTeam;
	public ETeam currentTeam 
	{ 
		get { return this.m_CurrentTeam; }
		set { this.m_CurrentTeam = value; }
	}
	[SerializeField]	protected Button m_Button;
	[SerializeField]	protected int m_PosX;
	public int posX 
	{ 
		get { return this.m_PosX; }
		set { this.m_PosX = value; }
	}
	[SerializeField]	protected int m_PosY;
	public int posY 
	{ 
		get { return this.m_PosY; }
		set { this.m_PosY = value; }
	}

	public virtual void Init()
	{
		this.m_Button = this.GetComponent<Button>();
	}

	public virtual void Setup(CChess chess, string name, int x, int y, System.Action<CCell> onPressed)
	{
		this.m_CurrentChess = chess;
		this.name = name;
		this.m_CurrentTeam = ETeam.NONE;
		this.m_PosX = x;
		this.m_PosY = y;
		this.m_Button.onClick.RemoveAllListeners();
		this.m_Button.onClick.AddListener (() => {
			if (onPressed != null)
			{
				onPressed (this);
			}
		});
	}

	public virtual void Clear()
	{
		this.m_CurrentChess = null;
		this.m_CurrentTeam = ETeam.NONE;
		// this.m_Button.onClick.RemoveAllListeners();
	}
	
}
