using System.Collections.Generic;

namespace GameHub.RhythmGame
{
    public sealed class ChartLayer
    {
        private readonly NoteData[] notes;
        private readonly int[] measureStarts;

        public ChartLayer(DifficultyParameters parameters, List<NoteData> notes, List<int> noteMeasures, int measureCount)
        {
            Parameters = parameters;
            this.notes = notes.ToArray();
            measureStarts = new int[measureCount + 1];
            int note = 0;
            for (int measure = 0; measure <= measureCount; measure++)
            {
                while (note < noteMeasures.Count && noteMeasures[note] < measure)
                    note++;
                measureStarts[measure] = note;
            }
        }

        public DifficultyParameters Parameters { get; }

        public float Level => Parameters.Level;

        public int NoteCount => notes.Length;

        public NoteData Note(int index)
        {
            return notes[index];
        }

        public int MeasureStart(int measure)
        {
            return measureStarts[measure];
        }

        public int MeasureEnd(int measure)
        {
            return measureStarts[measure + 1];
        }

        public int NotesInMeasure(int measure)
        {
            return measureStarts[measure + 1] - measureStarts[measure];
        }
    }
}
