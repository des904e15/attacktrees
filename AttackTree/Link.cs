using DeadDog.PDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AttackTree
{
    public class Link
    {
        private Node from, to;

        private static string GetValue(string node, string key)
        {
            var m = Regex.Match(node, $@"[^\[]+\[[^\]]*({key})=(""([^""]+)""|[^""][^,\]]+)");

            var temp = m.Success ? m.Groups[2].Value : null;

            if (temp?[0] == '"')
                temp = temp.Substring(1, temp.Length - 2);

            if (temp?.Contains("\\n") ?? false)
                temp = temp.Replace("\\n", "\n");

            return temp;
        }

        public static Link[] GetLinks(string dot, Node[] nodes)
        {
            var regex = new Regex(@"(?<from>[a-zA-Z]+)[ \t]*->[ \t]*(?<to>[a-zA-Z]+)[ \t]*\[[^\]]+\]");

            var links = from m in regex.Matches(dot).OfType<Match>()
                        let f = m.Groups["from"].Value
                        let t = m.Groups["to"].Value
                        select new Link(nodes.First(n => n.Name == f), nodes.First(n => n.Name == t));

            return links.ToArray();
        }

        public Link(Node from, Node to)
        {
            this.from = from;
            this.to = to;
        }

        public Node From => from;
        public Node To => to;
    }
}
