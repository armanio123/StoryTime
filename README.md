# What's StoryTime?

When I was a kid, I loved reading the Choose Your Own Adventure books, where you got to pick the plot to follow and where my character and decisions are part of the book.

With the rise of digital assistants, home devices, etc, it is now possible to live your own adventure through voice, with Cortana as your own personal narrator.

The user decide the plot by using voice or typing the option. We have also created a simple text framework for anybody to create their own stories and share them with the world.

# Create your own story

With focus on the writer, we decided that the best and easy approach for writing a story is to use the markup language Markdown. You don't need to know all about it as we only used some of its features. Nonetheless, if you like to know more about it you can read it [here](https://guides.github.com/features/mastering-markdown/).

## Start with a title

The first thing is creating a file with extension `.md` and adding a title. We'll use `TheOgre.md`:

```markdown
# The Ogre
by anonymous author.
```

The first line `# The Ogre` will be recognized by Cortana and read it as is.

## Add a chapter.

Let's add some content to your story. After the title's paragraph add the following text:

```markdown
## #Chapter1
An ogre has seen you and is running towards you. There is a sword far away from you. You can either

## #Ending1
You keep running away from the ogre but he's too fast for you. Tired and without air you're not match for him. You died.

## #Ending2
You defend yourself with all your might and manage to defeat the ogre. Congratulations. 
```

Notice the double `##` on each paragraph. This is important as it will define the start of a paragraph. StoryTime is not going to narrate or display this line. This line needs to be composed of only letters or numbers and start with another `#`. This is used as an identifier for the `Choices` explained below.

## Performing choices

Right now the story is not much than just paragraphs with no context. You need to add some choices for the user to decide what to do.

To allow multiple paths on your story you need to add the following text after a paragraph:

```markdown
## Choices
* [run away](#Ending1)
* [or punch him](#Ending2)
```

Only the text contained between square brackets `[]` is narrated to the user.

## Story so far

Your story should look like this:

`TheOgre.md`
```markdown
## #Chapter1
An ogre has detect you and is running towards you. There's is a sword a little far away from you. You can either

## Choices
* [run away](#Ending1)
* [or punch him](#Ending2)

## #Ending1
You keep running away from the ogre but he's too fast for you. Tired and without air you're not match for him. You died.

## #Ending2
You defend yourself with all your might and manage to defeat the ogre. Congratulations. 
```

What will happen on StoryTime?
1. Cortana will launch by narrating the title and author.
1. Followed by the first paragraph (`Chapter1`) and `Choices`.
1. User will talk one of the choices. This doesn't need to be exact it can be a similar text to the desired choice. e.g. user says `run away`.
1. The app will recognize the user's choice and prompt the identified action. e.g. After saying `run away` StoryTime will narrate the content of `Ending2`.

# The Stats section. Advanced stuff.

StoryTime is not only an app to display the user paragraphs and choices. It can also keep track of the player's life, items or any other kind of attribute you decide. We call this `Stats`.

Adding `Stats` to your story will enable a more immersive experience. You can enable or disable certain actions depending on the previous choices. 

For example, you could define a stat `hp` that will keep track of the plsyer's health. The choices the user selects can increase or reduce the `hp` stat, disabling certain actions when there's not enough `hp`.

Let's modify our story to add this feature.

## Create a `Stats` section.

A `Stats` section will initialize some player's attributes when the story starts. 

After the title's paragraph (`The Ogre`) add the following section:

```markdown
## Stats
* hp: 10
* inventory: ["shield"]
```

`hp` will be a numeric stat that can be added or subtracted and will keep track of the users health.
`inventory` is a stat that either defines if the player has or doesn't have something. In this example the user is carrying a `shield`.

## Actions have consequences

Every choice the user performs can have an effect on the stats. It can either add, remove, decrease or increase a stat.

### Modifying a stat

If you'll like for a choice to either increase or decrease a stat you need to specify it under each choice.

Replace your previous choices with the following and add one more paragraph:

```markdown
## Choices
* [try to talk to the ogre](#ConfrontOgre)
    * hp: -5
* [or pick up the sword](#ConfrontOgre)
    * hp: -3
    * inventory: +["sword"]

## #ConfrontOgre
the ogre is upon you and without a doubt he's about to attack. Will you
```

In this example, if the user selects the first choice his `hp` will be reduced by `5`. On the second, the `hp` will only be reduced by `3` but it will add a `sword` to the `inventory`. Notice how both of the choices point to the same paragraph. This will lead the user to the same place but with different stats.

The stats will not be neither narrated nor displayed to the user. 

It's important to indent the "stat" so StoryTime recognizes this is a consecuence of a choice.

### Conditionally displaying choices

`Stats` can be used to conditionally narrate the choices on your story. Let's add this to your story as well. After the `ConfrontOgre` paragraph add:

```markdown
## Choices
* [run away](#Ending1)
    * hp: -5
* [or defend using the sword](#Ending2)
    * **inventory: ["sword"]**
* [or punch him](#Ending2)
    * **hp: 6**
    * inventory: -["shield"]
```

`run away` is always narrated as it doesn't have any condition.

`or defend using the sword` will only be narrated if the user has a `sword` on the `inventory` stat; otherwise, the choice will not show up.

`or punch him` only exists if the user has `hp` equal or higher than `6`.

Notice that some of the options are not only conditional but they also affect the stats if selected as well.

### The whole story

Your story should look similar to this:

`The Ogre.md`
```markdown
# The Ogre
by anonymous author.

## Stats
* hp: 10
* inventory: ["shield"]

## #Chapter1
An ogre has detect you and is running towards you. There's is a sword a little far away from you. You can either

## Choices
* [try to talk to the ogre](#ConfrontOgre)
    * hp: -5
* [or pick up the sword](#ConfrontOgre)
    * hp: -3
    * inventory: +["sword"]

## #ConfrontOgre
The ogre is upon you and without a doubt he's about to attack. Will you

## Choices
* [run away](#Ending1)
    * hp: -5
* [or defend using the sword](#Ending2)
    * **inventory: ["sword"]**
* [or punch him](#Ending2)
    * **hp: 6**
    * inventory: -["shield"]

## #Ending1
You keep running away from the ogre but he's too fast for you. Tired and without air you're not match for him. You died.

## #Ending2
You defend yourself with all your might and manage to defeat the ogre. Congratulations.
```