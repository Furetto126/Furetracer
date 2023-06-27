using System.Text;

namespace Lib
{
    class FilteringStringWriter : TextWriter
    {
        private TextWriter writer;

        public FilteringStringWriter(TextWriter writer)
        {
            this.writer = writer;
        }
        public override Encoding Encoding => writer.Encoding;

        public override void Write(string value)
        {
            if (!IsExcluded(value))
            {
                writer.Write(value);
            }
        }

      
        private static bool IsExcluded(string value) {
            return value.Contains("imgui");
        }
    }
}
