/*
 * RichText.cs
 * 
 * @author mosframe / https://github.com/mosframe
 * 
 */

namespace Mosframe {

    using System.Text;
	
	/// <summary>
	/// RichText
	/// </summary>
    
	public class RichText {

        public class Token {

            public const string Bold    = "b"       ;
            public const string Italic  = "i"       ;
            public const string Size    = "size"    ;
            public const string Color   = "color"   ;
        }

        public static string Bold       ( object obj                    ) { return Tag( obj, Token.Bold         ); }
        public static string Italic     ( object obj                    ) { return Tag( obj, Token.Italic       ); }
        public static string Size       ( object obj, int       size    ) { return Tag( obj, Token.Size , size  ); }
        public static string Color      ( object obj, string    color   ) { return Tag( obj, Token.Color, color ); }

        // fixed colors

        public static string Aqua       ( object obj ) { return Tag( obj, Token.Color, HtmlColor.Aqua       ); }
        public static string Black      ( object obj ) { return Tag( obj, Token.Color, HtmlColor.Black      ); }
        public static string Blue       ( object obj ) { return Tag( obj, Token.Color, HtmlColor.Blue       ); }
        public static string Brown      ( object obj ) { return Tag( obj, Token.Color, HtmlColor.Brown      ); }
        public static string Cyan       ( object obj ) { return Tag( obj, Token.Color, HtmlColor.Cyan       ); }
        public static string DarkBlue   ( object obj ) { return Tag( obj, Token.Color, HtmlColor.DarkBlue   ); }
        public static string Green      ( object obj ) { return Tag( obj, Token.Color, HtmlColor.Green      ); }
        public static string Grey       ( object obj ) { return Tag( obj, Token.Color, HtmlColor.Grey       ); }
        public static string LightBlue  ( object obj ) { return Tag( obj, Token.Color, HtmlColor.LightBlue  ); }
        public static string Lime       ( object obj ) { return Tag( obj, Token.Color, HtmlColor.Lime       ); }
        public static string Magenta    ( object obj ) { return Tag( obj, Token.Color, HtmlColor.Magenta    ); }
        public static string Maroon     ( object obj ) { return Tag( obj, Token.Color, HtmlColor.Maroon     ); }
        public static string Navy       ( object obj ) { return Tag( obj, Token.Color, HtmlColor.Navy       ); }
        public static string Olive      ( object obj ) { return Tag( obj, Token.Color, HtmlColor.Olive      ); }
        public static string Orange     ( object obj ) { return Tag( obj, Token.Color, HtmlColor.Orange     ); }
        public static string Purple     ( object obj ) { return Tag( obj, Token.Color, HtmlColor.Purple     ); }
        public static string Red        ( object obj ) { return Tag( obj, Token.Color, HtmlColor.Red        ); }
        public static string Silver     ( object obj ) { return Tag( obj, Token.Color, HtmlColor.Silver     ); }
        public static string Teal       ( object obj ) { return Tag( obj, Token.Color, HtmlColor.Teal       ); }
        public static string White      ( object obj ) { return Tag( obj, Token.Color, HtmlColor.White      ); }
        public static string Yellow     ( object obj ) { return Tag( obj, Token.Color, HtmlColor.Yellow     ); }


        private static string Tag ( object obj, string token, object attribute=null ) {

            var sb = new StringBuilder();
            if( obj != null ) {
                var text = obj.ToString();
                var texts = text.Split('\n');
                if( attribute == null ) {
                    sb.Append("<").Append(token).Append(">").Append( texts[0] ).Append("</").Append(token).Append(">");
                    for( var i=1; i<texts.Length; ++i ) {
                        sb.Append("\n<").Append(token).Append(">").Append( texts[i] ).Append("</").Append(token).Append(">");
                    }
                } else {
                    sb.Append("<").Append(token).Append("=").Append(attribute).Append(">").Append( texts[0] ).Append("</").Append(token).Append(">");
                    for( var i=1; i<texts.Length; ++i ) {
                        sb.Append("\n<").Append(token).Append("=").Append(attribute).Append(">").Append( texts[i] ).Append("</").Append(token).Append(">");
                    }
                }
            }
            return sb.ToString();
        }


        public class OpenTags {

            public static string Bold    = string.Format("<{0}>", Token.Bold  );
            public static string Italic  = string.Format("<{0}>", Token.Italic);
            public static string Size    = string.Format("<{0}=", Token.Size  );
            public static string Color   = string.Format("<{0}=", Token.Color );
        }

        public class CloseTags {

            public static string Bold    = string.Format("</{0}>", Token.Bold  );
            public static string Italic  = string.Format("</{0}>", Token.Italic);
            public static string Size    = string.Format("</{0}>", Token.Size  );
            public static string Color   = string.Format("</{0}>", Token.Color );
        }
    }
}

