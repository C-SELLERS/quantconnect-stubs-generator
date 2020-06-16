using System.Xml;

namespace LeanPythonGenerator.Parse
{
    public static class XmlExtensions
    {
        public static string GetText(this XmlElement element)
        {
            var clone = element.CloneNode(true);

            for (int i = 0, iMax = clone.ChildNodes.Count; i < iMax; i++)
            {
                var child = clone.ChildNodes[i];

                if (child.Name != "see")
                {
                    continue;
                }

                var attribute = child.Attributes["cref"] ?? child.Attributes["paramref"] ?? child.Attributes["langword"];
                var newNode = clone.OwnerDocument.CreateTextNode(attribute.InnerText);

                clone.ReplaceChild(newNode, child);
            }

            return clone.InnerText.Trim();
        }
    }
}
