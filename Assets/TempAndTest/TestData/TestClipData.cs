﻿using System;
using AE_SkillEditor_Plus.RunTime;
using AE_SkillEditor_Plus.RunTime.Attribute;
using UnityEngine.Timeline;

namespace AE_SkillEditor_Plus.TempAndTest.TestData
{
    [AETrackName(Name = "测试轨道1")]
    [AEBindClip(ClipType = typeof(TestClipData))]
    [AETrackColor(1,0,0)]
    [Serializable]
    public class TestTrackData : StandardTrack
    {
    }
    [Serializable]
    public class TestClipData : StandardClip
    {
        public string Str;
    }
    
    [AETrackName(Name = "测试轨道2")]
    [AEBindClip(ClipType = typeof(TestClipData2))]
    [AETrackColor(0,1,0)]
    [Serializable]
    public class TestTrackData2 : StandardTrack
    {
    }
    [Serializable]
    public class TestClipData2 : StandardClip
    {
        public string Str;
    }
}