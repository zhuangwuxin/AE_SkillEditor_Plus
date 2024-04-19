﻿namespace AE_SkillEditor_Plus.Event
{
    public class ClipEvent : BaseEvent
    {
        public int TrackIndex;
        public int ClipIndex;
    }

    public class ClipMoveEvent : ClipEvent
    {
        public ClipMoveEvent()
        {
            this.EventType = EventType.ClipMove;
        }

        public float OffsetMouseX; //鼠标相对Start的偏移 单位是Rect的x
    }
    
    public class ClipMoveEndEvent : ClipEvent
    {
        public ClipMoveEndEvent()
        {
            this.EventType = EventType.ClipMoveEnd;
        }
    }

    public class ClipClickEvent : ClipEvent
    {
        public ClipClickEvent()
        {
            this.EventType = EventType.ClipClick;
        }

        public float OffsetMouseX; //鼠标相对Start的偏移 单位是Rect的x
    }

    public class ClipResizeEvent : ClipEvent
    {
        public ClipResizeEvent()
        {
            this.EventType = EventType.ClipResize;
        }

        public float OffsetMouseX; //鼠标相对End的偏移 单位是Rect的x
    }

    public class ClipResizeEndEvent : ClipEvent
    {
        public ClipResizeEndEvent()
        {
            this.EventType = EventType.ClipResizeEnd;
        }
    }
    
    public class ClipRightClickEvent : ClipEvent
    {
        public ClipRightClickEvent()
        {
            this.EventType = EventType.ClipRightClick;
        }
    }

    public class KeyboradEvent : ClipEvent
    {
        public KeyboradEvent()
        {
            this.EventType = EventType.ClipKeyborad;
        }

        public Shortcut Shortcut;
    }
}