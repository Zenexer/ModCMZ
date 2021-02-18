# ModCMZ #

This is a modding framework for CastleMiner Z that I tinkered with somewhere in the vicinity of 2011-2013.  I dug it up in 2021 while attempting to solve some log-standing glitches in the original game.  I fixed a few bugs in ModCMZ before moving it to a git repo, but it's still not suitable for general use; it's unstable, contains lots of debug code, doesn't follow good coding practices, and is generally just meant as a convenient sandbox.

The technique used here is a simplified version of the bytecode manipulation trick I used with another XNA-based game around the same time period.  However, unlike the far-more-complex version for that game, this framework does need to actually save copies of the assemblies to disk for now.  Those will appear in the Mirror folder.  This also makes debugging any bytecode manipulation easier, since they can be opened in a tool like dnSpy or dotPeek.

# License #

There isn't any yet.  I intend to release it under an open source license once I've polished it up a bit.  If I've given you a copy of this code or a binary derived from it, please keep it to yourself for now.  I believe I had Digital DNA's permission to release this at one point, but that was nearly a decade ago, so I should probably attempt to renew that permission.

This repo shouldn't contain any reverse-engineered code from CastleMiner Z.  The point of this complicated bytecode manipulation system is to avoid the need for modders to modify CastleMiner Z directly--plus it's a heckuvalot easier on modders.  This means that redistributation of mod code doesn't need to involve redistribution of reverse-engineered Digital DNA code.  I'm not a lawyer, so I can't say whether this has any legal benefits, but I personally find it far more respectful than throwing large chunks of reverse engineered code online.

# Compiling #

Before attempting to compile the solution:

1. Make sure you're using Visual Studio 2019.
2. Follow the instructions in /Libraries/DNA/README.txt.
3. If your CastleMiner Z installation is in an unusual location, add it to the list in `ModCMZ.Core.App.FindSteamFolder` (Project `ModCMZ.Core`, file `App.cs`, method `FindSteamFolder`).

# Known issues #

* References need to be handled better.
* The program needs to actually determine the correct location for CMZ instead of relying on a list of hardcoded locations.
* The legacy ClickOnce code either needs to be tested if that version of the game still works or removed if it doesn't.  I've only been testing with Steam.
* Keyboard input for the console is problematic because `IMessageFilter` isn't receiving `WM_KEYDOWN`.
* I need to actually develop some decent commands for the console.
* I don't remember what state the asset mod system is in.  I suspect I never finished it.  Some of the related tools aren't in this repo because they looked like they contained third-party code, and I couldn't easily determine the license.
* This was originally written for the legacy ClickOnce version, not the Steam version.  I got it running more or less okay with Steam, but it likely needs further testing and improvement.
* I'm using a very old version of Mono.Cecil.  I know I deliberately used deprecated methods in that version based on experience gained from modding another game; the replacements at the time weren't capable of working correctly with the techniques I used.  It's likely that those issues have since been resolved.
* The API needs to be expanded so that mods don't have to resort to injection if they want to hook vanilla code.
* The console isn't nearly as powerful as the one I made for the other game.  I should port that code, assuming I can find it.  In addition to poor keyboard handling due to the issues with `WM_KEYDOWN`, pasting, complex navigation, highlighting, formatting, command history, and scrolling don't work yet.
* The generic Windows console could use some love.  There's really no reason it shouldn't be able to take input.
* Message boxes are used for error handling in places they probably shouldn't be.  Error handling in general is terrible.
* I started implementing async support in the mod API.  In hindsight, that was probably a bad idea, and I need to remove all that code.  Too many issues with XNA.  You'll see some locks and such scattered about; these were meant to provide a sort of rudimentary level of thread and concurrency safety, but any guarantees were broken nearly a decade ago as I was tinkering.  I removed most of the threading and async stuff before the initial commit, but I definitely missed quite a bit.

# Accessing the console #

Press <kbd>~</kbd> while in-game to show the console.  Type `help` or `/help` for a list of commands.  Press <kbd>Esc</kbd>, <kbd>~</kbd>, or <kbd>Enter</kbd> (with an empty input field) to close the console.

There aren't really any useful built-in commands, but any concrete classes extending `ICommand` found in mod assemblies will be loaded automatically and invoked when a matching command is entered.

# To-do #

* Fix *Known issues* (above)
* Implement some basic commands
* Fix issues in the game itself, such as:
	* Laser weapon memory leak
	* Crashing when switching windows
	* Item duplication glitch - since this is part of the vanilla experience, make the fix optional
	* Various teleporter glitches - idem
	* Floating point glitch with coordinates - idem
	* If someone's game crashes during Endurance, they should be able to reconnect.  Likewise, if the host crashes, Endurance should be resumable.
* Provide better feedback when the game crashes so that the causes can actually be determined
* Implement server-side security
* Automatically recover borked maps
* Add borderless windowed mode
