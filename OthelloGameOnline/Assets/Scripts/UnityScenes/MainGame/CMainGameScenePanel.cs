using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SocketIO;

public class CMainGameScenePanel : CDefaultScene {

	#region Fields

	protected CCell m_CellPrefab;
	protected GameObject m_ChessGridPanel;

	protected CTable m_Table;
	protected List<CCell> m_ListCellResults;

	protected Button m_QuitButton;

	protected Animator m_PlayerDisplayAnimator;

	protected Text m_TimerText;

	protected CPlayerDisplay[] m_Players;
	protected GameObject m_YourTurnPanel;

	protected GameObject m_WaitingPlayerPanel;
	protected Text m_WaitingPlayerText;
	protected Button m_CancelWaitingButton;

	protected JSONObject m_SendChessJSON;
	protected List<CChessPosition> m_RemainChesses;

	#endregion

	#region Constructor

	public CMainGameScenePanel() : base()
	{

	}

	public CMainGameScenePanel(string sceneName, string sceneObject): base(sceneName, sceneObject)
	{
		
	}

	#endregion

	#region Implementation Default

	public override void OnInitObject()
	{
		base.OnInitObject();
		// UI 
		this.m_QuitButton = CRootManager.FindObjectWith (GameObject, "QuitButton").GetComponent<Button>();
		this.m_QuitButton.onClick.AddListener (this.OnQuitButtonClick);
		this.m_CellPrefab = Resources.Load<CCell> ("Table/WoodCell");
		this.m_ChessGridPanel = CRootManager.FindObjectWith(GameObject, "ChessGridPanel");
		this.m_Table = new CTable(CTable.BOARD_WIDTH, CTable.BOARD_HEIGHT);
		if (this.m_CellPrefab != null)
		{
			for (int y = 0; y < CTable.BOARD_HEIGHT; y++)
			{
				for (int x = 0; x < CTable.BOARD_WIDTH; x++)
				{
					var cell = CRootManager.Instantiate(this.m_CellPrefab);
					cell.transform.SetParent (this.m_ChessGridPanel.transform);
					cell.transform.localPosition = Vector3.zero;
					cell.transform.localRotation = Quaternion.identity;
					cell.transform.localScale = Vector3.one;
					cell.Init();
					cell.Setup (null, string.Format ("Cell {0}-{1}", x, y), x, y, this.OnBoardCellPressed);
					this.m_Table.grids [x, y] = cell;
				}
			}
		}
		this.m_YourTurnPanel = CRootManager.FindObjectWith(GameObject, "YourTurnPanel");
		this.m_YourTurnPanel.SetActive(false);
		this.m_WaitingPlayerPanel = CRootManager.FindObjectWith(GameObject, "WaitingPlayerPanel");
		this.m_WaitingPlayerText = CRootManager.FindObjectWith(GameObject, "WaitingPlayerText").GetComponent<Text>(); 
		this.m_CancelWaitingButton = CRootManager.FindObjectWith(GameObject, "CancelWaitingButton").GetComponent<Button>();
		this.m_CancelWaitingButton.onClick.AddListener (this.OnQuitButtonClick);
		// LOGIC
		this.m_ListCellResults = new List<CCell>();
		// ANIMATOR
		this.m_PlayerDisplayAnimator = CRootManager.FindObjectWith(GameObject, "PlayersPanel").GetComponent<Animator>();
		// TIMER
		this.m_TimerText = CRootManager.FindObjectWith(GameObject, "TimerText").GetComponent<Text>();
		// PLAYERS
		this.m_Players = new CPlayerDisplay[2]; 
		this.m_Players[0] = CRootManager.FindObjectWith(GameObject, "Player (1)").GetComponent<CPlayerDisplay>();
		this.m_Players[0].Init();
		this.m_Players[0].gameObject.SetActive (false);
		this.m_Players[1] = CRootManager.FindObjectWith(GameObject, "Player (2)").GetComponent<CPlayerDisplay>();
		this.m_Players[1].Init();
		this.m_Players[1].gameObject.SetActive (false);
		// EVENTS
		CSocketManager.Instance.On("receiveTurnIndex", this.OnReceiveTurnIndex);
		CSocketManager.Instance.On("newJoinRoom", this.OnNewJoinRoom);
		CSocketManager.Instance.On("startGame", this.OnStartGame);
		CSocketManager.Instance.On("clearRoom", this.OnClearRoom);
		CSocketManager.Instance.On("receiveChessPosition", this.OnReceiveChessPosition);
		CSocketManager.Instance.On("counterDownAnswer", this.OnCounterDownAnswer);

		// DATA
		this.m_SendChessJSON = new JSONObject();
		this.m_RemainChesses = new List<CChessPosition>();
	}

	public override void OnStartObject()
	{
		base.OnStartObject();
		// CURRENT GOLD
		CGameSetting.CURRENT_GOLD_MATCH = 0;
	}

	public override void OnDestroyObject()
	{
		base.OnDestroyObject();
		// CLEAR BOARD
		this.OnClearBoard();
		// CURRENT GOLD
		CGameSetting.CURRENT_GOLD_MATCH = 0;
	}

	public override void OnBackPress()
	{
		// base.OnBackPress()	
	}
	
	#endregion

	#region Private

	private void OnReceiveTurnIndex(SocketIOEvent ev)
	{
		// Debug.Log ("OnReceiveTurnIndex " + ev.data.ToString());
		CGameSetting.CLIENT_TURN_INDEX = int.Parse (ev.data.GetField("turnIndex").ToString());
		CGameSetting.CLIENT_TEAM = (ETeam) int.Parse (ev.data.GetField("team").ToString());
	}

	private void OnNewJoinRoom(SocketIOEvent ev)
	{
		// Debug.Log ("OnNewJoinRoom " + ev.data.ToString());
		this.m_WaitingPlayerPanel.SetActive(true);
		var room = ev.data.GetField ("roomInfo");
		var roomName = room.GetField("roomName");
		var players = room.GetField ("players").list;
		var roomWaitingDisplay = roomName + "\n";
		for (int i = 0; i < players.Count; i++)
		{
			roomWaitingDisplay += players[i].GetField("playerName").ToString() + "\n";
		}
		this.m_WaitingPlayerText.text = roomWaitingDisplay;

		CSoundManager.Instance.Play ("sfx_new_round");
	}

	private void OnClearRoom(SocketIOEvent ev)
	{
		// Debug.Log ("OnClearRoom " + ev.data.ToString());
		if (this.GameObject.activeInHierarchy == false)
			return;
		if (CGameSetting.IS_PLAYED_GAME)
		{
			this.OnEndGame();
		}
		else
		{
			CRootManager.Instance.BackTo ("LobbyDisplayPanel");
		}
	}

	private void OnStartGame (SocketIOEvent ev)
	{
		// Debug.Log ("OnStartGame " + ev.data.ToString());
		this.m_WaitingPlayerPanel.SetActive(false);
		CGameSetting.IS_PLAYED_GAME = true;
		// START CHESSES
		var startChesses = ev.data.GetField("startChesses").list;
		for (int i = 0; i < startChesses.Count; i++)
		{
			var chess = startChesses[i];
			var x = int.Parse (chess.GetField("x").ToString());
			var y = int.Parse (chess.GetField("y").ToString());
			var team = (ETeam) int.Parse (chess.GetField("team").ToString());
			this.m_Table.grids [x, y].currentChess = CGameSetting.GetChess();
			this.m_Table.grids [x, y].currentTeam = team;
			this.m_Table.grids [x, y].currentChess.Init ();
			this.m_Table.grids [x, y].currentChess.Set (team);
		}
		// PLAYERS
		var players = ev.data.GetField("players").GetField("players").list;
		for (int i = 0; i < 2; i++)
		{
			var avatar = int.Parse (players[i].GetField("playerAvatar").ToString());
			var playerName = players[i].GetField("playerName").ToString().Replace("\"", string.Empty);
			var team = (ETeam) int.Parse (players[i].GetField("team").ToString());
			this.m_Players[i].playerTeam = team;
			this.m_Players[i].Setup (avatar, 
				team == ETeam.WHITE_TEAM ? "white_chess" : "black_chess", 
				playerName);
			this.m_Players[i].gameObject.SetActive (true);
		}
		// FIRST TEAM
		var firstTeam = (ETeam) int.Parse (ev.data.GetField("firstTeam").ToString());
		this.ActivePlayer(firstTeam);
		// TURN
		CGameSetting.CLIENT_COUNT_TURN = players.Count;
		CGameSetting.CURRENT_TEAM = firstTeam;
		// DATA
		this.m_RemainChesses.Clear();
		this.m_RemainChesses.TrimExcess();
	}

	private void OnQuitButtonClick()
	{
		if (this.GameObject.activeInHierarchy == false)
			return;
		// BACK
		var confirm = CRootManager.Instance.ShowPopup("ConfirmPopup") as CConfirmPopup;
		confirm.Show ("QUIT", 
			"Are you want quit game ?",
			"OK", () => {
				CSocketManager.Instance.Emit("leaveRoom");
			},
			"CANCEL", () => {
				confirm.OnBackPress();
			}
		);
		CSoundManager.Instance.Play ("sfx_click");
	}

	private void OnReceiveChessPosition(SocketIOEvent ev)
	{
		// Debug.Log ("OnReceiveChessPosition " + ev.data.ToString());
		// var playerName = ev.data.GetField("playerName").ToString();
		var currentPos = ev.data.GetField("currentPos");
		var x = int.Parse (currentPos.GetField("x").ToString());
		var y = int.Parse (currentPos.GetField("y").ToString());
		var team = (ETeam) int.Parse (currentPos.GetField("team").ToString());
		var activeTeam = (ETeam) int.Parse (ev.data.GetField("activeTeam").ToString()); 
		this.OnBoardCellPressed(x, y, team);
		this.ActivePlayer (activeTeam);
		CGameSetting.CURRENT_TEAM = activeTeam;
		if (this.m_Table.IsTableAvailable(activeTeam))
		{
			this.OnUpdateGame();
		}
		else
		{
			CSocketManager.Instance.Emit("leaveRoom");
		}
	}

	private void OnCounterDownAnswer(SocketIOEvent ev)
	{
		var answerTimer = int.Parse (ev.data.GetField("answerTimer").ToString());
		var minute = answerTimer / 60;
		var second = answerTimer % 60;
		this.m_TimerText.text = string.Format("{0}:{1}", minute.ToString("d2"), second.ToString("d2"));
	}

	#endregion

	#region Public

	public virtual void OnUpdateGame()
	{
		for (int i = 0; i < this.m_Players.Length; i++)
		{
			var team = this.m_Players[i].playerTeam;
			var chessCount = this.m_Table.GetCountChess(team);
			this.m_Players[i].chessCount = chessCount;
		}
	}

	public virtual void OnEndGame()
	{
		var whiteTeam = this.m_Table.GetCountChess(ETeam.WHITE_TEAM);
		var blackTeam = this.m_Table.GetCountChess(ETeam.BLACK_TEAM);
		var goldEarned = Mathf.Abs (whiteTeam - blackTeam);
		var result = CRootManager.Instance.ShowPopup("MatchResultPopup") as CMatchResultPopup;
		var winnerName = "DRAW";
		var largePoint = -999;
		for (int i = 0; i < this.m_Players.Length; i++)
		{
			var team = this.m_Players[i].playerTeam;
			var chessCount = this.m_Table.GetCountChess(team);
			if (chessCount > largePoint)
			{
				winnerName = this.m_Players[i].playerName;
				largePoint = chessCount;
			}
		}
		result.Show (winnerName, this.m_Players, goldEarned > 0, () => {
			CRootManager.Instance.BackTo ("LobbyDisplayPanel");
		});

		if (winnerName == CGameSetting.USER_NAME)
			CGameSetting.USER_GOLD += goldEarned;

		CGameSetting.IS_PLAYED_GAME = false;
		CGameSetting.CURRENT_GOLD_MATCH = goldEarned;

		for (int i = 0; i < this.m_Players.Length; i++)
		{
			this.m_Players[i].gameObject.SetActive(false);
		}
	}

	public virtual void OnClearBoard()
	{
		this.m_Table.Clear();
		CGameSetting.CLIENT_COUNT_TURN = 0;
	}

	public virtual void OnBoardCellPressed(int x, int y, ETeam team)
	{
		var cell = this.m_Table.grids[x, y];
		if (this.m_Table.OnCheckCell(team, cell, ref this.m_ListCellResults))
		{
			// SET UP CHESS
			var chess = CGameSetting.GetChess();
			chess.Init ();
			chess.Set (team);
			// CELL
			cell.currentChess = chess;
			cell.currentTeam = team;
			// UPDATE UI
			for (int i = 0; i < this.m_ListCellResults.Count; i++)
			{
				var currentCell = this.m_ListCellResults[i];
				var currentChess = currentCell.currentChess;
				currentCell.currentTeam = team;
				currentChess.Change (team);
			}
			CSoundManager.Instance.Play ("sfx_gain_point");
			this.m_RemainChesses.Add (new CChessPosition() { x = x, y = y, team = team });
		}
	}

	public virtual void OnBoardCellPressed(CCell cell)
	{
		// Debug.Log (cell.name);
		if (cell.currentChess != null)
			return;
		if (CGameSetting.CURRENT_TEAM != CGameSetting.CLIENT_TEAM)
		{
			var confirm = CRootManager.Instance.ShowPopup("ConfirmPopup") as CConfirmPopup;
			confirm.Show("ERROR", "This is not your turn.", "OK", () => {
				confirm.OnBackPress();
			});
		}
		else
		{
			if (this.m_Table.OnCheckCell(CGameSetting.CLIENT_TEAM, cell, ref this.m_ListCellResults))
			{
				this.m_SendChessJSON.Clear();
				this.m_SendChessJSON.AddField("posX", cell.posX);
				this.m_SendChessJSON.AddField("posY", cell.posY);
				this.m_SendChessJSON.AddField("turnIndex", CGameSetting.CLIENT_TURN_INDEX);
				CSocketManager.Instance.Emit("sendChessPosition", this.m_SendChessJSON);
			}
		}
	}

	public virtual void ActivePlayer(ETeam team)
	{
		if (this.m_PlayerDisplayAnimator != null)
		{
			this.m_PlayerDisplayAnimator.SetInteger ("IsActivePlayer", (int) team);
		}
		if (this.m_YourTurnPanel != null)
		{
			this.m_YourTurnPanel.gameObject.SetActive (team == CGameSetting.CLIENT_TEAM);
		}
	}

	#endregion

}
