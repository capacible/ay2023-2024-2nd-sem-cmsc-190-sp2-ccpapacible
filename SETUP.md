# Setup
## Requirements
Unity Version 2022.2.6f1

## Accessing the Unity project
The code in this repository only includes the author-created assets and data. To open the project:
1) Create a new Unity project.
2) Clone this repository into the Unity project.

You may build the project directly when opening it in Unity, or if you only want to play the game, simply run the game in the build folder.

### IMPORTANT!
This repository _does not_ have the initial conditional probability table files! Hence, building the game from the Unity interface requires that they re-generate the CPT files. To do so:
1) Open the Unity project
2) Go to Tools/Training/Generate algorithms
3) Go to Tools/Training/Infer initial Dialogue CPTs.

Once these steps are completed, you can now build the game and run it by pressing CTRL + B.

Changing the data of ANY CSV in the Data folder _necessitates_ that the user repeat steps 2 and 3. Before doing so, the user must generate the XML version of the CSV that was modified.
This can be found in Tools/XML.
- Changing any of the dialogue files ("dialogue_Xxxxx") requires the player to go to Tools/XML/Generate dialogue XML files, go to Tools/XML/Generate ids XML from single-column CSV, and then repeat steps 2 and 3.
- Changing the speaker CSV file requires the player to go to Tools/XML/Generate speaker XML.

Unrelated to the dialogue generation is the Tools/ScriptableObjects section. This section is merely for automating the generation of ScriptableObjects whose data will be dragged onto an NPC template. Unless new NPC archetypes are made, there is no need touch it.

### Running the game
The working build has only been tested for Windows only (player respondents as well as the author use Windows). The game may not be compatible with other OS.

To run the game:
1) Download the .zip file for the build.
2) Unzip the file.
3) Open SPDemo.exe

The game takes several minutes to initialize--this is normal.
