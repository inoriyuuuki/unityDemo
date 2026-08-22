using FMBG.Skills;
using FMBG.Timeline;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace FMBG.Tests.EditMode
{
    /// <summary>技能时间轴数据模型与播放器派发的确定性测试。</summary>
    public sealed class SkillTimelineTests
    {
        private sealed class RecorderClip : SkillTimelineClip
        {
            public int EnterCount;
            public int UpdateCount;
            public int ExitCount;
            public int ReverseCount;
            public float LastLocalTime;

            public override string DefaultName => "Recorder";

            public override void OnEnter(SkillClipContext context)
            {
                EnterCount++;
            }

            public override void OnUpdate(SkillClipContext context, float localTime)
            {
                UpdateCount++;
                LastLocalTime = localTime;
            }

            public override void OnExit(SkillClipContext context)
            {
                ExitCount++;
            }

            public override void OnReverse(SkillClipContext context)
            {
                ReverseCount++;
            }
        }

        [Test]
        public void SkillTimeline_AddRemoveAndContentLength()
        {
            SkillTimeline timeline = ScriptableObject.CreateInstance<SkillTimeline>();
            timeline.Length = 1f;

            timeline.AddClip(new FaceTargetClip { StartTime = 0f, Duration = 0.2f });
            timeline.AddClip(new MeleeHitWindowClip { StartTime = 0.2f, Duration = 0.15f });

            Assert.AreEqual(2, timeline.Clips.Count);
            Assert.AreEqual(0.35f, timeline.ContentLength, 0.001f);

            timeline.AddClip(new DashClip { StartTime = 0.5f, Duration = 0.3f });
            Assert.AreEqual(0.8f, timeline.ContentLength, 0.001f);
            Assert.AreEqual(1f, timeline.Length, 0.001f, "Length 应至少覆盖内容");

            SkillTimelineClip hit = timeline.Clips[1];
            timeline.RemoveClip(hit);
            Assert.AreEqual(2, timeline.Clips.Count);
            Assert.AreEqual(0.8f, timeline.ContentLength, 0.001f);
        }

        [Test]
        public void SkillTimeline_SortOrdersByStartTime()
        {
            SkillTimeline timeline = ScriptableObject.CreateInstance<SkillTimeline>();
            timeline.AddClip(new AttackAnimationClip { StartTime = 0.5f, Duration = 0.2f });
            timeline.AddClip(new SpawnProjectileClip { StartTime = 0.1f, Duration = 0.01f });
            timeline.AddClip(new FaceTargetClip { StartTime = 0f, Duration = 0.3f });

            timeline.SortClips();

            Assert.AreEqual(0f, timeline.Clips[0].StartTime, 0.001f);
            Assert.AreEqual(0.1f, timeline.Clips[1].StartTime, 0.001f);
            Assert.AreEqual(0.5f, timeline.Clips[2].StartTime, 0.001f);
        }

        [Test]
        public void SkillTimelineClip_DefaultTrackTypes()
        {
            Assert.AreEqual(SkillTrackType.Animation, new AttackAnimationClip().TrackType);
            Assert.AreEqual(SkillTrackType.Animation, new FaceTargetClip().TrackType);
            Assert.AreEqual(SkillTrackType.Gameplay, new MeleeHitWindowClip().TrackType);
            Assert.AreEqual(SkillTrackType.Gameplay, new SpawnProjectileClip().TrackType);
            Assert.AreEqual(SkillTrackType.Effect, new SpawnEffectClip().TrackType);
            Assert.AreEqual(SkillTrackType.Audio, new PlaySkillAudioClip().TrackType);
        }

        [Test]
        public void SkillTimeline_TrackTypeEditable_RegroupsClips()
        {
            SkillTimeline timeline = ScriptableObject.CreateInstance<SkillTimeline>();
            AttackAnimationClip clip = new() { StartTime = 0f, Duration = 0.4f };
            timeline.AddClip(clip);

            Assert.AreEqual(1, timeline.GetClipsByTrack(SkillTrackType.Animation).Count);
            Assert.AreEqual(0, timeline.GetClipsByTrack(SkillTrackType.Effect).Count);

            clip.TrackType = SkillTrackType.Effect;

            Assert.AreEqual(0, timeline.GetClipsByTrack(SkillTrackType.Animation).Count);
            Assert.AreEqual(1, timeline.GetClipsByTrack(SkillTrackType.Effect).Count);
        }

        [Test]
        public void SkillTimeline_SerializeReference_RoundTripsTypes()
        {
            const string path = "Assets/Game/Tests/EditMode/Generated_SkillTimeline_Test.asset";

            SkillTimeline timeline = ScriptableObject.CreateInstance<SkillTimeline>();
            timeline.name = "Generated_SkillTimeline_Test";
            timeline.Length = 1f;
            timeline.AddClip(new AttackAnimationClip { StartTime = 0f, Duration = 0.4f });
            timeline.AddClip(new MeleeHitWindowClip
            {
                StartTime = 0.08f,
                Duration = 0.1f,
                hitboxSize = new Vector3(2f, 3f, 4f)
            });
            timeline.AddClip(new SpawnProjectileClip { StartTime = 0.05f, Duration = 0.01f, projectileSpeed = 25f });
            timeline.AddClip(new PlaySkillAudioClip { StartTime = 0.06f, Duration = 0.01f, volume = 0.7f });

            AssetDatabase.CreateAsset(timeline, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            try
            {
                SkillTimeline loaded = AssetDatabase.LoadAssetAtPath<SkillTimeline>(path);
                Assert.IsNotNull(loaded);
                Assert.AreEqual(4, loaded.Clips.Count);

                Assert.IsInstanceOf<AttackAnimationClip>(loaded.Clips[0]);
                Assert.IsInstanceOf<MeleeHitWindowClip>(loaded.Clips[1]);
                Assert.IsInstanceOf<SpawnProjectileClip>(loaded.Clips[2]);
                Assert.IsInstanceOf<PlaySkillAudioClip>(loaded.Clips[3]);

                Assert.AreEqual(SkillTrackType.Animation, loaded.Clips[0].TrackType);
                Assert.AreEqual(SkillTrackType.Gameplay, loaded.Clips[1].TrackType);
                Assert.AreEqual(SkillTrackType.Gameplay, loaded.Clips[2].TrackType);
                Assert.AreEqual(SkillTrackType.Audio, loaded.Clips[3].TrackType);

                MeleeHitWindowClip hit = (MeleeHitWindowClip)loaded.Clips[1];
                Assert.AreEqual(0.08f, hit.StartTime, 0.001f);
                Assert.AreEqual(0.1f, hit.Duration, 0.001f);
                Assert.AreEqual(new Vector3(2f, 3f, 4f), hit.hitboxSize);

                SpawnProjectileClip spawn = (SpawnProjectileClip)loaded.Clips[2];
                Assert.AreEqual(25f, spawn.projectileSpeed, 0.001f);
            }
            finally
            {
                AssetDatabase.DeleteAsset(path);
            }
        }

        [Test]
        public void SkillTimelinePlayer_DispatchesEnterUpdateExit()
        {
            SkillTimeline timeline = ScriptableObject.CreateInstance<SkillTimeline>();
            timeline.Length = 0.3f;
            RecorderClip recorder = new() { StartTime = 0.1f, Duration = 0.2f };
            timeline.AddClip(recorder);

            GameObject go = new("TimelinePlayer_Test");
            SkillTimelinePlayer player = go.AddComponent<SkillTimelinePlayer>();
            bool finished = false;
            player.PlayTimeline(timeline, null, () => finished = true);

            Assert.IsTrue(player.IsPlaying);

            // 0 → 0.15：进入 + 一次更新（localTime = 0.05）
            player.Tick(0.15f);
            Assert.AreEqual(1, recorder.EnterCount);
            Assert.AreEqual(1, recorder.UpdateCount);
            Assert.AreEqual(0.05f, recorder.LastLocalTime, 0.001f);

            // 0.15 → 0.35：跨过结束时间（EndTime = 0.3），退出
            player.Tick(0.2f);
            Assert.AreEqual(1, recorder.ExitCount);

            // 播放结束回调
            Assert.IsTrue(finished);
            Assert.IsFalse(player.IsPlaying);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void SkillTimelinePlayer_StopReversesEnteredClips()
        {
            SkillTimeline timeline = ScriptableObject.CreateInstance<SkillTimeline>();
            timeline.Length = 0.5f;
            RecorderClip recorder = new() { StartTime = 0.1f, Duration = 0.4f };
            timeline.AddClip(recorder);

            GameObject go = new("TimelinePlayer_StopTest");
            SkillTimelinePlayer player = go.AddComponent<SkillTimelinePlayer>();
            player.PlayTimeline(timeline, null, null);

            player.Tick(0.2f);
            Assert.AreEqual(1, recorder.EnterCount);
            Assert.AreEqual(1, recorder.UpdateCount);

            player.Stop();
            Assert.AreEqual(1, recorder.ExitCount, "Stop 应对活动 Clip 调用 OnExit");
            Assert.AreEqual(1, recorder.ReverseCount, "Stop 应对进入过的 Clip 调用 OnReverse");
            Assert.IsFalse(player.IsPlaying);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void SkillTimelinePlayer_InstantClipsFireInsideSingleTick()
        {
            SkillTimeline timeline = ScriptableObject.CreateInstance<SkillTimeline>();
            timeline.Length = 0.06f;
            RecorderClip instant = new() { StartTime = 0.05f, Duration = 0.01f };
            timeline.AddClip(instant);

            GameObject go = new("TimelinePlayer_InstantTest");
            SkillTimelinePlayer player = go.AddComponent<SkillTimelinePlayer>();
            player.PlayTimeline(timeline, null, null);

            // 一帧直接跳过 0.05~0.06，瞬时 Clip 仍应触发 OnEnter + OnExit
            player.Tick(0.3f);
            Assert.AreEqual(1, instant.EnterCount);
            Assert.AreEqual(1, instant.ExitCount);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void SkillTimelinePlayer_CompletionInvokesCallbackOnce()
        {
            SkillTimeline timeline = ScriptableObject.CreateInstance<SkillTimeline>();
            timeline.Length = 0.1f;
            timeline.AddClip(new FaceTargetClip { StartTime = 0f, Duration = 0.1f });

            GameObject go = new("TimelinePlayer_CallbackTest");
            SkillTimelinePlayer player = go.AddComponent<SkillTimelinePlayer>();
            int callbackCount = 0;
            player.PlayTimeline(timeline, null, () => callbackCount++);

            player.Tick(0.2f);
            player.Tick(0.2f);

            Assert.AreEqual(1, callbackCount, "播放结束回调只能触发一次");
            Assert.IsFalse(player.IsPlaying);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void SkillTimelinePlayer_NullTimelineFinishesImmediately()
        {
            GameObject go = new("TimelinePlayer_NullTest");
            SkillTimelinePlayer player = go.AddComponent<SkillTimelinePlayer>();
            int callbackCount = 0;

            player.PlayTimeline(null, null, () => callbackCount++);

            Assert.IsFalse(player.IsPlaying);
            Assert.AreEqual(1, callbackCount);

            Object.DestroyImmediate(go);
        }
    }
}
