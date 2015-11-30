using DeadDog.PDF;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace AttackTree
{
    public static class GraphBuilder
    {
        static IEnumerable<string> GetLinks(string text)
        {
            var regex = new Regex(@"[a-zA-Z]+[ \t]*->[ \t]*[a-zA-Z]+[ \t]*\[[^\]]+\]");

            return from m in regex.Matches(text).OfType<Match>()
                   select m.Value;
        }

        public static void Build(string dot, string outputpath, bool open)
        {
            Vector2D pagesize;
            var nodes = Node.GetNodes(dot, out pagesize);
            var links = Link.GetLinks(dot, nodes);

            PDFDocument doc = new PDFDocument();
            var page = doc.Pages.Add(new Page(pagesize));

            var space = new Vector1D(0.15, UnitsOfMeasure.Centimeters);

            for (int i = 0; i < links.Length; i++)
            {
                var link = links[i];

                var allTo = links.Where(x => x.To == link.To).ToList();
                var allFrom = links.Where(x => x.From == link.From).ToList();

                var p1 = new Vector2D(link.From.Center.X, link.From.GateRect.Bottom);
                var p3 = new Vector2D(link.To.Center.X, link.To.BoxRect.Top);

                p1.X += allFrom.IndexOf(link) * space - ((allFrom.Count - 1) * space) / 2;

                page.Objects.Add(new Connector(p1, p3 - p1, link.From.Center.X));
            }

            for (int i = 0; i < nodes.Length; i++)
                page.Objects.AddRange(HandleNode(nodes[i]));

            doc.Create(outputpath, open);
        }

        static IEnumerable<PDFObject> HandleNode(Node node)
        {
            yield return new Box(node.BoxSize) { Offset = node.BoxOffset };
            yield return new TextLine(Node.Font) { Alignment = TextAlignment.Center, Text = node.Label, Offset = node.TextOffset };

            switch (node.NodeType)
            {
                case Node.NodeTypes.AND:
                    yield return new AndGate(node.GateSize.Y) { Offset = node.GateOffset };
                    break;

                case Node.NodeTypes.OR:
                    yield return new OrGate(node.GateSize.Y) { Offset = node.GateOffset };
                    break;
            }
        }
    }
}
