namespace MoogleEngine;

public class SearchItem
{
    public SearchItem(string title, string text, string snippet, float score)
    {
        this.Title = title;
        this.Snippet = snippet;
        this.Score = score;
        this.Text=text;
    }

    public string Title { get; private set; }

    public string Snippet { get; private set; }

    public float Score { get; private set; }
    
    public string Text{get; private set;}
}
