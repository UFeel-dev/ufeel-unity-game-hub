using System.Collections.Generic;

namespace GameHub.RhythmGame
{
    public sealed class NoteJudge
    {
        private const double Linger = 1.0;

        private readonly List<PlayableNote> notes = new(128);
        private readonly List<JudgementEvent> events = new(16);

        public IReadOnlyList<PlayableNote> Notes => notes;

        public IReadOnlyList<JudgementEvent> FrameEvents => events;

        public void Add(NoteData data)
        {
            notes.Add(new PlayableNote(data));
        }

        public void Clear()
        {
            notes.Clear();
            events.Clear();
        }

        public void Tick(double songTime, ILaneInput input, double rate)
        {
            events.Clear();
            for (int lane = 0; lane < LaneMask.LaneCount; lane++)
            {
                if (input.WasPressed(lane))
                    Press(lane, songTime, rate);
                if (input.WasReleased(lane))
                    Release(lane, songTime, rate);
            }
            Expire(songTime, rate);
            RemoveFinished(songTime);
        }

        private void Press(int lane, double songTime, double rate)
        {
            foreach (PlayableNote note in notes)
            {
                if (note.Lane != lane || note.HeadResult.HasValue)
                    continue;

                double offset = (songTime - note.Time) / rate;
                if (offset < -HitWindows.Meh)
                    return;

                Judgement? judgement = HitWindows.JudgeHead(offset);
                if (!judgement.HasValue)
                    continue;

                note.JudgeHead(judgement.Value);
                Emit(note, judgement.Value, offset, false, true);
                return;
            }
        }

        private void Release(int lane, double songTime, double rate)
        {
            foreach (PlayableNote note in notes)
            {
                if (note.Lane != lane || !note.IsHolding)
                    continue;

                double offset = (songTime - note.EndTime) / rate;
                Judgement judgement = offset < -HitWindows.EarlyRelease ? Judgement.Miss : HitWindows.JudgeTail(offset);
                note.JudgeTail(judgement);
                Emit(note, judgement, offset, true, true);
                return;
            }
        }

        private void Expire(double songTime, double rate)
        {
            foreach (PlayableNote note in notes)
            {
                double offset = (songTime - note.Time) / rate;
                if (!note.HeadResult.HasValue && offset > HitWindows.Meh)
                {
                    note.JudgeHead(Judgement.Miss);
                    Emit(note, Judgement.Miss, offset, false, true);
                    if (note.IsHold)
                    {
                        note.JudgeTail(Judgement.Miss);
                        Emit(note, Judgement.Miss, (songTime - note.EndTime) / rate, true, false);
                    }
                }
                else if (note.IsHolding && songTime >= note.EndTime)
                {
                    note.JudgeTail(Judgement.Perfect);
                    Emit(note, Judgement.Perfect, 0d, true, true);
                }
            }
        }

        private void RemoveFinished(double songTime)
        {
            int kept = 0;
            for (int i = 0; i < notes.Count; i++)
            {
                PlayableNote note = notes[i];
                if (note.IsDone && songTime > note.EndTime + Linger)
                    continue;
                notes[kept++] = note;
            }
            notes.RemoveRange(kept, notes.Count - kept);
        }

        private void Emit(PlayableNote note, Judgement judgement, double offset, bool isTail, bool drainsHealth)
        {
            events.Add(new JudgementEvent(note.Lane, judgement, offset, isTail, drainsHealth, note.Data.Level));
        }
    }
}
