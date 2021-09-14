# Chatdown
A simple dialogue system for unity. It utilizes a simple text format with keys and events to feed information into a script to handle the entire conversation. The name comes from markdown as the formatting of the file uses markdown type keys only the inline actions utilize `<>` to seperate from the the rest of the text.

## Getting started
If you like to use Chatdown simply drag and drop the main script `Chatdown.cs` to your game folder. In order to use it you will need to drag and drop it onto a game object. It will then take any TextAsset convert it to text split it up and handle the conversation based on the information of each line.

## Writing out dialogue
There are three dialogue types: `questions` `options` and `basic` and everything is controlled by `-` which will indicate the current depth and thread.

### Question and Options
Questions and options are always paired together a question can never appear without options following it. Options also need to be more than one other wise it might as well not be presented to the user as an option. If you are to write a question the following format should be followed:
```
## Type out your question
1. Type your option
  - Nested Response add more '-' based on nested depth
2. Type your option
  - Nested Response add more '-' based on nested depth
 ```
 
 `##` Is the flag to let Chatdown know that this is a questions it will then be searching for the options looking for `[0-9]. ` the number and period. It will then build out the options and await a response before moving forward. 
 
#### Nested question
```
## Type out your question
1. Type your option
  - Nested Response add more '-' based on nested depth
  - ## Nested question
  - 1. Nested option will go here
  - - Nested response when that nested question is selected
  - ## Second Nested question
  - 2. Nested option will go here
  - - Nested response when that nested question is selected
2. Type your option
  - Nested Response add more '-' based on nested depth
 ```
#### Basic text
```
A basic message
```

### Actions
What are actions? Actions are anything you want them to be. Currently in Chatdown there are events, emotions, font size changes, speed of text, and ability to jump to lines. You are not required to utilize any of the actions written out and you can create your own simply create a Regex pattern using the format `"<custom."` as a cached regex pattern see variables above and add it to the [ExecuteString](https://github.com/BrianAA/chatdown/blob/60fe9b568be8bf64d2b51ee4e76d67b24e3c26a0/Chatdown.cs#L148-L187) method in the actiontype as well as the switch statement. 

#### Basic use of an action
All actions are the same they will fire exactly where you place them the most important rule of them is that they should always have a space before and after the action. For example:
```
I am not yelling... <font.big> Now I am yelling!
```
In this example the action will invoke after `...` and then the text will be big.

### Replacing variables
There is support to swap out variables for the text of your choice. If need things to be dynamic like the name of the character because it will vary from player to player then using variables will definitely be something you want to make use of. To use any variable you need to set up a regex pattern simialr to the event pattern using `<variable.` Currently there is support built in for `<character.playyer>` and will swap it out whenever it is used with the player name. 

#### Basic use of variables
```
Hi there <character.player>!
```


## Want to contribute and improve!
I welcome anyone to improve on this concept there somethings feel free to make a pull request or open an issue so I can improve it! 
