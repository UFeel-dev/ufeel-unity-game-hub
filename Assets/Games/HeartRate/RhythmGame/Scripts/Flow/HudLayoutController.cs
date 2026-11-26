using System.Collections.Generic;
using GameHub.Common;
using UnityEngine;

namespace GameHub.RhythmGame
{
    public sealed class HudLayoutController
    {
        private const float Step = 10f;
        private const float FineStep = 2f;
        private const float RepeatDelay = 0.25f;
        private const float MoveSpeed = 320f;
        private const float FineSpeed = 60f;
        private const double NoteSpacing = 0.25;
        private const double Buffer = 3.0;
        public const float DemoLevel = 6f;
        private const int BackAction = 1;
        private const int NextAction = 2;
        private const int ResetAction = 3;

        private static readonly KeyHint[] Hints =
        {
            new("Move", 0, KeyHint.Up, KeyHint.Down),
            new("Next", NextAction, "Tab"),
            new("Reset", ResetAction, "R"),
            new("Done", BackAction, "Esc"),
        };

        private static readonly int[] DemoLanes = { 0, 1, 2, 3, 2, 1, 0, 2, 1, 3 };
        private static readonly Judgement[] DemoJudgements = { Judgement.Perfect, Judgement.Perfect, Judgement.Great, Judgement.Perfect, Judgement.Good, Judgement.Perfect, Judgement.Great };
        private static readonly double[] DemoOffsets = { 0.008, -0.012, 0.045, -0.004, -0.09, 0.02, -0.05 };

        private readonly SessionContext context;
        private readonly List<PlayableNote> notes = new(32);
        private readonly IdleInput idle = new();

        private ScoreKeeper keeper = new();
        private HudElement selected;
        private bool dragging;
        private float holdTime;
        private double time;
        private double nextNote;
        private int demoIndex;

        public HudLayoutController(SessionContext context)
        {
            this.context = context;
        }

        private StageView Stage => context.Stage;

        public void Enter()
        {
            time = 0d;
            nextNote = 1d;
            demoIndex = 0;
            notes.Clear();
            keeper = new ScoreKeeper();
            Stage.ShowGameplay(true);
            Stage.Hud.ResetRun();
            Stage.Hud.ApplyLayout();
            Stage.Playfield.SetStyle(RhythmSettings.NoteStyle);
            Stage.Playfield.SetKeyLabels(KeyBindings.PrimaryLabels(), RhythmSettings.ShowKeys);
            Stage.HudLayout.SetVisible(true);
            Stage.KeyBar.Set(Hints);
            PointerInput.ShowCursor(true);
        }

        public void Leave()
        {
            Stage.HudLayout.SetVisible(false);
            Stage.ShowGameplay(false);
            PlayerPrefs.Save();
        }

        public Navigation Tick(float deltaTime)
        {
            RunDemo(deltaTime);

            switch (Stage.KeyBar.Clicked(context.Mouse))
            {
                case BackAction:
                    context.Sounds.Back();
                    return Navigation.Back;
                case NextAction:
                    Next();
                    break;
                case ResetAction:
                    ResetLayout();
                    break;
            }

            if (MenuKeys.Back())
            {
                context.Sounds.Back();
                return Navigation.Back;
            }

            if (LayoutInput.GetKeyDown(KeyCode.Tab))
                Next();
            else if (LayoutInput.GetKeyDown(KeyCode.R))
                ResetLayout();

            HandleKeys(deltaTime);
            HandlePointer();
            Stage.HudLayout.Frame(Stage.Hud.ElementRect(selected), HudLayout.Label(selected));
            return Navigation.None;
        }

        private static int HeldDirection()
        {
            if (MenuKeys.Up(true))
                return 1;
            return MenuKeys.Down(true) ? -1 : 0;
        }

        private void HandleKeys(float deltaTime)
        {
            int direction = HeldDirection();
            if (direction == 0)
            {
                holdTime = 0f;
                return;
            }

            bool fine = MenuKeys.Fine();
            bool pressed = MenuKeys.Up(false) || MenuKeys.Down(false);
            if (pressed)
            {
                holdTime = 0f;
                Move(HudLayout.Y(selected) + (direction * (fine ? FineStep : Step)));
                return;
            }

            holdTime += deltaTime;
            if (holdTime >= RepeatDelay)
                Move(HudLayout.Y(selected) + (direction * (fine ? FineSpeed : MoveSpeed) * deltaTime));
        }

        private void HandlePointer()
        {
            PointerInput mouse = context.Mouse;
            if (mouse.Pressed)
            {
                for (int i = 0; i < HudLayout.ElementCount; i++)
                {
                    if (mouse.IsOver(Stage.Hud.ElementRect((HudElement)i)))
                    {
                        selected = (HudElement)i;
                        dragging = true;
                    }
                }
            }

            if (!mouse.Held)
                dragging = false;
            if (!dragging)
                return;

            RectTransform parent = (RectTransform)Stage.Hud.ElementRect(selected).parent;
            Move(mouse.LocalIn(parent).y + (parent.rect.height * parent.pivot.y));
        }

        private void Move(float y)
        {
            HudLayout.SetY(selected, y);
            Stage.Hud.SetElementY(selected, HudLayout.Y(selected));
        }

        private void Next()
        {
            selected = (HudElement)(((int)selected + 1) % HudLayout.ElementCount);
            context.Sounds.Move();
        }

        private void ResetLayout()
        {
            HudLayout.Reset();
            Stage.Hud.ApplyLayout();
            context.Sounds.Confirm();
        }

        private void RunDemo(float deltaTime)
        {
            time += deltaTime;
            double scrollTime = RhythmSettings.ScrollTimeMs / 1000d;
            while (nextNote < time + scrollTime + Buffer)
            {
                notes.Add(new PlayableNote(new NoteData(DemoLanes[demoIndex % DemoLanes.Length], nextNote, nextNote, DemoLevel)));
                demoIndex++;
                nextNote += NoteSpacing;
            }

            while (notes.Count > 0 && notes[0].Time <= time)
            {
                int index = demoIndex + notes.Count;
                Judgement judgement = DemoJudgements[index % DemoJudgements.Length];
                JudgementEvent judgementEvent = new(notes[0].Lane, judgement, DemoOffsets[index % DemoOffsets.Length], false, false, DemoLevel);
                keeper.Apply(judgementEvent);
                Stage.Hud.ShowJudgement(judgementEvent);
                Stage.Playfield.Burst(notes[0].Lane, RhythmTheme.Judgement(judgement));
                notes.RemoveAt(0);
            }

            float pulse = context.Monitor.Pulse;
            Stage.Playfield.Render(time, notes, scrollTime, null);
            Stage.Playfield.Tick(deltaTime, idle, pulse, RhythmTheme.Level(DemoLevel));
            Stage.Playfield.SetHealth(keeper.Health, pulse);
            Stage.Hud.Tick(deltaTime, keeper);
            Stage.Hud.TickHeart(deltaTime, context.Monitor.SmoothedBpm, context.Monitor.HasReading, pulse, DemoLevel, 0f, false);
        }

        private sealed class IdleInput : ILaneInput
        {
            public bool IsHeld(int lane)
            {
                return false;
            }

            public bool WasPressed(int lane)
            {
                return false;
            }

            public bool WasReleased(int lane)
            {
                return false;
            }
        }
    }
}
