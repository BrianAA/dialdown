using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class Chatdown : MonoBehaviour
{
    public TextAsset convoFile; //file to read for testing
    private string[] reader; //String reader
    public int currentRow = 0; //current line being read
    public int prevRowIndex = -1;//inital value is not positive
    string ReadText; //Text that is read by string reader
    public List<int> currentOptions = new List<int>();//stores current options and what row number they are on
    int currentDepth = 0;//Current thread to focus

    [System.Serializable]
    public enum stateOfDialogue {ready, awaitingReponse, buildingOptions, endOfDialogue, awaitingEvent }; //states
    bool containsEnd = false;//if current thread has an End;
    public stateOfDialogue dialogueState = stateOfDialogue.ready; //dialuge state

    enum actionType { isEvent, isEmotion, isFont, isSpeed, isVariable, noAction }; //nested action types in string

    //Cache regex for performance
    private static Regex regexOptions = new Regex(" [0-9]. ", RegexOptions.Compiled);
    private static Regex regexQuestion = new Regex("## ", RegexOptions.Compiled);
    private static Regex regexPlayerName = new Regex("<character.player>", RegexOptions.Compiled);
    private static Regex regexFormater = new Regex("- ", RegexOptions.Compiled);
    private static Regex regexTrimmer = new Regex(@"^\s+", RegexOptions.Compiled);
    private static Regex regexEvent = new Regex("<event.*?>", RegexOptions.Compiled);
    private static Regex regexEmotion = new Regex("<emotion.*?>", RegexOptions.Compiled);
    private static Regex regexSpeed = new Regex("<speed.*?>", RegexOptions.Compiled);
    private static Regex regexFont = new Regex("<font.*?", RegexOptions.Compiled);
    private static Regex regexJump = new Regex("<jump.*?>", RegexOptions.Compiled);
    private static Regex regexVariable = new Regex("<(variable.*?)>", RegexOptions.Compiled);


    ////Test script performance
    //System.Diagnostics.Stopwatch Stopwatch = new System.Diagnostics.Stopwatch();

    // Start is called before the first frame update
    void Awake()
    {
        //splits the entire convo up
        reader = convoFile.text.Split(new[] { System.Environment.NewLine }, System.StringSplitOptions.None);
    }
    /// <summary>
    /// Takes a textasset and send to dialogue handler
    /// </summary>
    /// <param name="dialogueFile"></param>
    public void StartDialogue(TextAsset dialogueFile)
    {
        reader = dialogueFile.text.Split(new[] { System.Environment.NewLine }, System.StringSplitOptions.None);
        ReadConvo();
    }

    /// <summary>
    /// Reads the array of strings based on the current row (i.e. index of array)
    /// </summary>
    void ReadConvo()
    {
        bool ValidLine = currentRow < reader.Length;
        //Reads until there is nothing left to read

        //Checks if its end of data and not repeating the same line
        if (ValidLine && currentRow != prevRowIndex)
        {
            ReadText = reader[currentRow];//gets the next line
            prevRowIndex = currentRow;
            int lineDepth = Regex.Matches(ReadText, "- ").Count; //Checks depth of line

            if (lineDepth == currentDepth)
            {
                string prepString = prepareLineofText(ReadText); //replaces string variables

                if (regexQuestion.IsMatch(prepString)&&dialogueState!=stateOfDialogue.buildingOptions) //checks if a question
                {
                    prepString = regexQuestion.Replace(prepString, ""); //removes ##
                    dialogueState = stateOfDialogue.buildingOptions; //sets state to build options
                    ExecuteString(prepString);
                }
                else if (regexOptions.IsMatch(prepString))//if its not a question
                {
                    prepString = regexOptions.Replace(prepString, ""); //removes number
                    currentOptions.Add(currentRow); //adds to option to display
                }
                else if (regexJump.IsMatch(prepString))//if its a jump
                {
                    HandleJump(prepString);
                }
                else //if just a regular message or other event, etc
                {
                    //if currently building option and reaches a line that is not an option
                    if (dialogueState == stateOfDialogue.buildingOptions)
                    {
                        dialogueState = stateOfDialogue.awaitingReponse; //set to awaiting the reponse show options
                    }
                    //Not an option or a question and not waiting for options just plain message
                    else
                    {
                        //passes string to ui and invokes method
                        if (dialogueState != stateOfDialogue.awaitingReponse)
                        {
                            ExecuteString(prepString); //passes string to ui and invokes methods
                        }
                    }
                }
            }
            if (dialogueState == stateOfDialogue.buildingOptions)
            {
                currentRow++;//proceed to next row
                ReadConvo(); //repeat
            }
        }
    }

    //Handles going to the next line of text
    public void processNextLine()
    {
        if (Ui_Dialogue.currentStatus == Ui_Dialogue.status.processing) return;//makes sure the text finish displaying
        if (dialogueState == stateOfDialogue.awaitingReponse) return;
        if (dialogueState == stateOfDialogue.endOfDialogue) return;
        currentRow++;
        ReadConvo();
    }

    /// <summary>
    /// Selects the option and fires this thread
    /// </summary>
    /// <param name="selectedOption"></param>
    public void SelectOption(int selectedOption)
    {
        // IF PROCESSING THE TEXT VIA UI STOP CHECK BEFORE ALLOWING USERS TO SELECT OPTION
        // Example if(UiDialogue.isProcessing) return

        if (selectedOption > currentOptions.Count) return;//makes sure the option is valid choice
        currentRow = currentOptions[selectedOption - 1] + 1; //sets the current row to option
        currentOptions.Clear(); //Clear out options
        currentDepth++; //shifts to nested thread
        dialogueState = stateOfDialogue.ready; //change from awaiting reponse to chatting
        ReadConvo(); //invoke convo
    }
    /// <summary>
    /// When event completes call this method to continue conversation
    /// </summary>
    public void EventCompleted()
    {
        dialogueState = stateOfDialogue.ready;//event ready to go next line
        currentRow++;//proceed to next row
        ReadConvo(); //invoke convo
    }

    //Prepares string to be displayed (replaces location, names & variables)
    string prepareLineofText(string ReadText)
    {
        string prepString = regexFormater.Replace(ReadText, "");//Removes '- ' from text
        prepString = regexPlayerName.Replace(prepString, "Player");// Replaces with characters name
        prepString = regexVariable.IsMatch(prepString) ? HandleVariable(prepString) : prepString;
        prepString= regexTrimmer.Replace(prepString, ""); //Removes any leading whitesspace
        return prepString;
    }

    //Sets up the string to send the line back word by word firing the action where it is placed
    void ExecuteString(string prepString)
    {
        string[] splitStrings = prepString.Split(' ');//Splits string into words
        actionType inlineAction;
        EventManager.TriggerEvent("NextLine");//clears ui Dialogue for the next line of text

        //Loop through all the words
        for (int i = 0; i < splitStrings.Length; i++)
        {
            //checks action type
            inlineAction = regexEvent.IsMatch(splitStrings[i]) ? actionType.isEvent:
                regexEmotion.IsMatch(splitStrings[i]) ? actionType.isEmotion:
                regexFont.IsMatch(splitStrings[i]) ? actionType.isFont:
                regexSpeed.IsMatch(splitStrings[i]) ? actionType.isSpeed:actionType.noAction;

            //Switch based on action type
            switch (inlineAction)
            {
                case actionType.isEvent:
                    HandleEvent(splitStrings[i]);
                    break;
                case actionType.isEmotion:
                    Debug.Log("Triggered Emotion"); //handle emotion animation
                    break;
                case actionType.isFont:
                    Debug.Log("Changed Font size"); //handle fontsize change on ui
                    break;
                case actionType.isSpeed:
                    Debug.Log("Changed speed of text"); //handle speed of text in ui
                    break;
                default:
                    //If it just text send to UI dialouge
                    if (splitStrings[i] != "" && splitStrings[i] != " ")
                    {
                        Debug.Log("Handle text");//Handle text
                    };
                    break;
            }
        }
        if (containsEnd)//if its an end of dialogue its ready to close out
        {
            //Wait for dialouge to finish displaying before ending
            // example while (UiDialogue.isprocessing) return
            dialogueState = stateOfDialogue.endOfDialogue;//Sets dialogue to end close out
            EventManager.TriggerEvent("EndDialogue");//Close Dialogue;
            Debug.Log("Ended Dialogue");
        }
    }

    int setDepth(string Line)
    {
        return Regex.Matches(Line, "- ").Count; //Checks depth of line
    }

    void HandleEvent(string eventName)
    {
        //handles if the dialogue comes to an end
        if (eventName == "<event.end>")
        {
            containsEnd = true;
        }
        else
        {
            string splitString = eventName.Split('.')[1]; //split string after <event.
            string methodToInvoke = splitString.Substring(0, splitString.Length - 1); //remove the trailing > from nameofEvent>
            Debug.Log("Invoking " + methodToInvoke); // Invoke the event best to use a event manager
            dialogueState = stateOfDialogue.awaitingEvent;
            prevRowIndex = currentRow;
        }
    }

    void HandleJump(string prepString)
    {
        string stringToParse = prepString.Split('.')[1].Substring(0, prepString.Split('.')[1].Length - 1);// returns everything after <jump. and subtracts last >
        if (stringToParse != "")
        {
            Debug.Log("Jumping to line " + stringToParse);
            currentRow = int.Parse(stringToParse) - 1;//Set the row to jump to as current
            currentDepth = setDepth(reader[currentRow]);//get the depth it should be reading at
            ReadConvo();
        }
    }

    string HandleVariable(string stringToChange)
    {
        Debug.Log("Handling variable");
        //Example string processString = regexVariable.Replace(stringToChange, "$200.00");
        return processString;
    }

    void Update()
    {
        //Simulates controls
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            SelectOption(1);
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            SelectOption(2);
        }
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            SelectOption(3);
        }

        //Starts conversation
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ReadConvo();
        }
        //Complete event
        if (Input.GetKeyDown(KeyCode.Return))
        {
            EventCompleted();
        }
        //next line to process
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            processNextLine();
        }
    }
}


