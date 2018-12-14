using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CPlayerDisplay : MonoBehaviour {

    protected Image m_AvatarImage;
    protected Image m_ChessTypeImage;
    protected Text m_PlayerChessCountText;
    protected Text m_PlayerNameText;

    protected int m_PlayerAvatar;
    public int playerAvatar 
    { 
        get
        {
            return this.m_PlayerAvatar;
        }
        set
        {
            this.m_PlayerAvatar = value;
            this.m_AvatarImage.sprite = CGameSetting.GetAvatarSprite(value);
        }
    }

    protected string m_PlayerName;
    public string playerName 
    { 
        get
        {
            return this.m_PlayerName;
        }
        set
        {
            this.m_PlayerName = value;
            this.m_PlayerNameText.text = value;
        }
    }
    protected int m_ChessCount;
    public int chessCount 
    {
        get 
        {
            return this.m_ChessCount;
        }
        set
        {
            this.m_ChessCount = value;
            this.m_PlayerChessCountText.text = value.ToString();
        }
    }

    public ETeam playerTeam;

    protected virtual void Awake()
    {

    }

    public virtual void Init()
    {
        this.m_AvatarImage = CRootManager.FindObjectWith (this.gameObject, "AvatarImage").GetComponent<Image>();
        this.m_ChessTypeImage = CRootManager.FindObjectWith (this.gameObject, "ChessTypeImage").GetComponent<Image>();
        this.m_PlayerChessCountText = CRootManager.FindObjectWith (this.gameObject, "PlayerChessCountText").GetComponent<Text>();
        this.m_PlayerNameText = CRootManager.FindObjectWith (this.gameObject, "PlayerNameText").GetComponent<Text>();
    }

    public virtual void Setup(int avatar, string chessPath, string playerName)
    {
        this.m_ChessTypeImage.sprite = CGameSetting.GetChessSprite(chessPath);
        this.playerAvatar = avatar;
        this.playerName = playerName;
        this.chessCount = 2;
    }
	
}
