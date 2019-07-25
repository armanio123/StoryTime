# Table Demo
by aaward@microsoft.com

Hackathon 2019 live demo story


## Stats
* initialized: ["true"]
* test: 1

## #Node_Root_Startup

  OK, I am ready. Have you ever played dungeons and dragons before?

## Actions
* [respond_affirmative](#Node_Startup_Yes)
* [respond_negatory](#Node_Startup_No)
* [metacommand_repeat](#Node_Root_Startup)

## #Node_Startup_Yes

  Excellent.

  As we play, you can speak to me in the first person, as you take on the role of your character in the story. Just speak naturally, I have a language understanding AI that helps me figure out what you are trying to do, and the more you experiment, the better I will get at it. for example, if you say "I want to examine the broken pillar more closely" or "I throw a rock right at the goblin's face", I will understand what you mean. Are you ready to continue?

## Actions
* [respond_affirmative](#Node_Root_Introduction)
* [respond_negatory](#Node_Startup_NoContinue)
* [continue](#Node_Root_Introduction)
* [metacommand_repeat](#Node_Startup_Yes)
  
## #Node_Startup_No

  D and D is a role playing game. It's kind of like reading a story where you control the main character, and the story changes in response to your actions. You tell me what you want to do, and I describe the things that you see and hear around you.  

  Are you ready to begin, or would you like some more explanation?

## Actions
* [respond_affirmative](#Node_Startup_Yes)
* [respond_negatory](#Node_Startup_NoExplanation)
* [explanation](#Node_Startup_NoExplanation)
* [metacommand_repeat](#Node_Startup_No)

## #Node_Startup_NoExplanation

  Sorry, extended rules information has not been implemented yet.
  
## Actions
* [default](#Node_Startup_Yes)
  * *initialized: ["true"]*
  
## #Node_Startup_NoContinue

  Well, then I'm not really sure what we are doing here.
  
## Actions
* [default](#Note_Root_End)
  * *initialized: ["true"]*

## #Node_Root_Introduction

## Actions
* [default](#Node_Introduction_Desc1)
  * *initialized: ["true"]*

## #Node_Introduction_Desc1

  You will be taking on the role of Caladon, a warrior and mercenary in a medieval fantasy world. Your travels have brought you to the town of Redlake, where the locals have offered you a reward for clearing out the nearby abandoned mine of certain undesirable creatures. Shall we begin?

## Actions
* [respond_affirmative](#Node_Root_Background)
* [respond_negatory](#Node_Startup_NoContinue)
* [begin](#Node_Root_Background)
* [metacommand_repeat](#Node_Introduction_Desc1)

  
  
## #Node_Root_Background

You step beneath the final rotten support beam in the damp, rocky passage, and the walls open up into what remains of the staging area for the ore carts before they were loaded onto caravans for shipping to the great cities of the north. The broken and rusted remains of one of the carts leans against the wall to your right, half buried in rubble. The faint whistling of the wind passing the entrance of the mine behind you takes on a slightly deeper tone as it expands to fill this small chamber.

The space itself extends about ten feet to either side from the passageway in which you now stand. The back wall, dimly lit but still visible, stands about twice as far from you as the width of the room. Four flimsy wooden pillars support a sagging frame above, one of which remains attached only at the top, with the bottom half broken away and laying on the smooth rock floor.

A second passageway leads off into the darkness to the left, and it looks like someone has placed a makeshift door in a similar opening on the back wall.

On a small pile of stacked stones in the far right corner sits a lumpy, awkward little humanoid with oversized ears and gangly limbs, dressed only in some thin and tattered leather half-breeches. It stares thoughtlessly at its toes, twiddling them in the dust, occasionally mumbling incoherently.

The vile creature is clearly a goblin; the citizens of Redlake have offered a handful of good coin for their ears. With luck, you may be able to salvage them both!

Leaping across the room before the little brute is aware of your entry, you release your sword from its hip-sheath, and with the same motion, slash it across the monster's midsection. The goblin squeals in terror and flings itself backward into the corner, barely avoiding the tip of the blade, but knocking its head against the wall in the process.

Dazed and unstable, it wobbles on its spindly legs, trying to get its bearings while cowering in anticipation of the next inevitable blow.


## Actions
* [metacommand_repeat](#Node_Root_Background)



## #Node_Root_Entrance

## Actions
* [metacommand_repeat](#Node_Root_Entrance)



## #Node_Root_Greeting

## Actions
* [metacommand_repeat](#Node_Root_Greeting)

## #Node_Greeting_Desc1


## #Node_Root_Storage

## Actions
* [metacommand_repeat](#Node_Root_Storage)



## #Node_Root_Barricade

## Actions
* [metacommand_repeat](#Node_Root_Barricade)



## #Node_Root_Treasure

## Actions
* [metacommand_repeat](#Node_Root_Treasure)



## #Node_Root_Undefined

  This option has not been implemented yet. Sorry for the inconvenience.

## Actions
* [continue](#Note_Root_End)
  * *initialized: ["true"]*

## #Note_Root_End

  You may now return to the real world.
