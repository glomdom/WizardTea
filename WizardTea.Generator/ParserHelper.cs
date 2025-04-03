namespace WizardTea.Generator;

public static class ParserHelper {
    public static void RewriteValueBasedOnType(ref string val, string type) {
        switch (type) {
            case "float": {
                val += "f";

                break;
            }
        }

        if (type.ToLower().EndsWith("flag")) {
            val = $"({type}){val}";
        }
    }
}