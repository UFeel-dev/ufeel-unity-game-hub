using System.Collections.Generic;

namespace GameHub.RhythmGame
{
    public static class ChartSetBuilder
    {
        public static ChartSet Build(SongAnalysis analysis, PatternCatalogue catalogue, int seed)
        {
            RhythmLayers rhythm = new(analysis);
            LaneAssigner lanes = new(analysis, catalogue, seed);
            ChartLayer[] layers = new ChartLayer[DifficultySettings.LayerCount];
            List<ChartEvent>[]? previous = null;
            for (int layer = 0; layer < layers.Length; layer++)
            {
                List<ChartEvent>[] measures = rhythm.Plan(layer, previous);
                previous = measures;

                List<ChartEvent> events = new();
                foreach (List<ChartEvent> measure in measures)
                    events.AddRange(measure);

                DifficultyParameters parameters = DifficultyTable.At(DifficultySettings.LayerLevel(layer));
                HoldPlanner.Apply(events, parameters, analysis);
                layers[layer] = lanes.Assign(parameters, layer, events);
            }
            return new ChartSet(analysis, layers);
        }
    }
}
