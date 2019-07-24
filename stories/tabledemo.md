# Table Demo
by aaward@microsoft.com

Hackathon 2019 live demo story


## Stats
* initialized: ["true"]


## #Node_00_Startup

  CDM 7.24.0833

  OK, I am ready. Have you ever played dungeons and dragons before?

## Actions
  * [respond_affirmative] (#N_00_Affirmative)
  * [respond_negatory]    (#N_00_Negatory)


## #N_00_Affirmative

  Excellent.

  As we play, you can speak to me in the first person, as you take on the role of your character in the story. Just speak naturally, I have a language understanding AI that helps me figure out what you are trying to do, and the more you experiment, the better I will get at it. for example, if you say “I want to examine the broken pillar more closely" or “I throw a rock right at the goblin's face", I will understand what you mean.

## Actions
  * [continue] (#Node_99_End)

  
## #N_00_Negatory

  D and D is a role playing game. It’s kind of like reading a story where you control the main character, and the story changes in response to your actions. You tell me what you want to do, and I describe the things that you see and hear around you.

  Are you ready to begin, or would you like some more explanation?

## Actions
  * [respond_affirmative] (#Node_99_End)
  * [respond_negatory]    (#Node_99_End)
  * [explanation]         (#Node_99_End)

  
## #Node_99_End

  You may now return to the real world.
