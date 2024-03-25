namespace ContextAndType;

public class ParentViewModel
{
    public string Text => "ParentText";
    public ChildViewModel Child { get; } = new ChildViewModel();
}

public class ChildViewModel
{
    public string Text => "ChildText";
    public string Text2 => "ChildText2";
}