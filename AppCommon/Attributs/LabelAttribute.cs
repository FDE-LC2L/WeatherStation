namespace AppCommon.Attributs
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class LabelAttribute : Attribute
    {
        public string Text { get; }

        public LabelAttribute(string text)
        {
            Text = text;
        }
    }
}
