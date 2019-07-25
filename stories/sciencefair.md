# Table Demo
by aaward@microsoft.com

Hackathon 2019 live demo story


## Stats
* initialized: ["true"]
* test: 1

## #Node_Root_Startup

  1222
  OK, I am ready. Have you ever played dungeons and dragons before?

## Actions
* [respond_affirmative](#Node_Startup_Yes)
* [respond_negatory](#Node_Startup_No)
* [metacommand_repeat](#Node_Root_Startup)

## #Node_Startup_Yes

  Excellent.

  As we play, you can speak to me in the first person, as you take on the role of your character in the story. Just speak naturally, I have a language understanding AI that helps me figure out what you are trying to do, and the more you experiment, the better I will get at it. for example, if you say "I want to examine the broken pillar more closely" or "I throw a rock right at the goblin's face", I will understand what you mean.

## Actions
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
* [default](#Node_Root_Introduction)
  * *initialized: ["true"]*

  
  
## #Node_Root_Introduction

## Actions
* [default](#Node_Introduction_Desc1)
  * *initialized: ["true"]*
* [metacommand_repeat](#Node_Root_Introduction)

## Node_Introduction_Desc1

  You will be taking on the role of Caladon, a warrior and mercenary in a medieval fantasy world. Your travels have brought you to the town of Redlake, where the locals have offered you a reward for clearing out the nearby abandoned mine of certain undesirable creatures. Are you ready to begin?

## Actions
* [metacommand_repeat](#Node_Introduction_Desc1)

  
  
## #Node_Root_Background

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





## #Welcome
You will be taking on the role of Caladon, a warrior and mercenary in a medieval fantasy world. Your travels have brought you to the town of Redlake, where the locals have offered you a reward for clearing out the nearby abandoned mine of certain undesirable creatures. Are you ready to begin?

## Actions
* [yes](#GreetingRoom)
* [skip to delay](#GreetingRoom0Delay0Return)  

## #Node_Root_Undefined

  This option has not been implemented yet. Sorry for the inconvenience.

## Actions
* [continue](#Note_Root_End)
  * *initialized: ["true"]*

## #Note_Root_End

  You may now return to the real world.
