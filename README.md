# SramComparer
allows to compare unknown buffers in a SNES' SRAM file. 

## Features
* Comparison of Unknowns only (*default*) or whole game buffer (**optional**)
* Comparison of whole sram outside games (**optional**)
* Display of differences in decimal, hex and binary format
* Comparison of all games (*default*) or one specific current game with same or different comparison game (**optional**)
* Export comparison result as text file
* Backup / restore functionality of current and comparison sram file
* Display of changed or all checksums (**optional**)
* All settings can be set by cmd line arguments

## How to run
This library is based on .net 5 runtime.

Users see "Run apps - Runtime" column, coders see "Build apps - SDK" column at:  
https://dotnet.microsoft.com/

## How to use
**Steps**:

***1.1*** Most emulators have the option to save the game's S-RAM automatically after a change occurs.
     Make sure this is enabled if existing. Otherwise you have manually ensure that the emulator updates
     the srm file.  
***1.2*** Start the tool by passing the game's srm filepath as first command parameter. The file can also be
     dragged onto the tool.

***2.***   Switch to this tool and press (ow) to create/overwrite a comparison copy of your current SRAM file allow
     comparison after your next change of the game's srm file.

***3.1*** Cause in-game a SRAM change by doing a specific action. (e.g. let a game progress event happen or
    open a chest) This change needs to be saved in the inn to make the game update the srm file.  
***3.2*** Press (c) to compare the current srm file with comparison file.
     (If the SRAM file did not change at all, you probably did not save in-game or the emulator didn't
     (yet!) update the SRAM (srm) file automatically. Check the modification date of the game's srm file.
     For Example: Snes9x's default srm update period is 30 seconds. Decrease it to a lower value, like 1,
     but not to 0.

***4.1*** Once you got the offset of a single changed byte (or bit) that has changed after your intended action you might have found a meaning for that 
     specific bit/byte. Then press (e) to export the comparison result to a textfile in your export
     directory.  
***4.2*** Rename the file expressing your current discovery.  
     If it is reproducable, consider to report the exported file to the tool's creator
     and/or contribute the change to SRAM structure to the tool's github repository.

***5.***   To start a "fresh" comparison without previous SRAM changes, press again (ow) to save your current SRAM file
     as comparison file and start again at step 3.1.

***6.1*** (optional, advanced) If you have more than one slot with changes to comparison file, press (sg) to
     set the game's save slot (1-4) to avoid comparing other game slots. If twi different games should be
     compared with each other, additionally press (scg) to set the the slot of comparison srm file, too.  
***6.2*** (optional, advanced) Press (fwg | fng) to set comparison modes.
     If you are unsure, leave it as it is to compare as less as possible bytes.

***7.***   (optional) Current and comparison srm file can be backed-up press (b|bc) or restored (r|rc) individually.

## Screenshots (from SoE implementation)
![Commands](https://i.ibb.co/vHdwJq7/Commands.png "Commands")
![Comparison_UnknownsOnly](https://i.ibb.co/MNG8qR1/Comparison-Unknowns-Only.png "Comparison (unknowns only)")
![Comparison_FlagWholeGame](https://i.ibb.co/ZKTqD0H/Comparison-Flag-Whole-Game.png "Comparison (whole game)")
