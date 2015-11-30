using DeadDog.PDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AttackTree
{
    public class Node
    {
        public enum NodeTypes
        {
            AND,
            OR,
            Unknown
        }

        private static readonly Vector1D levelspacer = new Vector1D(1.5, UnitsOfMeasure.Centimeters);

        private static string GetName(string node)
        {
            return Regex.Match(node, "^[a-zA-Z0-9_]+").Value;
        }
        private static string GetValue(string node, string key)
        {
            var m = Regex.Match(node, $@"[^\[]+\[[^\]]*({key})=(""([^""]+)""|[^""][^,\]]*)");

            var temp = m.Success ? m.Groups[2].Value : null;

            if (temp?[0] == '"')
                temp = temp.Substring(1, temp.Length - 2);

            if (temp?.Contains("\\n") ?? false)
                temp = temp.Replace("\\n", "\n");

            return temp;
        }
        private static Vector1D GetOne(string node, string key, UnitsOfMeasure unit)
        {
            return GetValues(node, key, unit).First();
        }
        private static Vector2D GetTwo(string node, string key, UnitsOfMeasure unit)
        {
            var values = GetValues(node, key, unit).ToArray();
            return new Vector2D(values[0], values[1]);
        }
        private static IEnumerable<Vector1D> GetValues(string node, string key, UnitsOfMeasure unit)
        {
            var value = GetValue(node, key);
            if (value == null)
                yield break;

            var format = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

            foreach (var v in value.Split(','))
            {
                double d = double.Parse(v, format);
                yield return new Vector1D(d, unit);
            }
        }

        private Vector2D center;
        private Vector2D size;
        private string name;
        private string label;
        private NodeTypes nodeType;

        public static Node[] GetNodes(string dot, out Vector2D pagesize)
        {
            var regex = new Regex(@"(?<!->)[ \t]([a-zA-Z0-9_]+[ \t]*\[[^\]]+\])");

            var nodes = (from m in regex.Matches(dot).OfType<Match>()
                         select m.Groups[1].Value).ToArray();
            var graph = nodes.First(x => GetName(x) == "graph");
            nodes = (from n in nodes
                     let name = GetName(n)
                     where name != "node" && name != "edge" && name != "graph"
                     select n).ToArray();

            var bounds = GetValues(graph, "bb", UnitsOfMeasure.Points).ToArray();
            var levels = nodes.Select(x => GetTwo(x, "pos", UnitsOfMeasure.Points).Y).Distinct().OrderBy(x => x).ToList();
            pagesize = new Vector2D(bounds[2], bounds[3] + (levels.Count - 1) * levelspacer);

            var result = nodes.Select(x => new Node(x, levels)).ToArray();
            for (int i = 0; i < result.Length; i++)
            {
                var y = result[i].center.Y;
                var h = result[i].size.Y;

                y = pagesize.Y - y;

                result[i].center.Y = y;
            }

            return result;
        }

        private Node(string n, List<Vector1D> levels)
        {
            this.center = GetTwo(n, "pos", UnitsOfMeasure.Points);
            this.size = new Vector2D(GetOne(n, "width", UnitsOfMeasure.Inches), GetOne(n, "height", UnitsOfMeasure.Inches));
            this.name = GetName(n);
            this.label = GetValue(n, "label") ?? name;

            var index = levels.IndexOf(center.Y);
            center.Y += index * levelspacer;

            var shape = GetValue(n, "shape");
            this.nodeType = shape == "AND" ? NodeTypes.AND : shape == "OR" ? NodeTypes.OR : NodeTypes.Unknown;
        }

        public Vector2D Center => center;
        public Vector2D Size => size;
        public string Name => name;
        public string Label => label;
        public NodeTypes NodeType => nodeType;

        public static Vector1D Margin => new Vector1D(0.2, UnitsOfMeasure.Centimeters);

        public static readonly Vector1D GateHeight = new Vector1D(0.5, UnitsOfMeasure.Inches);
        private static Vector2D boxMargin => new Vector2D(Margin, Margin);

        public Vector4D TextRect => new Vector4D(TextOffset, TextSize);
        public Vector2D TextOffset => center + new Vector2D(Vector1D.Zero, boxMargin.Y - size.Y / 2);
        public Vector2D TextSize => TextLine.CalculateSize(label, font);

        public Vector4D BoxRect => new Vector4D(BoxOffset, BoxSize);
        public Vector2D BoxOffset => center - new Vector2D(TextSize.X / 2 + boxMargin.X, size.Y / 2);
        public Vector2D BoxSize => TextSize + 2 * boxMargin;

        public Vector4D GateRect => new Vector4D(GateOffset, GateSize);
        public Vector2D GateOffset => center + new Vector2D(-GateSize.X / 2, BoxSize.Y - size.Y / 2 + Margin);
        public Vector2D GateSize => new Vector2D(0.8 * GateHeight, GateHeight);

        private static FontInfo font = new FontInfo("Calibri", 12);
        public static FontInfo Font => font;
    }
}
