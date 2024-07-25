namespace Mal.OnyxTemplate.DocumentModel
{
    public class TextBlock : DocumentBlock
    {
        public TextBlock(string text)
        {
            Text = text;
        }

        public string Text { get; }

        public override string ToString() => Text;
    }
}