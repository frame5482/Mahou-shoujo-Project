using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

    [CreateAssetMenu(fileName = "NewMission", menuName = "MissionSystem/Mission")]
    public class QuestBase : ScriptableObject
    {
        [Header("Left Panel Info")]
        public string missionTitle;
        [TextArea] public string descriptionTH;
        [TextArea] public string descriptionEN;
        [TextArea] public string descriptionJP;

        [Header("Phase 1: Briefing Dialogue")]
        // เราจะใช้ Class ที่หน้าตาเหมือน TextData ของนายท่านมาเก็บข้อมูลตรงนี้
        public MissionDialogueData briefingLine;

        [Header("Phase 2: Choices")]
        public List<MissionChoice> choices;
    }

    [System.Serializable]
    public class MissionDialogueData
    {
        public string speakerName;
        public Sprite speakerImage;
        public Sprite bgImage;
        [TextArea] public string thSentence;
        [TextArea] public string engSentence;
        [TextArea] public string jpSentence;
    }

    [System.Serializable]
    public class MissionChoice
    {
        public string buttonText; // ชื่อปุ่ม (เช่น "บุกโจมตี")

        // ผลลัพธ์เมื่อเลือกข้อนี้
        public MissionDialogueData resultLine;

    // เงื่อนไข (DEX, Skill) - เอาตามโค้ดเก่าที่ข้าให้ไป
        public string Skillneed;
        public int minDex;
        
    }
