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
                    foreach (var o in addAND(new Vector4D(node.GateOffset, node.GateSize)))
                        yield return o;

                    yield return new Line(new Vector2D(Vector1D.Zero, node.GateRect.Top - node.BoxRect.Bottom))
                    { Offset = new Vector2D(node.Center.X, node.BoxRect.Bottom) };
                    break;

                case Node.NodeTypes.OR:
                    foreach (var o in addOR(new Vector4D(node.GateOffset, node.GateSize)))
                        yield return o;

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

        public static IEnumerable<PDFObject> addAND(Vector4D rect)
        {
            return addAND(rect, Color.Black, new Vector1D(2, UnitsOfMeasure.Points));
        }
        public static IEnumerable<PDFObject> addAND(Vector4D rect, Color color, Vector1D linewidth)
        {
            var arcsize = rect.Width / 2f;
            if (arcsize > rect.Height)
                arcsize = rect.Height;

            yield return new Arc()
            {
                BorderColor = color,
                BorderWidth = linewidth,
                HasFill = false,
                StartAngle = 90,
                Extent = 90,
                Offset = new Vector2D(rect.X, rect.Y),
                Size = new Vector2D(arcsize * 2, arcsize * 2)
            };
            yield return new Arc()
            {
                BorderColor = color,
                BorderWidth = linewidth,
                HasFill = false,
                StartAngle = 0,
                Extent = 90,
                Offset = new Vector2D(rect.Right - arcsize * 2, rect.Y),
                Size = new Vector2D(arcsize * 2, arcsize * 2)
            };

            yield return new Line(new Vector2D(Vector1D.Zero, rect.Height - arcsize)) { BorderColor = color, BorderWidth = linewidth, Offset = new Vector2D(rect.X, rect.Y + arcsize) };
            yield return new Line(new Vector2D(Vector1D.Zero, rect.Height - arcsize)) { BorderColor = color, BorderWidth = linewidth, Offset = new Vector2D(rect.Right, rect.Y + arcsize) };
            yield return new Line(new Vector2D(rect.Width + linewidth, Vector1D.Zero)) { BorderColor = color, BorderWidth = linewidth, Offset = new Vector2D(rect.X - linewidth / 2, rect.Bottom) };
        }
        public static IEnumerable<PDFObject> addOR(Vector4D rect)
        {
            return addOR(rect, Color.Black, new Vector1D(2, UnitsOfMeasure.Points));
        }
        public static IEnumerable<PDFObject> addOR(Vector4D rect, Color color, Vector1D linewidth)
        {
            //yield return new Box(rect.Size) { BorderColor = color, Offset = rect.Offset, FillColor = Color.FromArgb(70, Color.DodgerBlue) };

            var sin = Math.Sin(Math.PI / 4);
            var cos = Math.Cos(Math.PI / 4);

            var s = rect.Height + rect.Width / 2;

            yield return new Arc()
            {
                BorderColor = color,
                BorderWidth = linewidth,
                HasFill = false,
                StartAngle = 135,
                Extent = 45,
                Offset = new Vector2D(rect.X, rect.Bottom - s),
                Size = new Vector2D(2 * s, 2 * s)
            };

            yield return new Arc()
            {
                BorderColor = color,
                BorderWidth = linewidth,
                HasFill = false,
                StartAngle = 0,
                Extent = 45,
                Offset = new Vector2D(rect.Right - 2 * s, rect.Bottom - s),
                Size = new Vector2D(2 * s, 2 * s)
            };

            //yield return new Line(new Vector2D(Vector1D.Zero, rect.Height - arcsize)) { BorderColor = color, BorderWidth = linewidth, Offset = new Vector2D(rect.X, rect.Y + arcsize) };
            //yield return new Line(new Vector2D(Vector1D.Zero, rect.Height - arcsize)) { BorderColor = color, BorderWidth = linewidth, Offset = new Vector2D(rect.Right, rect.Y + arcsize) };
            yield return new Line(new Vector2D(rect.Width + linewidth, Vector1D.Zero)) { BorderColor = color, BorderWidth = linewidth, Offset = new Vector2D(rect.X - linewidth / 2, rect.Bottom) };
        }
    }
}
