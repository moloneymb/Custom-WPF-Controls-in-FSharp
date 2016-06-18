using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xml;

namespace Explorer
{
	/// <summary>
	/// Creates a complete Uri for each example. This is used in conjunction with the schema of
	/// BookMeta.xml
	/// </summary>
	public class XmlToUriConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
			{
				return null;
			}

			XmlNode elt = value as XmlNode;

			string uri = null;

			if (elt.Name == "Chapter")
			{
				if (elt.Attributes["MetaFile"] != null)
				{
					string metaFile = elt.Attributes["MetaFile"].Value;
					uri = "/Chapters;component/ChapterDescriptions/" + metaFile;
				}
			}
			else if (elt.Name == "Item")
			{
				string chapterNum = string.Format("{0:00}", int.Parse(elt.ParentNode.Attributes["Order"].Value));
				string file = elt.Attributes["File"].Value;

				uri = "/Chapters;component/Chapter" + chapterNum + "/" + file;
			}

			return uri;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new System.NotImplementedException();
		}
	}
}