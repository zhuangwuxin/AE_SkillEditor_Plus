﻿using System;
using System.Collections.Generic;
using System.Reflection;
using AE_SkillEditor_Plus.Editor.UI.Controller;
using AE_SkillEditor_Plus.AEUIEvent;
using AE_SkillEditor_Plus.Factory;
using AE_SkillEditor_Plus.RunTime;
using AE_SkillEditor_Plus.RunTime.Attribute;
using AE_SkillEditor_Plus.UI;
using AE_SkillEditor_Plus.UI.Data;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace AE_SkillEditor_Plus.Editor.Window
{
    public class AETimelineEditorWindow : EditorWindow
    {
        #region Static

        public static string Title = "AETimeline编辑器";

        //打开窗口
        [MenuItem("Tools/AETimeline编辑器")]
        public static void OpenWindiw()
        {
            var window = CreateWindow<AETimelineEditorWindow>(Title);
        }

        #endregion

        private int TrackHeight = 30; //轨道高度
        private float widthPreFrame = 1f;

        private float WidthPreFrame
        {
            get { return widthPreFrame; }
            set
            {
                if ((value) <= 0.005f) widthPreFrame = 0.005f;
                else if (value >= 194f) widthPreFrame = 194f;
                else widthPreFrame = value;
            }
        } //每帧多宽

        private int HeadWidth = 260; //轨道头部宽度

        private int IntervalHeight = 10; //轨道间隔
        private int ControllerHeight = 38; //控件高度
        private int currentFrameID;

        private int CurrentFrameID
        {
            get { return currentFrameID; }
            set
            {
                if (value < 0) return;
                if (currentFrameID == value) return;
                currentFrameID = value;
                CurrentFrameIDChange();
            }
        } //当前帧

        private StandardClip tempObject; //临时的一个变量 保存CtrlC的数据

        public int MouseCurrentFrameID //鼠标位置转化为FrameID
            => (int)((UnityEngine.Event.current.mousePosition.x - HeadWidth) / WidthPreFrame);

        private int[] HighLight = { -1, -1 }; //选中的Clip

        private bool IsPlaying; //是否在播放
        public int FPS = 60; //帧率
        private float OneFrameTimer; //一帧时长的计时器

        private AETimelineAsset Asset;
        private string assetPath = "";

        public string AssetPath
        {
            get { return assetPath; }
            set
            {
                if (value == "") return;
                assetPath = value;
                Asset = AETimelineFactory.LoadTimeLineAsset(assetPath);
            }
        }

        private Vector2 ScrollPosHead;
        private Vector2 ScrollPosBody;
        private Vector2 ScrollPosTimeline;

        //初始化
        private void OnEnable()
        {
            EventCenter.AddEventListener(this, ProcessEvent);
            if (AssetPath != "") Asset = AETimelineFactory.LoadTimeLineAsset(AssetPath);
        }

        //销毁
        private void OnDestroy()
        {
            EventCenter.RemoveEventListener(this, ProcessEvent);
        }

        //获取UI参数
        private List<TrackStyleData> GetTrackStyleFromData()
        {
            if (Asset == null) return new List<TrackStyleData>();
            var trackStyles = new List<TrackStyleData>();

            for (var i = 0; i < Asset.Tracks.Count; i++)
            {
                var trackData = Asset.Tracks[i];
                var trackStyle = new TrackStyleData();
                trackStyle.Clips = new List<ClipStyleData>();
                string name = trackData.GetType().GetCustomAttribute<AETrackNameAttribute>().Name;
                trackStyle.Name = name;
                var colorAttribute = trackData.GetType().GetCustomAttribute<AETrackColorAttribute>();
                trackStyle.Color = colorAttribute == null
                    ? new Color(251f / 255, 0f / 255, 253f / 255)
                    : colorAttribute.Color;
                for (var j = 0; j < trackData.Clips.Count; j++)
                {
                    var clip = trackData.Clips[j];
                    trackStyle.Clips.Add(new ClipStyleData()
                    {
                        Name = clip.Name,
                        StartID = clip.StartID,
                        EndID = clip.StartID + clip.Duration,
                    });
                }

                trackStyles.Add(trackStyle);
            }

            return trackStyles;
        }

        //更新UI
        private void OnGUI()
        {
            var styleData = GetTrackStyleFromData();
            //划分左右
            var leftRect = new Rect(0, 0, HeadWidth, position.height);
            var rightRect = new Rect(HeadWidth, 0, position.width - HeadWidth, position.height);

            var controllerRect = new Rect(leftRect.x, leftRect.y, leftRect.width, ControllerHeight);
            var trackHeadRect = new Rect(leftRect.x, 10 + leftRect.y + ControllerHeight, leftRect.width,
                leftRect.height - ControllerHeight - 10);
            var trackbodyRect = new Rect(rightRect.x, 10 + rightRect.y + ControllerHeight,
                rightRect.width,
                rightRect.height - ControllerHeight - 10);
            var timelineRect = new Rect(rightRect.x, rightRect.y, rightRect.width, ControllerHeight);

            // 事件添加轨道
            if (leftRect.Contains(UnityEngine.Event.current.mousePosition) &&
                UnityEngine.Event.current.type == UnityEngine.EventType.MouseDown &&
                UnityEngine.Event.current.button == 1)
            {
                GenericMenu menu = new GenericMenu();
                // 添加菜单项
                //拿到所有StandardTrack的子类型
                CreateAddTreakMenu(Asset.Tracks.Count, menu);
                // 显示菜单
                menu.ShowAsContext();
            }

            //时间轴
            var newTimeLineRect = new Rect(timelineRect.x, timelineRect.y, timelineRect.width + ScrollPosTimeline.x,
                timelineRect.height);
            ScrollPosTimeline.y = 0;
            ScrollPosTimeline = GUI.BeginScrollView(timelineRect, ScrollPosTimeline, newTimeLineRect, GUIStyle.none,
                GUIStyle.none);
            // EditorGUI.DrawRect(newTimeLineRect,
            // new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
            TimeLine.UpdateGUI(this, CurrentFrameID, WidthPreFrame, newTimeLineRect);
            GUI.EndScrollView();

            //轨道体
            var newTrackBodyRect = new Rect(trackbodyRect.x, trackbodyRect.y, trackbodyRect.width + ScrollPosBody.x,
                trackbodyRect.height + ScrollPosBody.y);
            ScrollPosBody = GUI.BeginScrollView(trackbodyRect, ScrollPosBody, newTrackBodyRect
                , true, true);
            ScrollPosTimeline.x = ScrollPosBody.x;
            ScrollPosHead.y = ScrollPosBody.y;
            // EditorGUI.DrawRect(newTrackBodyRect,
            // new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
            float height = trackbodyRect.y;
            for (var index = 0; index < styleData.Count; index++)
            {
                var trackData = styleData[index];
                var track = new TrackStyle();
                //为轨道划分Rect
                var rectBody = new Rect(trackbodyRect.x, height, newTrackBodyRect.width, TrackHeight);
                TrackBodyStyle.UpdateUI(this, rectBody, HighLight, widthPreFrame, trackData, index);
                height += TrackHeight + IntervalHeight;
            }

            GUI.EndScrollView();

            TimeLine.UpdateCurFrameUI(this, CurrentFrameID, WidthPreFrame,
                new Rect(timelineRect.x - ScrollPosBody.x, timelineRect.y, timelineRect.width + ScrollPosBody.x,
                    timelineRect.height));

            EditorGUI.DrawRect(new Rect(leftRect.x, leftRect.y, leftRect.width - 10f, leftRect.height),
                new Color(56f / 255, 56f / 255, 56f / 255));

            //控件
            // EditorGUI.DrawRect(controllerRect,
            // new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
            Controller.UpdateGUI(this, controllerRect);

            //轨道头部
            var newTrackHeadRect = new Rect(trackHeadRect.x, trackHeadRect.y, trackHeadRect.width,
                trackHeadRect.height + ScrollPosHead.y);
            ScrollPosHead.x = 0;
            ScrollPosHead = GUI.BeginScrollView(trackHeadRect, ScrollPosHead, newTrackHeadRect
                , GUIStyle.none, GUIStyle.none);
            // EditorGUI.DrawRect(newTrackHeadRect,
            // new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
            height = trackHeadRect.y;
            for (var index = 0; index < styleData.Count; index++)
            {
                var trackData = styleData[index];
                var track = new TrackStyle();
                //为轨道划分Rect
                var rectHead = new Rect(newTrackHeadRect.x, height, newTrackHeadRect.width, TrackHeight);
                TrackHeadStyle.UpdateUI(this, rectHead, trackData, index);
                height += TrackHeight + IntervalHeight;
            }

            GUI.EndScrollView();
        }

        private void Update()
        {
            if (!IsPlaying) return;
            OneFrameTimer -= 1f / FPS;
            if (OneFrameTimer <= 0)
            {
                OneFrameTimer = 1f / FPS;
                Tick(CurrentFrameID);
                CurrentFrameID += 1;
            }
        }

        private void Tick(int frameID)
        {
            // Debug.Log(UnityEngine.Event.current.mousePosition.x);
            // Debug.Log(currentFrameID);
        }

        #region 事件处理

        //处理事件
        private void ProcessEvent(BaseEvent baseEvent)
        {
            switch (baseEvent.AeuiEventType)
            {
                //Clip事件
                case AEUIEventType.ClipMove:
                {
                    ClipMove((ClipMoveEvent)baseEvent);
                    break;
                }
                case AEUIEventType.ClipResize:
                {
                    ClipResize((ClipResizeEvent)baseEvent);
                    break;
                }
                case AEUIEventType.ClipKeyborad:
                {
                    ProcessKeyborad((KeyboradEvent)baseEvent);
                    break;
                }
                case AEUIEventType.ClipClick:
                    ClipClick((ClipClickEvent)baseEvent);
                    break;
                case AEUIEventType.ClipRightClick:
                    ClipRightClick((ClipRightClickEvent)baseEvent);
                    break;
                case AEUIEventType.ClipResizeEnd:
                case AEUIEventType.ClipMoveEnd:
                    AETimelineFactory.Save(Asset, AssetPath);
                    break;
                //控件事件
                case AEUIEventType.Controller:
                {
                    ProcessController((ControllerEvent)baseEvent);
                    break;
                }
                //Timeline
                case AEUIEventType.TimelineScale:
                {
                    ProcessTimelineScale((TimelineScaleEvent)baseEvent);
                    break;
                }
                case AEUIEventType.TimelineDrag:
                {
                    ProcessTimelineDrag((TimelineDragEvent)baseEvent);
                    break;
                }
                //右键轨道头部
                case AEUIEventType.HeadRightClick:
                    ProcessHeadRightClick((HeadRightClickEvent)baseEvent);
                    break;
                //右键轨道体
                case AEUIEventType.BodyRightClick:
                    ProcessBodyRightClick((BodyRightClick)baseEvent);
                    break;
            }
        }

        //当前帧改变事件
        private void CurrentFrameIDChange()
        {
            Tick(CurrentFrameID);
            Repaint();
        }

        //在时间轴上拖动
        private void ProcessTimelineDrag(TimelineDragEvent dragEvent)
        {
            CurrentFrameID = MouseCurrentFrameID;
        }

        //时轴缩放
        private void ProcessTimelineScale(TimelineScaleEvent scaleEvent)
        {
            // Debug.Log(scaleEvent.AEUIEventType + "  " + UnityEngine.Event.current.delta.y);
            WidthPreFrame -= Mathf.Sign(UnityEngine.Event.current.delta.y) * WidthPreFrame * 0.2f;
            // Debug.Log(WidthPreFrame);
            //仅仅是+=1的话没有卡死
            Repaint();
        }

        //控件事件
        private void ProcessController(ControllerEvent controller)
        {
            switch (controller.ControllerType)
            {
                case ControllerType.ToMostBegin:
                    CurrentFrameID = 0;
                    break;
                case ControllerType.ToPre:
                    CurrentFrameID -= 1;
                    break;
                case ControllerType.Play:
                    IsPlaying = !IsPlaying;
                    break;
                case ControllerType.ToNext:
                    CurrentFrameID += 1;
                    break;
                case ControllerType.ToMostEnd:
                    //TODO:未完成
                    CurrentFrameID += 1;
                    break;
            }
        }

        //Clip选中
        private void ClipClick(ClipClickEvent click)
        {
            // Debug.Log("选中 " + click.TrackIndex + " " + click.ClipIndex);
            HighLight[0] = click.TrackIndex;
            HighLight[1] = click.ClipIndex;
            Repaint();
        }

        //Clip右键
        private void ClipRightClick(ClipRightClickEvent click)
        {
            Debug.Log("Clip右键 " + click.TrackIndex + " " + click.ClipIndex);
        }

        //Clip大小改变
        private void ClipResize(ClipResizeEvent clipResize)
        {
            AETimelineFactory.ResizeClip(Asset, clipResize.TrackIndex, clipResize.ClipIndex, (int)(MouseCurrentFrameID -
                (clipResize.OffsetMouseX / WidthPreFrame) -
                Asset.Tracks[clipResize.TrackIndex].Clips[clipResize.ClipIndex].StartID));
            Repaint();
        }

        //Clip移动事件
        private void ClipMove(ClipMoveEvent clipMove)
        {
            AETimelineFactory.MoveClip(Asset, AssetPath, clipMove.TrackIndex, clipMove.ClipIndex,
                (int)(MouseCurrentFrameID - (clipMove.OffsetMouseX / WidthPreFrame)));
            Repaint();
        }

        //处理按键事件
        public void ProcessKeyborad(KeyboradEvent keyborad)
        {
            // Debug.Log(keyborad.Shortcut + "--" + keyborad.TrackIndex + "--" + keyborad.ClipIndex);
            switch (keyborad.Shortcut)
            {
                case Shortcut.CtrlC:
                    tempObject = AETimelineFactory.CopyClip(Asset, keyborad.TrackIndex, keyborad.ClipIndex);
                    break;
                case Shortcut.CtrlV:
                    var temp = AETimelineFactory.CopyClip(Asset, tempObject);
                    AETimelineFactory.AddClip(Asset, AssetPath, keyborad.TrackIndex, MouseCurrentFrameID, temp);
                    break;
                case Shortcut.CtrlX:
                    tempObject = AETimelineFactory.CopyClip(Asset, keyborad.TrackIndex, keyborad.ClipIndex);
                    AETimelineFactory.RemoveClip(Asset, AssetPath, keyborad.TrackIndex, keyborad.ClipIndex);
                    HighLight[0] = -1;
                    HighLight[1] = -1;
                    break;
                case Shortcut.Delete:
                    AETimelineFactory.RemoveClip(Asset, AssetPath, keyborad.TrackIndex, keyborad.ClipIndex);
                    HighLight[0] = -1;
                    HighLight[1] = -1;
                    break;
            }

            Repaint();
        }

        //右键轨道头部
        private void ProcessHeadRightClick(HeadRightClickEvent click)
        {
            GenericMenu menu = new GenericMenu();
            //创建轨道菜单
            CreateAddTreakMenu(click.TrackIndex, menu);
            menu.AddItem(new GUIContent("删除轨道"), false, () =>
            {
                AETimelineFactory.RemoveTrack(Asset, AssetPath, click.TrackIndex);
                HighLight[0] = -1;
                HighLight[1] = -1;
            });
            // 显示菜单
            menu.ShowAsContext();
        }

        // 添加创建轨道菜单项
        private void CreateAddTreakMenu(int trackIndex, GenericMenu menu)
        {
            //拿到所有StandardTrack的子类型
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            //遍历子类型得到菜单
            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(StandardTrack)))
                    {
                        var obj = Activator.CreateInstance(type) as StandardTrack;
                        menu.AddItem(new GUIContent("添加轨道/" + type.Name), false,
                            () => { AETimelineFactory.CreatTrack(Asset, AssetPath, trackIndex, obj); });
                    }
                }
            }
        }

        //右键轨道体
        private void ProcessBodyRightClick(BodyRightClick click)
        {
            GenericMenu menu = new GenericMenu();
            // 添加添加Clip
            menu.AddItem(new GUIContent("添加Clip"), false,
                () => { AETimelineFactory.AddClip(Asset, AssetPath, click.TrackIndex, click.MouseFrameID); });
            // 显示菜单
            menu.ShowAsContext();
        }

        #endregion
    }
}