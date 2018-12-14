using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ETeam: int 
{
	NONE = 0,
	WHITE_TEAM = 1,
	BLACK_TEAM = 2
}

public class CTable {

	protected CCell[,] m_Grids;
	public CCell[,] grids 
	{ 
		get { return this.m_Grids; }
		set { this.m_Grids = value; }
	}
	
	public static int BOARD_WIDTH = 8;
	public static int BOARD_HEIGHT = 8;

	public CTable()
	{
		// LOGIC
		this.m_Grids = new CCell[BOARD_WIDTH, BOARD_HEIGHT];
	}

	public CTable(int width, int height)
	{
		BOARD_WIDTH = width;
		BOARD_HEIGHT = height;
		// LOGIC
		this.m_Grids = new CCell[BOARD_WIDTH, BOARD_HEIGHT];
	}

	public virtual void Clear()
	{
		for (int y = 0; y < CTable.BOARD_HEIGHT; y++)
		{
			for (int x = 0; x < CTable.BOARD_WIDTH; x++)
			{
				var cell = this.m_Grids [x, y];
				var chess = cell.currentChess;
				if (chess != null)
				{
					chess.Clear();
					CGameSetting.SetChess (chess);
				}
				cell.Clear();
			}
		}
	}

	public virtual bool IsTableAvailable()
	{
		for (int y = 0; y < CTable.BOARD_HEIGHT; y++)
		{
			for (int x = 0; x < CTable.BOARD_WIDTH; x++)
			{
				var cell = this.m_Grids[x, y];
				if (cell.currentTeam == ETeam.NONE)
					return true;
			}
		}
		return false;
	}

	protected List<CCell> m_ListCellResults = new List<CCell>();
	public virtual bool IsTableAvailable(ETeam team)
	{
		// DIMENSION RESULT
		this.ClearResult();
		for (int y = 0; y < CTable.BOARD_HEIGHT; y++)
		{
			for (int x = 0; x < CTable.BOARD_WIDTH; x++)
			{
				var cell = this.m_Grids[x, y];
				if (cell.currentTeam == ETeam.NONE)
				{
					if (this.OnCheckCell(team, cell, ref this.m_ListCellResults))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	protected Dictionary<int, List<CCell>> m_DimensionResult = new Dictionary<int, List<CCell>>();
	public virtual bool OnCheckCell(ETeam team, CCell value, ref List<CCell> results)
	{
		if (team == ETeam.NONE)
			return false;
		if (results == null)
			return false;
		results.Clear();
		results.TrimExcess();
		results.Add (value);
		if (value == null)
			return false;
		// DIMENSION RESULT
		this.ClearResult();
		this.CheckDimensionResult(team, value);
		// UPDATE VALUE
		foreach (var item in this.m_DimensionResult)
		{
			var cells = item.Value;
			if (cells.Count > 1)
			{
				var last = cells[cells.Count - 1];
				if (last.currentTeam == team)
				{
					results.AddRange (cells);
				}
			}
		}
		// RESULTS
		return results.Count > 1;
	}

	public virtual void CheckDimensionResult(ETeam team, CCell value)
	{
		var checkX = value.posX;
		var checkY = value.posY;
		var dimension = 1;
		while (dimension < 9) {
			switch (dimension)
			{
				// TOP == 1
				case 1:
				if (checkY - 1 >= 0) {
					var cell = this.m_Grids[checkX, checkY - 1];
					if (cell.currentTeam != ETeam.NONE) {
						if (cell.currentTeam != team) {
							this.m_DimensionResult[dimension].Add(cell);
							checkX = cell.posX;
							checkY = cell.posY;
						} else {
							this.m_DimensionResult[dimension].Add(cell);
							dimension += 1;
							checkX = value.posX;
							checkY = value.posY;
						}
					}
					else
					{
						dimension += 1;
						checkX = value.posX;
						checkY = value.posY;
					}
				} else {
					dimension += 1;
					checkX = value.posX;
					checkY = value.posY;
				}
				break;
				// TOP RIGHT == 2
				case 2:
				if (checkX + 1 < BOARD_WIDTH && checkY - 1 >= 0) {
					var cell = this.m_Grids[checkX + 1, checkY - 1];
					if (cell.currentTeam != ETeam.NONE) {
						if (cell.currentTeam != team) {
							this.m_DimensionResult[dimension].Add(cell);
							checkX = cell.posX;
							checkY = cell.posY;
						} else {
							this.m_DimensionResult[dimension].Add(cell);
							dimension += 1;
							checkX = value.posX;
							checkY = value.posY;
						}
					}
					else
					{
						dimension += 1;
						checkX = value.posX;
						checkY = value.posY;
					}
				} else {
					dimension += 1;
					checkX = value.posX;
					checkY = value.posY;
				}
				break;
				// RIGHT == 3
				case 3:
				if (checkX + 1 < BOARD_WIDTH) {
					var cell = this.m_Grids[checkX + 1, checkY];
					if (cell.currentTeam != ETeam.NONE) {
						if (cell.currentTeam != team) {
							this.m_DimensionResult[dimension].Add(cell);
							checkX = cell.posX;
							checkY = cell.posY;
						} else {
							this.m_DimensionResult[dimension].Add(cell);
							dimension += 1;
							checkX = value.posX;
							checkY = value.posY;
						}
					}
					else
					{
						dimension += 1;
						checkX = value.posX;
						checkY = value.posY;
					}
				} else {
					dimension += 1;
					checkX = value.posX;
					checkY = value.posY;
				}
				break;
				// DOWN RIGHT == 4
				case 4:
				if (checkX + 1 < BOARD_WIDTH && checkY + 1 < BOARD_HEIGHT) {
					var cell = this.m_Grids[checkX + 1, checkY + 1];
					if (cell.currentTeam != ETeam.NONE) {
						if (cell.currentTeam != team) {
							this.m_DimensionResult[dimension].Add(cell);
							checkX = cell.posX;
							checkY = cell.posY;
						} else {
							this.m_DimensionResult[dimension].Add(cell);
							dimension += 1;
							checkX = value.posX;
							checkY = value.posY;
						}
					}
					else
					{
						dimension += 1;
						checkX = value.posX;
						checkY = value.posY;
					}
				} else {
					dimension += 1;
					checkX = value.posX;
					checkY = value.posY;
				}
				break;
				// DOWN == 5
				case 5:
				if (checkY + 1 < BOARD_HEIGHT) {
					var cell = this.m_Grids[checkX, checkY + 1];
					if (cell.currentTeam != ETeam.NONE) {
						if (cell.currentTeam != team) {
							this.m_DimensionResult[dimension].Add(cell);
							checkX = cell.posX;
							checkY = cell.posY;
						} else {
							this.m_DimensionResult[dimension].Add(cell);
							dimension += 1;
							checkX = value.posX;
							checkY = value.posY;
						}
					}
					else
					{
						dimension += 1;
						checkX = value.posX;
						checkY = value.posY;
					}
				} else {
					dimension += 1;
					checkX = value.posX;
					checkY = value.posY;
				}
				break;
				// LEFT DOWN == 6
				case 6:
				if (checkX - 1 >= 0 && checkY + 1 < BOARD_HEIGHT) {
					var cell = this.m_Grids[checkX - 1, checkY + 1];
					if (cell.currentTeam != ETeam.NONE) {
						if (cell.currentTeam != team) {
							this.m_DimensionResult[dimension].Add(cell);
							checkX = cell.posX;
							checkY = cell.posY;
						} else {
							this.m_DimensionResult[dimension].Add(cell);
							dimension += 1;
							checkX = value.posX;
							checkY = value.posY;
						}
					}
					else
					{
						dimension += 1;
						checkX = value.posX;
						checkY = value.posY;
					}
				} else {
					dimension += 1;
					checkX = value.posX;
					checkY = value.posY;
				}
				break;
				// LEFT == 7
				case 7:
				if (checkX - 1 >= 0) {
					var cell = this.m_Grids[checkX - 1, checkY];
					if (cell.currentTeam != ETeam.NONE) {
						if (cell.currentTeam != team) {
							this.m_DimensionResult[dimension].Add(cell);
							checkX = cell.posX;
							checkY = cell.posY;
						} else {
							this.m_DimensionResult[dimension].Add(cell);
							dimension += 1;
							checkX = value.posX;
							checkY = value.posY;
						}
					}
					else
					{
						dimension += 1;
						checkX = value.posX;
						checkY = value.posY;
					}
				} else {
					dimension += 1;
					checkX = value.posX;
					checkY = value.posY;
				}
				break;
				// LEFT TOP == 8
				case 8:
				if (checkX - 1 >= 0 && checkY - 1 >= 0) {
					var cell = this.m_Grids[checkX - 1, checkY - 1];
					if (cell.currentTeam != ETeam.NONE) {
						if (cell.currentTeam != team) {
							this.m_DimensionResult[dimension].Add(cell);
							checkX = cell.posX;
							checkY = cell.posY;
						} else {
							this.m_DimensionResult[dimension].Add(cell);
							dimension += 1;
							checkX = value.posX;
							checkY = value.posY;
						}
					}
					else
					{
						dimension += 1;
						checkX = value.posX;
						checkY = value.posY;
					}
				} else {
					dimension += 1;
					checkX = value.posX;
					checkY = value.posY;
				}
				break;
			}
		}
	}

	public virtual void ClearResult()
	{
		this.m_DimensionResult.Clear();
		for (int face = 1; face < 9; face++)
		{
			if (this.m_DimensionResult.ContainsKey(face))
			{
				this.m_DimensionResult[face].Clear();
				this.m_DimensionResult[face].TrimExcess();
			}
			else
			{
				this.m_DimensionResult[face] = new List<CCell>();
			}
		}
	}

	public virtual int GetCountChess(ETeam team)
	{
		var results = 0;
		for (int y = 0; y < CTable.BOARD_HEIGHT; y++)
		{
			for (int x = 0; x < CTable.BOARD_WIDTH; x++)
			{
				var cell = this.m_Grids[x, y];
				if (cell.currentTeam == team)
					results++;
			}
		}
		return results;
	}

}
