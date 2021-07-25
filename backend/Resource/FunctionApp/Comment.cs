using System;
public class Comment
{
    public int comment_id { get; set; }
    public int post_id { get; set; }
    public int author_id { get; set; }
    public string username { get; set; }

    public int parent_id { get; set; }
    public string content_body { get; set; }
    public DateTime created_time { get; set; }
    public DateTime last_edit_time { get; set; }
    public int up_count { get; set; }
    public int down_count { get; set; }
    public bool is_admin {get; set; }
}