using System.Linq;

namespace Bogaculta.IO
{
    public static class Strings
    {
        public static string GetTypeName(this object obj)
        {
            return obj.GetType().FullName?.Split('.')
                .Last().Split('+', 2)[0].ToLowerInvariant();
        }

        public static string GetText(this bool? value)
        {
            if (value == null) return "?";
            return value.Value ? "ok" : "fail";
        }
    }
}