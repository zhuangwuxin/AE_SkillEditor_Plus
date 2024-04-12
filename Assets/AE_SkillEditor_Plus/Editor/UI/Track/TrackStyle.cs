﻿using AE_SkillEditor_Plus.UI.Data;
using UnityEngine;

namespace AE_SkillEditor_Plus.UI
{
    public class TrackStyle
    {
        public int TrackID;

        public void UpdateUI(ClipEditorWindow window,Rect rect, int headWidth,TrackStyleData data,int trackIndex)
        {
            //划分
            var headRect = new Rect(rect.x, rect.y, headWidth * data.WidthPreFrame,
                rect.height);
            TrackHeadStyle.UpdateUI(headRect, data,trackIndex);
            //划分
            var bodyRect = new Rect(rect.x + headWidth * data.WidthPreFrame,
                rect.y,
                rect.width - headWidth * data.WidthPreFrame,
                rect.height);
            TrackBodyStyle.UpdateUI(window,bodyRect, data,trackIndex);
        }
    }
}