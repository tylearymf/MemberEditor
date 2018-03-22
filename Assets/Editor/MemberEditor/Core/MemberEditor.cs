namespace Tylearymf.MemberEditor
{
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public sealed class MemberEditor : OdinEditorWindow
    {
        [MenuItem("Window/MemberEditor #&v")]
        static void Open()
        {
            var tView = GetWindow<MemberEditor>();
            tView.UseScrollView = true;
            tView.Show();
        }

        protected override void OnInitialize()
        {
            MemberHelper.Init();

            mEntityViews.ForEach(x =>
            {
                if (x != null) x.Start();
            });

            mStaticViews.ForEach(x =>
            {
                if (x != null) x.Start();
            });

            mNotifyViews.ForEach(x =>
            {
                if (x != null) x.Start();
            });

            mProtocolViews.ForEach(x =>
            {
                if (x != null) x.Start();
            });

            if (mSettingView != null)
            {
                mSettingView.Start();
            }
        }

        protected override void OnEndDrawEditors()
        {
            mEntityViews.ForEach(x =>
            {
                if (x != null) x.Update();
            });

            mStaticViews.ForEach(x =>
            {
                if (x != null) x.Update();
            });

            if (mSettingView != null)
            {
                mSettingView.Update();
            }
        }

        [TabGroup("实体调用", false, 0)]
        [LabelText("实体对象列表")]
        [DisableContextMenu(true, true)]
        List<EntityView> mEntityViews = new List<EntityView>()
        {
            new EntityView()
        };

        [TabGroup("静态调用", false, 1)]
        [LabelText("静态对象列表")]
        [DisableContextMenu(true, true)]
        List<StaticView> mStaticViews = new List<StaticView>()
        {
            new StaticView()
        };

        [TabGroup("通知下发", false, 2)]
        [LabelText("通知对象列表")]
        [DisableContextMenu(true, true)]
        List<NotifyView> mNotifyViews = new List<NotifyView>()
        {
            new NotifyView()
        };

        [TabGroup("协议发送", false, 3)]
        [LabelText("协议对象列表")]
        [DisableContextMenu(true, true)]
        List<ProtocolView> mProtocolViews = new List<ProtocolView>()
        {
            new ProtocolView()
        };

        [TabGroup("设置", false, 4)]
        [LabelText("设置")]
        SettingView mSettingView = new SettingView();
    }
}