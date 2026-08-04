![image](Tools/OpenSoMRuntimeLogo.png)\
Open SoM Runtime aims to create a full replacement of the original Sword of Moonlight Runtime, 
to resolve long standing issues and provide a more adaptable solution for creating SoM games, 
and enhancing the process.  This is done as "soft" reverse engineering, where the project
code is not reverse engineered, but actual reverse engineering is used to ensure accuracy of critical
game systems.

The project is currency in it's infancy, and seeking contributors!  Although, even at this stage - games are loading up,
events are (partially) being parsed, menus are displayed and maps can be explored!


## Goals
The initial goal is to have a fit for purpose replacement of the runtime in full.  It should:
- Replace the som_rt in previously published titles
- Be packaged with games published with [Lawful Blade](https://github.com/FromSoft-Modding-Committee-FSMC/LawfulBlade), through a runtime extension
- Replace som_db in the editing enviroment (map test, preview project)

Eventual goals will be to have a more expansible runtime environment, with different graphical modes such as a PSX or modern render mode.  Other expansions could be to the scripting system, to facilitate new commands.  Additionally, striving for 99.9% SoM accurate behaviour (minus the severe bugs) will be a goal, with toggles for improved behaviours.



## Game Compatibility
The majority of games will currently load up and play through all sequences (both slideshow and video types), display the titlescreen, and load into the initial map.  You are able to explore, although collision is currently ignored. (it is loaded, the player controller just has no response to collision)

There are some exceptions:
### Dark Destiny
Will not load correctly, due to a "SomEx prevention stratagy" which was employed at the time to stop the game from being converted to Ex, which although understood will require a work around.

### Games using SoM Ex
Will not load correctly, SomEx has a slightly different file system, and slightly modified file formats, and in addition - very different behaviour. These changes were very sparsely documented, and as such it would require essentially reversing two entire runtimes.

### Games using Video
These load correctly, but depending on the video codec used - may not play the sequences.  The old recommendation was to use the CINEPAK video codec, which is ancient, terrible... and doesn't work with Video for Windows through Unity.



## Other Notes
Compiled Game Data for the Sword of Moonlight project "Reign" is included for development reasons.  Reign is created by myself, so I felt most comfortable with distribution of that.  Additional games may be used for testing by simply setting up a folder structure similar to Reign in streamingAssets, and copying the content of a downloaded SoM game (likely from https://www.swordofmoonlight.com/games, lol) into the folder.  You must then set the parameter of "Multi Game Name" on GameManager to the name of the game folder (which will also be prefixed with "GameData_", e.g. "GameData_ReturnToMelanat", "GameData_TearsOfTheMoon")



## Contributors
- [StolenBattenberg](https://github.com/TheStolenBattenberg)

We are seeking contributors!  Get your hands dirty, and dig in!  Please review the existing code for style considerations.



## Third Party Asset Declaration
Open SoM Runtime is using the following third party assets:
- [Roman SD](https://www.dafont.com/roman-sd.font)
- [Shangri-La NF](https://www.dafont.com/shangri.font)



## Third Party Code Declaration
- [MeltySynth](https://github.com/sinshu/meltysynth): Used for MIDI playback. MIT License.
