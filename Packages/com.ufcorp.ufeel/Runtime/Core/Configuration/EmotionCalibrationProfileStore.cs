#nullable enable

using System;

namespace UFeel
{
    public sealed class EmotionCalibrationProfileStore
    {
        public const string FileName = "emotion-profile-v1.txt";

        public EmotionCalibrationProfileStore(string path)
        {
            Path = ProfileStorage.AbsolutePath(path);
        }

        public string Path { get; }

        public static EmotionCalibrationProfileStore CreateDefault()
        {
            return new EmotionCalibrationProfileStore(ProfileStorage.DefaultPath(FileName));
        }

        /// <summary>Invalid also when the profile was recorded with another emotion model.</summary>
        public ProfileLoadResult Load(UFeelProcessor processor)
        {
            if (processor is null)
                throw new ArgumentNullException(nameof(processor));

            if (!ProfileStorage.HasFiles(Path))
                return ProfileLoadResult.Missing;

            return processor.LoadEmotionCalibrationProfile(Path)
                ? ProfileLoadResult.Loaded
                : ProfileLoadResult.Invalid;
        }

        public bool Save(UFeelProcessor processor)
        {
            if (processor is null)
                throw new ArgumentNullException(nameof(processor));

            ProfileStorage.CreateDirectoryFor(Path);
            return processor.SaveEmotionCalibrationProfile(Path);
        }

        public bool Remove() => ProfileStorage.RemoveFiles(Path);
    }
}
