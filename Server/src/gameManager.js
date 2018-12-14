
var GameRoom = require('./gameRoom'); // ROOM LOGIC

users = []; // Array User names
rooms = {}; // Rooms
column = 7; 
row = 8;

const MAXIMUM_ROOMS = 10; // Maximum rooms
const MAXIMUM_PLAYERS = 2; // MAXIMUM PLAYERS
const TIME_TO_ANSWER = 30;

var GameManager = function (http) {
    var io = require('socket.io')(http); // Require socket.io
	
	activeTimer();
    // INTERVAL TIMER 
    function activeTimer()
    {
        this.roomTimer = setInterval(function() {
            for (let i = 0; i < MAXIMUM_ROOMS; i++) {
                const roomName = 'Room-' + (i + 1);
                if (typeof (rooms [roomName]) !== 'undefined')
                {
                    rooms [roomName].updateTimer();
                }
            }
        }, 1000);
    }
    
    joinRoom = function(socket, roomName)
    {
        if(roomName && socket.player) {
            if (typeof(rooms [roomName]) === 'undefined') {
                rooms [roomName] = new GameRoom();
				rooms [roomName].roomState = 'WAITING_PLAYER';
            }
            rooms [roomName].roomName = roomName;
            rooms [roomName].maximumPlayers = MAXIMUM_PLAYERS;
            if (rooms [roomName].contain (socket) == false) {
                if (rooms [roomName].length() < MAXIMUM_PLAYERS) {
                    rooms [roomName].join (socket);
                    socket.room = rooms [roomName];
                    rooms [roomName].emitAll('newJoinRoom', {
                        roomInfo: rooms [roomName].getInfo()
                    });    
                    // console.log ("A player join room. " + roomName + " Room: " + rooms [roomName].length());
                    if (rooms [roomName].length() >= MAXIMUM_PLAYERS)
                    {
                        socket.room.chessLists.push({ x: 3, y: 3, team: 1 });
                        socket.room.chessLists.push({ x: 4, y: 3, team: 2 });
                        socket.room.chessLists.push({ x: 3, y: 4, team: 2 });
                        socket.room.chessLists.push({ x: 4, y: 4, team: 1 });
						
						rooms [roomName].answerTimer = TIME_TO_ANSWER;
						rooms [roomName].timeLimitToAnswer = TIME_TO_ANSWER;
						rooms [roomName].roomState = 'PLAYING_GAME';
                        rooms [roomName].emitAll('startGame', {
                            firstTeam: 1,
                            startChesses: socket.room.chessLists,
							players: rooms [roomName].getInfo()
                        });  
                    }
                } else {
                    socket.emit('joinRoomFailed', {
                        msg: "Room is full. Please try again late."
                    });
                }
            } else {
                socket.emit('joinRoomFailed', {
                    msg: "You are already join room."
                });
            }
        }
    }

    // On client connect.
    io.on('connection', function(socket) {
        // console.log('A user connected ' + (socket.client.id));
        // Welcome message
        socket.emit('welcome', { 
            msg: 'Welcome to connect game caro duel online.'
        });
        // INIT PLAYER
        // Set player name.
        socket.on('setPlayerData', function(data) {
            if (data) {
                var isDuplicateName = false;
                for (let i = 0; i < users.length; i++) {
                    const u = users[i];
                    if (u.playerName == data.playerName) {
                        isDuplicateName = true;    
                        break;
                    }
                }
                if(isDuplicateName) {
                    socket.emit('msgError', { 
                        msg: data.playerName  + ' username is taken! Try some other username.'
                    });
                } else {
                    if (data.playerName.length < 5 || data.playerName.length > 22) {
                        socket.emit('msgError', { 
                            msg: data.playerName  + ' username must longer than 5 character'
                        });
                    } else {
                        socket.player = {
                            playerAvatar: data.playerAvatar,
                            playerName: data.playerName,
                            turnIndex: -1,
                            team: 0,
                        };
                        users.push(data);
                        socket.emit('receivePlayerData', { 
                            id: socket.client.id,
                            playerAvatar: socket.player.playerAvatar,
                            playerName: socket.player.playerName
                        });
                    }
                }
            }
        });
        // Receive beep mesg
        socket.on('beep', function(data) {
        socket.emit('boop');
        })
        // INIT ROOM
        // Get all room status
        socket.on('getRoomsStatus', function() {
            var results = [];
            for (let i = 0; i < MAXIMUM_ROOMS; i++) {
                const roomName = 'Room-' + (i + 1);
                const playerCount = typeof (rooms [roomName]) !== 'undefined' 
                                        ? rooms [roomName].length()
                                        : 0;
                results.push ({
                    roomName: roomName,
                    players: playerCount,
                    maximumPlayers: MAXIMUM_PLAYERS,
                });
            }
            socket.emit('updateRoomStatus', {
                rooms: results
            });
        });
        // Join or create room by name. 
        socket.on('joinOrCreateRoom', function(playerJoin) {
            joinRoom (socket, playerJoin.roomName);
        });
        // Join or create room by name. 
        socket.on('joinExistedRoom', function(data) {
			if (data)
			{
				socket.player.playerAvatar = data.playerAvatar;
			}
            for (let i = 0; i < MAXIMUM_ROOMS; i++) {
                const roomName = 'Room-' + (i + 1);
                const playerCount = typeof (rooms [roomName]) !== 'undefined' 
                                        ? rooms [roomName].length()
                                        : 0;
                if (playerCount >= 0 && playerCount < MAXIMUM_PLAYERS)
                {
                    joinRoom (socket, roomName);
                    return;
                }
            }
        });
        // Send chess position with check available.
        socket.on('sendChessPosition', function(msg) {
            if(msg && socket.player && socket.room) {
                if (socket.room.length() > 1) {
                    var gameCurrentTurn = socket.room.currentTurn();
                    var currentPos = {
                        x: msg.posX,     // parseInt
                        y: msg.posY,     // parseInt
                        team: gameCurrentTurn + 1 // 1 or 2
                    }
                    var sendChecking = socket.player.turnIndex == msg.turnIndex 
                                    && socket.player.turnIndex == gameCurrentTurn;
                    // CAN NOT USE INDEXOF HERE  
                    for (let i = 0; i < socket.room.chessLists.length; i++) {
                        const chess = socket.room.chessLists[i];
						// console.log (JSON.stringify(currentPos) +" / "+ JSON.stringify(chess));
                        if (currentPos.x == chess.x && currentPos.y == chess.y) {
                            sendChecking = false;
                            break;
                        }
                    }
                    if (sendChecking) {
                        socket.room.addNewPos (currentPos);
						gameCurrentTurn = socket.room.currentTurn();
                        socket.room.emitAll('receiveChessPosition', {
                            playerName: socket.player.playerName,
                            currentPos,
                            turnIndex: gameCurrentTurn,
							activeTeam: gameCurrentTurn + 1
                        });
                    } else {
                        socket.emit('receiveChessFail', {
                            msg: msg.turnIndex != gameCurrentTurn 
                                ? "This is NOT your turn."
                                : "You can NOT do that."
                        });
                    }
                }
            }
        });
        // Receive client chat in current room.
        socket.on('sendRoomChat', function(msg) {
            if(msg && socket.room) {
                socket.room.emitAll('msgChatRoom', {
                    user: socket.player.playerName,
                    message: msg.message
                });
            }
        });
        // Receive world chat.
        socket.on('sendWorldChat', function(msg) {
            if(msg && socket.player) {
                // socket.broadcast.emit => will send the message to all the other clients except the newly created connection
                io.sockets.emit('msgWorldChat', {
                    user: socket.player.playerName,
                    message: msg.message
                });
            }
        });
        // Receive leave room mesg.
        socket.on('leaveRoom', function() {
            // console.log ('User leave room...' + socket.id);
            if (socket.room) {
                var roomName = socket.room.roomName;
                socket.room.clearRoom();
                delete socket.room;
                delete rooms [roomName];
            }
        });
        // DISCONNECT
        // Disconnect and clear room.
        socket.on('disconnect', function() {
            // console.log ('User disconnect...' + socket.id);
            if (socket.player) {
                for (let i = 0; i < users.length; i++) {
                    const u = users[i];
                    if (u.playerName == socket.player.playerName) {
                        users.splice(i, 1);  
                        break;
                    }
                }
            }
            // LEAVE ROOM
            if (socket.room) {
                var roomName = socket.room.roomName;
                socket.room.clearRoom();
                delete socket.room;
                delete rooms [roomName];
            }
        });
    });
    // SHUFFLE ARRAY
    this.shuffle = function(a) {
        var j, x, i;
        for (i = a.length - 1; i > 0; i--) {
            j = Math.floor(Math.random() * (i + 1));
            x = a[i];
            a[i] = a[j];
            a[j] = x;
        }
        return a;
    }
};
// INIT
module.exports = GameManager;