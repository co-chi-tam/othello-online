
const DELTA_TIMER = 1;

function GameRoom() {
    // Current room name.
    this.roomName = '';
    // All players 
    this.players = [];
    this.maximumPlayers = 2;
	
	this.roomState = 'WAITING_PLAYER'; // WAITING_PLAYER | PLAYING_GAME | END_GAME
    this.timeLimitToAnswer = 30;
    this.answerTimer = 30;
	
    // Turn index X or O
    this.turns = [0, 1];    // 'WHITE', 'BLACK'
    this.teams = [1, 2];
    // LOGIC GAME
    // Remain all chess actived
    this.chessLists = [];
    this.turnCount = 0;
    // this.tableGrid = [
    //     [0, 0, 0, 0, 0, 0, 0, 0],
    //     [0, 0, 0, 0, 0, 0, 0, 0],
    //     [0, 0, 0, 0, 0, 0, 0, 0],
    //     [0, 0, 0, 1, 2, 0, 0, 0],
    //     [0, 0, 0, 2, 1, 0, 0, 0],
    //     [0, 0, 0, 0, 0, 0, 0, 0],
    //     [0, 0, 0, 0, 0, 0, 0, 0],
    //     [0, 0, 0, 0, 0, 0, 0, 0]
    // ];
	
	// UPDATE PER SECONDS.
    this.updateTimer = function() {
		if (this.roomState == 'PLAYING_GAME')
		{
			// TIMER TO ANSWER
			if (this.answerTimer > 0)
			{
				this.emitAll("counterDownAnswer", { answerTimer: this.answerTimer });
			}
			else
			{
				this.clearRoom();
			}
			this.answerTimer -= DELTA_TIMER;
		}
    }
    
    // Get current turn.
    this.currentTurn = function() {
        return this.chessLists.length % this.maximumPlayers; // WHITE or BLACK
    }

    // GET CURRENT PLAYER
    this.currentPlayer = function() {
        var index = this.currentTurn();
        if (index > -1)
        {
            return this.players[index];
        }
        return null;
    }

    // Join room and set turn index for player
    this.join = function (player) {
        if (this.players.indexOf (player) == -1) {
            this.players.push (player);
            for (let i = 0; i < this.players.length; i++) {
                const ply = this.players[i];
                const turn = this.turns [i % this.maximumPlayers];
                const team = this.teams [i % this.maximumPlayers];
                ply.player.turnIndex = turn; // WHITE or BLACK
                ply.player.team = team;
                ply.emit('receiveTurnIndex', {
                    turnIndex: turn,
                    team: team
                });
            }
        }
    };

    // Clear room
    this.clearRoom = function() {
        for (let i = 0; i < this.players.length; i++) {
            const ply = this.players[i];
            ply.emit('clearRoom', {
                msg: "Room is empty or player is quit."
            });
        }
        this.players = [];
        this.chessLists = [];
        this.turnCount = 0;
    };
    
    // Leave room 
    this.leave = function(player) {
        if (this.players.indexOf (player) > -1) {
            this.players.splice (this.players.indexOf (player), 1);
            console.log ('User LEAVE ROOM...' + player.player);
        }
    };
	
	// ADD TURN 
	this.addNewPos = function (pos)
	{
		// TIMER
        this.answerTimer = this.timeLimitToAnswer;
		
		this.chessLists.push(pos);
	}
    
    // Send all mesg for players in room.
    this.emitAll  = function (name, obj) {
        for (let i = 0; i < this.players.length; i++) {
            const player = this.players[i];
            player.emit(name, obj);
        }
    };

    // Get rom info.
    this.getInfo = function() {
        var playerInfoes = [];
        for (let i = 0; i < this.players.length; i++) {
            const player = this.players[i];
            playerInfoes.push (player.player);
        }
        return {
            roomName: this.roomName,
            players: playerInfoes
        };
    }

    // If client contain in room.
    this.contain = function (player) {
        return this.players.indexOf (player) > -1;
    };
    
    // Get amount of players in room.
    this.length  = function () {
        return this.players.length;
    };
};
// INIT
module.exports = GameRoom;