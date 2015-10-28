using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meetup2Slack
{
    public class MeetupNotification
    {
        public string link { get; set; }
        public long id { get; set; }
        public Target target { get; set; }
        public string text { get; set; }
        public string kind { get; set; }
    }

    public class Target
    {
        public string type { get; set; }
        public string event_id { get; set; }
        public int group_id { get; set; }
        public string group_urlname { get; set; }
        public int? comment_id { get; set; }
        public bool? pending { get; set; }
    }

}
