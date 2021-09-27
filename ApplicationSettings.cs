namespace ZembryoAnalyser
{
    public static class ApplicationSettings
    {
        public static SettingsDatabase Settings { get; set; }

        static ApplicationSettings()
        {
            Settings = new SettingsDatabase();
        }
    }
}
