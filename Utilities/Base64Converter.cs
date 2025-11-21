namespace EducaMente.Utilities
{
    public class Base64Converter
    {
        // Codifica un texto plano a Base64 usando UTF-8.
        public static string EncodeBase64(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                return string.Empty;

            var bytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(bytes);
        }

        // Decodifica un texto en Base64 a texto plano usando UTF-8.
        public static string DecodeBase64(string base64Text)
        {
            if (string.IsNullOrWhiteSpace(base64Text))
                return string.Empty;

            var bytes = Convert.FromBase64String(base64Text);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }
}
