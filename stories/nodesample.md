# Node Sample
by aaward@microsoft.com

Story structure reference for Cortana is my Dungeon Master


## Stats
* default: ["true"]
* node_NODENAME_count: 1


## #Startup

Workaround for bug preventing trigger in starting section. Are you ready to begin?

## Actions
* [respond_affirmative](#Node_Root_NODENAME)


## #Node_Root_NODENAME

## Actions
* [default](#Node_NODENAME_DescriptionHub)
  * *default: ["true"]*
  * node_NODENAME_count: +1


## #Node_NODENAME_Comment

This node is never visited. It exists for documentation purposes.

Root jumps to description hub and increments visit counter.
Description hub jumps to descriptions
Descriptions jump to action hub
Action hub controls the things that can be done from this node, both local options and globals (generally metacommands).

Bugs being worked around here include:
 - no triggering from start section
 - stat changes not taking effect immediately


## #Node_NODENAME_DescriptionHub

## Actions
* [trigger1](#Node_NODENAME_Desc3)
  * *node_NODENAME_count: 3*
* [trigger2](#Node_NODENAME_Desc2)
  * *node_NODENAME_count: 2*
* [trigger3](#Node_NODENAME_Desc1)
  * *node_NODENAME_count: 1*
* [default](#Node_Root_Error)
  * *default: ["true"]*

## #Node_NODENAME_Desc1

  Description 1

## Actions
* [default](#Node_NODENAME_ActionHub)
  * *default: ["true"]*

## #Node_NODENAME_Desc2

  Description 2

## Actions
* [default](#Node_NODENAME_ActionHub)
  * *default: ["true"]*

## #Node_NODENAME_Desc3  

  Description 3

## Actions
* [default](#Node_NODENAME_ActionHub)
  * *default: ["true"]*
  * node_NODENAME_count: +1


## #Node_NODENAME_ActionHub

## Actions
* [use_sense_sight](#Node_Root_NODENAME)
* [use_sense_sight around](#Node_Root_NODENAME)
* [use_sense_sight myself](#Node_Root_Undefined)
* [use_sense_sight rock](#Node_Root_Undefined)
* [metacommand_repeat](#Node_NODENAME_DescriptionHub)
* [metacommand_whoami](#Node_Root_Undefined)
* [metacommand_whoami](#Node_Root_Undefined)






## #Node_Root_Error

  Fatal program error, aborting.

## Actions
* [continue](#Note_Root_End)
  * *default: ["true"]*

  
  
  
## #Node_Root_Undefined

  This option has not been implemented yet. Sorry for the inconvenience.

## Actions
* [continue](#Note_Root_End)
  * *initialized: ["true"]*

  
  
  
## #Note_Root_End

  You may now return to the real world.
  
