#nullable enable

using System;

namespace UFeel
{
    public sealed class EyeTrackingCalibrationProfileStore
    {
        public const string FileName = "eyeTracking-profile-v4.txt";

        public EyeTrackingCalibrationProfileStore(string path)
        {
            Path = ProfileStorage.AbsolutePath(path);
        }

        public string Path { get; }

        public static EyeTrackingCalibrationProfileStore CreateDefault()
        {
            return new EyeTrackingCalibrationProfileStore(ProfileStorage.DefaultPath(FileName));
        }

        public ProfileLoadResult Load(UFeelProcessor processor)
        {
            if (processor is null)
                throw new ArgumentNullException(nameof(processor));

            if (!ProfileStorage.HasFiles(Path))
                return ProfileLoadResult.Missing;

            return processor.LoadEyeTrackingCalibrationProfile(Path)
                ? ProfileLoadResult.Loaded
                : ProfileLoadResult.Invalid;
        }

        public bool Save(UFeelProcessor processor)
        {
            if (processor is null)
                throw new ArgumentNullException(nameof(processor));

            ProfileStorage.CreateDirectoryFor(Path);
            return processor.SaveEyeTrackingCalibrationProfile(Path);
        }
    }
}
