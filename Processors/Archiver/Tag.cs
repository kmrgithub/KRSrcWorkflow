using System;
using System.Collections.Generic;

namespace Archiver
{
    public class Tag
    {
        public int Start { get; private set; }
        public string Content { get; private set; }
        public List<Tag> InnerTags { get; private set; }
        public string Self { get; private set; }
        public bool IsValid { get; private set; }
        public int Length { get; private set; }

        public Tag(string text)
        {
            this.Start = text.IndexOf('<') + 1;
            this.IsValid = this.Start > 0;
            this.InnerTags = new List<Tag>();
            this.Self =  this.IsValid ? this.BreakUp(text) : this.Invalidate();
        }

        private string BreakUp(string text)
        {
            try
            {
                int openTagEnd = text.IndexOf('>', this.Start);
                int tagLength = openTagEnd - this.Start;
                string self = this.GetTagTrimmed(text.Substring(this.Start, tagLength));
                string closeTag = string.Format("</{0}>", self);
                int closeTagStart = text.IndexOf(closeTag, openTagEnd, StringComparison.CurrentCulture);
                int contentLength = closeTagStart - openTagEnd - 1;

                this.Length = (closeTagStart + self.Length + 2) - (this.Start - 1);

                string innerText = text.Substring(openTagEnd + 1, contentLength);

                int innerTagsLength = this.GetInnerTags(innerText);
                int innerTextLength = contentLength - innerTagsLength;
                this.Content = text.Substring(openTagEnd + innerTagsLength + 1, innerTextLength);

                return self;
            }
            catch (Exception ex)
            {
                KRSrcWorkflow.WFLogger.NLogger.ErrorException("ERROR: BreakUp failed!", ex);
                this.IsValid = false;
                this.Content = string.Empty;
                return string.Empty;
            }
        }

        private string GetTagTrimmed(string tag)
        {
            int whiteSpace = tag.IndexOfAny(new char[] {' ', '\t', '\r', '\n'});
            if (whiteSpace > 0)
            {
                return tag.Substring(0, whiteSpace);
            }

            return tag;
        }

        private int GetInnerTags(string innerContent)
        {
            int len = 0;
            string leftToEvaluate = innerContent;
            Tag inner = new Tag(leftToEvaluate);
            while (inner.IsValid)
            {
                this.InnerTags.Add(inner);
                int innerLength = inner.Start + inner.Length;
                len += innerLength;
                leftToEvaluate = leftToEvaluate.Substring(innerLength, leftToEvaluate.Length - innerLength);
                inner = new Tag(leftToEvaluate);
            }

            return len;
        }

        private string Invalidate()
        {
            this.Content = string.Empty;
            return string.Empty;
        }

    }
}
