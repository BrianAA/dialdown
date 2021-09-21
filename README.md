# Chatdown
Chatdown is a dialogue system for unity3d. It uses a text file in a markdown style formatting and processes that markdown into a dialogue with questions, options, nested dialogue, variables, and more. It can be extended to suite your dialogue needs and tries unopinionated on how it is implemented ui. The text file is dependant highly on formatting, but very easy to learn. 

## Getting started
Chatdown can be up and running with little time as possible. You can then spend time on designing a ui dialogue and refining the user experience.

1. First copy or download the `Chatdown.cs` file into your unity asset folder
2. You will then need to add it to any gameobject that make sense for your setup.
3. To start using you can drag a textfile into the public textasset for testing but you will want to load text assets via `StartDialogue` method ideally.
```
    public void StartDialogue(TextAsset dialogueFile)
    {
        reader = dialogueFile.text.Split(new[] { System.Environment.NewLine }, System.StringSplitOptions.None);
        ReadConvo();
    }
```
5. Next handle all the cases `ExecuteString()` fires. (emotion,events,speed,font)and you are ready to roll. 


## Writing out dialogue
There are three dialogue types: `questions` `options` and `basic` and everything is controlled by `-` which will indicate the current depth and thread.

### Question and Options
Questions and options are always paired together a question should never appear without options following it unless using a loop type conversation. 

##### Standard Question and option
```
## Type out your question
1. Type your option
  - Nested Response add more '-' based on nested depth
2. Type your option
  - Nested Response add more '-' based on nested depth
```
##### Standard Question and option with loop
```
## How can I help you?
1. I want to sell
  - Nested Response add more '-' based on nested depth
  - <jump.7>
2. I want to buy
  - Nested Response add more '-' based on nested depth
  - <jump.7>
3. Uh nevermind..
  - <event.end>
## Anything else I can help you with?
```
 
 `##` is the flag to let Chatdown know that this line is a questions switch Chatdown dialogueState to `buildingOptions` This will allow Chatdown to loop through the entire text file looking for options. Options are flagged by a number, period and a space example `1. ` The order of the options will always be in the order they are from top to bottom regardless of the actual number.

 
#### Nesting

Nesting conversations are triggered when options and questions are used. A thread is indicated by `-` if there is none it is the current thread is 0. 

##### Example of nesting threads
```
## How can I help you?
1. I want to buy
  - Great I got plenty to offer! 
  - ## So what will it be for you today? <event.openShop>
  - 1. 10 potions - <variable.potionValueBulk>
  - - Great! here you go! <event.Sold.potionValueBulk>
  - - <jump.1>
  - 2. 1 magic spell <variable.magicSpellValue>
  - - Great! here you go! <variable.magicSpellValue>
  - - <jump.1>
2. I want to sell
  - Let see what you got! <event.openSellMode>
  - ## That will be <variable.currentSaleTotal> sound good?
  - 1. Sure! <event.sold>
    - - <jump.1>
  - 2. No thanks Ill keep my stuff <event.cancel>
    - - <jump.1>
3. I am okay thanks..
  - <event.end>
 ```


#### Basic text
Basic text is treated per line. 

##### One line of text
```
A basic message
```

##### 2 line of text
```
A basic message
A another line because I made it on another line
```

### Actions
What are actions? Actions are anything you want them to be. Currently in Chatdown there are events, emotions, font size changes, speed of text as examples and can be deleted or modified. 

Actions are flagged based on the regex pattern. You can create you own utilizing any regex pattern that isn't already used. 

Actions can be anything really change the text speed, or maybe open and fire in game methods. Chat down works best if you have an event handler in your game that listens for event triggers. 


#### Basic use of an action
All actions are the same they will fire exactly where you place them the most important rule of them is that they should always have a space before and after the action. For example:
```
I am not yelling... <font.big> Now I am yelling!
```
In this example the action will invoke after `...` and then the text will be big.

### Jump action
Jump action is powerful action built into chatdown. It lets you jump to any line even if its a line that was read before. This can be helpful for repeating options or dialgoue like a store clerk or the user needs to go back options. 

```
Wow look who it is <character.player>
## How can I help you?
 1. I want to buy some stuff
    - Sorry I have nothing to sell
    - <event.end>
 2. I want to sell some stuff
    - Great! Let see what you got <event.sell>
    - ## That will be <event.calculate> sound good?
    - 1. Sure!
    - - <event.sold>
    - - <jump.18>
    - 2. No thanks Ill keep my stuff
    - - <event.cancelSale>
    - - <jump.18>
 3. Oh nevermind...
    - <event.end> 
## Anything else I can help you with?
<jump.3>
```

### Replacing variables
There is support to swap out variables for the text of your choice. If need things to be dynamic like the name of the character because it will vary from player to player then using variables will definitely be something you want to make use of. To use any variable you need to can add `<variable.yourvariable>` it will replace it with a global accessible variable you assign. Characters and are handle seperately but follow similar patterns `<characters.nameOfCharacter>` 

#### Basic use of variables
```
Hi there <character.player>!
```

#### Use of variable that needs to be calculated programmatically 
For variables that need to be calculated the `handleVariable` method will be where you want to handle variables and prepare the string prior to `Chatdown` passing it to `ExecuteString()`

##### Example
```
<!-- This assumes getTotalAmount was calculated in an event or prior -->
That will be <variables.getTotalAmount>!
```

## Using VScode
Not required but highly reccomended to utilize VScode snippets. They help write out dialogue a lot faster I simply pull up the snippet and it will drop inplace the thing that I need question with 2, 3, or 4 options etc. Highly recommend it! Write your own following the format or using [snippet generator](https://snippet-generator.app/)

## Want to contribute and improve!
I welcome anyone to improve on this concept there somethings feel free to make a pull request or open an issue so I can improve it! 
