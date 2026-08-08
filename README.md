# TimeLink-Chess
A chess game with a unique timeline-linking mechanic.

# Mechanic
- A player can return to an earlier position and make a different move.
- This creates a new board (timeline).
- All timelines are linked together.
- If a piece is captured in one timeline, it is removed from every timeline.
- If a pawn is promoted in one timeline, it is promoted in every timeline.

# Timeline Rules
- A player can only create a new timeline in their own turn.
- When creating a timeline from a previous board state, the player must be the player whose turn it is in that history state.
- A player cannot create a new timeline if any piece belonging to the opponent has been captured since the selected history position.
- Each player can create only one timeline.
- Every board must be played at least once within four of the player's own turns. A player cannot ignore a board indefinitely.
- A player can never make two consecutive moves on the same board.
- A player may create a new timeline while in check.
- Being checkmated on another board does not cause a loss if the player is not required to move on that board.
- If a player can capture the opponent's king on their next move, they win the game immediately.
- If a player is checkmated on a board when that board requires the player to move, they lose the game.

# Development
- This project starts by implementing a classic chess game following a YouTube tutorial:
https://www.youtube.com/watch?v=GEkSE6eZMGc&list=PLFk1_lkqT8MahHPi40ON-jyo5wiqnyHsL&index=1
- The custom mechanics like timeline linking will be added afterward.
