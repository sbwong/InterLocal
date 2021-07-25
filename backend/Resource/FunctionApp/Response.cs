using System;
using System.Collections.Generic;

public class Response
{
    public int post_id { get; set; }
    public int author_id { get; set; }
    public string username { get; set; }
    public string content { get; set; }
    public string content_type { get; set; }
    public string title { get; set; }
    public DateTime created_time { get; set; }
    public DateTime last_edit_time { get; set; }
    public int up_count { get; set; }
    public int down_count { get; set; }
    public List<Comment> comments { get; set; }
    public List<string> tags { get; set; }
    public bool? is_upvote { get; set; }
    public bool is_admin {get; set; }
}