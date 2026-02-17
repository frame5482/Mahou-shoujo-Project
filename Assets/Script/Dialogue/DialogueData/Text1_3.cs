using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DialogueData;
using UnityEngine.UI;

public class Text1_3 : Textbase
{ 
    
    public Sprite emty;
    public Sprite Smallemty;

    public Sprite[] Xeno;
    public Sprite[] SmallXeno;

    public Sprite[] REI;
    public Sprite[] SmallREI;

   
   
    
    public Sprite[] BGImage;
    public Sprite[] StoryImage;


 


    public void Start()
    {

        TextData.Add(new DialogueData
        {
            speakerName = "XXXXXXXXXXXXXXXXX",

            Thaisentence = "เอาละ จะมีอะไรอยู่ข้างในกันนะ",
            ENGsentence = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
            Jpsentence = "さて、中には何があるのだろうか。",

            speakerImage = REI[0],
            SmallImage = SmallREI[0],

            BGImage = BGImage[0],
            StoryImage = StoryImage[0]
        });

        TextData.Add(new DialogueData
        {
            speakerName = "Narrator",

            Thaisentence = "เรย์เปิดประตู",
            ENGsentence = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX opens the door",
            Jpsentence = "レイが扉を開ける",

            speakerImage = emty,
            SmallImage = Smallemty,

            BGImage = BGImage[0],
            StoryImage = StoryImage[0]
        });

    }



}
