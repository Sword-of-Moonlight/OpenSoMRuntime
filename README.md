The OpenSoMRuntime aims to create a full replacement of the original Sword of Moonlight Runtime, 
to resolve long standing issues and provide a more adaptable solution for creating SoM games, 
and enhancing the process.

Many bits of the game are already pieced together, and lots of systems already have their initial parts in place.

# Game Compatibility List:
### Reign
Reign will display all game sequences, and load up to the title screen. From the title screen, you may progress to the first map.
No issues occur during this process, but missing implementations make further progression impossible.

### Trismegistus
Trismegistus will display all game sequences, and load up to the title screen. From the title screen, you may progress to the first map.
No issues occur during this process, but missing implementations make further progression impossible.

### Other Games
All other games should be considered to be in the same state as Trismegistus and Reign, some excepections exist - such as Dark Destiny, which due to an "anti Michael" hack,
needs additional consideration in order to function.  Additional, Dark Destiny video files will not play back.

# Non Conclusive Task List:
- [x] Sequences
- [x] Title Screen
- [ ] Player Controller
- [x] Loading MPX files
- [x] Loading MDO, MHM, MDL, MSM files
- [x] Loading TXR, BMP
- [x] Loading WAV, MID, SND
- [ ] NPC Behaviours
- [ ] Enemy Behaviours
- [ ] Object Behaviours
- [ ] Item Behaviours
- [ ] Event Processing

# Other Notes:
Compiled Game Data for the Sword of Moonlight project "Reign" is included for development reasons.  Reign is created by myself, so I felt most comfortable with distribution of that.  Additional games may be used for testing by simply setting up a folder structure similar to Reign in streamingAssets, and copying the content of a downloaded SoM game (likely from https://www.swordofmoonlight.com/games, lol) into the folder.  You must then set the parameter of "Multi Game Name" on GameManager to the name of the game folder (which will also be prefixed with "GameData_", e.g. "GameData_ReturnToMelanat", "GameData_TearsOfTheMoon", "GameData_HopeDefarrrrred")
