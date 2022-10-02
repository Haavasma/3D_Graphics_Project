# Online Jenga


This is a project developed for a 3D-graphics subject in NTNU.


## Frontend

A unity-application for playing jenga online.

The application includes a simplified version of jenga where the player has to 
remove a piece turn by turn without making the tower fall.


The application communicates with a server using custom-made library for communicating
with the game server. 


## Server

The game server is a networking server made from scratch using TCP and UDP sockets
to set up communication between players.
The server includes a system for queueing up players as well as setting up channels for queued 
players to communicate in. 

Since playing jenga online in real-time requires fast but not necessarily concise information between players,
the communcation of jenga piece locations and states is transferred with UDP sockets.

The system for queueing up players and finding other players, as well as setting up games and determining results
is using TCP. 


## How to play

WIP: I've currently no time to continue this project or maintaining it, but might revisit and rework it for a full finished game
later. 
