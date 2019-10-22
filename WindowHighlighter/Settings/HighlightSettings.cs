using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;

namespace WindowHighlighter.Settings
{
    public class HighlightSettings
    {
        private const string SettingsFileName = "settings.xml";

        public int HighlighterWidth { get; set; }
        public Color HighlighterColor { get; set; }
        public ObservableCollection<InterestingWindow> InterestingWindows { get; set; }

        public HighlightSettings()
        {
            HighlighterWidth = 10;
            HighlighterColor = Colors.Red;
            InterestingWindows = new ObservableCollection<InterestingWindow>();
        }

        public void SaveSettings()
        {
            var xmlDocument = new XmlDocument();
            var serializer = new XmlSerializer(typeof(HighlightSettings));
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, this);
                stream.Position = 0;
                xmlDocument.Load(stream);
                xmlDocument.Save(SettingsFileName);
            }
        }

        public static HighlightSettings LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsFileName)) return new HighlightSettings();

                var xmlDocument = new XmlDocument();
                xmlDocument.Load(SettingsFileName);
                var xmlString = xmlDocument.OuterXml;
                if (string.IsNullOrEmpty(xmlString)) return new HighlightSettings();
                HighlightSettings settings;
                using (var read = new StringReader(xmlString))
                {
                    var outType = typeof(HighlightSettings);
                    var serializer = new XmlSerializer(outType);
                    using (var reader = new XmlTextReader(read))
                        settings = (HighlightSettings)serializer.Deserialize(reader);
                }
                if (settings.InterestingWindows == null) settings.InterestingWindows = new ObservableCollection<InterestingWindow>();
                return settings;
            }
            catch (Exception)
            {
                return new HighlightSettings();
            }
        }
    }
}
