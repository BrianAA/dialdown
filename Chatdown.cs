using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class Chatdown : MonoBehaviour
{
    public TextAsset convoFile; //file to read
    private string[] reader; //String reader
    int currentRow = 0; //current line being read
    string ReadText; //Text that is read by string reader

    List<int> currentOptions = new List<int>();//stores current options and what row number they are on
    int currentDepth = 0;//Current thread to focus
    [System.Serializable]
    enum stateOfDialogue { awaitingReponse, buildingOptions, chatting, endOfThread }; //states
    stateOfDialogue dialogueState = stateOfDialogue.chatting; //dialuge state

    enum actionType { isEvent, isEmotion, isFont, isSpeed, isJump, noAction }; //nested action types in string

    //Cache regex for performance
    private static Regex regexOptions = new Regex("[0-9]. ", RegexOptions.Compiled);
    private static Regex regexQuestion = new Regex("## ", RegexOptions.Compiled);
    private static Regex regexPlayerName = new Regex("<character.player>", RegexOptions.Compiled);
    private static Regex regexFormater = new Regex("- ", RegexOptions.Compiled);
    private static Regex regexEvent = new Regex("<event.", RegexOptions.Compiled);
    private static Regex regexEmotion = new Regex("<emotion.", RegexOptions.Compiled);
    private static Regex regexSpeed = new Regex("<speed.", RegexOptions.Compiled);
    private static Regex regexFont = new Regex("<font.", RegexOptions.Compiled);
    private static Regex regexJump = new Regex("<jump.", RegexOptions.Compiled);
    private static Regex selectNumber = new Regex("[0-9]", RegexOptions.Compiled);

    ////Test script performance
    //System.Diagnostics.Stopwatch Stopwatch = new System.Diagnostics.Stopwatch();

    // Start is called before the first frame update
    void Awake()
    {
        //splits the entire convo up
        reader = convoFile.text.Split(new[] { System.Environment.NewLine }, System.StringSplitOptions.None);
        Stopwatch.Start();
        ReadConvo();
    }
    /// <summary>
    /// Takes a textasset and send to dialogue handler
    /// </summary>
    /// <param name="dialogueFile"></param>
    public void StartDialogue(TextAsset dialogueFile)
    {
        reader= dialogueFile.text.Split(new[] { System.Environment.NewLine }, System.StringSplitOptions.None);
        ReadConvo();
    }

    /// <summary>
    /// Reads the array of strings based on the current row (i.e. index of array)
    /// </summary>
    void ReadConvo()
    {
        bool ValidLine = currentRow < reader.Length;
        //Reads until there is nothing left to read

        //Checks if its end of data
        if (ValidLine)
        {
            ReadText = reader[currentRow];//gets the next line
            int lineDepth = Regex.Matches(ReadText, "- ").Count; //Checks depth of line

            //Ensures string is correct depth
            if (lineDepth == currentDepth && //If it has the same depth of current thread
                dialogueState != stateOfDialogue.awaitingReponse && //is not awaiting a response
                dialogueState != stateOfDialogue.endOfThread) // is not the end of a thread see line 98
            {
                string prepString = prepareLineofText(ReadText); //replaces string variables
                if (regexQuestion.IsMatch(prepString)) //checks if a question
                {
                    prepString = regexQuestion.Replace(prepString, ""); //removes ##
                    dialogueState = stateOfDialogue.buildingOptions; //sets state to build options
                    ExecuteString(prepString); //passes string to ui and invokes methods
                }
                else if (regexOptions.IsMatch(prepString))//if its not a question
                {
                    prepString = regexOptions.Replace(prepString, ""); //removes number
                    currentOptions.Add(currentRow); //adds to option to display
                }
                else //if just a regular message or other event, etc
                {
                    //if currently building option and reaches a line that is not an option
                    if (dialogueState == stateOfDialogue.buildingOptions)
                    {
                        dialogueState = stateOfDialogue.awaitingReponse;
                    }
                    //Not an option or a question and not waiting for options just plain message
                    else
                    {
                        ExecuteString(prepString); //passes string to ui and invokes method
                    }
                }

                //repeats method until reaches an end of a thread
                currentRow++;//proceed to next row
                ReadConvo();//repeat
            }
            //handles lines not in the current thread
            else
            {
                //Only if the linedepth is less than the current depth will be an end of a thread
                if (lineDepth < currentDepth && dialogueState != stateOfDialogue.endOfThread)
                {
                    dialogueState = stateOfDialogue.endOfThread;
                }
                currentRow++;//proceed to next row
                ReadConvo(); //repeat
            }
        }
    }

    /// <summary>
    /// Selects the option and fires this thread
    /// </summary>
    /// <param name="selectedOption"></param>
    public void SelectOption(int selectedOption)
    {
        currentRow = currentOptions[selectedOption - 1] + 1;
        currentOptions.Clear(); //Clear out options
        currentDepth++; //shifts to nested thread
        dialogueState = stateOfDialogue.chatting; //change from awaiting reponse to chatting
        ReadConvo(); //invoke convo
    }

    //Prepares string to be displayed (replaces location, names)
    string prepareLineofText(string ReadText)
    {
        string prepString = regexFormater.Replace(ReadText, "");//Removes '- ' from text
        prepString = regexPlayerName.Replace(prepString, "Brian");// Replaces with characters name

        return prepString;
    }

    //Sets up the string to send the line back word by word firing the action where it is placed
    void ExecuteString(string prepString)
    {
        string[] splitStrings = prepString.Split(' ');//Splits string into words
        actionType isAction = actionType.noAction;

        for (int i = 0; i < splitStrings.Length; i++)
        {
            //checks action type
            isAction = regexEvent.IsMatch(splitStrings[i]) ? actionType.isEvent :
                regexEmotion.IsMatch(splitStrings[i]) ? actionType.isEmotion :
                regexFont.IsMatch(splitStrings[i]) ? actionType.isFont :
                regexSpeed.IsMatch(splitStrings[i]) ? actionType.isSpeed :
                regexJump.IsMatch(splitStrings[i]) ? actionType.isJump : actionType.noAction;
            //Switch based on action type
            switch (isAction)
            {
                case actionType.isEvent:
                    Debug.Log("Executed action");//handle action
                    break;
                case actionType.isEmotion:
                    Debug.Log("Triggered Emotion"); //handle emotions
                    break;
                case actionType.isFont:
                    Debug.Log("Changed Font size"); //handle font size change
                    break;
                case actionType.isSpeed:
                    Debug.Log("Changed speed of text"); //handle speed of text
                    break;
                case actionType.isJump:
                    MatchCollection matches = selectNumber.Matches(splitStrings[i]); //find the numbers
                    string stringToParse=""; //empty string
                    foreach (Match m in matches)
                    {
                        stringToParse = stringToParse + m.Value; //builds string to parse
                    }
                    if (stringToParse != "") //ensures a valid string to parse
                    {
                        currentRow = int.Parse(stringToParse) - 1; //line to jump to in reader
                        currentDepth = setDepth(reader[currentRow]); //resets depth to current line
                        ReadConvo(); //read line of text
                    }
                    break;
                default:
                    if (splitStrings[i] != "" && splitStrings[i] != " ")
                    {
                        //Split string into characters or just pass the words to ui
                    };
                    break;
            }
        }
    }

    int setDepth(string Line)
    {
        return Regex.Matches(Line, "- ").Count; //Checks and returns depth of line
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            SelectOption(1);
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            SelectOption(2);
        }
    }
}


