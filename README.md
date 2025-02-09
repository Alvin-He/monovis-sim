

# Unity Simulation Support Repo for Monovis

this is used to simulate the robot and apriltags

For communications with the rest of monovis, please make sure the monovis-ui server is also running

by alvin-he

check comments in files for more info

# Import precedures 

open this project in a unity editor, u may need to delete the Library folder if Unity crashes everytime you open the project

When Opening the project for the 1st time, **the unity editor will crash**, ***THIS IS EXPECTED***. Simply wait for it to crash, close the crash report, then reopen
the project. It should boot up normally into an editor windows the 2nd time.

If the heirachy is empty, don't worry, just Open the scenes folder under Assets inside editor and double click on the SmapleScene (the only Scene in the folder).
The Scene should magically open it self

also there may be sometimes lighting artifcats on linux probably due to some loading problem. If you see them, don't close the editor. Just wait a minute and then Unity should find it and fix it for you

# Running with NVIDIA GPU on linux
Follow the instructions here: [https://askubuntu.com/a/1368463](https://askubuntu.com/a/1368463)

Make a prime-run scipt that bascially makes unityhub to use the descret nvidia gpu for rendering. Place it into /usr/local/bin for quick access

then do `prime-run unityhub` in your shell when ever you start up unity hub. Or edit the application launcher on your linux desktop provider to use the command to launch Unity

This will drastically help with performance on linux 
