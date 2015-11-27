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

            PDFDocument doc = new PDFDocument();
            var page = doc.Pages.Add(new Page(pagesize));

            for (int i = 0; i < nodes.Length; i++)
                page.Objects.AddRange(HandleNode(nodes[i]));

            var links = Link.GetLinks(dot, nodes);

            for (int i = 0; i < links.Length; i++)
                page.Objects.AddRange(Handlelink(links[i]));

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

                    yield return new Line(new Vector2D(Vector1D.Zero, node.GateRect.Top - node.BoxRect.Bottom))
                    { Offset = new Vector2D(node.Center.X, node.BoxRect.Bottom) };
                    break;

                case Node.NodeTypes.OR:
                    yield return new OrGate(node.GateSize.Y) { Offset = node.GateOffset };

                    yield return new Line(new Vector2D(Vector1D.Zero, node.GateRect.Top - node.BoxRect.Bottom))
                    { Offset = new Vector2D(node.Center.X, node.BoxRect.Bottom) };
                    break;
                default:
                    //yield return new Box(node.Size) { Offset = node.Center - node.Size / 2, FillColor = node.NodeType == Node.NodeTypes.OR ? Color.OrangeRed : Color.GreenYellow };
                    break;
            }

            //yield return new Box(node.Size) { Offset = node.Center - node.Size / 2, FillColor = node.NodeType == Node.NodeTypes.OR ? Color.OrangeRed : Color.GreenYellow };
        }
        static IEnumerable<PDFObject> Handlelink(Link link)
        {
            var p1 = new Vector2D(link.From.Center.X, link.From.GateRect.Bottom);
            var p3 = new Vector2D(link.To.Center.X, link.To.BoxRect.Top);

            var size = p3 - p1;
            var p2 = p3 - new Vector2D(Vector1D.Zero, size.Y / 2);

            if (link.From.NodeType == Node.NodeTypes.Unknown)
            {
                var p1_ = new Vector2D(link.From.Center.X, link.From.BoxRect.Bottom);
                yield return new Line(new Vector2D(Vector1D.Zero, (p2 - p1_).Y)) { Offset = p1_ };
            }
            else
                yield return new Line(new Vector2D(Vector1D.Zero, size.Y / 2)) { Offset = p1 };
            yield return new Line(new Vector2D(-size.X, Vector1D.Zero)) { Offset = p2 };
            yield return new Line(new Vector2D(Vector1D.Zero, size.Y / 2)) { Offset = p2 };
        }
    }
}
