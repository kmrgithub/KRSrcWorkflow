using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SearchDemo.Model;
using GalaSoft.MvvmLight.Messaging;

namespace SearchDemo.Message
{
    public class SearchResultMessage : GenericMessage<IEnumerable<SearchResult>> 
    {
        
        public SearchResultMessage(IEnumerable<SearchResult> content): base(content) {}
        public SearchResultMessage(object sender, IEnumerable<SearchResult> content) : base(sender,content){}
        public SearchResultMessage(object sender, object target, IEnumerable<SearchResult> content) : base(sender,target,content){}
    }
}
