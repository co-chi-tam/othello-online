using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CChessPosition {

	public int x;
	public int y;
	public ETeam team;

	public CChessPosition()
	{
		this.x = -1;
		this.y = -1;
		this.team = ETeam.NONE;
	}

}
