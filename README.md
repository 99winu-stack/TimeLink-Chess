# TimeLink-Chess
A chess game with a unique timeline-linking mechanic.

# Core Mechanics
- During their "global" turn, a player can return to an earlier position and create a new timeline by making a different move from that position.
- The global turn determines which player is allowed to make the next move across all timelines.
- Each timeline maintains its own turn order, which determines which player would normally move next on that specific board.
- All timelines share the same pieces. If a piece is captured in one timeline, it is removed from every timeline.
- If a pawn is promoted in one timeline, it is promoted in every timeline.

# Timeline Rules
- A player can only create a new timeline during their own turn.
- When creating a timeline from a previous board state, the player must be the player whose turn it is in that history state.
- A player cannot create a new timeline if any piece belonging to the opponent has been captured since the selected history position.
- Each player can create only one timeline.
- A player cannot ignore a board indefinitely.
- A player can never make two consecutive moves on the same board.
- A player may create a new timeline while in check.
- Being checkmated on another board does not cause a loss if the player is not required to move on that board.
- If a player is checkmated on a board when that board requires the player to move, they lose the game.
- If a player can capture the opponent's king on their next move, they win the game immediately.

# Development
- This project starts by implementing a classic chess game following a YouTube tutorial:
https://www.youtube.com/watch?v=GEkSE6eZMGc&list=PLFk1_lkqT8MahHPi40ON-jyo5wiqnyHsL&index=1
- The project was then extended with custom mechanics, including linked timelines, cross-timeline captures, and timeline-specific gameplay rules.

# Future Plans
- Implement online multiplayer.
- Provide a downloadable release.
