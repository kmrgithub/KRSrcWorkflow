using System.Collections.Generic;

namespace Archiver
{
    public class Flatten
    {
        public static List<Tag> Go(Tag root)
        {
            List<Tag> ret = new List<Tag>();
            Go(root, ret);
            return ret;
        }

        private static void Go(Tag node, List<Tag> toList)
        {
            toList.Add(node);
            foreach (Tag tag in node.InnerTags)
            {
                Flatten.Go(tag, toList);
            }
        }
    }
}
