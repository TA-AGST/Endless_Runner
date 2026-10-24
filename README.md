# Endless Runner

A three lane endless runner made in Unity (URP). The road never ends, the speed grows the longer you run.

> **Note:** Logic build mein AI ki help li gayi hai.

## Controls
| Key | Action |
|---|---|
| A / D (or left / right arrow) | Change lane |
| Space (or W / up arrow) | Jump |
| S (or down arrow) | Slide (in the air: drop down fast) |
| R | Run again after a crash |
| F1 | God mode, for testing |

## Features
- **Endless road:** the road is made of 30 m pieces. A piece that is behind the player is moved to the front again, so no new objects are created while running.
- **Three obstacles you can see by their colour:** orange barrier (jump over it), blue beam (slide under it), red wall (change lane).
- **Fair patterns:** every row of obstacles can be passed. There is always a free lane, or something to jump over or slide under, the free lane stays near the last one, and there is enough road between two rows to react.
- **Difficulty grows:** the speed goes from 40 to 108 km/h in 150 seconds, harder patterns appear more often, and the camera opens up with the speed.
- The runner is animated in code: running legs, leaning in the turns, sliding and falling after a crash.

## Run it
1. Unity **6000.3.25f1** (Universal 3D / URP).
2. Open `Assets/Scenes/SampleScene.unity` and press Play.